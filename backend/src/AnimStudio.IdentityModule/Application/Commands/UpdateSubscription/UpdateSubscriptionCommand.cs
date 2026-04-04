using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.UpdateSubscription;

/// <summary>
/// Fired by the Stripe webhook handler when a subscription is created, updated, or renewed.
/// Updates the local subscription record with the latest Stripe state.
/// </summary>
public sealed record UpdateSubscriptionCommand(
    string StripeCustomerId,
    string StripeSubscriptionId,
    string StripePriceId,
    string Status,
    DateTimeOffset CurrentPeriodEnd,
    bool CancelAtPeriodEnd) : IRequest<Result<bool>>;
