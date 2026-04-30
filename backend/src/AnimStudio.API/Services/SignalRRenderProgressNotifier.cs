using AnimStudio.API.Hubs;
using AnimStudio.DeliveryModule.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Services;

/// <summary>
/// Routes render progress events to SignalR clients subscribed to the episode group.
/// </summary>
public sealed class SignalRRenderProgressNotifier(
    IHubContext<ProgressHub> hubContext,
    ILogger<SignalRRenderProgressNotifier> logger)
    : IRenderProgressNotifier
{
    public Task NotifyProgressAsync(
        Guid renderId, Guid episodeId, int percent, string stage, CancellationToken ct = default)
    {
        return BroadcastAsync("RenderProgress", new
        {
            renderId, episodeId, percent, stage,
        }, episodeId, ct);
    }

    public Task NotifyCompleteAsync(
        Guid renderId, Guid episodeId, string? cdnUrl, string? srtUrl,
        double durationSeconds, CancellationToken ct = default)
    {
        return BroadcastAsync("RenderComplete", new
        {
            renderId, episodeId, cdnUrl, srtUrl, durationSeconds,
        }, episodeId, ct);
    }

    public Task NotifyFailedAsync(
        Guid renderId, Guid episodeId, string errorMessage, CancellationToken ct = default)
    {
        return BroadcastAsync("RenderFailed", new
        {
            renderId, episodeId, errorMessage,
        }, episodeId, ct);
    }

    private async Task BroadcastAsync(string method, object payload, Guid episodeId, CancellationToken ct)
    {
        try
        {
            // Use episode group so any client listening on that episode's progress receives it.
            await hubContext.Clients.Group($"episode:{episodeId}")
                .SendAsync(method, payload, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error broadcasting {Method} for episode {Id}", method, episodeId);
        }
    }
}
