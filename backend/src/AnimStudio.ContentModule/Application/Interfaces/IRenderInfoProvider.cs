namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Cross-module bridge: ContentModule handlers call this to fetch render metadata
/// (video URL + duration) without taking a direct dependency on DeliveryModule.
/// Implemented in the API layer via <c>RenderInfoProvider</c>.
/// </summary>
public interface IRenderInfoProvider
{
    Task<RenderInfoResult?> GetByIdAsync(Guid renderId, CancellationToken ct = default);

    public sealed record RenderInfoResult(string? VideoUrl, double DurationSeconds);
}
