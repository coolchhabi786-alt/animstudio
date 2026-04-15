using AnimStudio.ContentModule.Application.Commands.GenerateScript;
using AnimStudio.ContentModule.Application.Commands.RegenerateScript;
using AnimStudio.ContentModule.Application.Commands.SaveScript;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Queries.GetScript;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// REST endpoints for the Script Workshop — screenplay generation and editing.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class ScriptController(ISender mediator) : ControllerBase
{
    // ── POST /api/v1/episodes/{id}/script ── Enqueue script generation ─────────

    /// <summary>Enqueues an AI scriptwriting job for the episode. Returns 202 + jobId.</summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/script")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate(
        Guid id,
        [FromBody] GenerateScriptRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateScriptCommand(id, req.DirectorNotes), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "CHARACTERS_NOT_READY" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    // ── GET /api/v1/episodes/{id}/script ── Get current script ────────────────

    /// <summary>Returns the current script or 404 if none has been generated yet.</summary>
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/script")]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetScriptQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        if (result.Value is null)
            return NotFound(new { error = "No script has been generated for this episode yet.", code = "NO_SCRIPT" });

        return Ok(result.Value);
    }

    // ── PUT /api/v1/episodes/{id}/script ── Save manual edits ─────────────────

    /// <summary>
    /// Saves user edits to the screenplay. Marks the script as manually edited.
    /// Validates that all characters referenced in dialogue exist in the episode roster.
    /// </summary>
    [HttpPut("api/v{version:apiVersion}/episodes/{id:guid}/script")]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Save(
        Guid id,
        [FromBody] SaveScriptRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SaveScriptCommand(id, req.Screenplay), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "INVALID_CHARACTERS" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return Ok(result.Value);
    }

    // ── POST /api/v1/episodes/{id}/script/regenerate ── Re-enqueue with notes ──

    /// <summary>Re-enqueues a scriptwriting job with optional director notes. Returns 202 + jobId.</summary>
    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/script/regenerate")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Regenerate(
        Guid id,
        [FromBody] RegenerateScriptRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RegenerateScriptCommand(id, req.DirectorNotes), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "CHARACTERS_NOT_READY" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };
        }

        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }
}
