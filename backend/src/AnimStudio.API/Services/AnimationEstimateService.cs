using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.API.Services;

/// <summary>
/// Counts shots from the episode's storyboard and prices them against the
/// chosen animation backend. Rates come from <c>Animation:Rates:*</c>
/// configuration with sensible defaults (Kling=$0.056/clip, Local=$0).
/// </summary>
public sealed class AnimationEstimateService(
    IStoryboardRepository storyboards,
    IConfiguration configuration) : IAnimationEstimateService
{
    private const decimal DefaultKlingRate = 0.056m;
    private const decimal DefaultLocalRate = 0m;

    public async Task<AnimationEstimateDto?> EstimateAsync(
        Guid episodeId,
        AnimationBackend backend,
        CancellationToken ct = default)
    {
        var storyboard = await storyboards.GetByEpisodeIdAsync(episodeId, ct);
        if (storyboard is null)
            return null;

        var unit = GetUnitCostUsd(backend);
        var shots = storyboard.Shots
            .OrderBy(s => s.SceneNumber)
            .ThenBy(s => s.ShotIndex)
            .ToList();

        var breakdown = shots
            .Select(s => new AnimationEstimateLineItem(
                s.SceneNumber, s.ShotIndex, s.Id, unit))
            .ToList();

        return new AnimationEstimateDto(
            episodeId,
            backend,
            shots.Count,
            unit,
            unit * shots.Count,
            breakdown);
    }

    public decimal GetUnitCostUsd(AnimationBackend backend) => backend switch
    {
        AnimationBackend.Kling => configuration.GetValue<decimal?>("Animation:Rates:Kling") ?? DefaultKlingRate,
        AnimationBackend.Local => configuration.GetValue<decimal?>("Animation:Rates:Local") ?? DefaultLocalRate,
        _ => 0m,
    };
}
