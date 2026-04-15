using AnimStudio.IdentityModule.Domain.Entities;

namespace AnimStudio.IdentityModule.Domain.Interfaces;

/// <summary>Repository contract for <see cref="Subscription"/> and <see cref="Plan"/> persistence.</summary>
public interface ISubscriptionRepository
{
    Task<Subscription?> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Plan>> GetAllActivePlansAsync(CancellationToken cancellationToken = default);
    Task<Plan> GetPlanByStripePriceIdAsync(string stripePriceId, CancellationToken cancellationToken = default);
    Task<Plan> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<Plan> GetDefaultPlanAsync(CancellationToken cancellationToken = default);
    Task AddSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
