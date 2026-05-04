namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Cross-module bridge: ContentModule raises "episode completed" → API layer
/// resolves the subscription via IdentityModule and increments usage.
/// </summary>
public interface IUsageMeteringService
{
    Task IncrementEpisodeUsageAsync(Guid episodeId, Guid projectId, CancellationToken ct = default);
}
