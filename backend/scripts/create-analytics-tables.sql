-- AnimStudio: Create analytics schema tables
-- Run this in SSMS or Azure Data Studio:
--   Server:   localhost,1433
--   Login:    sa
--   Password: AnimStudio!Dev123
--   Database: AnimStudio  (select it first: USE AnimStudio)
--
-- This script is fully idempotent — safe to run multiple times.

USE AnimStudio;
GO

-- ── 1. analytics schema ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'analytics')
BEGIN
    EXEC('CREATE SCHEMA [analytics]');
    PRINT 'Created schema: analytics';
END
ELSE
    PRINT 'Schema analytics already exists — skipping';
GO

-- ── 2. analytics.Notifications ─────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'Notifications' AND s.name = 'analytics')
BEGIN
    CREATE TABLE [analytics].[Notifications] (
        [Id]                UNIQUEIDENTIFIER NOT NULL,
        [UserId]            UNIQUEIDENTIFIER NOT NULL,
        [Type]              NVARCHAR(30)     NOT NULL,
        [Title]             NVARCHAR(200)    NOT NULL,
        [Body]              NVARCHAR(2000)   NOT NULL,
        [IsRead]            BIT              NOT NULL DEFAULT 0,
        [ReadAt]            DATETIMEOFFSET   NULL,
        [RelatedEntityId]   UNIQUEIDENTIFIER NULL,
        [RelatedEntityType] NVARCHAR(100)    NULL,
        [CreatedAt]         DATETIMEOFFSET   NOT NULL,
        [UpdatedAt]         DATETIMEOFFSET   NOT NULL,
        [IsDeleted]         BIT              NOT NULL DEFAULT 0,
        [DeletedAt]         DATETIMEOFFSET   NULL,
        [DeletedByUserId]   UNIQUEIDENTIFIER NULL,
        [RowVersion]        ROWVERSION       NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_Notifications_UserId]
        ON [analytics].[Notifications] ([UserId]);
    CREATE INDEX [IX_Notifications_UserId_IsRead]
        ON [analytics].[Notifications] ([UserId], [IsRead]);
    PRINT 'Created table: analytics.Notifications';
END
ELSE
    PRINT 'Table analytics.Notifications already exists — skipping';
GO

-- ── 3. analytics.VideoViews ────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'VideoViews' AND s.name = 'analytics')
BEGIN
    CREATE TABLE [analytics].[VideoViews] (
        [Id]             UNIQUEIDENTIFIER NOT NULL,
        [EpisodeId]      UNIQUEIDENTIFIER NOT NULL,
        [RenderId]       UNIQUEIDENTIFIER NOT NULL,
        [ViewerIpHash]   NVARCHAR(128)    NULL,
        [Source]         NVARCHAR(30)     NOT NULL,
        [ReviewLinkId]   UNIQUEIDENTIFIER NULL,
        [ViewedAt]       DATETIMEOFFSET   NOT NULL,
        CONSTRAINT [PK_VideoViews] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_VideoViews_EpisodeId]
        ON [analytics].[VideoViews] ([EpisodeId]);
    CREATE INDEX [IX_VideoViews_RenderId]
        ON [analytics].[VideoViews] ([RenderId]);
    CREATE INDEX [IX_VideoViews_ReviewLinkId]
        ON [analytics].[VideoViews] ([ReviewLinkId])
        WHERE [ReviewLinkId] IS NOT NULL;
    PRINT 'Created table: analytics.VideoViews';
END
ELSE
    PRINT 'Table analytics.VideoViews already exists — skipping';
GO

-- ── 4. Mark migration as applied (so EF does not try to re-run it) ─────────────
IF NOT EXISTS (
    SELECT 1 FROM [dbo].[__EFMigrationsHistory]
    WHERE MigrationId = '20260504043008_InitAnalyticsModule')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] (MigrationId, ProductVersion)
    VALUES ('20260504043008_InitAnalyticsModule', '8.0.0');
    PRINT 'Marked analytics migration as applied in __EFMigrationsHistory';
END
ELSE
    PRINT 'Analytics migration already recorded in __EFMigrationsHistory — skipping';
GO

-- ── Verify ─────────────────────────────────────────────────────────────────────
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'analytics'
ORDER BY TABLE_NAME;
GO
