using AnimStudio.IdentityModule.Jobs;
using AnimStudio.SharedKernel.Jobs;
using Hangfire;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Registers all Hangfire recurring jobs when the application starts.
/// Using IHostedService ensures recurring jobs are registered AFTER
/// Hangfire storage is fully initialized, avoiding the JobStorage.Current pitfall.
/// </summary>
public sealed class RecurringJobsHostedService(
    IRecurringJobManager jobs,
    ILogger<RecurringJobsHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Queue: critical — outbox publisher every minute
            jobs.AddOrUpdate<OutboxPublisherJob>(
            "outbox-publisher",
            "critical",
            j => j.ExecuteAsync(CancellationToken.None),
            "* * * * *");

        // Queue: default — daily usage reset at midnight UTC
        jobs.AddOrUpdate<UsageResetJob>(
            "usage-reset",
            "default",
            j => j.ExecuteAsync(CancellationToken.None),
            Cron.Daily());

        // Queue: default — dead-letter retry every 5 minutes
        jobs.AddOrUpdate<DeadLetterRetryJob>(
            "dead-letter-retry",
            "default",
            j => j.ExecuteAsync(CancellationToken.None),
            "*/5 * * * *");

        // Queue: low — monthly purge of soft-deleted entities at 2am on 1st
        jobs.AddOrUpdate<PurgeDeletedEntitiesJob>(
            "purge-deleted",
            "low",
            j => j.ExecuteAsync(CancellationToken.None),
            "0 2 1 * *");

        logger.LogInformation("Hangfire recurring jobs registered: outbox-publisher, usage-reset, dead-letter-retry, purge-deleted");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
