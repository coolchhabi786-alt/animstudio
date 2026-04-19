using AnimStudio.ContentModule.Application.Commands.CloneVoice;
using AnimStudio.ContentModule.Application.Commands.PreviewVoice;
using AnimStudio.ContentModule.Application.Commands.UpdateVoiceAssignments;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Queries.GetVoiceAssignments;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for the Voice Studio — voice assignment, TTS preview, and voice cloning.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class VoiceController(ISender mediator) : ControllerBase
{
    // ── GET /api/v1/episodes/{id}/voices ── Get all voice assignments ────────

    /// <summary>Returns all character voice assignments for the episode.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/voices")]
    [ProducesResponseType(typeof(List<VoiceAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVoiceAssignments(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetVoiceAssignmentsQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── PUT /api/v1/episodes/{id}/voices ── Batch update voice assignments ───

    /// <summary>Batch update voice assignments for all characters in the episode.</summary>
    [HttpPut("api/v{version:apiVersion}/episodes/{id:guid}/voices")]
    [ProducesResponseType(typeof(List<VoiceAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BatchUpdateVoices(
        Guid id,
        [FromBody] BatchUpdateVoicesRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateVoiceAssignmentsCommand(id, req.Assignments), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "CHARACTER_NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return Ok(result.Value);
    }

    // ── POST /api/v1/voices/preview ── TTS preview ──────────────────────────

    /// <summary>Generates a TTS audio preview. Returns a signed Blob URL with 60-second expiry.</summary>
    [HttpPost("api/v{version:apiVersion}/voices/preview")]
    [ProducesResponseType(typeof(VoicePreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewVoice(
        [FromBody] VoicePreviewRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new PreviewVoiceCommand(req.Text, req.VoiceName, req.Language), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── POST /api/v1/voices/clone ── Voice cloning (Studio tier only) ────────

    /// <summary>Initiates a voice cloning process from an audio sample. Requires Studio subscription tier.</summary>
    [HttpPost("api/v{version:apiVersion}/voices/clone")]
    [ProducesResponseType(typeof(VoiceCloneResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloneVoice(
        [FromBody] VoiceCloneRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CloneVoiceCommand(req.CharacterId, req.AudioSampleUrl), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "TIER_REQUIRED" => StatusCode(StatusCodes.Status403Forbidden,
                    new { error = result.Error, code = result.ErrorCode }),
                "CLONE_NOT_AVAILABLE" => BadRequest(
                    new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }
}
