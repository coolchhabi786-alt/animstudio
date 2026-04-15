using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Events;

/// <summary>Raised when a new Character record is first created (Draft state).</summary>
/// <param name="CharacterId">The new character's identity.</param>
/// <param name="TeamId">The owning team.</param>
/// <param name="Name">Display name for logging.</param>
public sealed record CharacterCreatedEvent(
    Guid CharacterId,
    Guid TeamId,
    string Name) : IDomainEvent;

/// <summary>
/// Raised on every training-stage transition, including intermediate progress ticks.
/// </summary>
/// <param name="CharacterId">Character whose training advanced.</param>
/// <param name="TeamId">Owning team — used to route the SignalR broadcast.</param>
/// <param name="Status">New <see cref="TrainingStatus"/>.</param>
/// <param name="ProgressPercent">0–100 completion for the current stage.</param>
public sealed record CharacterTrainingProgressedEvent(
    Guid CharacterId,
    Guid TeamId,
    TrainingStatus Status,
    int ProgressPercent) : IDomainEvent;

/// <summary>
/// Raised when a character reaches <see cref="TrainingStatus.Ready"/> and its LoRA
/// weights are available for episode rendering.
/// </summary>
/// <param name="CharacterId">Character that is now ready.</param>
/// <param name="TeamId">Owning team.</param>
/// <param name="LoraWeightsUrl">Blob Storage URL of the trained weights.</param>
/// <param name="TriggerWord">Short prompt token for this character.</param>
public sealed record CharacterReadyEvent(
    Guid CharacterId,
    Guid TeamId,
    string LoraWeightsUrl,
    string TriggerWord) : IDomainEvent;

/// <summary>Raised when training fails. The character may be retried.</summary>
/// <param name="CharacterId">Failed character.</param>
/// <param name="TeamId">Owning team.</param>
/// <param name="Reason">Optional human-readable failure reason.</param>
public sealed record CharacterTrainingFailedEvent(
    Guid CharacterId,
    Guid TeamId,
    string? Reason) : IDomainEvent;
