using System;
using System.Text.RegularExpressions;

namespace AnimStudio.IdentityModule.Domain.ValueObjects
{
    /// <summary>
    /// Represents an email address.
    /// </summary>
    public record Email
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets the email address as a string.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="value">The email address.</param>
        /// <exception cref="ArgumentException">Thrown if the email is invalid.</exception>
        public Email(string value)
        {
            if (!IsValid(value))
            {
                throw new ArgumentException("Invalid email address.", nameof(value));
            }

            Value = value;
        }

        /// <summary>
        /// Validates the format of an email address.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email is valid; otherwise, false.</returns>
        public static bool IsValid(string email)
        {
            return EmailRegex.IsMatch(email);
        }
    }
}