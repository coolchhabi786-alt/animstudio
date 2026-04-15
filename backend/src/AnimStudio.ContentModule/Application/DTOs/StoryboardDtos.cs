namespace AnimStudio.ContentModule.Application.DTOs;

// ── Storyboard DTOs ───────────────────────────────────────────────────────────

public sealed record StoryboardShotDto(
    Guid Id,
    Guid StoryboardId,
    int SceneNumber,
    int ShotIndex,
    string? ImageUrl,
    string Description,
    string? StyleOverride,
    int RegenerationCount,
    DateTimeOffset UpdatedAt);

public sealed record StoryboardDto(
    Guid Id,
    Guid EpisodeId,
    string ScreenplayTitle,
    string? DirectorNotes,
    List<StoryboardShotDto> Shots,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// ── Request bodies ────────────────────────────────────────────────────────────

public sealed record GenerateStoryboardRequest(string? DirectorNotes = null);

public sealed record RegenerateShotRequest(string? StyleOverride = null);

public sealed record UpdateShotStyleRequest(string? StyleOverride);

// ── SignalR broadcast payload (mirrors ShotUpdated event) ─────────────────────

public sealed record ShotUpdatedPayload(
    Guid ShotId,
    Guid StoryboardId,
    Guid EpisodeId,
    string? ImageUrl,
    int RegenerationCount);
