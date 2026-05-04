using AnimStudio.AnalyticsModule.Application.Commands.TrackVideoView;
using AnimStudio.AnalyticsModule.Domain.Enums;
using MediatR;

namespace AnimStudio.API.Hosted;

public sealed class TrackVideoViewHangfireProcessor(ISender sender)
{
    public async Task ProcessAsync(
        Guid            episodeId,
        Guid            renderId,
        VideoViewSource source,
        string?         viewerIpHash,
        Guid?           reviewLinkId,
        CancellationToken ct = default)
    {
        await sender.Send(
            new TrackVideoViewCommand(episodeId, renderId, source, viewerIpHash, reviewLinkId), ct);
    }
}
