using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetAnimationClips;

/// <summary>Returns all animation clips for an episode, ordered by scene/shot.</summary>
public sealed record GetAnimationClipsQuery(Guid EpisodeId)
    : IRequest<Result<List<AnimationClipDto>>>;

public sealed class GetAnimationClipsHandler(
    IEpisodeRepository episodes,
    IAnimationClipRepository clips)
    : IRequestHandler<GetAnimationClipsQuery, Result<List<AnimationClipDto>>>
{
    public async Task<Result<List<AnimationClipDto>>> Handle(
        GetAnimationClipsQuery query, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(query.EpisodeId, ct);
        if (episode is null)
            return Result<List<AnimationClipDto>>.Failure("Episode not found.", "NOT_FOUND");

        var items = await clips.GetByEpisodeIdAsync(query.EpisodeId, ct);
        var dtos = items
            .OrderBy(c => c.SceneNumber)
            .ThenBy(c => c.ShotIndex)
            .Select(c => new AnimationClipDto(
                c.Id, c.EpisodeId, c.SceneNumber, c.ShotIndex,
                c.StoryboardShotId, c.ClipUrl, c.DurationSeconds,
                c.Status, c.CreatedAt))
            .ToList();

        return Result<List<AnimationClipDto>>.Success(dtos);
    }
}
