using System;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events
{
    /// <summary>
    /// Event triggered when a new user is registered.
    /// </summary>
    public class UserRegistered : IDomainEvent
    {
        /// <summary>
        /// Gets the ID of the registered user.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// Gets the email of the registered user.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Initializes an instance of <see cref="UserRegistered"/>.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="email">The user's email.</param>
        public UserRegistered(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}