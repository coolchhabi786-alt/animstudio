using AnimStudio.SharedKernel.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimStudio.SharedKernel.Jobs;

/// <summary>
/// Retries outbox messages that have been in <see cref="OutboxMessageStatus.Failed"/> state
/// for at least one hour, up to a maximum retry count.
/// </summary>
public sealed class DeadLetterRetryJob(
    SharedDbContext db,
    ILogger<DeadLetterRetryJob> logger)
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan FailedCooldown = TimeSpan.FromHours(1);

    [DisableConcurrentExecution(timeoutInSeconds: 30)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var threshold = DateTimeOffset.UtcNow - FailedCooldown;

        var retryable = await db.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Failed
                     && m.RetryCount < MaxRetries
                     && m.OccurredAt <= threshold)
            .ToListAsync(cancellationToken);

        if (retryable.Count == 0)
            return;

        foreach (var message in retryable)
            message.Status = OutboxMessageStatus.Pending;

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("DeadLetterRetryJob: re-queued {Count} messages for retry", retryable.Count);
    }
}
