using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a subscription is cancelled.
    /// </summary>
    public class SubscriptionCancelled : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the cancelled subscription.
        /// </summary>
        public Guid SubscriptionId { get; }

        /// <summary>
        /// Gets the ID of the team associated with the subscription.
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Gets the timestamp when the subscription was cancelled.
        /// </summary>
        public DateTimeOffset CancelledAt { get; }

        /// <summary>
        /// Initializes an instance of <see cref="SubscriptionCancelled"/>.
        /// </summary>
        /// <param name="subscriptionId">The subscription's ID.</param>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="cancelledAt">The cancellation timestamp.</param>
        public SubscriptionCancelled(Guid subscriptionId, Guid teamId, DateTimeOffset cancelledAt)
        {
            SubscriptionId = subscriptionId;
            TeamId = teamId;
            CancelledAt = cancelledAt;
        }
    }
}