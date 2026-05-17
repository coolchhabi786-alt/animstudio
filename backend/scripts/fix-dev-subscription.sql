-- AnimStudio: Complete dev data fix
-- Creates user → team → team membership → subscription in the right order.
-- Idempotent — safe to run multiple times.
--
-- Run in SSMS / Azure Data Studio:
--   Server: localhost,1433  |  Login: sa / AnimStudio!Dev123  |  DB: AnimStudio

USE AnimStudio;
GO

DECLARE @devTeamId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000002';
DECLARE @devSubId  UNIQUEIDENTIFIER = 'D0000001-0000-0000-0000-000000000001';
DECLARE @devPlanId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @devEmail  NVARCHAR(320)    = 'dev@animstudio.local';
DECLARE @now       DATETIMEOFFSET   = SYSDATETIMEOFFSET();

-- ── 1. Resolve the owner user ID ───────────────────────────────────────────────
-- Auto-registration creates the user with a random internal Id but the DevUserId
-- as ExternalId. We need their actual DB Id for FK constraints on Teams.OwnerId.
DECLARE @ownerUserId UNIQUEIDENTIFIER;
SELECT @ownerUserId = Id FROM [identity].[Users] WHERE Email = @devEmail;

IF @ownerUserId IS NULL
BEGIN
    -- Completely fresh DB — create the dev user with the fixed Id
    SET @ownerUserId = '00000000-0000-0000-0000-000000000001';
    INSERT INTO [identity].[Users]
        (Id, ExternalId, Email, DisplayName, AvatarUrl,
         CreatedAt, UpdatedAt, IsDeleted)
    VALUES
        (@ownerUserId, '00000000-0000-0000-0000-000000000001',
         @devEmail, 'Dev User', NULL,
         @now, @now, 0);
    PRINT 'Created dev user';
END
ELSE
    PRINT 'User found: ' + CAST(@ownerUserId AS NVARCHAR(50));
GO

-- Re-declare in new batch (GO resets local vars)
DECLARE @devTeamId   UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000002';
DECLARE @devSubId    UNIQUEIDENTIFIER = 'D0000001-0000-0000-0000-000000000001';
DECLARE @devPlanId   UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @devEmail    NVARCHAR(320)    = 'dev@animstudio.local';
DECLARE @now         DATETIMEOFFSET   = SYSDATETIMEOFFSET();
DECLARE @ownerUserId UNIQUEIDENTIFIER;

SELECT @ownerUserId = Id FROM [identity].[Users] WHERE Email = @devEmail;

-- ── 2. Create team ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [identity].[Teams] WHERE Id = @devTeamId)
BEGIN
    INSERT INTO [identity].[Teams]
        (Id, Name, LogoUrl, OwnerId, CreatedAt, UpdatedAt, IsDeleted)
    VALUES
        (@devTeamId, 'Dev Team', NULL, @ownerUserId, @now, @now, 0);
    PRINT 'Created team';
END
ELSE
    PRINT 'Team exists — skipping';

-- ── 3. Create team membership (Owner) ─────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [identity].[TeamMembers]
               WHERE TeamId = @devTeamId AND UserId = @ownerUserId)
BEGIN
    INSERT INTO [identity].[TeamMembers]
        (TeamId, UserId, Role, JoinedAt, InviteToken, InviteExpiresAt, InviteAcceptedAt)
    VALUES
        (@devTeamId, @ownerUserId, 'Owner', @now, NULL, NULL, @now);
    PRINT 'Created team membership (Owner)';
END
ELSE
    PRINT 'Team membership exists — skipping';

-- ── 4. Ensure Pro plan exists ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [identity].[Plans] WHERE Id = @devPlanId)
BEGIN
    INSERT INTO [identity].[Plans]
        (Id, Name, StripePriceId, Price, EpisodesPerMonth,
         MaxCharacters, MaxTeamMembers, IsActive, IsDefault)
    VALUES
        (@devPlanId, 'Pro', 'price_dev_pro', 49.00, 20, 10, 5, 1, 1);
    PRINT 'Created Plan: Pro';
END
ELSE
    PRINT 'Plan exists — skipping';

-- ── 5. Upsert subscription ─────────────────────────────────────────────────────
-- Team now exists so FK is satisfied for both UPDATE and INSERT.
IF EXISTS (SELECT 1 FROM [identity].[Subscriptions] WHERE Id = @devSubId)
BEGIN
    -- Row exists (possibly for old team) — redirect to correct team
    UPDATE [identity].[Subscriptions]
    SET  TeamId            = @devTeamId,
         PlanId            = @devPlanId,
         Status            = 'Active',
         CurrentPeriodEnd  = DATEADD(MONTH, 1, @now),
         CancelAtPeriodEnd = 0,
         UsageResetAt      = DATEADD(MONTH, 1, @now),
         UpdatedAt         = @now
    WHERE Id = @devSubId;
    PRINT 'Subscription updated (re-targeted to correct team, status=Active)';
END
ELSE IF EXISTS (SELECT 1 FROM [identity].[Subscriptions] WHERE TeamId = @devTeamId)
BEGIN
    UPDATE [identity].[Subscriptions]
    SET  Status            = 'Active',
         CurrentPeriodEnd  = DATEADD(MONTH, 1, @now),
         CancelAtPeriodEnd = 0,
         UsageResetAt      = DATEADD(MONTH, 1, @now),
         UpdatedAt         = @now
    WHERE TeamId = @devTeamId;
    PRINT 'Team subscription refreshed (Active, period extended)';
END
ELSE
BEGIN
    INSERT INTO [identity].[Subscriptions]
        (Id, TeamId, PlanId, StripeCustomerId, StripeSubscriptionId,
         Status, CurrentPeriodStart, CurrentPeriodEnd, TrialEndsAt,
         CancelAtPeriodEnd, UsageEpisodesThisMonth, UsageResetAt,
         CreatedAt, UpdatedAt, IsDeleted)
    VALUES
        (@devSubId, @devTeamId, @devPlanId, 'cus_alpha_001', NULL,
         'Active', @now, DATEADD(MONTH, 1, @now), NULL,
         0, 0, DATEADD(MONTH, 1, @now),
         @now, @now, 0);
    PRINT 'Subscription created';
END
GO

-- ── Verify ─────────────────────────────────────────────────────────────────────
SELECT
    u.Email,
    t.Id  AS TeamId,
    t.Name AS TeamName,
    s.Status,
    s.CurrentPeriodEnd,
    p.Name AS PlanName
FROM  [identity].[Teams]         t
JOIN  [identity].[Users]         u ON u.Id = t.OwnerId
JOIN  [identity].[Subscriptions] s ON s.TeamId = t.Id
JOIN  [identity].[Plans]         p ON p.Id = s.PlanId
WHERE t.Id = '00000000-0000-0000-0000-000000000002';
GO
