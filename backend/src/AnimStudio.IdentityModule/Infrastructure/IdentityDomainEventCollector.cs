using AnimStudio.IdentityModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.IdentityModule.Infrastructure;

/// <summary>
/// Harvests domain events from all <see cref="AggregateRoot{TId}"/> entities
/// currently tracked by <see cref="IdentityDbContext"/>.
///
/// Registered as scoped so it shares the same DbContext instance as the handler.
/// Called by <see cref="AnimStudio.SharedKernel.Behaviours.TransactionBehaviour{TRequest,TResponse}"/>
/// after the handler completes.
/// </summary>
internal sealed class IdentityDomainEventCollector(IdentityDbContext db) : IDomainEventCollector
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
