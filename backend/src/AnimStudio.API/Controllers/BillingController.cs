using AnimStudio.IdentityModule.Application.Commands.CancelSubscription;
using AnimStudio.IdentityModule.Application.Commands.HandleStripeWebhook;
using AnimStudio.IdentityModule.Application.Queries.GetAllPlans;
using AnimStudio.IdentityModule.Application.Queries.GetSubscription;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AnimStudio.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billing")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class BillingController(ISender mediator) : ControllerBase
{
    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        var teamIdClaim = User.FindFirst("animstudio_team_id")?.Value;
        if (!Guid.TryParse(teamIdClaim, out var teamId))
            return Unauthorized();

        var result = await mediator.Send(new GetSubscriptionQuery(teamId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllPlansQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpDelete("subscription")]
    public async Task<IActionResult> CancelSubscription(CancellationToken ct)
    {
        var teamIdClaim = User.FindFirst("animstudio_team_id")?.Value;
        if (!Guid.TryParse(teamIdClaim, out var teamId))
            return Unauthorized();

        var result = await mediator.Send(new CancelSubscriptionCommand(teamId), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        // Stripe signature validation requires the raw, unmodified request body.
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
            return BadRequest("Empty request body.");

        var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(stripeSignature))
            return BadRequest("Missing Stripe-Signature header.");

        var result = await mediator.Send(
            new HandleStripeWebhookCommand(json, stripeSignature), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "INVALID_WEBHOOK_SIGNATURE")
                return BadRequest(new { error = "Invalid webhook signature." });

            return Ok(); // Acknowledge receipt; idempotent retry will reconcile later.
        }

        return Ok();
    }
}
