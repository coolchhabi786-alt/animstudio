using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.AttachCharacter;

/// <summary>
/// Attaches an existing <see cref="Character"/> to an <see cref="Episode"/>.
/// Only <see cref="TrainingStatus.Ready"/> characters may be attached.
/// </summary>
/// <param name="TeamId">Caller's team — used for BOLA check.</param>
/// <param name="EpisodeId">Target episode.</param>
/// <param name="CharacterId">Character to attach.</param>
public sealed record AttachCharacterCommand(Guid TeamId, Guid EpisodeId, Guid CharacterId)
    : IRequest<Result<Unit>>;

/// <summary>Validates <see cref="AttachCharacterCommand"/>.</summary>
public sealed class AttachCharacterValidator : AbstractValidator<AttachCharacterCommand>
{
    public AttachCharacterValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.CharacterId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="AttachCharacterCommand"/>:
/// 1. Verifies the episode exists and belongs to the caller's team.
/// 2. Verifies the character is Ready and belongs to the same team.
/// 3. Prevents duplicate attachment.
/// 4. Creates the <see cref="EpisodeCharacter"/> join record.
/// </summary>
public sealed class AttachCharacterCommandHandler(
    IEpisodeRepository episodes,
    ICharacterRepository characters) : IRequestHandler<AttachCharacterCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AttachCharacterCommand cmd, CancellationToken ct)
    {
        var teamId = cmd.TeamId;

        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<Unit>.Failure("Episode not found.", "NOT_FOUND");

        var character = await characters.GetByIdAsync(cmd.CharacterId, ct);
        if (character is null)
            return Result<Unit>.Failure("Character not found.", "NOT_FOUND");

        // BOLA: both resources must belong to the caller's team
        if (character.TeamId != teamId)
            return Result<Unit>.Failure("Character not found.", "NOT_FOUND");

        if (character.TrainingStatus != TrainingStatus.Ready)
            return Result<Unit>.Failure(
                $"Character must be in 'Ready' status to be attached. Current status: {character.TrainingStatus}.",
                "CHARACTER_NOT_READY");

        // Prevent duplicates
        var existing = await characters.GetEpisodeCharacterAsync(cmd.EpisodeId, cmd.CharacterId, ct);
        if (existing is not null)
            return Result<Unit>.Failure("Character is already attached to this episode.", "ALREADY_ATTACHED");

        var link = new EpisodeCharacter
        {
            EpisodeId = cmd.EpisodeId,
            CharacterId = cmd.CharacterId,
            AttachedAt = DateTimeOffset.UtcNow,
        };
        await characters.AttachToEpisodeAsync(link, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
