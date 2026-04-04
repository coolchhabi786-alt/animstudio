using AnimStudio.IdentityModule.Application.Commands.RegisterUser;
using AnimStudio.IdentityModule.Application.Queries.GetCurrentUser;
using AnimStudio.IdentityModule.Application.Commands.UpdateUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(ISender mediator) : ControllerBase
{
    /// <summary>Registers or retrieves the current authenticated user (idempotent).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { userId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Returns the profile of the authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("animstudio_user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new GetCurrentUserQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Updates display name and avatar of the authenticated user.</summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserProfileRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("animstudio_user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(
            new UpdateUserProfileCommand(userId, request.DisplayName, request.AvatarUrl), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    public sealed record UpdateUserProfileRequest(string DisplayName, string? AvatarUrl);
}
