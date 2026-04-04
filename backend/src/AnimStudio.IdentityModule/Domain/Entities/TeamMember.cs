using System;
using AnimStudio.IdentityModule.Domain.ValueObjects;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Entities
{
    /// <summary>
    /// Represents a member of a team within the system.
    /// </summary>
    public class TeamMember
    {
        /// <summary>
        /// Gets or sets the ID of the team that the member belongs to.
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who is the team member.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the role of the team member.
        /// </summary>
        public TeamRole Role { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user joined the team.
        /// </summary>
        public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the invite token for joining the team.
        /// </summary>
        public string? InviteToken { get; set; }

        /// <summary>
        /// Gets or sets the expiry timestamp for the invite token.
        /// </summary>
        public DateTimeOffset? InviteExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the invite was accepted.
        /// </summary>
        public DateTimeOffset? InviteAcceptedAt { get; set; }

        /// <summary>
        /// Gets or sets the reference to the team entity.
        /// </summary>
        public Team Team { get; set; } = null!;

        /// <summary>
        /// Gets or sets the reference to the user entity.
        /// </summary>
        public User User { get; set; } = null!;
    }
}