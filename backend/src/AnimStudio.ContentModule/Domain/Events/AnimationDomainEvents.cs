using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Events;

/// <summary>Raised when an animation job is approved and enqueued.</summary>
public sealed record AnimationJobApprovedEvent(
    Guid AnimationJobId,
    Guid EpisodeId,
    AnimationBackend Backend,
    decimal EstimatedCostUsd,
    Guid? ApprovedByUserId) : IDomainEvent;

/// <summary>Raised when an animation job completes (success or failure).</summary>
public sealed record AnimationJobCompletedEvent(
    Guid AnimationJobId,
    Guid EpisodeId,
    AnimationStatus FinalStatus,
    decimal? ActualCostUsd) : IDomainEvent;

/// <summary>Raised when a rendered clip transitions to <see cref="ClipStatus.Ready"/>.</summary>
public sealed record AnimationClipReadyEvent(
    Guid AnimationClipId,
    Guid EpisodeId,
    int SceneNumber,
    int ShotIndex,
    string ClipUrl) : IDomainEvent;
