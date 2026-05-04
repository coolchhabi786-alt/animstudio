using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.IdentityModule.Domain.Interfaces;
using MediatR;

namespace AnimStudio.API.Services;

/// <summary>
/// Resolves project → team → subscription and increments the monthly usage counter,
/// raising SubscriptionUsageWarning/QuotaExceeded domain events as needed.
/// </summary>
public sealed class UsageMeteringService(
    IProjectRepository      projects,
    ISubscriptionRepository subscriptions,
    IPublisher              publisher,
    ILogger<UsageMeteringService> logger)
    : IUsageMeteringService
{
    public async Task IncrementEpisodeUsageAsync(Guid episodeId, Guid projectId, CancellationToken ct = default)
    {
        try
        {
            var project = await projects.GetByIdAsync(projectId, ct);
            if (project is null)
            {
                logger.LogWarning("UsageMeteringService: project {Id} not found", projectId);
                return;
            }

            var subscription = await subscriptions.GetByTeamIdAsync(project.TeamId, ct);
            if (subscription is null)
            {
                logger.LogWarning("UsageMeteringService: no subscription for team {Id}", project.TeamId);
                return;
            }

            var plan = await subscriptions.GetPlanByIdAsync(subscription.PlanId, ct);
            subscription.IncrementEpisodeUsage(plan?.EpisodesPerMonth ?? 0);
            await subscriptions.UpdateSubscriptionAsync(subscription, ct);

            foreach (var evt in subscription.DomainEvents)
                await publisher.Publish(evt, ct);
            subscription.ClearDomainEvents();

            logger.LogInformation(
                "Episode usage incremented for team {TeamId}: {Usage}/{Quota}",
                project.TeamId, subscription.UsageEpisodesThisMonth, plan?.EpisodesPerMonth ?? 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UsageMeteringService failed for episode {Id}", episodeId);
        }
    }
}
