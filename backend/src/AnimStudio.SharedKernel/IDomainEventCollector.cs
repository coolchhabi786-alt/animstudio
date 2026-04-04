namespace AnimStudio.SharedKernel;

/// <summary>
/// Implemented by each module's infrastructure adapter to expose domain events
/// raised by its aggregates during a request.
///
/// The <see cref="Behaviours.TransactionBehaviour{TRequest,TResponse}"/> resolves ALL
/// registered collectors (one per module) and drains their domain events into the
/// transactional outbox after the handler completes.
/// </summary>
public interface IDomainEventCollector
{
    /// <summary>
    /// Returns all pending domain events for this module's aggregates and clears
    /// the internal list so events are not processed twice.
    /// </summary>
    IReadOnlyList<IDomainEvent> CollectAndClear();
}
