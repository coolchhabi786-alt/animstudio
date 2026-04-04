using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a subscription is activated.
    /// </summary>
    public class SubscriptionActivated : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the activated subscription.
        /// </summary>
        public Guid SubscriptionId { get; }

        /// <summary>
        /// Gets the ID of the team associated with the subscription.
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Gets the ID of the plan tied to the subscription.
        /// </summary>
        public Guid PlanId { get; }

        /// <summary>
        /// Initializes an instance of <see cref="SubscriptionActivated"/>.
        /// </summary>
        /// <param name="subscriptionId">The subscription's ID.</param>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="planId">The plan's ID.</param>
        public SubscriptionActivated(Guid subscriptionId, Guid teamId, Guid planId)
        {
            SubscriptionId = subscriptionId;
            TeamId = teamId;
            PlanId = planId;
        }
    }
}