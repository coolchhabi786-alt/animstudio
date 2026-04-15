USE [AnimStudio];
GO

/* =============================================================================
   AnimStudio — Comprehensive Team / User Data Query
   =============================================================================
   Usage
   -----
   Set EITHER @TeamId OR @UserId (or both).
   All 8 result sets will return data scoped to that Team / User.

   Result sets
   -----------
   RS-1  Team identity    — team + member + subscription + plan limits
   RS-2  Team members     — all members with roles
   RS-3  Projects         — with per-project episode counts and completion rate
   RS-4  Episodes         — full pipeline status with latest job info
   RS-5  Characters       — team gallery with training progress + cast count
   RS-6  Episode cast     — which characters are in which episodes
   RS-7  Scripts          — one row per episode that has a script
   RS-8  Jobs timeline    — all jobs ordered newest first
   RS-9  Saga states      — active production sagas
   RS-10 Outbox messages  — recent 20 messages relevant to this team/user
   ============================================================================= */

-- ── Parameters — set one or both ─────────────────────────────────────────────
DECLARE @TeamId UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000001';
DECLARE @UserId UNIQUEIDENTIFIER = '4327FA31-FF93-4B68-AF4D-1FC56AFA33C5';
-- To query by TeamId only : set @UserId = NULL
-- To query by UserId only : set @TeamId = NULL
-- Both values provided    : returns data for the team the user belongs to

-- ── Resolve team if only UserId was supplied ──────────────────────────────────
IF @TeamId IS NULL AND @UserId IS NOT NULL
BEGIN
    SELECT @TeamId = tm.TeamId
    FROM   [identity].[TeamMembers] tm
    WHERE  tm.UserId = @UserId
      AND  NOT EXISTS (
               SELECT 1 FROM [identity].[Teams] t
               WHERE  t.Id = tm.TeamId AND t.IsDeleted = 1);
END

-- ── Resolve userId if only TeamId was supplied ────────────────────────────────
IF @UserId IS NULL AND @TeamId IS NOT NULL
BEGIN
    SELECT TOP 1 @UserId = tm.UserId
    FROM   [identity].[TeamMembers] tm
    WHERE  tm.TeamId = @TeamId
      AND  tm.Role = 'Owner';
END

PRINT 'Resolved TeamId : ' + CAST(ISNULL(@TeamId, '(null)') AS NVARCHAR(50));
PRINT 'Resolved UserId : ' + CAST(ISNULL(@UserId, '(null)') AS NVARCHAR(50));
PRINT '';

-- =============================================================================
PRINT '══ RS-1  Team Identity ══════════════════════════════════════════════════';
-- =============================================================================
SELECT
    t.Id                            AS TeamId,
    t.Name                          AS TeamName,
    t.LogoUrl,
    t.OwnerId,
    u.Email                         AS OwnerEmail,
    u.DisplayName                   AS OwnerDisplayName,
    p.Name                          AS PlanName,
    p.Price                         AS PlanPrice,
    p.EpisodesPerMonth,
    p.MaxCharacters,
    p.MaxTeamMembers,
    s.Status                        AS SubscriptionStatus,
    s.StripeSubscriptionId,
    s.CurrentPeriodStart,
    s.CurrentPeriodEnd,
    s.TrialEndsAt,
    s.CancelAtPeriodEnd,
    s.UsageEpisodesThisMonth        AS EpisodesUsedThisMonth,
    p.EpisodesPerMonth
        - s.UsageEpisodesThisMonth  AS EpisodesRemaining,
    s.UsageResetAt,
    t.CreatedAt                     AS TeamCreatedAt
FROM        [identity].[Teams]         t
JOIN        [identity].[Users]         u  ON u.Id  = t.OwnerId
JOIN        [identity].[Subscriptions] s  ON s.TeamId = t.Id AND s.IsDeleted = 0
JOIN        [identity].[Plans]         p  ON p.Id  = s.PlanId
WHERE t.Id = @TeamId
  AND t.IsDeleted = 0;

