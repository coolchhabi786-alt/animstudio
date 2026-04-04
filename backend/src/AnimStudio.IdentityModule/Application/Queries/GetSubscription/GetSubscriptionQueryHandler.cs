using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetSubscription;

internal sealed class GetSubscriptionQueryHandler(
    ISubscriptionRepository subscriptionRepository) : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTeamIdAsync(request.TeamId, cancellationToken);
        if (subscription is null)
            return Result<SubscriptionDto>.Failure("No subscription found for this team.", "SUBSCRIPTION_NOT_FOUND");

        var dto = new SubscriptionDto(
            Id: subscription.Id,
            PlanName: subscription.Plan?.Name ?? string.Empty,
            Status: subscription.Status.ToString(),
            EpisodesUsedThisMonth: subscription.UsageEpisodesThisMonth,
            EpisodesPerMonth: subscription.Plan?.EpisodesPerMonth ?? 0,
            CurrentPeriodEnd: subscription.CurrentPeriodEnd,
            TrialEndsAt: subscription.TrialEndsAt,
            CancelAtPeriodEnd: subscription.CancelAtPeriodEnd,
            StripeCustomerId: subscription.StripeCustomerId ?? string.Empty);

        return Result<SubscriptionDto>.Success(dto);
    }
}
