using System.Security.Claims;
using AnimStudio.API.Hosted;
using AnimStudio.DeliveryModule.Application.Commands.StartRender;
using AnimStudio.DeliveryModule.Application.DTOs;
using AnimStudio.DeliveryModule.Application.Queries.GetRender;
using AnimStudio.DeliveryModule.Application.Queries.GetRenderHistory;
using Asp.Versioning;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for Phase 9 — post-production rendering and delivery.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class RenderController(
    ISender mediator,
    IBackgroundJobClient backgroundJobs) : ControllerBase
{
    // ── POST /api/v1/episodes/{id}/renders ─────────────────────────────────

    /// <summary>Starts a new render job for the episode.</summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/renders")]
    [ProducesResponseType(typeof(RenderDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartRender(
        Guid id,
        [FromBody] StartRenderRequest req,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(
            new StartRenderCommand(id, req.AspectRatio, userId), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error, code = result.ErrorCode });

        // Enqueue Hangfire job to process the render asynchronously.
        backgroundJobs.Enqueue<RenderHangfireProcessor>(
            x => x.ProcessAsync(result.Value!.Id, CancellationToken.None));

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    // ── GET /api/v1/episodes/{id}/renders ──────────────────────────────────

    /// <summary>Returns all renders for the episode, newest first.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/renders")]
    [ProducesResponseType(typeof(List<RenderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRenderHistory(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRenderHistoryQuery(id), ct);
        return Ok(result.Value);
    }

    // ── GET /api/v1/renders/{id} ────────────────────────────────────────────

    /// <summary>Returns a single render by its ID.</summary>
    [HttpGet("api/v{version:apiVersion}/renders/{id:guid}")]
    [ProducesResponseType(typeof(RenderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRender(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRenderQuery(id), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error, code = result.ErrorCode });
    }
}
