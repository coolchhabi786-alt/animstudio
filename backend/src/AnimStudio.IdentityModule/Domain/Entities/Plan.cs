using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Entities
{
    /// <summary>
    /// Represents a subscription plan within the system.
    /// </summary>
    public class Plan : AggregateRoot<Guid>
    {
        /// <summary>
        /// Gets or sets the name of the plan.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Stripe price identifier for the plan.
        /// </summary>
        public string StripePriceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of episodes allowed per month.
        /// </summary>
        public int EpisodesPerMonth { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters allowed.
        /// </summary>
        public int MaxCharacters { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of team members allowed.
        /// </summary>
        public int MaxTeamMembers { get; set; }

        /// <summary>
        /// Gets or sets the price of the plan.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plan is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the plan is the default one.
        /// </summary>
        public bool IsDefault { get; set; } = false;
    }
}