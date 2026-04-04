using System;
using AnimStudio.IdentityModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Entities
{
    /// <summary>
    /// Represents a user within the system.
    /// </summary>
    public class User : AggregateRoot<Guid>
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }

        // Required by EF Core
        private User() { }

        /// <summary>Creates a new user and raises <see cref="UserRegistered"/>.</summary>
        public static User Create(Guid id, string externalId, string email, string displayName, string? avatarUrl = null)
        {
            var user = new User
            {
                Id = id,
                ExternalId = externalId,
                Email = email,
                DisplayName = displayName,
                AvatarUrl = avatarUrl,
            };
            user.AddDomainEvent(new UserRegistered(id, email));
            return user;
        }

        /// <summary>Updates display name and avatar URL.</summary>
        public void UpdateProfile(string displayName, string? avatarUrl)
        {
            DisplayName = displayName;
            AvatarUrl = avatarUrl;
        }

        /// <summary>Records the last login timestamp.</summary>
        public void RecordLogin() => LastLoginAt = DateTimeOffset.UtcNow;
    }
}