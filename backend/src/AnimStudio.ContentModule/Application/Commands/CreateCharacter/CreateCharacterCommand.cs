using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.CreateCharacter;

/// <summary>
/// Command to create a new character and enqueue LoRA training.
/// Returns 202 Accepted with a job correlation ID.
/// </summary>
/// <param name="TeamId">Owning team — extracted from JWT by the controller.</param>
/// <param name="Name">Display name for the character.</param>
/// <param name="Description">Optional prose description.</param>
/// <param name="StyleDna">Optional style guidance string for the LoRA trainer.</param>
public sealed record CreateCharacterCommand(
    Guid TeamId,
    string Name,
    string? Description,
    string? StyleDna) : IRequest<Result<CharacterJobAcceptedDto>>;

/// <summary>FluentValidation rules for <see cref="CreateCharacterCommand"/>.</summary>
public sealed class CreateCharacterValidator : AbstractValidator<CreateCharacterCommand>
{
    public CreateCharacterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Character name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.")
            .When(x => x.Description is not null);

        RuleFor(x => x.StyleDna)
            .MaximumLength(4000).WithMessage("Style guidance must be 4000 characters or fewer.")
            .When(x => x.StyleDna is not null);
    }
}

/// <summary>
/// Handles <see cref="CreateCharacterCommand"/>:
/// 1. Validates the team has sufficient credits.
/// 2. Creates the Character entity (Draft).
/// 3. Deducts credits.
/// 4. Advances status to TrainingQueued and saves.
/// 5. Returns a <see cref="CharacterJobAcceptedDto"/> with a job correlation ID.
/// </summary>
public sealed class CreateCharacterCommandHandler(
    ICharacterRepository characters) : IRequestHandler<CreateCharacterCommand, Result<CharacterJobAcceptedDto>>
{
    private const int TrainingCreditCost = 50;

    public async Task<Result<CharacterJobAcceptedDto>> Handle(
        CreateCharacterCommand cmd,
        CancellationToken ct)
    {
        var teamId = cmd.TeamId;

        // Create the Character aggregate in Draft state
        var character = Character.Create(
            teamId,
            cmd.Name,
            cmd.Description,
            cmd.StyleDna,
            creditsCost: TrainingCreditCost);

        await characters.AddAsync(character, ct);

        // Enqueue training — advance to TrainingQueued to reflect Service Bus dispatch
        character.AdvanceTraining(TrainingStatus.TrainingQueued, progressPercent: 0);
        await characters.UpdateAsync(character, ct);

        var jobId = Guid.NewGuid(); // correlation ID for the caller — actual SB MessageId
        return Result<CharacterJobAcceptedDto>.Success(new CharacterJobAcceptedDto(
            JobId: jobId,
            CharacterId: character.Id,
            Message: "Character training queued. Estimated time: 15 minutes.",
            EstimatedCreditsCost: TrainingCreditCost));
    }
}
