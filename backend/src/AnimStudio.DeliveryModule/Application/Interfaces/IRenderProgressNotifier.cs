namespace AnimStudio.DeliveryModule.Application.Interfaces;

/// <summary>
/// Broadcasts render progress events to connected clients (e.g. via SignalR).
/// Implemented in the API layer; registered via DI.
/// </summary>
public interface IRenderProgressNotifier
{
    Task NotifyProgressAsync(Guid renderId, Guid episodeId, int percent, string stage, CancellationToken ct = default);
    Task NotifyCompleteAsync(Guid renderId, Guid episodeId, string? cdnUrl, string? srtUrl, double durationSeconds, CancellationToken ct = default);
    Task NotifyFailedAsync(Guid renderId, Guid episodeId, string errorMessage, CancellationToken ct = default);
}
