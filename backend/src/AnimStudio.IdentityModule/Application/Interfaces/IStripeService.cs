using AnimStudio.SharedKernel;
using Stripe;

namespace AnimStudio.IdentityModule.Application.Interfaces;

/// <summary>Abstraction over Stripe API operations required by the IdentityModule.</summary>
public interface IStripeService
{
    /// <summary>Creates a Stripe Customer and returns the customer ID.</summary>
    Task<Result<string>> CreateCustomerAsync(
        string email, string teamName, CancellationToken cancellationToken = default);

    /// <summary>Creates a hosted Checkout Session and returns the redirect URL.</summary>
    Task<Result<string>> CreateCheckoutSessionAsync(
        string customerId, string priceId, string successUrl, string cancelUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a Customer Portal Session and returns the redirect URL.</summary>
    Task<Result<string>> CreatePortalSessionAsync(
        string customerId, string returnUrl, CancellationToken cancellationToken = default);

    /// <summary>Cancels a subscription — either at period end or immediately.</summary>
    Task<Result<bool>> CancelSubscriptionAsync(
        string stripeSubscriptionId, bool cancelImmediately = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and deserialises a Stripe webhook payload.
    /// Returns the <see cref="Event"/> on success; an error on invalid signature.
    /// </summary>
    Result<Event> HandleWebhookEvent(string json, string stripeSignature);





}
