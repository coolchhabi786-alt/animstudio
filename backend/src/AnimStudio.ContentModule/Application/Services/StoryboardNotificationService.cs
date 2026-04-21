using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimStudio.ContentModule.Application.Services;

/// <summary>
/// Handles storyboard domain events and broadcasts real-time shot updates to
/// the team's SignalR group via <see cref="IStoryboardShotNotifier"/>.
///
/// Triggered by the outbox publisher after each write command flushes its events.
/// The episode → project lookup is used to resolve the team ID for SignalR routing.
/// </summary>
public sealed class StoryboardNotificationService(
    IEpisodeRepository episodes,
    IProjectRepository projects,
    IStoryboardShotNotifier notifier,
    ILogger<StoryboardNotificationService> logger)
    : INotificationHandler<StoryboardShotRegeneratedEvent>,
      INotificationHandler<StoryboardShotImageUpdatedEvent>
{
    // ── StoryboardShotRegeneratedEvent ───────────────────────────────────────
    // Fires for both RegenerateShot and UpdateShotStyle commands (the style command
    // always increments the regen counter). One notification per user action.

    public async Task Handle(StoryboardShotRegeneratedEvent notification, CancellationToken ct)
    {
        var teamId = await ResolveTeamIdAsync(notification.EpisodeId, ct);
        if (teamId is null) return;

        await notifier.NotifyShotUpdatedAsync(
            teamId.Value,
            notification.StoryboardId,
            notification.EpisodeId,
            notification.ShotId,
            imageUrl: null,
            notification.RegenerationCount,
            ct);

        logger.LogInformation(
            "ShotRegenerated broadcast → team:{TeamId} shot:{ShotId} (count={Count})",
            teamId.Value, notification.ShotId, notification.RegenerationCount);
    }

    // ── StoryboardShotImageUpdatedEvent ──────────────────────────────────────
    // Fires when a StoryboardGen job completion sets the CDN URL on the shot.

    public async Task Handle(StoryboardShotImageUpdatedEvent notification, CancellationToken ct)
    {
        var teamId = await ResolveTeamIdAsync(notification.EpisodeId, ct);
        if (teamId is null) return;

        await notifier.NotifyShotUpdatedAsync(
            teamId.Value,
            notification.StoryboardId,
            notification.EpisodeId,
            notification.ShotId,
            notification.ImageUrl,
            notification.RegenerationCount,
            ct);

        logger.LogInformation(
            "ShotImageUpdated broadcast → team:{TeamId} shot:{ShotId}",
            teamId.Value, notification.ShotId);
    }

    // ── Shared ───────────────────────────────────────────────────────────────

    private async Task<Guid?> ResolveTeamIdAsync(Guid episodeId, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(episodeId, ct);
        if (episode is null)
        {
            logger.LogWarning("Episode {EpisodeId} not found — skipping storyboard SignalR broadcast", episodeId);
            return null;
        }

        var project = await projects.GetByIdAsync(episode.ProjectId, ct);
        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} not found — skipping storyboard SignalR broadcast", episode.ProjectId);
            return null;
        }

        return project.TeamId;
    }
}
