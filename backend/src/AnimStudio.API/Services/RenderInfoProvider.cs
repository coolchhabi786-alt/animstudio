using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.DeliveryModule.Application.Interfaces;

namespace AnimStudio.API.Services;

/// <summary>
/// API-layer bridge that satisfies <see cref="IRenderInfoProvider"/> by querying
/// <see cref="IRenderRepository"/> from DeliveryModule.
/// Keeps ContentModule free of a direct DeliveryModule dependency.
/// </summary>
public sealed class RenderInfoProvider(IRenderRepository renders) : IRenderInfoProvider
{
    public async Task<IRenderInfoProvider.RenderInfoResult?> GetByIdAsync(
        Guid renderId, CancellationToken ct = default)
    {
        var render = await renders.GetByIdAsync(renderId, ct);
        if (render is null) return null;

        var videoUrl = render.CdnUrl ?? render.FinalVideoUrl;
        return new IRenderInfoProvider.RenderInfoResult(videoUrl, render.DurationSeconds);
    }
}
