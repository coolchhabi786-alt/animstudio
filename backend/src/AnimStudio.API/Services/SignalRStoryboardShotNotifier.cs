using AnimStudio.API.Hubs;
using AnimStudio.ContentModule.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Services;

/// <summary>
/// Implements <see cref="IStoryboardShotNotifier"/> using ASP.NET Core SignalR.
/// Broadcasts shot regeneration/style updates to the team's connected clients
/// via <see cref="ProgressHub"/> (team groups are shared across episode, character
/// and storyboard events so a single <c>JoinTeamGroup</c> covers everything).
/// </summary>
public sealed class SignalRStoryboardShotNotifier(
    IHubContext<ProgressHub> hubContext) : IStoryboardShotNotifier
{
    /// <inheritdoc/>
    public Task NotifyShotUpdatedAsync(
        Guid teamId,
        Guid storyboardId,
        Guid episodeId,
        Guid shotId,
        string? imageUrl,
        int regenerationCount,
        CancellationToken ct = default)
    {
        return hubContext
            .Clients
            .Group($"team:{teamId}")
            .SendAsync(
                "ShotUpdated",
                new { shotId, storyboardId, episodeId, imageUrl, regenerationCount },
                ct);
    }
}
