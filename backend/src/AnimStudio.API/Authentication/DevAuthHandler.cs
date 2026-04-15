using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AnimStudio.API.Authentication;

/// <summary>
/// Development-only authentication handler.
/// Auto-authenticates every request as a seeded dev user so you can
/// exercise all API endpoints locally without an Azure AD app registration.
///
/// NEVER registered outside of Development environment.
/// </summary>
public sealed class DevAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevAuth";

    // Fixed dev identifiers — matches the seeded user in local DB
    public static readonly Guid DevUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid DevTeamId = Guid.Parse("C0000001-0000-0000-0000-000000000001");
    public const string DevUserEmail = "dev@animstudio.local";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // If a real Bearer token is present let the normal JWT handler deal with it
        if (Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, DevUserId.ToString()),
            new Claim("animstudio_user_id",      DevUserId.ToString()),
            new Claim(ClaimTypes.Email,           DevUserEmail),
            new Claim(ClaimTypes.Name,            "Dev User"),
            new Claim("animstudio_team_id",       DevTeamId.ToString()),
            new Claim("roles",                    "AnimStudio.Admin"),
        };

        var identity  = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
