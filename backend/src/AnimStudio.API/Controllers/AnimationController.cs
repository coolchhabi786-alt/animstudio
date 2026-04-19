using System.Security.Claims;
using AnimStudio.ContentModule.Application.Commands.ApproveAnimation;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Queries.GetAnimationClips;
using AnimStudio.ContentModule.Application.Queries.GetAnimationClipSignedUrl;
using AnimStudio.ContentModule.Application.Queries.GetAnimationEstimate;
using AnimStudio.ContentModule.Domain.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for the Animation Studio — cost estimate, approval/enqueue,
/// clip list, and signed clip URL retrieval.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class AnimationController(ISender mediator) : ControllerBase
{
    // ── GET /api/v1/episodes/{id}/animation/estimate ────────────────────────

    /// <summary>Returns an itemised cost estimate for animating the episode.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/animation/estimate")]
    [ProducesResponseType(typeof(AnimationEstimateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstimate(
        Guid id,
        [FromQuery] AnimationBackend? backend,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetAnimationEstimateQuery(id, backend ?? AnimationBackend.Kling), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── POST /api/v1/episodes/{id}/animation ────────────────────────────────

    /// <summary>Approves the estimate and enqueues an animation job.</summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/animation")]
    [ProducesResponseType(typeof(AnimationJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApproveAnimation(
        Guid id,
        [FromBody] ApproveAnimationRequest req,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(
            new ApproveAnimationCommand(id, req.Backend, userId), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "STORYBOARD_NOT_READY" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                "STORYBOARD_EMPTY" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                "ANIMATION_ALREADY_ACTIVE" => Conflict(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    // ── GET /api/v1/episodes/{id}/animation ─────────────────────────────────

    /// <summary>Lists all animation clips for the episode.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/animation")]
    [ProducesResponseType(typeof(List<AnimationClipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClips(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAnimationClipsQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── GET /api/v1/episodes/{id}/animation/clips/{clipId} ──────────────────

    /// <summary>Issues a short-lived signed Blob URL for a single rendered clip.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/animation/clips/{clipId:guid}")]
    [ProducesResponseType(typeof(SignedClipUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClipSignedUrl(
        Guid id, Guid clipId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAnimationClipSignedUrlQuery(id, clipId), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }
}
