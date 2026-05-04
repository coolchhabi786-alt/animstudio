using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.AnalyticsModule.Domain.Entities;
using AnimStudio.AnalyticsModule.Domain.Enums;
using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimStudio.AnalyticsModule.Application.EventHandlers;

public sealed class EpisodeFailedEventHandler(
    ContentDbContext         contentDb,
    IdentityDbContext        identityDb,
    INotificationRepository  notifications,
    ILogger<EpisodeFailedEventHandler> logger)
    : INotificationHandler<EpisodeFailedEvent>
{
    public async Task Handle(EpisodeFailedEvent evt, CancellationToken ct)
    {
        try
        {
            var episode = await contentDb.Episodes.FindAsync([evt.EpisodeId], ct);
            if (episode is null)
            {
                logger.LogWarning("EpisodeFailedEvent: episode {Id} not found", evt.EpisodeId);
                return;
            }

            var project = await contentDb.Projects.FindAsync([episode.ProjectId], ct);
            if (project is null) return;

            var team = await identityDb.Teams.FirstOrDefaultAsync(t => t.Id == project.TeamId, ct);
            if (team is null) return;

            var notification = Notification.Create(
                userId:            team.OwnerId,
                type:              NotificationType.JobFailed,
                title:             "Episode Processing Failed",
                body:              $"\"{episode.Name}\" failed at stage {evt.FailedAtStage}: {evt.Error}",
                relatedEntityId:   evt.EpisodeId,
                relatedEntityType: "Episode");

            await notifications.AddAsync(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EpisodeFailedEventHandler failed for episode {Id}", evt.EpisodeId);
        }
    }
}
