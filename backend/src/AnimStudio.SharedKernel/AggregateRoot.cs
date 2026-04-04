using System.Collections.Generic;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// An aggregate root base class extending the base entity and containing domain events.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate root ID.</typeparam>
    public abstract class AggregateRoot<TId> : Entity<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// Gets the list of domain events associated with this aggregate root.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Adds a domain event to the aggregate root.
        /// </summary>
        /// <param name="domainEvent">The domain event to add.</param>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clears all domain events from the aggregate root.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}