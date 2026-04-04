using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace AnimStudio.IdentityModule.Infrastructure.Services;

internal sealed class StripeService : IStripeService
{
    private readonly CustomerService _customers;
    private readonly Stripe.Checkout.SessionService _sessions;
    private readonly Stripe.BillingPortal.SessionService _portalSessions;
    private readonly SubscriptionService _subscriptions;
    private readonly string _webhookSecret;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _logger = logger;
        var apiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
        _webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        StripeConfiguration.ApiKey = apiKey;
        _customers = new CustomerService();
        _sessions = new Stripe.Checkout.SessionService();
        _portalSessions = new Stripe.BillingPortal.SessionService();
        _subscriptions = new SubscriptionService();
    }

    public async Task<Result<string>> CreateCustomerAsync(
        string email, string teamName, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = teamName,
                Metadata = new Dictionary<string, string> { ["source"] = "animstudio" },
            };
            var customer = await _customers.CreateAsync(options, cancellationToken: cancellationToken);
            return Result<string>.Success(customer.Id);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating customer for {Email}", email);
            return Result<string>.Failure(ex.StripeError?.Message ?? ex.Message, "STRIPE_ERROR");
        }
    }

    public async Task<Result<string>> CreateCheckoutSessionAsync(
        string customerId, string priceId, string successUrl, string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                Mode = "subscription",
                LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                AllowPromotionCodes = true,
            };
            var session = await _sessions.CreateAsync(options, cancellationToken: cancellationToken);
            return Result<string>.Success(session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session for customer {CustomerId}", customerId);
            return Result<string>.Failure(ex.StripeError?.Message ?? ex.Message, "STRIPE_ERROR");
        }
    }

    public async Task<Result<string>> CreatePortalSessionAsync(
        string customerId, string returnUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions { Customer = customerId, ReturnUrl = returnUrl };
            var session = await _portalSessions.CreateAsync(options, cancellationToken: cancellationToken);
            return Result<string>.Success(session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating portal session for customer {CustomerId}", customerId);
            return Result<string>.Failure(ex.StripeError?.Message ?? ex.Message, "STRIPE_ERROR");
        }
    }

    public async Task<Result<bool>> CancelSubscriptionAsync(
        string stripeSubscriptionId, bool cancelImmediately = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancelImmediately)
                await _subscriptions.CancelAsync(stripeSubscriptionId, cancellationToken: cancellationToken);
            else
                await _subscriptions.UpdateAsync(stripeSubscriptionId,
                    new SubscriptionUpdateOptions { CancelAtPeriodEnd = true },
                    cancellationToken: cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error cancelling subscription {Id}", stripeSubscriptionId);
            return Result<bool>.Failure(ex.StripeError?.Message ?? ex.Message, "STRIPE_ERROR");
        }
    }

    public Result<Event> HandleWebhookEvent(string json, string stripeSignature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
            return Result<Event>.Success(stripeEvent);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature");
            return Result<Event>.Failure("Invalid webhook signature.", "INVALID_SIGNATURE");
        }
    }
}
