using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a team member joins a team.
    /// </summary>
    public class TeamMemberJoined : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the user who joined the team.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// Gets the ID of the team the user joined.
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Gets the timestamp of when the user joined the team.
        /// </summary>
        public DateTimeOffset JoinTimestamp { get; }

        /// <summary>
        /// Initializes an instance of <see cref="TeamMemberJoined"/>.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="joinTimestamp">The timestamp of the join.</param>
        public TeamMemberJoined(Guid userId, Guid teamId, DateTimeOffset joinTimestamp)
        {
            UserId = userId;
            TeamId = teamId;
            JoinTimestamp = joinTimestamp;
        }
    }
}