using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.ContentModule.Application.DTOs;

/// <summary>Animation job row returned by POST /episodes/{id}/animation.</summary>
public sealed record AnimationJobDto(
    Guid Id,
    Guid EpisodeId,
    AnimationBackend Backend,
    decimal EstimatedCostUsd,
    decimal? ActualCostUsd,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    AnimationStatus Status,
    DateTimeOffset CreatedAt);

/// <summary>Rendered (or in-flight) clip row returned by GET /episodes/{id}/animation.</summary>
public sealed record AnimationClipDto(
    Guid Id,
    Guid EpisodeId,
    int SceneNumber,
    int ShotIndex,
    Guid? StoryboardShotId,
    string? ClipUrl,
    double? DurationSeconds,
    ClipStatus Status,
    DateTimeOffset CreatedAt);

/// <summary>Line item in the itemised cost breakdown.</summary>
public sealed record AnimationEstimateLineItem(
    int SceneNumber,
    int ShotIndex,
    Guid? StoryboardShotId,
    decimal UnitCostUsd);

/// <summary>Response for GET /episodes/{id}/animation/estimate.</summary>
public sealed record AnimationEstimateDto(
    Guid EpisodeId,
    AnimationBackend Backend,
    int ShotCount,
    decimal UnitCostUsd,
    decimal TotalCostUsd,
    List<AnimationEstimateLineItem> Breakdown);

/// <summary>Request body for POST /episodes/{id}/animation.</summary>
public sealed record ApproveAnimationRequest(AnimationBackend Backend);

/// <summary>Response for GET /episodes/{id}/animation/clips/{clipId}.</summary>
public sealed record SignedClipUrlDto(
    Guid ClipId,
    string Url,
    DateTimeOffset ExpiresAt);
