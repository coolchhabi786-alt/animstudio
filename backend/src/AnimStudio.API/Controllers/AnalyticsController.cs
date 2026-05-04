using AnimStudio.AnalyticsModule.Application.Queries.GetEpisodeAnalytics;
using AnimStudio.AnalyticsModule.Application.Queries.GetTeamAnalytics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimStudio.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class AnalyticsController(ISender sender) : ControllerBase
{
    [HttpGet("episodes/{episodeId:guid}/analytics")]
    public async Task<IActionResult> GetEpisodeAnalytics(Guid episodeId, CancellationToken ct)
    {
        var result = await sender.Send(new GetEpisodeAnalyticsQuery(episodeId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("teams/{teamId:guid}/analytics")]
    public async Task<IActionResult> GetTeamAnalytics(Guid teamId, CancellationToken ct)
    {
        var result = await sender.Send(new GetTeamAnalyticsQuery(teamId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
