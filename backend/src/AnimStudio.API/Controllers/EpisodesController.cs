using AnimStudio.ContentModule.Application.Commands.CreateEpisode;
using Asp.Versioning;
using AnimStudio.ContentModule.Application.Commands.DispatchEpisodeJob;
using AnimStudio.ContentModule.Application.Queries;
using AnimStudio.ContentModule.Domain;
using AnimStudio.IdentityModule.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class EpisodesController(ISender mediator) : ControllerBase
{
    [HttpPost("api/v{version:apiVersion}/projects/{projectId:guid}/episodes")]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateEpisodeRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateEpisodeCommand(projectId, req.Name, req.Idea ?? "", req.Style ?? "", req.TemplateId), ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return StatusCode(201, result.Value);
    }

    [HttpGet("api/v{version:apiVersion}/projects/{projectId:guid}/episodes")]
    public async Task<IActionResult> List(Guid projectId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetEpisodesQuery(projectId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetEpisodeQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/saga")]
    public async Task<IActionResult> GetSagaState(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSagaStateQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid id, [FromBody] DispatchRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<JobType>(req.JobType, ignoreCase: true, out var jobType))
            return BadRequest($"Invalid job type '{req.JobType}'. Valid values: {string.Join(", ", Enum.GetNames<JobType>())}");

        var result = await mediator.Send(new DispatchEpisodeJobCommand(id, jobType), ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Accepted(result.Value);
    }

    [HttpDelete("api/v{version:apiVersion}/episodes/{id:guid}")]
    [Authorize(Policy = "RequireTeamEditor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Soft-delete handled at the Episode aggregate. Not yet exposed as a separate command — Phase 3.
        return StatusCode(501, "Episode deletion will be available in Phase 3.");
    }

    public sealed record CreateEpisodeRequest(string Name, string? Idea, string? Style, Guid? TemplateId);
    public sealed record DispatchRequest(string JobType);
}
