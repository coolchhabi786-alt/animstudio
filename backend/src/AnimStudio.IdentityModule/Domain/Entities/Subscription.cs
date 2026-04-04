using System;
using AnimStudio.IdentityModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Entities
{
    /// <summary>
    /// Represents a subscription tied to a team and plan.
    /// </summary>
    public class Subscription : AggregateRoot<Guid>
    {
        public Guid TeamId { get; set; }
        public Guid PlanId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public DateTimeOffset CurrentPeriodStart { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset CurrentPeriodEnd { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
        public DateTimeOffset? TrialEndsAt { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public int UsageEpisodesThisMonth { get; set; }
        public DateTimeOffset UsageResetAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);

        // Navigation properties
        public Team Team { get; set; } = null!;
        public Plan Plan { get; set; } = null!;

        /// <summary>Creates a new active subscription on a free/trial plan.</summary>
        public static Subscription Create(Guid id, Guid teamId, Guid planId,
            string? stripeCustomerId = null, DateTimeOffset? trialEndsAt = null)
        {
            var status = trialEndsAt.HasValue ? SubscriptionStatus.Trialing : SubscriptionStatus.Active;
            return new Subscription
            {
                Id = id,
                TeamId = teamId,
                PlanId = planId,
                StripeCustomerId = stripeCustomerId,
                Status = status,
                TrialEndsAt = trialEndsAt,
                CurrentPeriodStart = DateTimeOffset.UtcNow,
                CurrentPeriodEnd = DateTimeOffset.UtcNow.AddMonths(1),
                UsageResetAt = DateTimeOffset.UtcNow.AddMonths(1),
            };
        }

        /// <summary>Applies Stripe webhook data to synchronize local state.</summary>
        public void UpdateFromStripe(string stripeSubscriptionId, Guid planId,
            SubscriptionStatus status, DateTimeOffset currentPeriodEnd, bool cancelAtPeriodEnd)
        {
            StripeSubscriptionId = stripeSubscriptionId;
            PlanId = planId;
            Status = status;
            CurrentPeriodEnd = currentPeriodEnd;
            CancelAtPeriodEnd = cancelAtPeriodEnd;

            if (status == SubscriptionStatus.Active || status == SubscriptionStatus.Trialing)
                TrialEndsAt = status == SubscriptionStatus.Trialing ? currentPeriodEnd : null;

            AddDomainEvent(new SubscriptionActivated(Id, TeamId, planId));
        }

        /// <summary>Cancels the subscription, either at period end or immediately.</summary>
        public void Cancel(bool immediately = false)
        {
            if (immediately)
            {
                Status = SubscriptionStatus.Cancelled;
                CancelAtPeriodEnd = false;
            }
            else
            {
                CancelAtPeriodEnd = true;
            }
            AddDomainEvent(new SubscriptionCancelled(Id, TeamId, DateTimeOffset.UtcNow));
        }

        /// <summary>Resets the episode usage counter at the start of a new billing period.</summary>
        public void ResetMonthlyUsage()
        {
            UsageEpisodesThisMonth = 0;
            UsageResetAt = DateTimeOffset.UtcNow.AddMonths(1);
        }
    }

    public enum SubscriptionStatus
    {
        Active,
        Trialing,
        PastDue,
        Cancelled,
    }
}