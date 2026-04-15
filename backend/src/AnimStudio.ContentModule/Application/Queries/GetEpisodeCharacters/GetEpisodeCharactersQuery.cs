using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetEpisodeCharacters;

/// <summary>
/// Returns all characters attached to a specific episode.
/// The caller must have access to the episode's project.
/// </summary>
/// <param name="EpisodeId">Episode whose character roster to fetch.</param>
public sealed record GetEpisodeCharactersQuery(Guid EpisodeId)
    : IRequest<Result<List<CharacterDto>>>;

/// <summary>Validates <see cref="GetEpisodeCharactersQuery"/>.</summary>
public sealed class GetEpisodeCharactersValidator : AbstractValidator<GetEpisodeCharactersQuery>
{
    public GetEpisodeCharactersValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
    }
}

/// <summary>Handles <see cref="GetEpisodeCharactersQuery"/>.</summary>
public sealed class GetEpisodeCharactersQueryHandler(
    IEpisodeRepository episodes,
    ICharacterRepository characters) : IRequestHandler<GetEpisodeCharactersQuery, Result<List<CharacterDto>>>
{
    public async Task<Result<List<CharacterDto>>> Handle(
        GetEpisodeCharactersQuery query, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(query.EpisodeId, ct);
        if (episode is null)
            return Result<List<CharacterDto>>.Failure("Episode not found.", "NOT_FOUND");

        var list = await characters.GetByEpisodeIdAsync(query.EpisodeId, ct);
        return Result<List<CharacterDto>>.Success(list.Select(c => c.ToDto()).ToList());
    }
}
