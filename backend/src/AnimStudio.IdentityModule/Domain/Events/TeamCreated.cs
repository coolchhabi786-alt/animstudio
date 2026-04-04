using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a new team is created.
    /// </summary>
    public class TeamCreated : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the created team.
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Gets the name of the created team.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ID of the user who owns the team.
        /// </summary>
        public Guid OwnerId { get; }

        /// <summary>
        /// Initializes an instance of <see cref="TeamCreated"/>.
        /// </summary>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="name">The team's name.</param>
        /// <param name="ownerId">The owner's ID.</param>
        public TeamCreated(Guid teamId, string name, Guid ownerId)
        {
            TeamId = teamId;
            Name = name;
            OwnerId = ownerId;
        }
    }
}