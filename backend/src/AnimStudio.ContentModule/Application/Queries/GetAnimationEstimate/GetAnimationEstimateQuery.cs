using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetAnimationEstimate;

/// <summary>
/// Returns an itemised cost estimate for animating an episode against the
/// chosen <paramref name="Backend"/>.
/// </summary>
public sealed record GetAnimationEstimateQuery(
    Guid EpisodeId,
    AnimationBackend Backend) : IRequest<Result<AnimationEstimateDto>>;

public sealed class GetAnimationEstimateHandler(
    IEpisodeRepository episodes,
    IAnimationEstimateService estimator)
    : IRequestHandler<GetAnimationEstimateQuery, Result<AnimationEstimateDto>>
{
    public async Task<Result<AnimationEstimateDto>> Handle(
        GetAnimationEstimateQuery query, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(query.EpisodeId, ct);
        if (episode is null)
            return Result<AnimationEstimateDto>.Failure("Episode not found.", "NOT_FOUND");

        var estimate = await estimator.EstimateAsync(query.EpisodeId, query.Backend, ct);
        if (estimate is null)
            return Result<AnimationEstimateDto>.Failure(
                "Storyboard not found for this episode.", "STORYBOARD_NOT_FOUND");

        return Result<AnimationEstimateDto>.Success(estimate);
    }
}
