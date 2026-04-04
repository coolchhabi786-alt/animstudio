using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a team member is invited.
    /// </summary>
    public class TeamMemberInvited : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the invited user.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// Gets the ID of the team the user is invited to.
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Gets the invite token.
        /// </summary>
        public string InviteToken { get; }

        /// <summary>
        /// Initializes an instance of <see cref="TeamMemberInvited"/>.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="inviteToken">The invite token.</param>
        public TeamMemberInvited(Guid userId, Guid teamId, string inviteToken)
        {
            UserId = userId;
            TeamId = teamId;
            InviteToken = inviteToken;
        }
    }
}