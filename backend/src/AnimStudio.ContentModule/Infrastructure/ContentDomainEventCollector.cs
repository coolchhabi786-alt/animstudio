using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Infrastructure;

/// <summary>
/// Harvests domain events from all <see cref="AggregateRoot{TId}"/> entities
/// tracked by <see cref="ContentDbContext"/> so the
/// <see cref="AnimStudio.SharedKernel.Behaviours.TransactionBehaviour{TRequest,TResponse}"/>
/// can flush them to the outbox after each command.
/// </summary>
internal sealed class ContentDomainEventCollector(ContentDbContext db) : IDomainEventCollector
{
    public IReadOnlyList<IDomainEvent> CollectAndClear()
    {
        var aggregates = db.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var agg in aggregates)
            agg.ClearDomainEvents();

        return events;
    }
}
