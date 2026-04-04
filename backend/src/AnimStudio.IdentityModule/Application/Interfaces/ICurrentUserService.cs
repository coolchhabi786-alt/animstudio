namespace AnimStudio.IdentityModule.Application.Interfaces;

/// <summary>Provides current authenticated user identity to application services.</summary>
public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    string GetCurrentUserEmail();
    /// <summary>Returns the teamId from the X-Team-Id claim or header, or Guid.Empty if not present.</summary>
    Guid GetCurrentTeamId();
    bool IsAuthenticated { get; }
}
