using AnimStudio.IdentityModule.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AnimStudio.IdentityModule.Infrastructure.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid GetCurrentUserId()
    {
        var value = User?.FindFirstValue("animstudio_user_id")
                    ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public string GetCurrentUserEmail()
        => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public Guid GetCurrentTeamId()
    {
        var value = User?.FindFirstValue("animstudio_team_id");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
