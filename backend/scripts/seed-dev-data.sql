USE [AnimStudio];
GO

/* =============================================================================
   AnimStudio — Local Dev Seed Script
   =============================================================================
   Prerequisites
   -------------
   All EF Core migrations must be applied before running this script:

     dotnet ef database update --project src\AnimStudio.SharedKernel   --startup-project src\AnimStudio.API
     dotnet ef database update --project src\AnimStudio.IdentityModule --startup-project src\AnimStudio.API
     dotnet ef database update --project src\AnimStudio.ContentModule  --startup-project src\AnimStudio.API

   Dev identity
   ------------
   Email      : dev@animstudio.local
   UserId     : 4327FA31-FF93-4B68-AF4D-1FC56AFA33C5   (row already in identity.Users)
   ExternalId : 00000000-0000-0000-0000-000000000001   (DevAuthHandler.DevUserId)
   TeamId     : C0000001-0000-0000-0000-000000000001   (DevAuthHandler.DevTeamId)

   What this seeds
   ---------------
   §0   identity.Users        — confirms dev user exists (inserts if missing)
   §1   identity.Plans        — Starter / Pro / Studio (EF migration seed guard)
   §2   identity.Teams        — "Dev Studio" owned by dev user
   §3   identity.TeamMembers  — dev user as Owner
   §4   identity.Subscriptions— Active Pro subscription
   §5   content.EpisodeTemplates (8 templates — Phase3 migration guard)
   §6   content.StylePresets  (8 presets  — Phase3 migration guard)
   §7   content.Characters    — 3 characters at different training stages
   §8   content.Projects      — 2 projects
   §9   content.Episodes      — 4 episodes (Idle / Script / Animation / Done)
   §10  content.EpisodeCharacters — join-table entries
   §11  content.Jobs          — 3 jobs (Completed / Completed / Running)
   §12  content.Scripts       — 2 scripts (AI-generated + manually edited)
   §13  shared.SagaStates     — saga tracking Episode 4
   §14  shared.OutboxMessages — 4 delivered + 1 pending

   Idempotent — safe to re-run; every INSERT is guarded with IF NOT EXISTS.
   Wrapped in a single transaction — rolls back entirely on any error.
   ============================================================================= */

BEGIN TRANSACTION;
BEGIN TRY

-- =============================================================================
PRINT '── §0  identity.Users — confirm dev user ────────────────────────────────';
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM [identity].[Users]
               WHERE [Id] = '4327FA31-FF93-4B68-AF4D-1FC56AFA33C5')
