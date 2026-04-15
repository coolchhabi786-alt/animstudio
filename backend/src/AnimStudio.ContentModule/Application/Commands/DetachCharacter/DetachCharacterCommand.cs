using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.DetachCharacter;

/// <summary>
/// Detaches a character from an episode by removing the EpisodeCharacter join record.
/// </summary>
/// <param name="TeamId">Caller's team — used for BOLA check.</param>
/// <param name="EpisodeId">Episode to detach from.</param>
/// <param name="CharacterId">Character to detach.</param>
public sealed record DetachCharacterCommand(Guid TeamId, Guid EpisodeId, Guid CharacterId)
    : IRequest<Result<Unit>>;

/// <summary>Validates <see cref="DetachCharacterCommand"/>.</summary>
public sealed class DetachCharacterValidator : AbstractValidator<DetachCharacterCommand>
{
    public DetachCharacterValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.CharacterId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="DetachCharacterCommand"/>:
/// 1. Verifies the episode exists and the character belongs to the caller's team.
/// 2. Locates and removes the EpisodeCharacter join record.
/// </summary>
public sealed class DetachCharacterCommandHandler(
    IEpisodeRepository episodes,
    ICharacterRepository characters) : IRequestHandler<DetachCharacterCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DetachCharacterCommand cmd, CancellationToken ct)
    {
        var teamId = cmd.TeamId;

        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<Unit>.Failure("Episode not found.", "NOT_FOUND");

        var character = await characters.GetByIdAsync(cmd.CharacterId, ct);
        if (character is null || character.TeamId != teamId)
            return Result<Unit>.Failure("Character not found.", "NOT_FOUND");

        var link = await characters.GetEpisodeCharacterAsync(cmd.EpisodeId, cmd.CharacterId, ct);
        if (link is null)
            return Result<Unit>.Failure("Character is not attached to this episode.", "NOT_FOUND");

        await characters.DetachFromEpisodeAsync(link, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}
