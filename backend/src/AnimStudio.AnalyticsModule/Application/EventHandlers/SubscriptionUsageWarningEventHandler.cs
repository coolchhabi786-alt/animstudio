using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.AnalyticsModule.Domain.Entities;
using AnimStudio.AnalyticsModule.Domain.Enums;
using AnimStudio.IdentityModule.Domain.Events;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimStudio.AnalyticsModule.Application.EventHandlers;

public sealed class SubscriptionUsageWarningEventHandler(
    IdentityDbContext        identityDb,
    INotificationRepository  notifications,
    ILogger<SubscriptionUsageWarningEventHandler> logger)
    : INotificationHandler<SubscriptionUsageWarningEvent>
{
    public async Task Handle(SubscriptionUsageWarningEvent evt, CancellationToken ct)
    {
        try
        {
            var team = await identityDb.Teams.FirstOrDefaultAsync(t => t.Id == evt.TeamId, ct);
            if (team is null) return;

            var notification = Notification.Create(
                userId:            team.OwnerId,
                type:              NotificationType.UsageWarning,
                title:             "Approaching Episode Limit",
                body:              $"You have used {evt.UsagePercent}% of your monthly episode quota.",
                relatedEntityId:   evt.SubscriptionId,
                relatedEntityType: "Subscription");

            await notifications.AddAsync(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SubscriptionUsageWarningEventHandler failed for team {Id}", evt.TeamId);
        }
    }
}
