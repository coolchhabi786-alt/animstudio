using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain
{
    /// <summary>
    /// Represents a user in the AnimStudio platform.
    /// </summary>
    public class User : AggregateRoot<Guid>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string DisplayName { get; set; }

        [Required]
        public string ExternalId { get; set; } // Maps to Entra External ID

        public DateTime? LastLoginAt { get; set; }

        // Navigation property for team memberships
        public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    }

    /// <summary>
    /// Represents a team.
    /// </summary>
    public class Team : AggregateRoot<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for team members
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

        // Navigation property for subscriptions
        public Subscription Subscription { get; set; }
    }

    /// <summary>
    /// Represents a member of a team.
    /// </summary>
    public class TeamMember
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public TeamRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public string InviteToken { get; set; }

        public DateTime? InviteExpiresAt { get; set; }

        public DateTime? InviteAcceptedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; }
        public User User { get; set; }
    }

    /// <summary>
    /// Represents a subscription plan.
    /// </summary>
    public class Plan : AggregateRoot<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string StripePriceId { get; set; }

        public int EpisodesPerMonth { get; set; }
        public int MaxCharacters { get; set; }
        public int MaxTeamMembers { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Represents a subscription tied to a team.
    /// </summary>
    public class Subscription : AggregateRoot<Guid>
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid PlanId { get; set; }

        public string StripeSubscriptionId { get; set; }
        public string StripeCustomerId { get; set; }

        [Required]
        public SubscriptionStatus Status { get; set; }

        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public int UsageEpisodesThisMonth { get; set; }
        public DateTime UsageResetAt { get; set; }

        // Navigation properties
        public Team Team { get; set; }
        public Plan Plan { get; set; }
    }

    /// <summary>
    /// Represents the status of a subscription.
    /// </summary>
    public enum SubscriptionStatus
    {
        Active,
        PastDue,
        Cancelled,
        Trialing
    }

    /// <summary>
    /// Represents roles within a team.
    /// </summary>
    public enum TeamRole
    {
        Owner = 1,
        Editor = 2,
        Viewer = 3
    }
}