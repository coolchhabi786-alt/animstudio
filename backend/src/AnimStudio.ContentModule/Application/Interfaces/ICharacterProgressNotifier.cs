namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Port for pushing real-time character training updates to connected clients.
/// Implemented in AnimStudio.API using SignalR IHubContext.
/// </summary>
public interface ICharacterProgressNotifier
{
    /// <summary>
    /// Sends a training progress update to all clients in the given team group.
    /// </summary>
    /// <param name="teamId">Routes the message to the correct SignalR group.</param>
    /// <param name="characterId">The character whose status changed.</param>
    /// <param name="status">New training status string (e.g. "Training", "Ready").</param>
    /// <param name="progressPercent">0–100 percent for the current stage.</param>
    /// <param name="stage">Human-readable stage label.</param>
    /// <param name="ct">Cancellation token.</param>
    Task NotifyAsync(
        Guid teamId,
        Guid characterId,
        string status,
        int progressPercent,
        string stage,
        CancellationToken ct = default);
}
