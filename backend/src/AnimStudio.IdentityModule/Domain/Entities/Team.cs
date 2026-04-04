using System;
using System.Collections.Generic;
using System.Linq;
using AnimStudio.IdentityModule.Domain.Events;
using AnimStudio.IdentityModule.Domain.ValueObjects;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Entities
{
    /// <summary>
    /// Represents a team within the system.
    /// </summary>
    public class Team : AggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public Guid OwnerId { get; set; }
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public Subscription? Subscription { get; set; }

        // Required by EF Core
        private Team() { }

        /// <summary>Creates a new team and adds the owner as the first member.</summary>
        public static Team Create(Guid id, string name, Guid ownerId, string? logoUrl = null)
        {
            var team = new Team
            {
                Id = id,
                Name = name,
                OwnerId = ownerId,
                LogoUrl = logoUrl,
            };

            var ownerMember = new TeamMember
            {
                TeamId = id,
                UserId = ownerId,
                Role = TeamRole.Owner,
                JoinedAt = DateTimeOffset.UtcNow,
                InviteAcceptedAt = DateTimeOffset.UtcNow,
            };
            team.Members.Add(ownerMember);
            team.AddDomainEvent(new TeamCreated(id, name, ownerId));
            return team;
        }

        /// <summary>Invites a user (who must be registered) to the team.</summary>
        /// <returns>The invite token on success; an error message on failure.</returns>
        public Result<string> InviteMember(Guid userId, TeamRole role)
        {
            if (role == TeamRole.Owner)
                return Result<string>.Failure("Cannot assign Owner role via invitation.");

            var alreadyMember = Members.Any(m => m.UserId == userId && m.InviteAcceptedAt.HasValue);
            if (alreadyMember)
                return Result<string>.Failure("User is already a member of this team.");

            var pendingInvite = Members.FirstOrDefault(m => m.UserId == userId && !m.InviteAcceptedAt.HasValue);
            if (pendingInvite is not null)
                return Result<string>.Failure("A pending invitation already exists for this user.");

            var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            var member = new TeamMember
            {
                TeamId = Id,
                UserId = userId,
                Role = role,
                JoinedAt = DateTimeOffset.UtcNow,
                InviteToken = token,
                InviteExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            };
            Members.Add(member);
            AddDomainEvent(new TeamMemberInvited(userId, Id, token));
            return Result<string>.Success(token);
        }

        /// <summary>Accepts a pending invite identified by <paramref name="token"/>.</summary>
        public Result AcceptInvite(string token)
        {
            var member = Members.FirstOrDefault(m => m.InviteToken == token);
            if (member is null)
                return Result.Failure("Invite token not found.");

            if (member.InviteExpiresAt < DateTimeOffset.UtcNow)
                return Result.Failure("Invite token has expired.");

            if (member.InviteAcceptedAt.HasValue)
                return Result.Failure("Invite has already been accepted.");

            member.InviteAcceptedAt = DateTimeOffset.UtcNow;
            member.InviteToken = null;
            member.InviteExpiresAt = null;
            AddDomainEvent(new TeamMemberJoined(member.UserId, Id, member.InviteAcceptedAt.Value));
            return Result.Success();
        }
    }
}