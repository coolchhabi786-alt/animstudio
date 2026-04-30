using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetStoryboard;

/// <summary>Returns the storyboard with all shots for an episode, or null if none exists.</summary>
public sealed record GetStoryboardQuery(Guid EpisodeId) : IRequest<Result<StoryboardDto?>>;

public sealed class GetStoryboardHandler(
    IStoryboardRepository storyboards,
    IEpisodeRepository episodes,
    IFileStorageService fileStorage)
    : IRequestHandler<GetStoryboardQuery, Result<StoryboardDto?>>
{
    public async Task<Result<StoryboardDto?>> Handle(GetStoryboardQuery query, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(query.EpisodeId, ct);
        if (episode is null)
            return Result<StoryboardDto?>.Failure("Episode not found.", "NOT_FOUND");

        var storyboard = await storyboards.GetByEpisodeIdAsync(query.EpisodeId, ct);
        if (storyboard is null)
            return Result<StoryboardDto?>.Success(null);

        var shots = storyboard.Shots
            .OrderBy(s => s.SceneNumber)
            .ThenBy(s => s.ShotIndex)
            .Select(s => new StoryboardShotDto(
                s.Id,
                s.StoryboardId,
                s.SceneNumber,
                s.ShotIndex,
                s.ImageUrl is not null ? fileStorage.GetFileUrl(s.ImageUrl) : null,
                s.Description,
                s.StyleOverride,
                s.RegenerationCount,
                s.UpdatedAt))
            .ToList();

        return Result<StoryboardDto?>.Success(new StoryboardDto(
            storyboard.Id,
            storyboard.EpisodeId,
            storyboard.ScreenplayTitle,
            storyboard.DirectorNotes,
            shots,
            storyboard.CreatedAt,
            storyboard.UpdatedAt));
    }
}
