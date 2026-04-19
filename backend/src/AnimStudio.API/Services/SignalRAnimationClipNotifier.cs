using AnimStudio.API.Hubs;
using AnimStudio.ContentModule.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Services;

/// <summary>
/// SignalR implementation of <see cref="IAnimationClipNotifier"/> — broadcasts
/// <c>ClipReady</c> to the team group on the shared <see cref="ProgressHub"/>.
/// </summary>
public sealed class SignalRAnimationClipNotifier(
    IHubContext<ProgressHub> hubContext) : IAnimationClipNotifier
{
    public Task PublishClipReadyAsync(
        Guid teamId,
        Guid episodeId,
        Guid clipId,
        int sceneNumber,
        int shotIndex,
        string clipUrl,
        CancellationToken ct = default)
    {
        return hubContext
            .Clients
            .Group($"team:{teamId}")
            .SendAsync(
                "ClipReady",
                new { episodeId, clipId, sceneNumber, shotIndex, clipUrl },
                ct);
    }
}
