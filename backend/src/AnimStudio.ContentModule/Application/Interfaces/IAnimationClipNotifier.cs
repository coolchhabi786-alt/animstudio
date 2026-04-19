namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Broadcasts per-clip completion over SignalR to the team group.
/// Implemented by <c>SignalRAnimationClipNotifier</c> in the API project.
/// </summary>
public interface IAnimationClipNotifier
{
    Task PublishClipReadyAsync(
        Guid teamId,
        Guid episodeId,
        Guid clipId,
        int sceneNumber,
        int shotIndex,
        string clipUrl,
        CancellationToken ct = default);
}
