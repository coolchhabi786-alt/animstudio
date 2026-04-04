using AnimStudio.IdentityModule.Application.Commands.RegisterUser;
using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Application.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimStudio.API.Controllers;

/// <summary>
/// Authentication endpoints. Returns the currently authenticated user.
/// On first sign-in the user is automatically registered (upserted) from JWT claims.
/// In local Development the DevAuthHandler provides synthetic claims so no Entra
/// configuration is required.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AuthController(ISender mediator, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>GET /api/auth/me — Return the current user's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await mediator.Send(new GetCurrentUserQuery(userId), ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        // User not found locally → first login → register from claims
        var email = currentUser.GetCurrentUserEmail();
        var externalId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")
                      ?? userId.ToString();
        var displayName = User.FindFirstValue(ClaimTypes.Name)
                       ?? email.Split('@')[0];

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var registerResult = await mediator.Send(
            new RegisterUserCommand(externalId, email, displayName, AvatarUrl: null), ct);

        if (!registerResult.IsSuccess)
            return StatusCode(500, new { error = registerResult.Error });

        var refetch = await mediator.Send(new GetCurrentUserQuery(registerResult.Value!), ct);
        return refetch.IsSuccess ? Ok(refetch.Value) : StatusCode(500, new { error = refetch.Error });
    }
}