-- =============================================================================
PRINT '══ RS-2  Team Members ═══════════════════════════════════════════════════';
-- =============================================================================
SELECT
    u.Id                AS UserId,
    u.Email,
    u.DisplayName,
    u.AvatarUrl,
    tm.Role,
    tm.JoinedAt,
    tm.InviteAcceptedAt,
    CASE WHEN tm.InviteToken IS NOT NULL
              AND tm.InviteAcceptedAt IS NULL
              AND (tm.InviteExpiresAt IS NULL OR tm.InviteExpiresAt > SYSDATETIMEOFFSET())
         THEN 'Pending'
         ELSE 'Active'
    END                 AS InviteStatus,
    u.LastLoginAt
FROM        [identity].[TeamMembers] tm
JOIN        [identity].[Users]       u ON u.Id = tm.UserId
WHERE tm.TeamId = @TeamId
ORDER BY
    CASE tm.Role WHEN 'Owner' THEN 0 WHEN 'Admin' THEN 1 ELSE 2 END,
    tm.JoinedAt;

-- =============================================================================
PRINT '══ RS-3  Projects ═══════════════════════════════════════════════════════';
-- =============================================================================
SELECT
    proj.Id                                         AS ProjectId,
    proj.Name                                       AS ProjectName,
    proj.Description,
    proj.ThumbnailUrl,
    COUNT(e.Id)                                     AS TotalEpisodes,
    SUM(CASE WHEN e.Status IN ('Done','Completed')
             THEN 1 ELSE 0 END)                     AS CompletedEpisodes,
    SUM(CASE WHEN e.Status = 'Animation'
             THEN 1 ELSE 0 END)                     AS InAnimationEpisodes,
    SUM(CASE WHEN e.Status = 'Script'
             THEN 1 ELSE 0 END)                     AS InScriptEpisodes,
    SUM(CASE WHEN e.Status IN ('Idle','Draft')
             THEN 1 ELSE 0 END)                     AS IdleEpisodes,
    SUM(CASE WHEN e.Status = 'Failed'
             THEN 1 ELSE 0 END)                     AS FailedEpisodes,
    proj.CreatedAt,
    proj.UpdatedAt
FROM        [content].[Projects] proj
LEFT JOIN   [content].[Episodes] e ON e.ProjectId = proj.Id AND e.IsDeleted = 0
WHERE proj.TeamId = @TeamId
  AND proj.IsDeleted = 0
GROUP BY
    proj.Id, proj.Name, proj.Description, proj.ThumbnailUrl,
    proj.CreatedAt, proj.UpdatedAt
ORDER BY proj.CreatedAt DESC;

-- =============================================================================
PRINT '══ RS-4  Episodes (pipeline status + latest job) ════════════════════════';
-- =============================================================================
SELECT
    proj.Name                   AS ProjectName,
    e.Id                        AS EpisodeId,
    e.Name                      AS EpisodeName,
    e.Status                    AS EpisodeStatus,
    e.Style,
    e.TemplateId,
    et.Title                    AS TemplateName,
    e.DirectorNotes,
    e.RenderedAt,
    -- Latest job for this episode
    j.Type                      AS LatestJobType,
    j.Status                    AS LatestJobStatus,
    j.QueuedAt                  AS JobQueuedAt,
    j.StartedAt                 AS JobStartedAt,
    j.CompletedAt               AS JobCompletedAt,
    CASE WHEN j.StartedAt IS NOT NULL
         THEN DATEDIFF(SECOND, j.StartedAt,
                       ISNULL(j.CompletedAt, SYSDATETIMEOFFSET()))
         ELSE NULL
    END                         AS JobDurationSeconds,
    j.ErrorMessage,
    -- Script
    CASE WHEN scr.Id IS NOT NULL THEN 1 ELSE 0 END AS HasScript,
    scr.IsManuallyEdited        AS ScriptManuallyEdited,
    -- Characters cast count
    COUNT(DISTINCT ec.CharacterId) AS CastCharacters,
    e.CreatedAt,
    e.UpdatedAt
