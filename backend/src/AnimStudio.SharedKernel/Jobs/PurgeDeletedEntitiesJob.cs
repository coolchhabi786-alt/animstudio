using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AnimStudio.SharedKernel.Persistence;

namespace AnimStudio.SharedKernel.Jobs;

/// <summary>
/// Permanently removes soft-deleted entities that have been deleted for more than 30 days.
/// Runs weekly.
/// </summary>
public sealed class PurgeDeletedEntitiesJob(
    SharedDbContext sharedDb,
    ILogger<PurgeDeletedEntitiesJob> logger)
{
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow - RetentionPeriod;

        // Purge processed outbox messages older than retention period
        var purgedOutbox = await sharedDb.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Delivered && m.ProcessedAt <= cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation(
            "PurgeDeletedEntitiesJob: purged {Outbox} delivered outbox messages older than {Cutoff:O}",
            purgedOutbox, cutoff);
    }
}