BEGIN
    -- RowVersion is a true SQL Server timestamp — omit from INSERT; auto-assigned.
    INSERT INTO [identity].[Users]
        ([Id], [Email], [DisplayName], [ExternalId], [AvatarUrl], [LastLoginAt],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('4327FA31-FF93-4B68-AF4D-1FC56AFA33C5',
         'dev@animstudio.local', 'Dev User',
         '00000000-0000-0000-0000-000000000001',
         NULL, NULL,
         '2026-04-04T04:37:56.1306037+00:00',
         '2026-04-04T04:37:56.1306037+00:00', 0);
    PRINT '  + identity.Users — Dev User inserted';
END
ELSE
    PRINT '  ✓ identity.Users — Dev User already exists';


-- =============================================================================
PRINT '── §1  identity.Plans (InitialCreate migration seed — guard only) ────────';
-- =============================================================================
-- RowVersion here is varbinary(max) — NOT a SQL timestamp — supply 0x (empty).

IF NOT EXISTS (SELECT 1 FROM [identity].[Plans]
               WHERE [Id] = '11111111-1111-1111-1111-111111111111')
BEGIN
    INSERT INTO [identity].[Plans]
        ([Id], [Name], [StripePriceId], [EpisodesPerMonth], [MaxCharacters],
         [MaxTeamMembers], [Price], [IsActive], [IsDefault],
         [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
    VALUES
        ('11111111-1111-1111-1111-111111111111',
         'Starter', 'price_starter', 3, 5, 2, 0.00, 1, 1,
         '2024-01-01T00:00:00.0000000+00:00',
         '2024-01-01T00:00:00.0000000+00:00', 0, 0x);
    PRINT '  + Plan: Starter inserted';
END
ELSE PRINT '  ✓ Plan: Starter already exists';

IF NOT EXISTS (SELECT 1 FROM [identity].[Plans]
               WHERE [Id] = '22222222-2222-2222-2222-222222222222')
BEGIN
    INSERT INTO [identity].[Plans]
        ([Id], [Name], [StripePriceId], [EpisodesPerMonth], [MaxCharacters],
         [MaxTeamMembers], [Price], [IsActive], [IsDefault],
         [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
    VALUES
        ('22222222-2222-2222-2222-222222222222',
         'Pro', 'price_pro_monthly', 20, 25, 5, 49.00, 1, 0,
         '2024-01-01T00:00:00.0000000+00:00',
         '2024-01-01T00:00:00.0000000+00:00', 0, 0x);
    PRINT '  + Plan: Pro inserted';
END
ELSE PRINT '  ✓ Plan: Pro already exists';

IF NOT EXISTS (SELECT 1 FROM [identity].[Plans]
               WHERE [Id] = '33333333-3333-3333-3333-333333333333')
BEGIN
    INSERT INTO [identity].[Plans]
        ([Id], [Name], [StripePriceId], [EpisodesPerMonth], [MaxCharacters],
         [MaxTeamMembers], [Price], [IsActive], [IsDefault],
         [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
    VALUES
        ('33333333-3333-3333-3333-333333333333',
         'Studio', 'price_studio_monthly', 100, 100, 20, 199.00, 1, 0,
         '2024-01-01T00:00:00.0000000+00:00',
         '2024-01-01T00:00:00.0000000+00:00', 0, 0x);
    PRINT '  + Plan: Studio inserted';
END
ELSE PRINT '  ✓ Plan: Studio already exists';


-- =============================================================================
PRINT '── §2  identity.Teams ───────────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server timestamp — omit; auto-assigned.

IF NOT EXISTS (SELECT 1 FROM [identity].[Teams]
               WHERE [Id] = 'C0000001-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [identity].[Teams]
        ([Id], [Name], [LogoUrl], [OwnerId],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('C0000001-0000-0000-0000-000000000001',
         'Dev Studio', NULL,
         '4327FA31-FF93-4B68-AF4D-1FC56AFA33C5',
         '2026-04-04T04:37:56.0000000+00:00',
         '2026-04-04T04:37:56.0000000+00:00', 0);
    PRINT '  + Team: Dev Studio inserted';
END
ELSE PRINT '  ✓ Team: Dev Studio already exists';


-- =============================================================================
PRINT '── §3  identity.TeamMembers ─────────────────────────────────────────────';
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM [identity].[TeamMembers]
               WHERE [TeamId] = 'C0000001-0000-0000-0000-000000000001'
                 AND [UserId]  = '4327FA31-FF93-4B68-AF4D-1FC56AFA33C5')
BEGIN
    INSERT INTO [identity].[TeamMembers]
        ([TeamId], [UserId], [Role], [JoinedAt],
         [InviteToken], [InviteExpiresAt], [InviteAcceptedAt])
    VALUES
        ('C0000001-0000-0000-0000-000000000001',
         '4327FA31-FF93-4B68-AF4D-1FC56AFA33C5',
         'Owner',
         '2026-04-04T04:37:56.0000000+00:00',
         NULL, NULL, NULL);
    PRINT '  + TeamMember: Dev User → Dev Studio (Owner) inserted';
END
ELSE PRINT '  ✓ TeamMember already exists';


-- =============================================================================
PRINT '── §4  identity.Subscriptions ───────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server timestamp — omit; auto-assigned.
-- Subscription is on the Pro plan (20 eps/month, 25 chars, 5 members).
-- 2 episodes already used this month (matching the 2 completed episodes seeded below).

IF NOT EXISTS (SELECT 1 FROM [identity].[Subscriptions]
               WHERE [TeamId] = 'C0000001-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [identity].[Subscriptions]
        ([Id], [TeamId], [PlanId],
         [StripeSubscriptionId], [StripeCustomerId],
         [Status],
         [CurrentPeriodStart], [CurrentPeriodEnd], [TrialEndsAt],
         [CancelAtPeriodEnd], [UsageEpisodesThisMonth], [UsageResetAt],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('BBBBBBBB-0001-0000-0000-000000000000',
         'C0000001-0000-0000-0000-000000000001',
         '22222222-2222-2222-2222-222222222222',  -- Pro plan
         NULL, NULL,
         'Active',
         '2026-04-01T00:00:00.0000000+00:00',
         '2026-05-01T00:00:00.0000000+00:00',
         NULL,
         0, 2,
         '2026-05-01T00:00:00.0000000+00:00',
         '2026-04-04T04:37:56.0000000+00:00',
         '2026-04-04T04:37:56.0000000+00:00', 0);
    PRINT '  + Subscription: Pro / Active inserted';
END
ELSE PRINT '  ✓ Subscription already exists';


-- =============================================================================
PRINT '── §5  content.EpisodeTemplates (Phase3Templates migration — guard) ──────';
-- =============================================================================
-- RowVersion is varbinary(max) — supply 0x.
-- These are normally inserted by the EF migration; this section is a safety net.

IF OBJECT_ID('content.EpisodeTemplates', 'U') IS NULL
BEGIN
    PRINT '  ⚠ content.EpisodeTemplates table not found — run Phase3Templates migration first';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0001-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0001-0000-0000-000000000003',
             'Kids Superhero Adventure', 'Kids',
             'A young hero discovers extraordinary powers and must use them to protect their neighbourhood from a quirky villain.',
             '{"acts":[{"name":"Act 1","description":"Hero discovers powers","beats":2},{"name":"Act 2","description":"Villain threatens the neighbourhood","beats":4},{"name":"Act 3","description":"Hero saves the day","beats":2}]}',
             'Pixar3D', NULL, NULL, 1, 1,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Kids Superhero Adventure';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0002-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0002-0000-0000-000000000003',
             'Family Comedy', 'Comedy',
             'A loveable family gets into a series of hilarious misunderstandings that snowball into chaos before a heartfelt resolution.',
             '{"acts":[{"name":"Act 1","description":"Setup the misunderstanding","beats":2},{"name":"Act 2","description":"Escalating chaos","beats":4},{"name":"Act 3","description":"Heartfelt resolution","beats":2}]}',
             'RetroCartoon', NULL, NULL, 1, 2,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Family Comedy';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0003-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0003-0000-0000-000000000003',
             'Mystery Thriller Short', 'Drama',
             'A detective unravels a web of secrets in a rain-soaked city, facing danger at every turn.',
             '{"acts":[{"name":"Act 1","description":"Crime discovered","beats":2},{"name":"Act 2","description":"Investigation deepens with false leads","beats":5},{"name":"Act 3","description":"Revelation and confrontation","beats":3}]}',
             'Realistic', NULL, NULL, 1, 3,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Mystery Thriller Short';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0004-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0004-0000-0000-000000000003',
             'Romance Vignette', 'Romance',
             'Two strangers meet by chance and navigate hesitant feelings through a series of bittersweet encounters.',
             '{"acts":[{"name":"Act 1","description":"Chance meeting","beats":2},{"name":"Act 2","description":"Growing connection and obstacles","beats":3},{"name":"Act 3","description":"Decisive moment","beats":2}]}',
             'WatercolorIllustration', NULL, NULL, 1, 4,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Romance Vignette';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0005-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0005-0000-0000-000000000003',
             'Horror Short', 'Horror',
             'A group of friends spend the night in an abandoned house and discover something terrifying that defies explanation.',
             '{"acts":[{"name":"Act 1","description":"Arrival and foreboding","beats":2},{"name":"Act 2","description":"Escalating dread and isolation","beats":4},{"name":"Act 3","description":"Shocking climax and ambiguous escape","beats":2}]}',
             'Cyberpunk', NULL, NULL, 1, 5,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Horror Short';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0006-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0006-0000-0000-000000000003',
             'Sci-Fi Action', 'SciFi',
             'A lone pilot discovers a derelict spacecraft that holds the key to saving humanity from an alien threat.',
             '{"acts":[{"name":"Act 1","description":"Discovery of the derelict ship","beats":2},{"name":"Act 2","description":"Uncovering the threat and preparing a response","beats":5},{"name":"Act 3","description":"Epic battle and sacrifice","beats":3}]}',
             'Cyberpunk', NULL, NULL, 1, 6,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Sci-Fi Action';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0007-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0007-0000-0000-000000000003',
             'Product Advert', 'Marketing',
             'A 60-second animated spot that dramatises the hero moment of your product solving a customer''s pain point.',
             '{"acts":[{"name":"Hook","description":"Relatable pain point","beats":1},{"name":"Solution","description":"Product introduced as hero","beats":2},{"name":"Proof","description":"Result and delight","beats":1},{"name":"CTA","description":"Brand moment and call to action","beats":1}]}',
             'Pixar3D', NULL, NULL, 1, 7,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Product Advert';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeTemplates]
                   WHERE [Id] = '11111111-0008-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[EpisodeTemplates]
            ([Id], [Title], [Genre], [Description], [PlotStructure], [DefaultStyle],
             [PreviewVideoUrl], [ThumbnailUrl], [IsActive], [SortOrder],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('11111111-0008-0000-0000-000000000003',
             'Brand Story', 'Marketing',
             'An origin story that communicates brand values, mission, and the people behind a company.',
             '{"acts":[{"name":"Origin","description":"Founding moment and challenge","beats":2},{"name":"Journey","description":"Growth and mission","beats":3},{"name":"Vision","description":"Future and invitation to join","beats":2}]}',
             'ComicBook', NULL, NULL, 1, 8,
             '2026-04-05T00:00:00.0000000+00:00',
             '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + Template: Brand Story';
    END

    PRINT '  ✓ content.EpisodeTemplates seeded';
END


-- =============================================================================
PRINT '── §6  content.StylePresets (Phase3Templates migration — guard) ──────────';
-- =============================================================================
-- RowVersion is varbinary(max) — supply 0x.
-- EF stores the Style enum value as its string name (HasConversion<string>()).

IF OBJECT_ID('content.StylePresets', 'U') IS NULL
BEGIN
    PRINT '  ⚠ content.StylePresets table not found — run Phase3Templates migration first';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0001-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0001-0000-0000-000000000003',
             'Pixar3D', 'Pixar 3D',
             'Vibrant 3D animation with Pixar-style characters, smooth subsurface scattering, and cinematic lighting.',
             NULL,
             'vibrant Pixar-style 3D animation, smooth subsurface scattering, cinematic three-point lighting, shallow depth of field, 4K render quality, expressive character design',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Pixar 3D';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0002-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0002-0000-0000-000000000003',
             'Anime', 'Anime',
             'Cel-shaded anime with dynamic action lines, expressive character emotions, and vibrant colour palettes.',
             NULL,
             'anime art style, cel-shaded illustration, dynamic speed lines, expressive emotive characters, vibrant saturated colors, clean linework, Studio Ghibli inspired',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Anime';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0003-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0003-0000-0000-000000000003',
             'WatercolorIllustration', 'Watercolor',
             'Soft painterly textures with gentle colour bleeding and a handcrafted storybook feel.',
             NULL,
             'soft watercolor illustration, painterly textures, gentle color bleeding, warm pastel tones, handcrafted storybook aesthetic, loose brushwork, visible paper texture',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Watercolor';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0004-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0004-0000-0000-000000000003',
             'ComicBook', 'Comic Book',
             'Bold outlines, halftone dot patterns, flat saturated colours, and dynamic superhero poses.',
             NULL,
             'comic book art style, bold black outlines, halftone dot shading, flat vibrant colors, dynamic action poses, CMYK print aesthetic, Marvel and DC inspired panel composition',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Comic Book';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0005-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0005-0000-0000-000000000003',
             'Realistic', 'Realistic CGI',
             'Photorealistic CGI with physically-based rendering, detailed surface textures, and dramatic cinematic lighting.',
             NULL,
             'photorealistic CGI, physically-based rendering, detailed surface textures, dramatic cinematic lighting, 8K resolution, ray-traced reflections and shadows, film grain',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Realistic CGI';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0006-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0006-0000-0000-000000000003',
             'PhotoStorybook', 'Photo Storybook',
             'Photo-real illustration combining photographic detail with painterly finishing touches.',
             NULL,
             'photo-real storybook illustration, photographic detail with painterly overlay, rich saturated colors, storybook composition, children''s picture book aesthetic',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Photo Storybook';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0007-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0007-0000-0000-000000000003',
             'RetroCartoon', 'Retro Cartoon',
             '1950s-1970s television cartoon aesthetic: limited palette, bold outlines, and rubberhose character movement.',
             NULL,
             'retro 1960s cartoon style, limited color palette, bold flat outlines, rubberhose character animation, Saturday morning cartoon aesthetic, flat background art',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Retro Cartoon';
    END

    IF NOT EXISTS (SELECT 1 FROM [content].[StylePresets]
                   WHERE [Id] = '22222222-0008-0000-0000-000000000003')
    BEGIN
        INSERT INTO [content].[StylePresets]
            ([Id], [Style], [DisplayName], [Description], [SampleImageUrl],
             [FluxStylePromptSuffix], [IsActive],
             [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
        VALUES
            ('22222222-0008-0000-0000-000000000003',
             'Cyberpunk', 'Cyberpunk',
             'Neon-drenched cityscapes, high-contrast shadows, glitch visual effects, and dark dystopian atmosphere.',
             NULL,
             'cyberpunk aesthetic, neon-lit rain-soaked cityscape, high contrast shadows, chromatic aberration glitch effect, dark dystopian atmosphere, Blade Runner and Ghost in the Shell inspired',
             1, '2026-04-05T00:00:00.0000000+00:00', '2026-04-05T00:00:00.0000000+00:00', 0, 0x);
        PRINT '  + StylePreset: Cyberpunk';
    END

    PRINT '  ✓ content.StylePresets seeded';
END


-- =============================================================================
PRINT '── §7  content.Characters ───────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server rowversion — omit; auto-assigned.
-- TrainingStatus stored as string via HasConversion<string>().

IF OBJECT_ID('content.Characters', 'U') IS NULL
BEGIN
    PRINT '  ⚠ content.Characters table not found — run Phase4Characters migration first';
END
ELSE
BEGIN
    -- ── Character 1: Professor Whiskerbolt ─────────────────────────────────────
    -- TrainingStatus = Ready, 100% — fully usable in Episodes.
    IF NOT EXISTS (SELECT 1 FROM [content].[Characters]
                   WHERE [Id] = 'EEEEEEEE-0001-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[Characters]
            ([Id], [TeamId], [Name], [Description], [StyleDna],
             [ImageUrl], [LoraWeightsUrl], [TriggerWord],
             [TrainingStatus], [TrainingProgressPercent], [CreditsCost],
             [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            ('EEEEEEEE-0001-0000-0000-000000000000',
             'C0000001-0000-0000-0000-000000000001',
             'Professor Whiskerbolt',
             'A wise and slightly eccentric elderly professor with a magnificent silver moustache. Wears a tweed jacket with elbow patches and round spectacles.',
             'elderly professor, tweed jacket, elbow patches, round spectacles, silver moustache, expressive eyes, warm smile, slightly dishevelled hair',
             'https://cdn.animstudio.local/characters/whiskerbolt/reference.png',
             'https://blob.animstudio.local/lora/whiskerbolt.safetensors',
             'PROF_WHISKERBOLT',
             'Ready', 100, 50,
             '2026-04-05T10:00:00.0000000+00:00',
             '2026-04-07T14:30:00.0000000+00:00', 0);
        PRINT '  + Character: Professor Whiskerbolt (Ready, 100%) inserted';
    END
    ELSE PRINT '  ✓ Character: Professor Whiskerbolt already exists';

    -- ── Character 2: Zara the Stargazer ───────────────────────────────────────
    -- TrainingStatus = Training, 60% — GPU job in progress; not yet usable.
    IF NOT EXISTS (SELECT 1 FROM [content].[Characters]
                   WHERE [Id] = 'EEEEEEEE-0002-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[Characters]
            ([Id], [TeamId], [Name], [Description], [StyleDna],
             [ImageUrl], [LoraWeightsUrl], [TriggerWord],
             [TrainingStatus], [TrainingProgressPercent], [CreditsCost],
             [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            ('EEEEEEEE-0002-0000-0000-000000000000',
             'C0000001-0000-0000-0000-000000000001',
             'Zara the Stargazer',
             'A teenage astronomer with a passion for the cosmos. Has silver-streaked hair, freckles, and always carries a battered telescope.',
             'teenage girl astronomer, silver-streaked hair, freckles, bright curious eyes, oversized hoodie, battered telescope prop, cosmic accessories',
             'https://cdn.animstudio.local/characters/zara/reference.png',
             NULL,
             'ZARA_STARGAZER',
             'Training', 60, 50,
             '2026-04-10T09:00:00.0000000+00:00',
             '2026-04-12T08:45:00.0000000+00:00', 0);
        PRINT '  + Character: Zara the Stargazer (Training, 60%) inserted';
    END
    ELSE PRINT '  ✓ Character: Zara the Stargazer already exists';

    -- ── Character 3: Commander Vex ─────────────────────────────────────────────
    -- TrainingStatus = Draft — just created; training not yet started.
    IF NOT EXISTS (SELECT 1 FROM [content].[Characters]
                   WHERE [Id] = 'EEEEEEEE-0003-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[Characters]
            ([Id], [TeamId], [Name], [Description], [StyleDna],
             [ImageUrl], [LoraWeightsUrl], [TriggerWord],
             [TrainingStatus], [TrainingProgressPercent], [CreditsCost],
             [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            ('EEEEEEEE-0003-0000-0000-000000000000',
             'C0000001-0000-0000-0000-000000000001',
             'Commander Vex',
             'An imposing galactic commander with chrome battle armour and piercing blue eyes. Commands authority but hides a troubled past.',
             'galactic commander, chrome battle armour, piercing blue eyes, stern expression, imposing stature, futuristic military uniform',
             NULL, NULL, NULL,
             'Draft', 0, 50,
             '2026-04-12T10:00:00.0000000+00:00',
             '2026-04-12T10:00:00.0000000+00:00', 0);
        PRINT '  + Character: Commander Vex (Draft, 0%) inserted';
    END
    ELSE PRINT '  ✓ Character: Commander Vex already exists';
END


-- =============================================================================
PRINT '── §8  content.Projects ─────────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server timestamp — omit; auto-assigned.

IF NOT EXISTS (SELECT 1 FROM [content].[Projects]
               WHERE [Id] = 'CCCCCCCC-0001-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Projects]
        ([Id], [TeamId], [Name], [Description], [ThumbnailUrl],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('CCCCCCCC-0001-0000-0000-000000000000',
         'C0000001-0000-0000-0000-000000000001',
         'The Whispering Woods',
         'A charming anthology of short animated episodes set in an enchanted forest where animals solve mysteries and learn life lessons.',
         'https://cdn.animstudio.local/projects/whispering-woods/thumb.png',
         '2026-04-05T10:00:00.0000000+00:00',
         '2026-04-12T00:00:00.0000000+00:00', 0);
    PRINT '  + Project: The Whispering Woods inserted';
END
ELSE PRINT '  ✓ Project: The Whispering Woods already exists';

IF NOT EXISTS (SELECT 1 FROM [content].[Projects]
               WHERE [Id] = 'CCCCCCCC-0002-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Projects]
        ([Id], [TeamId], [Name], [Description], [ThumbnailUrl],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('CCCCCCCC-0002-0000-0000-000000000000',
         'C0000001-0000-0000-0000-000000000001',
         'Neon City Chronicles',
         'A cyberpunk sci-fi series following a rogue AI detective and her human partner solving crimes across a rain-drenched metropolis.',
         'https://cdn.animstudio.local/projects/neon-city/thumb.png',
         '2026-04-08T14:00:00.0000000+00:00',
         '2026-04-12T00:00:00.0000000+00:00', 0);
    PRINT '  + Project: Neon City Chronicles inserted';
END
ELSE PRINT '  ✓ Project: Neon City Chronicles already exists';


-- =============================================================================
PRINT '── §9  content.Episodes ─────────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server timestamp — omit; auto-assigned.
-- Status stored as string via HasConversion<string>() (e.g. 'Script', not '3').
-- CharacterIds is a JSON array of GUID strings stored as nvarchar(max).

-- ── Episode 1: The Forest Guardian ────────────────────────────────────────────
-- Project 1 | Status = Script | Template = Kids Superhero Adventure
-- Pipeline is partway through — script has been generated, storyboard next.
IF NOT EXISTS (SELECT 1 FROM [content].[Episodes]
               WHERE [Id] = 'DDDDDDDD-0001-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Episodes]
        ([Id], [ProjectId], [Name], [Idea], [Style], [Status],
         [TemplateId], [CharacterIds], [DirectorNotes], [RenderedAt],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('DDDDDDDD-0001-0000-0000-000000000000',
         'CCCCCCCC-0001-0000-0000-000000000000',
         'The Forest Guardian',
         'A young woodland fox named Pip discovers he can talk to ancient trees and must convince the forest council to stop a logging threat.',
         'Pixar3D',
         'Script',
         '11111111-0001-0000-0000-000000000003',
         '["EEEEEEEE-0001-0000-0000-000000000000"]',
         'Keep the pacing snappy — target audience is 6-10. The council scene should feel epic but not scary.',
         NULL,
         '2026-04-06T09:00:00.0000000+00:00',
         '2026-04-10T16:00:00.0000000+00:00', 0);
    PRINT '  + Episode 1: The Forest Guardian (Script) inserted';
END
ELSE PRINT '  ✓ Episode 1: The Forest Guardian already exists';

-- ── Episode 2: The Great Picnic Disaster ──────────────────────────────────────
-- Project 1 | Status = Done | RenderedAt set — fully delivered episode.
IF NOT EXISTS (SELECT 1 FROM [content].[Episodes]
               WHERE [Id] = 'DDDDDDDD-0002-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Episodes]
        ([Id], [ProjectId], [Name], [Idea], [Style], [Status],
         [TemplateId], [CharacterIds], [DirectorNotes], [RenderedAt],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('DDDDDDDD-0002-0000-0000-000000000000',
         'CCCCCCCC-0001-0000-0000-000000000000',
         'The Great Picnic Disaster',
         'Professor Whiskerbolt organises the annual woodland picnic, but everything hilariously goes wrong when the ants declare war on the dessert table.',
         'RetroCartoon',
         'Done',
         '11111111-0002-0000-0000-000000000003',
         '["EEEEEEEE-0001-0000-0000-000000000000","EEEEEEEE-0002-0000-0000-000000000000"]',
         'The ant battle sequence should be pure slapstick — think Looney Tunes energy.',
         '2026-04-11T22:00:00.0000000+00:00',
         '2026-04-06T10:00:00.0000000+00:00',
         '2026-04-11T22:00:00.0000000+00:00', 0);
    PRINT '  + Episode 2: The Great Picnic Disaster (Done) inserted';
END
ELSE PRINT '  ✓ Episode 2: The Great Picnic Disaster already exists';

-- ── Episode 3: Dark Neon Rising ───────────────────────────────────────────────
-- Project 2 | Status = Idle — freshly created, pipeline not started.
IF NOT EXISTS (SELECT 1 FROM [content].[Episodes]
               WHERE [Id] = 'DDDDDDDD-0003-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Episodes]
        ([Id], [ProjectId], [Name], [Idea], [Style], [Status],
         [TemplateId], [CharacterIds], [DirectorNotes], [RenderedAt],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('DDDDDDDD-0003-0000-0000-000000000000',
         'CCCCCCCC-0002-0000-0000-000000000000',
         'Dark Neon Rising',
         'A mysterious blackout plunges Neon City into chaos. Detective Unit 7 and her partner race to find who shut off the city''s AI core.',
         'Cyberpunk',
         'Idle',
         '11111111-0006-0000-0000-000000000003',
         '[]',
         NULL,
         NULL,
         '2026-04-11T14:00:00.0000000+00:00',
         '2026-04-11T14:00:00.0000000+00:00', 0);
    PRINT '  + Episode 3: Dark Neon Rising (Idle) inserted';
END
ELSE PRINT '  ✓ Episode 3: Dark Neon Rising already exists';

-- ── Episode 4: Electric Storm ─────────────────────────────────────────────────
-- Project 2 | Status = Animation — job currently Running (see §11 Job 3).
IF NOT EXISTS (SELECT 1 FROM [content].[Episodes]
               WHERE [Id] = 'DDDDDDDD-0004-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Episodes]
        ([Id], [ProjectId], [Name], [Idea], [Style], [Status],
         [TemplateId], [CharacterIds], [DirectorNotes], [RenderedAt],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('DDDDDDDD-0004-0000-0000-000000000000',
         'CCCCCCCC-0002-0000-0000-000000000000',
         'Electric Storm',
         'Commander Vex commandeers a stolen energy weapon and threatens to collapse the city''s power grid unless the AI council surrenders.',
         'Cyberpunk',
         'Animation',
         '11111111-0006-0000-0000-000000000003',
         '["EEEEEEEE-0001-0000-0000-000000000000"]',
         'The final showdown needs slow-motion for the key turning point. Heavy rain VFX throughout.',
         NULL,
         '2026-04-09T08:00:00.0000000+00:00',
         '2026-04-12T06:00:00.0000000+00:00', 0);
    PRINT '  + Episode 4: Electric Storm (Animation) inserted';
END
ELSE PRINT '  ✓ Episode 4: Electric Storm already exists';


-- =============================================================================
PRINT '── §10 content.EpisodeCharacters ────────────────────────────────────────';
-- =============================================================================

IF OBJECT_ID('content.EpisodeCharacters', 'U') IS NULL
BEGIN
    PRINT '  ⚠ content.EpisodeCharacters table not found — run Phase4Characters migration first';
END
ELSE
BEGIN
    -- Episode 1 ← Professor Whiskerbolt
    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters]
                   WHERE [EpisodeId]   = 'DDDDDDDD-0001-0000-0000-000000000000'
                     AND [CharacterId] = 'EEEEEEEE-0001-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
        VALUES ('DDDDDDDD-0001-0000-0000-000000000000',
                'EEEEEEEE-0001-0000-0000-000000000000',
                '2026-04-06T11:00:00.0000000+00:00');
        PRINT '  + Episode 1 ← Prof. Whiskerbolt';
    END

    -- Episode 2 ← Professor Whiskerbolt
    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters]
                   WHERE [EpisodeId]   = 'DDDDDDDD-0002-0000-0000-000000000000'
                     AND [CharacterId] = 'EEEEEEEE-0001-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
        VALUES ('DDDDDDDD-0002-0000-0000-000000000000',
                'EEEEEEEE-0001-0000-0000-000000000000',
                '2026-04-06T12:00:00.0000000+00:00');
        PRINT '  + Episode 2 ← Prof. Whiskerbolt';
    END

    -- Episode 2 ← Zara the Stargazer
    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters]
                   WHERE [EpisodeId]   = 'DDDDDDDD-0002-0000-0000-000000000000'
                     AND [CharacterId] = 'EEEEEEEE-0002-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
        VALUES ('DDDDDDDD-0002-0000-0000-000000000000',
                'EEEEEEEE-0002-0000-0000-000000000000',
                '2026-04-06T12:05:00.0000000+00:00');
        PRINT '  + Episode 2 ← Zara the Stargazer';
    END

    -- Episode 4 ← Professor Whiskerbolt
    IF NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters]
                   WHERE [EpisodeId]   = 'DDDDDDDD-0004-0000-0000-000000000000'
                     AND [CharacterId] = 'EEEEEEEE-0001-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
        VALUES ('DDDDDDDD-0004-0000-0000-000000000000',
                'EEEEEEEE-0001-0000-0000-000000000000',
                '2026-04-09T09:00:00.0000000+00:00');
        PRINT '  + Episode 4 ← Prof. Whiskerbolt';
    END

    PRINT '  ✓ content.EpisodeCharacters seeded';
END


-- =============================================================================
PRINT '── §11 content.Jobs ─────────────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is varbinary(max) — supply 0x.
-- Type and Status stored as strings via HasConversion<string>().

-- ── Job 1: Script generation for Episode 1 — Completed ────────────────────────
IF NOT EXISTS (SELECT 1 FROM [content].[Jobs]
               WHERE [Id] = 'FFFFFFFF-0001-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Jobs]
        ([Id], [EpisodeId], [Type], [Status],
         [Payload], [Result], [ErrorMessage],
         [QueuedAt], [StartedAt], [CompletedAt], [AttemptNumber],
         [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
    VALUES
        ('FFFFFFFF-0001-0000-0000-000000000000',
         'DDDDDDDD-0001-0000-0000-000000000000',
         'Script', 'Completed',
         '{"episodeId":"DDDDDDDD-0001-0000-0000-000000000000","idea":"A young woodland fox named Pip discovers he can talk to ancient trees","style":"Pixar3D","templateId":"11111111-0001-0000-0000-000000000003"}',
         '{"status":"ok","scriptId":"AABB0001-0000-0000-0000-000000000000"}',
         NULL,
         '2026-04-10T14:00:00.0000000+00:00',
         '2026-04-10T14:01:00.0000000+00:00',
         '2026-04-10T14:08:00.0000000+00:00',
         1,
         '2026-04-10T14:00:00.0000000+00:00',
         '2026-04-10T14:08:00.0000000+00:00', 0, 0x);
    PRINT '  + Job 1: Script / Completed (Episode 1)';
END
ELSE PRINT '  ✓ Job 1 already exists';

-- ── Job 2: Animation for Episode 2 — Completed ────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [content].[Jobs]
               WHERE [Id] = 'FFFFFFFF-0002-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Jobs]
        ([Id], [EpisodeId], [Type], [Status],
         [Payload], [Result], [ErrorMessage],
         [QueuedAt], [StartedAt], [CompletedAt], [AttemptNumber],
         [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
    VALUES
        ('FFFFFFFF-0002-0000-0000-000000000000',
         'DDDDDDDD-0002-0000-0000-000000000000',
         'Animation', 'Completed',
         '{"episodeId":"DDDDDDDD-0002-0000-0000-000000000000","style":"RetroCartoon","quality":"high"}',
         '{"status":"ok","deliveryUrl":"https://cdn.animstudio.local/renders/ep2/final.mp4","durationSeconds":185}',
         NULL,
         '2026-04-11T18:00:00.0000000+00:00',
         '2026-04-11T18:05:00.0000000+00:00',
         '2026-04-11T21:45:00.0000000+00:00',
         1,
         '2026-04-11T18:00:00.0000000+00:00',
         '2026-04-11T21:45:00.0000000+00:00', 0, 0x);
    PRINT '  + Job 2: Animation / Completed (Episode 2)';
END
ELSE PRINT '  ✓ Job 2 already exists';

-- ── Job 3: Animation for Episode 4 — Running (in progress) ────────────────────
IF NOT EXISTS (SELECT 1 FROM [content].[Jobs]
               WHERE [Id] = 'FFFFFFFF-0003-0000-0000-000000000000')
BEGIN
    INSERT INTO [content].[Jobs]
        ([Id], [EpisodeId], [Type], [Status],
         [Payload], [Result], [ErrorMessage],
         [QueuedAt], [StartedAt], [CompletedAt], [AttemptNumber],
         [CreatedAt], [UpdatedAt], [IsDeleted], [RowVersion])
    VALUES
        ('FFFFFFFF-0003-0000-0000-000000000000',
         'DDDDDDDD-0004-0000-0000-000000000000',
         'Animation', 'Running',
         '{"episodeId":"DDDDDDDD-0004-0000-0000-000000000000","style":"Cyberpunk","quality":"high"}',
         NULL, NULL,
         '2026-04-12T05:00:00.0000000+00:00',
         '2026-04-12T05:10:00.0000000+00:00',
         NULL,
         1,
         '2026-04-12T05:00:00.0000000+00:00',
         '2026-04-12T05:10:00.0000000+00:00', 0, 0x);
    PRINT '  + Job 3: Animation / Running (Episode 4)';
END
ELSE PRINT '  ✓ Job 3 already exists';


-- =============================================================================
PRINT '── §12 content.Scripts ──────────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server rowversion — omit; auto-assigned.
-- RawJson schema: { title, scenes[{ scene_number, visual_description,
--   emotional_tone, dialogue[{ character, text, start_time, end_time }] }] }

IF OBJECT_ID('content.Scripts', 'U') IS NULL
BEGIN
    PRINT '  ⚠ content.Scripts table not found — run Phase5Script migration first';
END
ELSE
BEGIN
    -- ── Script 1: Episode 1 — AI-generated, not yet manually edited ────────────
    IF NOT EXISTS (SELECT 1 FROM [content].[Scripts]
                   WHERE [Id] = 'AABB0001-0000-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[Scripts]
            ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
             [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            ('AABB0001-0000-0000-0000-000000000000',
             'DDDDDDDD-0001-0000-0000-000000000000',
             'The Forest Guardian — Episode Script',
             '{"title":"The Forest Guardian","scenes":[{"scene_number":1,"visual_description":"Wide shot of a sunlit forest clearing. PIP, a small red fox with oversized ears, presses his paw against the bark of an ancient oak. Soft golden light filters through the canopy.","emotional_tone":"wonder, discovery","dialogue":[{"character":"Pip","text":"Hello? Is... is someone there?","start_time":0.0,"end_time":2.5},{"character":"Ancient Oak","text":"Finally. We have been waiting a long time for you, young one.","start_time":3.0,"end_time":6.5}]},{"scene_number":2,"visual_description":"A logging truck rumbles along a dirt road at the forest edge. Chainsaws echo in the distance. Pip watches from behind a bush, eyes wide with alarm.","emotional_tone":"tension, urgency","dialogue":[{"character":"Pip","text":"They are going to cut down the whole western grove!","start_time":0.0,"end_time":3.0},{"character":"Ancient Oak","text":"Then you must call the Forest Council. Only together can you stop them.","start_time":3.5,"end_time":7.0}]},{"scene_number":3,"visual_description":"A grand mossy amphitheatre carved from living trees. Dozens of woodland animals sit in tiered rows of roots and branches. Pip stands nervously at the centre podium, lit by shafts of golden light.","emotional_tone":"determination, hope","dialogue":[{"character":"Pip","text":"I know I am just a fox. But those trees have stood for a thousand years. We cannot let them fall without a fight.","start_time":0.0,"end_time":6.0},{"character":"Owl Elder","text":"The young fox speaks truth. All those in favour of the defence pact, raise a wing.","start_time":6.5,"end_time":10.0}]}]}',
             0,
             NULL,
             '2026-04-10T14:08:00.0000000+00:00',
             '2026-04-10T14:08:00.0000000+00:00', 0);
        PRINT '  + Script 1: The Forest Guardian (AI-generated) inserted';
    END
    ELSE PRINT '  ✓ Script 1 already exists';

    -- ── Script 2: Episode 2 — Done — manually edited by the dev user ───────────
    IF NOT EXISTS (SELECT 1 FROM [content].[Scripts]
                   WHERE [Id] = 'AABB0002-0000-0000-0000-000000000000')
    BEGIN
        INSERT INTO [content].[Scripts]
            ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
             [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            ('AABB0002-0000-0000-0000-000000000000',
             'DDDDDDDD-0002-0000-0000-000000000000',
             'The Great Picnic Disaster — Final Script',
             '{"title":"The Great Picnic Disaster","scenes":[{"scene_number":1,"visual_description":"A sunny forest glade decorated with bunting and checked tablecloths. PROFESSOR WHISKERBOLT adjusts a towering three-tiered cake, humming contentedly. ZARA arranges telescope-printed paper plates nearby.","emotional_tone":"cheerful, anticipatory","dialogue":[{"character":"Professor Whiskerbolt","text":"Ah, everything is perfectly perfect! Nothing could possibly go wrong this year.","start_time":0.0,"end_time":4.0},{"character":"Zara","text":"Professor, you say that every single year.","start_time":4.5,"end_time":6.5}]},{"scene_number":2,"visual_description":"Close-up on a crack in the earth. A thousand ants emerge in military formation, marching toward the dessert table. Their tiny general raises a bread crumb like a sword and points at the cake.","emotional_tone":"comedy, rising tension","dialogue":[{"character":"Ant General","text":"TROOPS — THE CAKE IS OURS. ADVANCE!","start_time":0.0,"end_time":2.5},{"character":"Professor Whiskerbolt","text":"Great galaxies! They have a GENERAL!","start_time":3.0,"end_time":5.0}]},{"scene_number":3,"visual_description":"Slow-motion: a cream pie sails through the air in a perfect parabolic arc. The Professor ducks. The pie hits the Ant General square in the face. A beat of silence. Then absolute chaos — animals, ants, and cake collide in spectacular slapstick across the entire clearing.","emotional_tone":"peak comedy chaos","dialogue":[{"character":"Zara","text":"I am going to need a significantly bigger telescope to document all of this.","start_time":0.0,"end_time":3.5}]}]}',
             1,
             'Ant general scene: added extra beat before cream pie lands for comedic timing. Slow-mo cue at 00:02:45.',
             '2026-04-08T10:00:00.0000000+00:00',
             '2026-04-09T15:30:00.0000000+00:00', 0);
        PRINT '  + Script 2: The Great Picnic Disaster (manually edited) inserted';
    END
    ELSE PRINT '  ✓ Script 2 already exists';

    PRINT '  ✓ content.Scripts seeded';
END


-- =============================================================================
PRINT '── §13 shared.SagaStates ────────────────────────────────────────────────';
-- =============================================================================
-- Tracks the production saga for Episode 4 currently in the Animation stage.
-- CurrentStage mirrors the EpisodeStatus string value at the active pipeline step.

IF NOT EXISTS (SELECT 1 FROM [shared].[SagaStates]
               WHERE [EpisodeId] = 'DDDDDDDD-0004-0000-0000-000000000000')
BEGIN
    INSERT INTO [shared].[SagaStates]
        ([Id], [EpisodeId], [CurrentStage], [IsCompensating], [LastError],
         [RetryCount], [StartedAt], [UpdatedAt])
    VALUES
        ('5A6A0001-0000-0000-0000-000000000000',
         'DDDDDDDD-0004-0000-0000-000000000000',
         'Animation',
         0, NULL, 0,
         '2026-04-09T08:00:00.0000000+00:00',
         '2026-04-12T05:10:00.0000000+00:00');
    PRINT '  + SagaState: Episode 4 / Animation inserted';
END
ELSE PRINT '  ✓ SagaState for Episode 4 already exists';


-- =============================================================================
PRINT '── §14 shared.OutboxMessages ────────────────────────────────────────────';
-- =============================================================================
-- Mix of Delivered (processed) and one Pending message to show backlog.
-- EventType uses fully-qualified domain event class names.

IF NOT EXISTS (SELECT 1 FROM [shared].[OutboxMessages]
               WHERE [Id] = '0B000001-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO [shared].[OutboxMessages]
        ([Id], [EventType], [Payload], [Status], [OccurredAt], [ProcessedAt], [RetryCount])
    VALUES
        ('0B000001-0000-0000-0000-000000000000',
         'AnimStudio.IdentityModule.Domain.Events.TeamCreated',
         '{"TeamId":"C0000001-0000-0000-0000-000000000001","OwnerId":"4327FA31-FF93-4B68-AF4D-1FC56AFA33C5","TeamName":"Dev Studio"}',
         'Delivered',
         '2026-04-04T04:37:57.0000000+00:00',
         '2026-04-04T04:38:00.0000000+00:00',
         0);
    PRINT '  + OutboxMessage: TeamCreated';
END
ELSE PRINT '  ✓ OutboxMessage: TeamCreated already exists';

IF NOT EXISTS (SELECT 1 FROM [shared].[OutboxMessages]
               WHERE [Id] = '0B000002-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO [shared].[OutboxMessages]
        ([Id], [EventType], [Payload], [Status], [OccurredAt], [ProcessedAt], [RetryCount])
    VALUES
        ('0B000002-0000-0000-0000-000000000000',
         'AnimStudio.IdentityModule.Domain.Events.SubscriptionActivated',
         '{"SubscriptionId":"BBBBBBBB-0001-0000-0000-000000000000","TeamId":"C0000001-0000-0000-0000-000000000001","PlanId":"22222222-2222-2222-2222-222222222222"}',
         'Delivered',
         '2026-04-04T04:38:01.0000000+00:00',
         '2026-04-04T04:38:05.0000000+00:00',
         0);
    PRINT '  + OutboxMessage: SubscriptionActivated';
END
ELSE PRINT '  ✓ OutboxMessage: SubscriptionActivated already exists';

IF NOT EXISTS (SELECT 1 FROM [shared].[OutboxMessages]
               WHERE [Id] = '0B000003-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO [shared].[OutboxMessages]
        ([Id], [EventType], [Payload], [Status], [OccurredAt], [ProcessedAt], [RetryCount])
    VALUES
        ('0B000003-0000-0000-0000-000000000000',
         'AnimStudio.ContentModule.Domain.Events.EpisodeCompleted',
         '{"EpisodeId":"DDDDDDDD-0002-0000-0000-000000000000","ProjectId":"CCCCCCCC-0001-0000-0000-000000000000"}',
         'Delivered',
         '2026-04-11T22:00:30.0000000+00:00',
         '2026-04-11T22:00:45.0000000+00:00',
         0);
    PRINT '  + OutboxMessage: EpisodeCompleted (The Great Picnic Disaster)';
END
ELSE PRINT '  ✓ OutboxMessage: EpisodeCompleted already exists';

IF NOT EXISTS (SELECT 1 FROM [shared].[OutboxMessages]
               WHERE [Id] = '0B000004-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO [shared].[OutboxMessages]
        ([Id], [EventType], [Payload], [Status], [OccurredAt], [ProcessedAt], [RetryCount])
    VALUES
        ('0B000004-0000-0000-0000-000000000000',
         'AnimStudio.ContentModule.Domain.Events.CharacterCreated',
         '{"CharacterId":"EEEEEEEE-0003-0000-0000-000000000000","TeamId":"C0000001-0000-0000-0000-000000000001","Name":"Commander Vex"}',
         'Delivered',
         '2026-04-12T10:00:05.0000000+00:00',
         '2026-04-12T10:00:12.0000000+00:00',
         0);
    PRINT '  + OutboxMessage: CharacterCreated (Commander Vex)';
END
ELSE PRINT '  ✓ OutboxMessage: CharacterCreated already exists';

-- Pending — not yet dispatched to Service Bus (simulates outbox backpressure)
IF NOT EXISTS (SELECT 1 FROM [shared].[OutboxMessages]
               WHERE [Id] = '0B000005-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO [shared].[OutboxMessages]
        ([Id], [EventType], [Payload], [Status], [OccurredAt], [ProcessedAt], [RetryCount])
    VALUES
        ('0B000005-0000-0000-0000-000000000000',
         'AnimStudio.ContentModule.Domain.Events.EpisodeStageAdvanced',
         '{"EpisodeId":"DDDDDDDD-0004-0000-0000-000000000000","NewStage":"Animation"}',
         'Pending',
         '2026-04-12T05:10:00.0000000+00:00',
         NULL,
         0);
    PRINT '  + OutboxMessage: EpisodeStageAdvanced (Electric Storm) [Pending]';
END
ELSE PRINT '  ✓ OutboxMessage: EpisodeStageAdvanced already exists';


-- =============================================================================
    COMMIT TRANSACTION;
    PRINT '';
    PRINT '════════════════════════════════════════════════════════════════════════';
    PRINT '  ✅  Seed completed successfully.';
    PRINT '';
    PRINT '  identity.Plans            3 rows (Starter / Pro / Studio)';
    PRINT '  identity.Teams            1 row  (Dev Studio)';
    PRINT '  identity.TeamMembers      1 row  (Dev User → Owner)';
    PRINT '  identity.Subscriptions    1 row  (Pro / Active)';
    PRINT '  content.EpisodeTemplates  8 rows (all genres)';
    PRINT '  content.StylePresets      8 rows (all styles)';
    PRINT '  content.Characters        3 rows (Ready / Training / Draft)';
    PRINT '  content.Projects          2 rows';
    PRINT '  content.Episodes          4 rows (Idle / Script / Animation / Done)';
    PRINT '  content.EpisodeCharacters 4 rows';
    PRINT '  content.Jobs              3 rows (Completed / Completed / Running)';
    PRINT '  content.Scripts           2 rows (AI-generated + manually edited)';
    PRINT '  shared.SagaStates         1 row  (Episode 4 / Animation)';
    PRINT '  shared.OutboxMessages     5 rows (4 Delivered + 1 Pending)';
    PRINT '════════════════════════════════════════════════════════════════════════';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '════════════════════════════════════════════════════════════════════════';
    PRINT '  ❌  Seed FAILED — transaction rolled back.';
    PRINT '  Error ' + CAST(ERROR_NUMBER() AS NVARCHAR) + ': ' + ERROR_MESSAGE();
    PRINT '  Line: ' + CAST(ERROR_LINE() AS NVARCHAR);
    PRINT '════════════════════════════════════════════════════════════════════════';
    THROW;
END CATCH;
