using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.UpdateEpisode;

/// <summary>Updates the idea, style and character-preferences of an existing episode.</summary>
public sealed record UpdateEpisodeCommand(
    Guid EpisodeId,
    string Idea,
    string? Style = null,
    string? CharacterPreferences = null)
    : IRequest<Result<EpisodeDto>>;

public sealed class UpdateEpisodeHandler(IEpisodeRepository episodes)
    : IRequestHandler<UpdateEpisodeCommand, Result<EpisodeDto>>
{
    public async Task<Result<EpisodeDto>> Handle(UpdateEpisodeCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<EpisodeDto>.Failure("Episode not found.", "NOT_FOUND");

        episode.UpdateDetails(cmd.Idea, cmd.Style, cmd.CharacterPreferences);
        await episodes.UpdateAsync(episode, ct);

        return Result<EpisodeDto>.Success(episode.ToDto());
    }
}
