using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimStudio.ContentModule.Application.EventHandlers;

/// <summary>
/// Bridges the domain event <see cref="AnimationClipReadyEvent"/> to the
/// team-scoped SignalR group via <see cref="IAnimationClipNotifier"/>.
/// Resolves TeamId by walking Episode → Project (both cached in the same
/// DbContext scope, so no extra round-trips in practice).
/// </summary>
public sealed class AnimationClipReadyEventHandler(
    IEpisodeRepository episodes,
    IProjectRepository projects,
    IAnimationClipNotifier notifier,
    ILogger<AnimationClipReadyEventHandler> logger)
    : INotificationHandler<AnimationClipReadyEvent>
{
    public async Task Handle(AnimationClipReadyEvent notification, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(notification.EpisodeId, ct);
        if (episode is null)
        {
            logger.LogWarning(
                "AnimationClipReady: episode {EpisodeId} not found — skipping SignalR broadcast",
                notification.EpisodeId);
            return;
        }

        var project = await projects.GetByIdAsync(episode.ProjectId, ct);
        if (project is null)
        {
            logger.LogWarning(
                "AnimationClipReady: project {ProjectId} not found — skipping SignalR broadcast",
                episode.ProjectId);
            return;
        }

        await notifier.PublishClipReadyAsync(
            teamId:      project.TeamId,
            episodeId:   notification.EpisodeId,
            clipId:      notification.AnimationClipId,
            sceneNumber: notification.SceneNumber,
            shotIndex:   notification.ShotIndex,
            clipUrl:     notification.ClipUrl,
            ct:          ct);

        logger.LogInformation(
            "ClipReady broadcast: team {TeamId}, episode {EpisodeId}, scene {Scene} shot {Shot}",
            project.TeamId, notification.EpisodeId, notification.SceneNumber, notification.ShotIndex);
    }
}
