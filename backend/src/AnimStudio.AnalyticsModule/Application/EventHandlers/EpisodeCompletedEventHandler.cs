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

public sealed class EpisodeCompletedEventHandler(
    ContentDbContext         contentDb,
    IdentityDbContext        identityDb,
    INotificationRepository  notifications,
    ILogger<EpisodeCompletedEventHandler> logger)
    : INotificationHandler<EpisodeCompletedEvent>
{
    public async Task Handle(EpisodeCompletedEvent evt, CancellationToken ct)
    {
        try
        {
            var episode = await contentDb.Episodes.FindAsync([evt.EpisodeId], ct);
            if (episode is null)
            {
                logger.LogWarning("EpisodeCompletedEvent: episode {Id} not found", evt.EpisodeId);
                return;
            }

            var project = await contentDb.Projects.FindAsync([episode.ProjectId], ct);
            if (project is null) return;

            var team = await identityDb.Teams.FirstOrDefaultAsync(t => t.Id == project.TeamId, ct);
            if (team is null) return;

            var notification = Notification.Create(
                userId:            team.OwnerId,
                type:              NotificationType.EpisodeComplete,
                title:             "Episode Complete",
                body:              $"\"{episode.Name}\" has finished processing and is ready to review.",
                relatedEntityId:   evt.EpisodeId,
                relatedEntityType: "Episode");

            await notifications.AddAsync(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EpisodeCompletedEventHandler failed for episode {Id}", evt.EpisodeId);
        }
    }
}
