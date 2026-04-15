using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AnimStudio.IdentityModule.Infrastructure.Repositories;

internal sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly IdentityDbContext _db;

    public SubscriptionRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Subscription?> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.TeamId == teamId, cancellationToken);
    }

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        return await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> GetAllActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Plans.Where(p => p.IsActive).OrderBy(p => p.Price).ToListAsync(cancellationToken);
        return list;
    }

    public async Task<Plan> GetPlanByStripePriceIdAsync(string stripePriceId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.StripePriceId == stripePriceId, cancellationToken);
        return plan!;
    }

    public async Task<Plan> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
        return plan!;
    }

    public async Task<Plan> GetDefaultPlanAsync(CancellationToken cancellationToken = default)
    {
        var plan = await _db.Plans.Where(p => p.IsDefault).FirstOrDefaultAsync(cancellationToken)
            ?? await _db.Plans.OrderBy(p => p.Price).FirstOrDefaultAsync(cancellationToken);
        return plan!;
    }

    public async Task AddSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _db.Subscriptions.AddAsync(subscription, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _db.Subscriptions.Update(subscription);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}

