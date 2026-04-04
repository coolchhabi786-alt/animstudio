using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.UpdateSubscription;

internal sealed class UpdateSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    ICacheService cacheService) : IRequestHandler<UpdateSubscriptionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SubscriptionStatus>(request.Status, ignoreCase: true, out var status))
            return Result<bool>.Failure($"Unknown subscription status '{request.Status}'.");

        var subscription = await subscriptionRepository.GetByStripeSubscriptionIdAsync(
            request.StripeSubscriptionId, cancellationToken);

        if (subscription is null)
            return Result<bool>.Failure($"Subscription '{request.StripeSubscriptionId}' not found.");

        var plan = await subscriptionRepository.GetPlanByStripePriceIdAsync(
            request.StripePriceId, cancellationToken);

        if (plan is null)
            return Result<bool>.Failure($"No plan found for Stripe price '{request.StripePriceId}'.");

        subscription.UpdateFromStripe(
            stripeSubscriptionId: request.StripeSubscriptionId,
            planId: plan.Id,
            status: status,
            currentPeriodEnd: request.CurrentPeriodEnd,
            cancelAtPeriodEnd: request.CancelAtPeriodEnd);

        await subscriptionRepository.UpdateSubscriptionAsync(subscription, cancellationToken);

        await cacheService.RemoveAsync($"subscription:{subscription.TeamId}", cancellationToken);

        return Result<bool>.Success(true);
    }
}
