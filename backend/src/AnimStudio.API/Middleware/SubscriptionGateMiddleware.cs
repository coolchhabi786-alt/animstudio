using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;

namespace AnimStudio.API.Middleware;

/// <summary>
/// Blocks requests to subscription-gated API paths when the team's subscription
/// is not in an active/trialing state. Exempt paths (auth, billing, health) pass through.
/// In Development the check is skipped entirely so all endpoints are testable without
/// a Stripe subscription.
/// </summary>
public sealed class SubscriptionGateMiddleware(
    RequestDelegate next,
    IWebHostEnvironment env,
    ILogger<SubscriptionGateMiddleware> logger)
{
    private static readonly string[] ExemptPrefixes =
    [
        "/api/auth",
        "/api/billing",
        "/health",
        "/hangfire",
        "/hubs",
        "/swagger",
        "/.well-known",  // Chrome DevTools probes and ACME challenge endpoints 
    ];

    public async Task InvokeAsync(HttpContext context, ISubscriptionRepository subscriptionRepository)
    {
        // ── Dev bypass ────────────────────────────────────────────────────────
        // In Development all endpoints are open so the full flow can be tested
        // without a real Stripe subscription. Remove/disable for staging/prod.
        if (env.IsDevelopment())
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (ExemptPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var teamIdClaim = context.User?.FindFirst("animstudio_team_id")?.Value;
        if (teamIdClaim is null || !Guid.TryParse(teamIdClaim, out var teamId))
        {
            await next(context);
            return;
        }

        var subscription = await subscriptionRepository.GetByTeamIdAsync(teamId, context.RequestAborted);
        if (subscription is null ||
            (subscription.Status != SubscriptionStatus.Active &&
             subscription.Status != SubscriptionStatus.Trialing))
        {
            logger.LogWarning("Access blocked for team {TeamId}: subscription status {Status}",
                teamId, subscription?.Status.ToString() ?? "None");

            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Subscription Required",
                detail = "Your subscription is inactive. Please update your billing details.",
                status = 402,
            });
            return;
        }

        await next(context);
    }
}
