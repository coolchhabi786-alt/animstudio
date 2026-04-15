using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.ContentModule.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace AnimStudio.ContentModule.Application.Services;

/// <summary>
/// Domain event handlers for character training lifecycle events.
/// Broadcasts real-time progress updates to the team's SignalR group via
/// <see cref="ICharacterProgressNotifier"/> (implemented in AnimStudio.API).
/// </summary>
public sealed class CharacterTrainingService(
    ICharacterProgressNotifier notifier,
    ILogger<CharacterTrainingService> logger)
    : INotificationHandler<CharacterTrainingProgressedEvent>,
      INotificationHandler<CharacterReadyEvent>,
      INotificationHandler<CharacterTrainingFailedEvent>
{
    // ── CharacterTrainingProgressedEvent ────────────────────────────────────
    public async Task Handle(CharacterTrainingProgressedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Character {CharacterId} training progressed → {Status} ({Percent}%)",
            notification.CharacterId, notification.Status, notification.ProgressPercent);

        await BroadcastAsync(
            notification.TeamId,
            notification.CharacterId,
            notification.Status.ToString(),
            notification.ProgressPercent,
            stage: notification.Status.ToString(),
            ct);
    }

    // ── CharacterReadyEvent ──────────────────────────────────────────────────
    public async Task Handle(CharacterReadyEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Character {CharacterId} is Ready. LoRA weights at {Url}",
            notification.CharacterId, notification.LoraWeightsUrl);

        await BroadcastAsync(
            notification.TeamId,
            notification.CharacterId,
            TrainingStatus.Ready.ToString(),
            progressPercent: 100,
            stage: TrainingStatus.Ready.ToString(),
            ct);
    }

    // ── CharacterTrainingFailedEvent ─────────────────────────────────────────
    public async Task Handle(CharacterTrainingFailedEvent notification, CancellationToken ct)
    {
        logger.LogWarning(
            "Character {CharacterId} training failed: {Reason}",
            notification.CharacterId, notification.Reason ?? "(no reason)");

        await BroadcastAsync(
            notification.TeamId,
            notification.CharacterId,
            TrainingStatus.Failed.ToString(),
            progressPercent: 0,
            stage: TrainingStatus.Failed.ToString(),
            ct);
    }

    // ──────────────────────────────────────── Shared ─────────────────────────

    private Task BroadcastAsync(
        Guid teamId,
        Guid characterId,
        string status,
        int progressPercent,
        string stage,
        CancellationToken ct)
    {
        return notifier.NotifyAsync(teamId, characterId, status, progressPercent, stage, ct);
    }
}
