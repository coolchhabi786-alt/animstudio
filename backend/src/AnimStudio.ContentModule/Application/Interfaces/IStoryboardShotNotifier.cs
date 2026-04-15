namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Port for pushing real-time storyboard shot updates to connected clients.
/// Implemented in AnimStudio.API using SignalR IHubContext&lt;ProgressHub&gt;.
///
/// Message method: <c>ShotUpdated</c><br/>
/// Group: <c>team:{teamId}</c>
/// </summary>
public interface IStoryboardShotNotifier
{
    /// <summary>
    /// Broadcasts a shot update to all clients in the team's SignalR group.
    /// </summary>
    /// <param name="teamId">Routes the message to the correct SignalR group.</param>
    /// <param name="storyboardId">Parent storyboard identifier.</param>
    /// <param name="episodeId">The episode the storyboard belongs to.</param>
    /// <param name="shotId">The shot that was updated.</param>
    /// <param name="imageUrl">The new CDN URL of the shot image (null if cleared).</param>
    /// <param name="regenerationCount">Current regeneration count for the shot.</param>
    /// <param name="ct">Cancellation token.</param>
    Task NotifyShotUpdatedAsync(
        Guid teamId,
        Guid storyboardId,
        Guid episodeId,
        Guid shotId,
        string? imageUrl,
        int regenerationCount,
        CancellationToken ct = default);
}
