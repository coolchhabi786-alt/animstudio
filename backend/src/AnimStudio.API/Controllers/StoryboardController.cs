using AnimStudio.ContentModule.Application.Commands.GenerateStoryboard;
using AnimStudio.ContentModule.Application.Commands.RegenerateShot;
using AnimStudio.ContentModule.Application.Commands.UpdateShotStyle;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Queries.GetStoryboard;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for the Storyboard Studio — shot planning, regeneration,
/// and per-shot style overrides.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class StoryboardController(ISender mediator) : ControllerBase
{
    // ── POST /api/v1/episodes/{id}/storyboard ── Enqueue storyboard planning ──

    /// <summary>Enqueues a StoryboardPlan job for the episode. Returns 202 + jobId.</summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/storyboard")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate(
        Guid id,
        [FromBody] GenerateStoryboardRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateStoryboardCommand(id, req.DirectorNotes), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "SCRIPT_NOT_READY" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    // ── GET /api/v1/episodes/{id}/storyboard ── Retrieve the storyboard ────────

    /// <summary>Returns the storyboard and all shots for an episode, or 404 if none exists.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/storyboard")]
    [ProducesResponseType(typeof(StoryboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStoryboardQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        if (result.Value is null)
            return NotFound(new { error = "No storyboard has been generated for this episode yet.", code = "NO_STORYBOARD" });

        return Ok(result.Value);
    }

    // ── POST /api/v1/shots/{shotId}/regenerate ── Re-queue a shot ──────────────

    /// <summary>Re-queues a single shot for regeneration, optionally with a style override.</summary>
    [HttpPost("api/v{version:apiVersion}/shots/{shotId:guid}/regenerate")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerateShot(
        Guid shotId,
        [FromBody] RegenerateShotRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RegenerateShotCommand(shotId, req.StyleOverride), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    // ── PUT /api/v1/shots/{shotId}/style ── Change style + re-render ───────────

    /// <summary>Persists the user's style override on a shot and re-queues it for rendering.</summary>
    [HttpPut("api/v{version:apiVersion}/shots/{shotId:guid}/style")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShotStyle(
        Guid shotId,
        [FromBody] UpdateShotStyleRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateShotStyleCommand(shotId, req.StyleOverride), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }
}
