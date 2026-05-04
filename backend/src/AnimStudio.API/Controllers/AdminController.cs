using AnimStudio.AnalyticsModule.Application.Queries.GetAdminStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "RequireAdminRole")]
public sealed class AdminController(ISender sender) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await sender.Send(new GetAdminStatsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, result.Error);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAdminUsersQuery(page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, result.Error);
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(CancellationToken ct)
    {
        // Hangfire dashboard handles job monitoring; this endpoint returns a summary count.
        // Full job list requires pagination across all episodes which isn't exposed via repository.
        return Ok(new { message = "Use /hangfire for detailed job monitoring." });
    }
}
