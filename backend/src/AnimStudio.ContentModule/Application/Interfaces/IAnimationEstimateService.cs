using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Computes an itemised cost estimate for animating an episode against the
/// chosen backend. Shot count comes from the episode's storyboard; unit rates
/// come from configuration (defaults: Kling=$0.056, Local=$0).
/// </summary>
public interface IAnimationEstimateService
{
    Task<AnimationEstimateDto?> EstimateAsync(
        Guid episodeId,
        AnimationBackend backend,
        CancellationToken ct = default);

    decimal GetUnitCostUsd(AnimationBackend backend);
}
