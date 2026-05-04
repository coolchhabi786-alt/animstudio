using AnimStudio.AnalyticsModule.Application.Commands.MarkAllNotificationsRead;
using AnimStudio.AnalyticsModule.Application.Commands.MarkNotificationRead;
using AnimStudio.AnalyticsModule.Application.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimStudio.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class NotificationController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await sender.Send(new GetNotificationsQuery(userId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, result.Error);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await sender.Send(new MarkNotificationReadCommand(id, userId), ct);
        if (!result.IsSuccess)
            return result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error)
                 : result.ErrorCode == "FORBIDDEN" ? Forbid()
                 : StatusCode(500, result.Error);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await sender.Send(new MarkAllNotificationsReadCommand(userId), ct);
        return result.IsSuccess ? NoContent() : StatusCode(500, result.Error);
    }

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("animstudio_user_id");
        return Guid.TryParse(raw, out var id) ? id : Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}
