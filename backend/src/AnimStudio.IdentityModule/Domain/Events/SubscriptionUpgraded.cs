using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a subscription is upgraded.
    /// </summary>
    public class SubscriptionUpgraded : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the upgraded subscription.
        /// </summary>
        public Guid SubscriptionId { get; }

        /// <summary>
        /// Gets the ID of the new plan after the upgrade.
        /// </summary>
        public Guid NewPlanId { get; }

        /// <summary>
        /// Gets the ID of the old plan before the upgrade.
        /// </summary>
        public Guid OldPlanId { get; }

        /// <summary>
        /// Initializes an instance of <see cref="SubscriptionUpgraded"/>.
        /// </summary>
        /// <param name="subscriptionId">The subscription's ID.</param>
        /// <param name="newPlanId">The new plan's ID.</param>
        /// <param name="oldPlanId">The old plan's ID.</param>
        public SubscriptionUpgraded(Guid subscriptionId, Guid newPlanId, Guid oldPlanId)
        {
            SubscriptionId = subscriptionId;
            NewPlanId = newPlanId;
            OldPlanId = oldPlanId;
        }
    }
}