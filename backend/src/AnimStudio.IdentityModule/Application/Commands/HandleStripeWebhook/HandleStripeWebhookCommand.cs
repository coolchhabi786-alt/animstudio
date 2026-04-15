using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.HandleStripeWebhook;

/// <summary>
/// Processes an incoming Stripe webhook event.
/// The controller is responsible for reading the raw request body and the
/// <c>Stripe-Signature</c> header before sending this command.
/// </summary>
public sealed record HandleStripeWebhookCommand(
    string Json,
    string StripeSignature) : IRequest<Result<bool>>;
