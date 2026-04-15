using AnimStudio.ContentModule.Application.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// Read-only endpoints for episode templates and visual style presets.
/// All mutations are performed by ops tooling (seed data / admin panel).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class TemplatesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/templates — returns all active episode templates.
    /// Optional <c>?genre=Kids|Comedy|Drama|Horror|Romance|SciFi|Marketing|Fantasy</c> filter.
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTemplates([FromQuery] string? genre, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTemplatesQuery(genre), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    /// <summary>
    /// GET /api/v1/templates/{id} — returns a single template by primary key.
    /// </summary>
    [HttpGet("templates/{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTemplateQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// GET /api/v1/styles — returns all active visual style presets including Flux prompt suffixes.
    /// </summary>
    [HttpGet("styles")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStyles(CancellationToken ct)
    {
        var result = await mediator.Send(new GetStylePresetsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }
}
