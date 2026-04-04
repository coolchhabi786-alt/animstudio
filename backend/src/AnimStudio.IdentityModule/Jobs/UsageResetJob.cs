using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimStudio.IdentityModule.Jobs;

/// <summary>
/// Resets the monthly episode usage counter for all active subscriptions.
/// Runs daily; processes only subscriptions where <c>UsageResetAt</c> has passed.
/// </summary>
public sealed class UsageResetJob(
    IdentityDbContext db,
    ILogger<UsageResetJob> logger)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var due = await db.Subscriptions
            .Where(s => s.UsageResetAt <= now
                    && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .ToListAsync(cancellationToken);

        if (due.Count == 0)
            return;

        foreach (var subscription in due)
            subscription.ResetMonthlyUsage();

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("UsageResetJob: reset {Count} subscriptions at {Now}", due.Count, now);
    }
}
