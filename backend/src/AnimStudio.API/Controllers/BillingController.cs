using AnimStudio.IdentityModule.Application.Commands.CancelSubscription;
using AnimStudio.IdentityModule.Application.Commands.UpdateSubscription;
using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Application.Queries.GetAllPlans;
using AnimStudio.IdentityModule.Application.Queries.GetSubscription;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AnimStudio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BillingController(ISender mediator, IStripeService stripeService) : ControllerBase
{
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllPlansQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500);
    }

    [HttpGet("subscription")]
    [Authorize]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        var teamIdClaim = User.FindFirst("animstudio_team_id")?.Value;
        if (!Guid.TryParse(teamIdClaim, out var teamId))
            return Unauthorized();

        var result = await mediator.Send(new GetSubscriptionQuery(teamId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost("subscription/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel([FromBody] CancelRequest request, CancellationToken ct)
    {
        var teamIdClaim = User.FindFirst("animstudio_team_id")?.Value;
        if (!Guid.TryParse(teamIdClaim, out var teamId))
            return Unauthorized();

        var result = await mediator.Send(new CancelSubscriptionCommand(teamId, request.Immediately), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Stripe webhook endpoint. Validates the signature and processes subscription lifecycle events.
    /// Must be excluded from the global rate limiter to avoid blocking Stripe.
    /// </summary>
    [HttpPost("webhook/stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        var eventResult = stripeService.HandleWebhookEvent(json, signature);
        if (!eventResult.IsSuccess)
            return BadRequest(new { error = eventResult.Error });

        var stripeEvent = eventResult.Value!;
        if (stripeEvent.Data.Object is not Subscription stripeSubscription)
            return Ok(); // unsupported event type — acknowledge immediately

        var command = new UpdateSubscriptionCommand(
            StripeCustomerId: stripeSubscription.CustomerId,
            StripeSubscriptionId: stripeSubscription.Id,
            StripePriceId: stripeSubscription.Items.Data.FirstOrDefault()?.Price.Id ?? string.Empty,
            Status: stripeSubscription.Status,
            CurrentPeriodEnd: new DateTimeOffset(stripeSubscription.CurrentPeriodEnd, TimeSpan.Zero),
            CancelAtPeriodEnd: stripeSubscription.CancelAtPeriodEnd);

        await mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var teamIdClaim = User.FindFirst("animstudio_team_id")?.Value;
        if (!Guid.TryParse(teamIdClaim, out var teamId))
            return Unauthorized();

        // Fetch team's Stripe customer ID via subscription query
        var subResult = await mediator.Send(new GetSubscriptionQuery(teamId), ct);
        if (!subResult.IsSuccess || subResult.Value is null)
            return BadRequest(new { error = "No active subscription record found for team." });

        var result = await stripeService.CreateCheckoutSessionAsync(
            subResult.Value.StripeCustomerId,
            request.PriceId,
            request.SuccessUrl,
            request.CancelUrl,
            ct);

        return result.IsSuccess
            ? Ok(new { url = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("portal")]
    [Authorize]
    public async Task<IActionResult> CreatePortal([FromBody] PortalRequest request, CancellationToken ct)
    {
        var teamIdClaim = User.FindFirst("animstudio_team_id")?.Value;
        if (!Guid.TryParse(teamIdClaim, out var teamId))
            return Unauthorized();

        var subResult = await mediator.Send(new GetSubscriptionQuery(teamId), ct);
        if (!subResult.IsSuccess || subResult.Value is null)
            return BadRequest(new { error = "No active subscription record found for team." });

        var result = await stripeService.CreatePortalSessionAsync(
            subResult.Value.StripeCustomerId,
            request.ReturnUrl,
            ct);

        return result.IsSuccess
            ? Ok(new { url = result.Value })
            : BadRequest(new { error = result.Error });
    }

    public sealed record CancelRequest(bool Immediately = false);
    public sealed record CheckoutRequest(string PriceId, string SuccessUrl, string CancelUrl);
    public sealed record PortalRequest(string ReturnUrl);
}
