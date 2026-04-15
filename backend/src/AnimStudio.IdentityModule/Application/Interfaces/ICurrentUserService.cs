using System;

namespace AnimStudio.IdentityModule.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid GetCurrentUserId();
        string GetCurrentUserEmail();
        Guid GetCurrentTeamId();
    }
}