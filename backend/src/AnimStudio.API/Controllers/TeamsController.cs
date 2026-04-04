using AnimStudio.IdentityModule.Application.Commands.AcceptTeamInvite;
using AnimStudio.IdentityModule.Application.Commands.CreateTeam;
using AnimStudio.IdentityModule.Application.Commands.InviteTeamMember;
using AnimStudio.IdentityModule.Application.Queries.GetTeam;
using AnimStudio.IdentityModule.Application.Queries.GetTeamMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TeamsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("animstudio_user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(
            new CreateTeamCommand(userId, request.Name, request.LogoUrl), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { teamId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeamQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeamMembersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost("{id:guid}/invites")]
    public async Task<IActionResult> InviteMember(
        Guid id,
        [FromBody] InviteMemberRequest request,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("animstudio_user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(
            new InviteTeamMemberCommand(id, userId, request.Email, request.Role), ct);
        return result.IsSuccess ? Ok(new { inviteToken = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("invites/accept")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvite(
        [FromBody] AcceptInviteRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new AcceptTeamInviteCommand(request.Token), ct);
        return result.IsSuccess ? Ok(new { teamId = result.Value }) : BadRequest(new { error = result.Error });
    }

    public sealed record CreateTeamRequest(string Name, string? LogoUrl);
    public sealed record InviteMemberRequest(string Email, string Role);
    public sealed record AcceptInviteRequest(string Token);
}
