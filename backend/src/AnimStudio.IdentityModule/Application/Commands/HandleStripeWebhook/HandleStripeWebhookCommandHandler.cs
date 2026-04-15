using AnimStudio.IdentityModule.Application.Commands.UpdateSubscription;
using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AnimStudio.IdentityModule.Application.Commands.HandleStripeWebhook;

internal sealed class HandleStripeWebhookCommandHandler(
    IStripeService stripeService,
    ISender mediator,
    ILogger<HandleStripeWebhookCommandHandler> logger)
    : IRequestHandler<HandleStripeWebhookCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var eventResult = stripeService.HandleWebhookEvent(request.Json, request.StripeSignature);
        if (!eventResult.IsSuccess)
        {
            logger.LogWarning("Stripe webhook signature validation failed: {Error}", eventResult.Error);
            return Result<bool>.Failure(eventResult.Error ?? "Invalid Stripe signature.", "INVALID_WEBHOOK_SIGNATURE");
        }

        var stripeEvent = eventResult.Value!;

        logger.LogInformation("Processing Stripe event {Type} id={Id}", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case Events.CustomerSubscriptionCreated:
            case Events.CustomerSubscriptionUpdated:
            case Events.InvoicePaymentSucceeded:
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                if (subscription is null) break;

                var result = await mediator.Send(new UpdateSubscriptionCommand(
                    StripeCustomerId: subscription.CustomerId,
                    StripeSubscriptionId: subscription.Id,
                    StripePriceId: subscription.Items.Data.FirstOrDefault()?.Price.Id ?? string.Empty,
                    Status: subscription.Status,
                    CurrentPeriodEnd: subscription.CurrentPeriodEnd,
                    CancelAtPeriodEnd: subscription.CancelAtPeriodEnd),
                    cancellationToken);

                if (!result.IsSuccess)
                    logger.LogWarning("UpdateSubscription failed for {SubscriptionId}: {Error}", subscription.Id, result.Error);

                break;
            }
            case Events.CustomerSubscriptionDeleted:
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                if (subscription is null) break;

                // Mark the subscription as canceled using the same UpdateSubscription path.
                var result = await mediator.Send(new UpdateSubscriptionCommand(
                    StripeCustomerId: subscription.CustomerId,
                    StripeSubscriptionId: subscription.Id,
                    StripePriceId: subscription.Items.Data.FirstOrDefault()?.Price.Id ?? string.Empty,
                    Status: "canceled",
                    CurrentPeriodEnd: subscription.CurrentPeriodEnd,
                    CancelAtPeriodEnd: false),
                    cancellationToken);

                if (!result.IsSuccess)
                    logger.LogWarning("CancelSubscription (via webhook) failed for {SubscriptionId}: {Error}", subscription.Id, result.Error);

                break;
            }
            default:
                logger.LogDebug("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                break;
        }

        return Result<bool>.Success(true);
    }
}
