using AnimStudio.SharedKernel.Persistence;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AnimStudio.SharedKernel.Jobs;

/// <summary>
/// Polls the shared outbox table and publishes pending domain events via MediatR.
/// Scheduled every 10 seconds via Hangfire recurring job.
/// </summary>
public sealed class OutboxPublisherJob(
    SharedDbContext db,
    IMediator mediator,
    ILogger<OutboxPublisherJob> logger)
{
    private const int BatchSize = 50;

    [DisableConcurrentExecution(timeoutInSeconds: 30)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pending = await db.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    logger.LogWarning("Cannot resolve event type {Type}; skipping message {Id}",
                        message.EventType, message.Id);
                    message.Status = OutboxMessageStatus.Failed;
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event is INotification notification)
                    await mediator.Publish(notification, cancellationToken);

                message.Status = OutboxMessageStatus.Delivered;
                message.ProcessedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                message.RetryCount++;
                if (message.RetryCount >= 5)
                    message.Status = OutboxMessageStatus.Failed;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
