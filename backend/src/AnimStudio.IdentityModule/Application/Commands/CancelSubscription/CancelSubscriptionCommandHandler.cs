using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.CancelSubscription;

internal sealed class CancelSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IStripeService stripeService,
    ICacheService cacheService) : IRequestHandler<CancelSubscriptionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTeamIdAsync(request.TeamId, cancellationToken);
        if (subscription is null)
            return Result<bool>.Failure("Active subscription not found for this team.");

        if (subscription.StripeSubscriptionId is null)
            return Result<bool>.Failure("Subscription is not linked to a Stripe subscription.");

        var stripeResult = await stripeService.CancelSubscriptionAsync(
            subscription.StripeSubscriptionId,
            cancelImmediately: request.Immediately,
            cancellationToken);

        if (!stripeResult.IsSuccess)
            return Result<bool>.Failure(stripeResult.Error!);

        subscription.Cancel(immediately: request.Immediately);
        await subscriptionRepository.UpdateSubscriptionAsync(subscription, cancellationToken);

        await cacheService.RemoveAsync($"subscription:{request.TeamId}", cancellationToken);

        return Result<bool>.Success(true);
    }
}
