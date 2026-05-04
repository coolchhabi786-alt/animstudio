using System.Security.Claims;
using AnimStudio.ContentModule.Application.Commands.SaveTimeline;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Queries.GetTimeline;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for Phase 10 — Timeline Editor.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class TimelineController(ISender mediator) : ControllerBase
{
    // ── GET /api/v1/episodes/{id}/timeline ─────────────────────────────────

    /// <summary>Returns the timeline for an episode.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/timeline")]
    [ProducesResponseType(typeof(TimelineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTimelineQuery(id), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error, code = result.ErrorCode });
    }

    // ── PUT /api/v1/episodes/{id}/timeline ─────────────────────────────────

    /// <summary>Creates or replaces the timeline for an episode.</summary>
    [HttpPut("api/v{version:apiVersion}/episodes/{id:guid}/timeline")]
    [ProducesResponseType(typeof(TimelineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveTimeline(
        Guid id,
        [FromBody] SaveTimelineRequest body,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new SaveTimelineCommand(id, body, userId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error, code = result.ErrorCode });
    }
}
