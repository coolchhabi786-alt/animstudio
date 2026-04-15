using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.DeleteCharacter;

/// <summary>
/// Soft-deletes a character. Blocked if the character is referenced by any
/// Episode that has not yet reached a terminal state (Done or Failed).
/// </summary>
/// <param name="TeamId">Caller's team — used for BOLA check.</param>
/// <param name="UserId">Caller's user ID — passed to the aggregate.</param>
/// <param name="CharacterId">Character to delete.</param>
public sealed record DeleteCharacterCommand(Guid TeamId, Guid UserId, Guid CharacterId) : IRequest<Result<Unit>>;

/// <summary>Validates <see cref="DeleteCharacterCommand"/>.</summary>
public sealed class DeleteCharacterValidator : AbstractValidator<DeleteCharacterCommand>
{
    public DeleteCharacterValidator()
    {
        RuleFor(x => x.CharacterId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="DeleteCharacterCommand"/>:
/// 1. Loads the character and verifies team ownership.
/// 2. Rejects if the character is attached to any active episode.
/// 3. Soft-deletes the character.
/// </summary>
public sealed class DeleteCharacterCommandHandler(
    ICharacterRepository characters) : IRequestHandler<DeleteCharacterCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteCharacterCommand cmd, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(cmd.CharacterId, ct);
        if (character is null)
            return Result<Unit>.Failure("Character not found.", "NOT_FOUND");

        // Enforce team ownership (OWASP BOLA)
        if (character.TeamId != cmd.TeamId)
            return Result<Unit>.Failure("Character not found.", "NOT_FOUND");

        // Block delete if used in an active episode
        var usedInActive = await characters.IsUsedInActiveEpisodeAsync(cmd.CharacterId, ct);
        if (usedInActive)
            return Result<Unit>.Failure(
                "Cannot delete a character that is cast in one or more active episodes.",
                "CHARACTER_IN_USE");

        character.Delete(cmd.UserId);
        await characters.UpdateAsync(character, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
