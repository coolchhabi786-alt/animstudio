using AnimStudio.SharedKernel.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AnimStudio.SharedKernel.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that flushes domain events to the transactional outbox
/// after every write command completes successfully.
///
/// Design decisions:
/// - Module repositories call their own DbContext.SaveChanges() inside the handler.
/// - After the handler succeeds, domain events are harvested from all registered
///   IDomainEventCollector implementations (one per module).
/// - Events are serialized to OutboxMessage rows in SharedDbContext and saved.
/// - This is a best-effort outbox: if the outbox save fails the business data is
///   already committed. Event handlers MUST be idempotent.
/// - Read queries (implementing ICacheKey) are passed through untouched.
/// </summary>
public sealed class TransactionBehaviour<TRequest, TResponse>(
    SharedDbContext sharedDb,
    IEnumerable<IDomainEventCollector> collectors,
    ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Read queries pass through without any transaction overhead
        if (request is ICacheKey)
            return await next();

        // Execute the handler
        var response = await next();

        // Harvest domain events raised during handler execution
        var domainEvents = collectors
            .SelectMany(c => c.CollectAndClear())
            .ToList();

        if (domainEvents.Count == 0)
            return response;

        var now = DateTimeOffset.UtcNow;
        foreach (var evt in domainEvents)
        {
            sharedDb.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = evt.GetType().AssemblyQualifiedName ?? evt.GetType().FullName!,
                Payload = JsonSerializer.Serialize(evt, evt.GetType(), JsonOpts),
                Status = OutboxMessageStatus.Pending,
                OccurredAt = now,
                RetryCount = 0,
            });
        }

        try
        {
            await sharedDb.SaveChangesAsync(cancellationToken);
            logger.LogDebug("Outbox: flushed {Count} domain event(s) for {RequestType}",
                domainEvents.Count, typeof(TRequest).Name);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Outbox flush failed after {RequestType}; {Count} event(s) may be lost.",
                typeof(TRequest).Name, domainEvents.Count);
        }

        return response;
    }
}
