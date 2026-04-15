using AnimStudio.ContentModule.Application.Commands.CreateProject;
using Asp.Versioning;
using AnimStudio.ContentModule.Application.Commands.DeleteProject;
using AnimStudio.ContentModule.Application.Commands.UpdateProject;
using AnimStudio.ContentModule.Application.Queries;
using AnimStudio.IdentityModule.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class ProjectsController(ISender mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(new CreateProjectCommand(teamId, req.Name, req.Description ?? ""), ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var teamId = currentUser.GetCurrentTeamId();
        var result = await mediator.Send(new GetProjectsQuery(teamId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest req, CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        var result = await mediator.Send(new UpdateProjectCommand(id, userId, req.Name, req.Description ?? "", req.ThumbnailUrl), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireTeamEditor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        var result = await mediator.Send(new DeleteProjectCommand(id, userId), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return NoContent();
    }

    public sealed record CreateProjectRequest(string Name, string? Description);
    public sealed record UpdateProjectRequest(string Name, string? Description, string? ThumbnailUrl);
}


