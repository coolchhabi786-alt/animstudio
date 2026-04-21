using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Events;

/// <summary>Raised when a storyboard is first created from the StoryboardPlan job.</summary>
public sealed record StoryboardCreatedEvent(Guid StoryboardId, Guid EpisodeId) : IDomainEvent;

/// <summary>Raised when the storyboard plan JSON is re-populated by a completed job.</summary>
public sealed record StoryboardUpdatedEvent(Guid StoryboardId, Guid EpisodeId) : IDomainEvent;

/// <summary>Raised when a single shot is re-queued for regeneration.</summary>
public sealed record StoryboardShotRegeneratedEvent(
    Guid StoryboardId,
    Guid ShotId,
    Guid EpisodeId,
    int RegenerationCount) : IDomainEvent;

/// <summary>Raised when a shot has its style override changed.</summary>
public sealed record StoryboardShotStyleOverriddenEvent(
    Guid StoryboardId,
    Guid ShotId,
    Guid EpisodeId,
    string? StyleOverride) : IDomainEvent;

/// <summary>Raised when a shot's generated image URL is updated after a generation job completes.</summary>
public sealed record StoryboardShotImageUpdatedEvent(
    Guid StoryboardId,
    Guid ShotId,
    Guid EpisodeId,
    string ImageUrl,
    int RegenerationCount) : IDomainEvent;