FROM        [content].[Episodes]        e
JOIN        [content].[Projects]        proj ON proj.Id = e.ProjectId
LEFT JOIN   [content].[EpisodeTemplates] et  ON et.Id  = e.TemplateId
LEFT JOIN   [content].[Scripts]         scr  ON scr.EpisodeId = e.Id  AND scr.IsDeleted = 0
LEFT JOIN   [content].[EpisodeCharacters] ec ON ec.EpisodeId = e.Id
-- Correlated sub-select picks only the most recent job per episode
LEFT JOIN   [content].[Jobs]            j   ON j.Id = (
                SELECT TOP 1 j2.Id
                FROM   [content].[Jobs] j2
                WHERE  j2.EpisodeId = e.Id
                ORDER  BY j2.QueuedAt DESC)
WHERE proj.TeamId = @TeamId
  AND proj.IsDeleted = 0
  AND e.IsDeleted   = 0
GROUP BY
    proj.Name, e.Id, e.Name, e.Status, e.Style, e.TemplateId, et.Title,
    e.DirectorNotes, e.RenderedAt,
    j.Type, j.Status, j.QueuedAt, j.StartedAt, j.CompletedAt, j.ErrorMessage,
    scr.Id, scr.IsManuallyEdited,
    e.CreatedAt, e.UpdatedAt
ORDER BY proj.Name, e.CreatedAt;

-- =============================================================================
PRINT '══ RS-5  Characters (team gallery) ══════════════════════════════════════';
-- =============================================================================
SELECT
    c.Id                        AS CharacterId,
    c.Name                      AS CharacterName,
    c.Description,
    c.StyleDna,
    c.ImageUrl,
    c.TriggerWord,
    c.TrainingStatus,
    c.TrainingProgressPercent,
    c.CreditsCost,
    c.LoraWeightsUrl,
    COUNT(DISTINCT ec.EpisodeId) AS EpisodesCastIn,
    c.CreatedAt,
    c.UpdatedAt
FROM        [content].[Characters]      c
LEFT JOIN   [content].[EpisodeCharacters] ec ON ec.CharacterId = c.Id
WHERE c.TeamId  = @TeamId
  AND c.IsDeleted = 0
GROUP BY
    c.Id, c.Name, c.Description, c.StyleDna, c.ImageUrl, c.TriggerWord,
    c.TrainingStatus, c.TrainingProgressPercent, c.CreditsCost,
    c.LoraWeightsUrl, c.CreatedAt, c.UpdatedAt
ORDER BY
    CASE c.TrainingStatus
        WHEN 'Ready'    THEN 0
        WHEN 'Training' THEN 1
        WHEN 'TrainingQueued' THEN 2
        WHEN 'PoseGeneration' THEN 3
        WHEN 'Draft'    THEN 4
        WHEN 'Failed'   THEN 5
        ELSE 6
    END,
    c.CreatedAt DESC;

-- =============================================================================
PRINT '══ RS-6  Episode Cast (character × episode matrix) ═════════════════════';
-- =============================================================================
SELECT
    proj.Name                   AS ProjectName,
    e.Name                      AS EpisodeName,
    e.Status                    AS EpisodeStatus,
    c.Name                      AS CharacterName,
    c.TrainingStatus            AS CharacterTrainingStatus,
    ec.AttachedAt
FROM        [content].[EpisodeCharacters] ec
JOIN        [content].[Episodes]   e    ON e.Id   = ec.EpisodeId  AND e.IsDeleted   = 0
JOIN        [content].[Projects]   proj ON proj.Id = e.ProjectId  AND proj.IsDeleted = 0
JOIN        [content].[Characters] c    ON c.Id   = ec.CharacterId AND c.IsDeleted   = 0
WHERE proj.TeamId = @TeamId
ORDER BY proj.Name, e.Name, ec.AttachedAt;

