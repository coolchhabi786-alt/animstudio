// ─────────────────────────────────────────────────────────────────────────────
// Phase 6 — Storyboard Studio — Entity Specification
// ─────────────────────────────────────────────────────────────────────────────
// Module:  AnimStudio.ContentModule
// Schema:  content.*
// Tables:  content.Storyboards, content.StoryboardShots
//
// Design decisions:
//   • Storyboard is a per-episode aggregate root. Unique index on EpisodeId
//     mirrors the Script pattern (one storyboard per episode).
//   • StoryboardShot is a child entity owned by Storyboard. It extends
//     Entity<Guid> (not AggregateRoot) because it does not raise its own
//     domain events — changes are coordinated by the Storyboard aggregate.
//   • RegenerationCount is tracked per-shot so the UI can warn at > 3.
//   • StyleOverride allows the director to change visual style on individual
//     shots without regenerating the whole storyboard.
//   • RawJson on Storyboard stores the Python StoryboardPlan JSON for
//     debugging and replay (mirrors Script.RawJson).
// ─────────────────────────────────────────────────────────────────────────────

using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Storyboard aggregate — one per episode. Owns a collection of StoryboardShot
/// children (up to 5 per scene). Created when the StoryboardPlan job completes
/// from the Python StoryboardCrew.
/// </summary>
public sealed class Storyboard : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public string ScreenplayTitle { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialised StoryboardPlan model produced by the Python engine.
    /// Schema: { screenplayTitle, scenePlans: [{ sceneNumber, shotPrompts: [...] }] }
    /// </summary>
    public string RawJson { get; private set; } = "{}";

    /// <summary>Optional director notes used when re-queuing regeneration jobs.</summary>
    public string? DirectorNotes { get; private set; }

    // Navigation property — populated via EF Core Include()
    private readonly List<StoryboardShot> _shots = new();
    public IReadOnlyCollection<StoryboardShot> Shots => _shots.AsReadOnly();

    private Storyboard() { }

    public static Storyboard Create(Guid episodeId, string screenplayTitle, string rawJson) => default!;
    public void UpdateFromJob(string rawJson, string screenplayTitle) { }
    public void SetDirectorNotes(string? notes) { }
    public void ReplaceShots(IEnumerable<StoryboardShot> shots) { }
}

/// <summary>
/// Individual panel in the storyboard grid. Up to 5 shots per scene.
/// Tracks regeneration count and per-shot style overrides.
/// </summary>
public sealed class StoryboardShot : Entity<Guid>
{
    public Guid StoryboardId { get; private set; }
    public int SceneNumber { get; private set; }
    public int ShotIndex { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? StyleOverride { get; private set; }
    public int RegenerationCount { get; private set; }

    private StoryboardShot() { }

    public static StoryboardShot Create(
        Guid storyboardId, int sceneNumber, int shotIndex,
        string description, string? styleOverride = null) => default!;

    public void UpdateImage(string imageUrl) { }
    public void IncrementRegeneration() { }
    public void SetStyleOverride(string? style) { }
}

// ─────────────────────────────────────────────────────────────────────────────
// Domain events (Domain/Events/StoryboardDomainEvents.cs)
// ─────────────────────────────────────────────────────────────────────────────

namespace AnimStudio.ContentModule.Domain.Events
{
    public sealed record StoryboardCreatedEvent(Guid StoryboardId, Guid EpisodeId) : IDomainEvent;
    public sealed record StoryboardUpdatedEvent(Guid StoryboardId, Guid EpisodeId) : IDomainEvent;
    public sealed record StoryboardShotRegeneratedEvent(
        Guid StoryboardId, Guid ShotId, int RegenerationCount) : IDomainEvent;
    public sealed record StoryboardShotStyleOverriddenEvent(
        Guid StoryboardId, Guid ShotId, string? StyleOverride) : IDomainEvent;
}

// ─────────────────────────────────────────────────────────────────────────────
// EF Core table definitions (for reference; actual config in ContentDbContext)
// ─────────────────────────────────────────────────────────────────────────────
//
// content.Storyboards
//   Id                 UNIQUEIDENTIFIER  NOT NULL PRIMARY KEY
//   EpisodeId          UNIQUEIDENTIFIER  NOT NULL  UNIQUE
//   ScreenplayTitle    NVARCHAR(500)     NOT NULL
//   RawJson            NVARCHAR(MAX)     NOT NULL
//   DirectorNotes      NVARCHAR(5000)    NULL
//   CreatedAt          DATETIMEOFFSET    NOT NULL
//   UpdatedAt          DATETIMEOFFSET    NOT NULL
//   IsDeleted          BIT               NOT NULL
//   DeletedAt          DATETIMEOFFSET    NULL
//   DeletedByUserId    UNIQUEIDENTIFIER  NULL
//   RowVersion         ROWVERSION        NOT NULL
//
// content.StoryboardShots
//   Id                  UNIQUEIDENTIFIER  NOT NULL PRIMARY KEY
//   StoryboardId        UNIQUEIDENTIFIER  NOT NULL  FK -> Storyboards(Id) CASCADE
//   SceneNumber         INT               NOT NULL
//   ShotIndex           INT               NOT NULL
//   ImageUrl            NVARCHAR(2048)    NULL
//   Description         NVARCHAR(2000)    NOT NULL
//   StyleOverride       NVARCHAR(500)     NULL
//   RegenerationCount   INT               NOT NULL  DEFAULT 0
//   CreatedAt           DATETIMEOFFSET    NOT NULL
//   UpdatedAt           DATETIMEOFFSET    NOT NULL
//   IsDeleted           BIT               NOT NULL
//   DeletedAt           DATETIMEOFFSET    NULL
//   DeletedByUserId     UNIQUEIDENTIFIER  NULL
//   RowVersion          ROWVERSION        NOT NULL
//
//   INDEX IX_StoryboardShots_StoryboardId ON (StoryboardId)
//   UNIQUE INDEX IX_StoryboardShots_Storyboard_Scene_Shot
//     ON (StoryboardId, SceneNumber, ShotIndex)
