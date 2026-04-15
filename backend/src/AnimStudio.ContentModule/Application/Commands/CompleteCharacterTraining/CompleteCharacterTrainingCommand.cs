using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.CompleteCharacterTraining;

/// <summary>
/// MediatR command invoked by <see cref="AnimStudio.API.Hosted.CompletionMessageProcessor"/>
/// when a training progress or completion message arrives from the GPU worker via Service Bus.
/// </summary>
/// <param name="CharacterId">Target character.</param>
/// <param name="TeamId">Owning team (used for SignalR routing).</param>
/// <param name="Status">New training status from the worker.</param>
/// <param name="ProgressPercent">Stage completion percentage (0–100).</param>
/// <param name="ImageUrl">Set when PoseGeneration completes.</param>
/// <param name="LoraWeightsUrl">Set when Training completes.</param>
/// <param name="TriggerWord">Set when Training completes.</param>
/// <param name="FailureReason">Populated only when <paramref name="Status"/> is Failed.</param>
public sealed record CompleteCharacterTrainingCommand(
    Guid CharacterId,
    Guid TeamId,
    TrainingStatus Status,
    int ProgressPercent,
    string? ImageUrl,
    string? LoraWeightsUrl,
    string? TriggerWord,
    string? FailureReason) : IRequest<Result<Unit>>;

/// <summary>Validates <see cref="CompleteCharacterTrainingCommand"/>.</summary>
public sealed class CompleteCharacterTrainingValidator : AbstractValidator<CompleteCharacterTrainingCommand>
{
    public CompleteCharacterTrainingValidator()
    {
        RuleFor(x => x.CharacterId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.ProgressPercent).InclusiveBetween(0, 100);
    }
}

/// <summary>
/// Handles <see cref="CompleteCharacterTrainingCommand"/>:
/// 1. Loads the character (no team auth — this is a system-to-system call).
/// 2. Updates training state via the domain aggregate.
/// 3. Persists changes.
/// (SignalR broadcast is handled by the domain event handler in CharacterTrainingService.)
/// </summary>
public sealed class CompleteCharacterTrainingHandler(
    ICharacterRepository characters) : IRequestHandler<CompleteCharacterTrainingCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CompleteCharacterTrainingCommand cmd, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(cmd.CharacterId, ct);
        if (character is null)
            return Result<Unit>.Failure($"Character {cmd.CharacterId} not found.", "NOT_FOUND");

        if (cmd.Status == TrainingStatus.Failed)
        {
            character.FailTraining(cmd.FailureReason);
        }
        else
        {
            character.AdvanceTraining(
                cmd.Status,
                cmd.ProgressPercent,
                cmd.ImageUrl,
                cmd.LoraWeightsUrl,
                cmd.TriggerWord);
        }

        await characters.UpdateAsync(character, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}