-- =============================================================================
PRINT '══ RS-7  Scripts ════════════════════════════════════════════════════════';
-- =============================================================================
SELECT
    proj.Name                   AS ProjectName,
    e.Name                      AS EpisodeName,
    e.Status                    AS EpisodeStatus,
    scr.Id                      AS ScriptId,
    scr.Title                   AS ScriptTitle,
    scr.IsManuallyEdited,
    scr.DirectorNotes,
    -- Scene count extracted from JSON
    (LEN(scr.RawJson) - LEN(REPLACE(scr.RawJson, '"scene_number"', '')))
        / LEN('"scene_number"')  AS ApproxSceneCount,
    scr.CreatedAt               AS ScriptCreatedAt,
    scr.UpdatedAt               AS ScriptLastUpdated
FROM        [content].[Scripts]  scr
JOIN        [content].[Episodes] e    ON e.Id   = scr.EpisodeId AND e.IsDeleted   = 0
JOIN        [content].[Projects] proj ON proj.Id = e.ProjectId  AND proj.IsDeleted = 0
WHERE proj.TeamId    = @TeamId
  AND scr.IsDeleted  = 0
ORDER BY proj.Name, e.Name;

-- =============================================================================
PRINT '══ RS-8  Jobs Timeline ══════════════════════════════════════════════════';
-- =============================================================================
SELECT
    proj.Name                   AS ProjectName,
    e.Name                      AS EpisodeName,
    e.Status                    AS EpisodeStatus,
    j.Id                        AS JobId,
    j.Type                      AS JobType,
    j.Status                    AS JobStatus,
    j.AttemptNumber,
    j.QueuedAt,
    j.StartedAt,
    j.CompletedAt,
    CASE WHEN j.StartedAt IS NOT NULL
         THEN DATEDIFF(SECOND, j.StartedAt,
                       ISNULL(j.CompletedAt, SYSDATETIMEOFFSET()))
         ELSE NULL
    END                         AS DurationSeconds,
    j.ErrorMessage,
    j.CreatedAt
FROM        [content].[Jobs]     j
JOIN        [content].[Episodes] e    ON e.Id   = j.EpisodeId  AND e.IsDeleted   = 0
JOIN        [content].[Projects] proj ON proj.Id = e.ProjectId AND proj.IsDeleted = 0
WHERE proj.TeamId = @TeamId
ORDER BY j.QueuedAt DESC;

-- =============================================================================
PRINT '══ RS-9  Saga States (active production pipelines) ══════════════════════';
-- =============================================================================
SELECT
    proj.Name                   AS ProjectName,
    e.Name                      AS EpisodeName,
    e.Status                    AS EpisodeStatus,
    ss.Id                       AS SagaId,
    ss.CurrentStage,
    ss.IsCompensating,
    ss.RetryCount,
    ss.LastError,
    ss.StartedAt,
    ss.UpdatedAt,
    DATEDIFF(MINUTE, ss.StartedAt, ss.UpdatedAt) AS RunningMinutes
FROM        [shared].[SagaStates] ss
JOIN        [content].[Episodes]  e    ON e.Id   = ss.EpisodeId AND e.IsDeleted   = 0
JOIN        [content].[Projects]  proj ON proj.Id = e.ProjectId AND proj.IsDeleted = 0
WHERE proj.TeamId = @TeamId
ORDER BY ss.StartedAt DESC;

-- =============================================================================
PRINT '══ RS-10  Outbox Messages (last 20 for this team/user) ══════════════════';
-- =============================================================================
SELECT TOP 20
    om.Id,
    om.EventType,
    om.Status,
    om.OccurredAt,
    om.ProcessedAt,
    om.RetryCount,
    -- Show payload preview (first 300 chars to keep output readable)
    LEFT(om.Payload, 300)       AS PayloadPreview
FROM [shared].[OutboxMessages] om
WHERE  om.Payload LIKE '%' + CAST(@TeamId AS NVARCHAR(36)) + '%'
    OR om.Payload LIKE '%' + CAST(@UserId AS NVARCHAR(36)) + '%'
ORDER BY om.OccurredAt DESC;
