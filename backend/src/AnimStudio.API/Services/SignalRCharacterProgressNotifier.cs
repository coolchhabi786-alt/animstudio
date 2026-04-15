using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Services;

/// <summary>
/// Implements <see cref="ICharacterProgressNotifier"/> using ASP.NET Core SignalR.
/// Broadcasts character training updates to the team's connected clients via
/// <see cref="CharacterProgressHub"/>.
/// </summary>
public sealed class SignalRCharacterProgressNotifier(
    IHubContext<CharacterProgressHub> hubContext) : ICharacterProgressNotifier
{
    /// <inheritdoc/>
    public Task NotifyAsync(
        Guid teamId,
        Guid characterId,
        string status,
        int progressPercent,
        string stage,
        CancellationToken ct = default)
    {
        return hubContext
            .Clients
            .Group($"team:{teamId}")
            .SendAsync(
                "CharacterTrainingUpdate",
                new { characterId, status, progressPercent, stage },
                ct);
    }
}
