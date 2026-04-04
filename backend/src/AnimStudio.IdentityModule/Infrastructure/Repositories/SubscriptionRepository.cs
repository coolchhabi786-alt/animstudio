using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.IdentityModule.Infrastructure.Repositories;

internal sealed class SubscriptionRepository(IdentityDbContext db) : ISubscriptionRepository
{
    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Subscription?> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        => await db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.TeamId == teamId, cancellationToken);

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId, CancellationToken cancellationToken = default)
        => await db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);

    public async Task<Plan?> GetPlanByStripePriceIdAsync(
        string stripePriceId, CancellationToken cancellationToken = default)
        => await db.Plans.FirstOrDefaultAsync(p => p.StripePriceId == stripePriceId, cancellationToken);

    public async Task<IReadOnlyList<Plan>> GetAllActivePlansAsync(CancellationToken cancellationToken = default)
        => await db.Plans.OrderBy(p => p.Price).ToListAsync(cancellationToken);

    public async Task<Plan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
        => await db.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

    public async Task<Plan?> GetDefaultPlanAsync(CancellationToken cancellationToken = default)
        => await db.Plans.OrderBy(p => p.Price).FirstOrDefaultAsync(cancellationToken);

    public async Task AddSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await db.Subscriptions.AddAsync(subscription, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        db.Subscriptions.Update(subscription);
        await db.SaveChangesAsync(cancellationToken);
    }
}
