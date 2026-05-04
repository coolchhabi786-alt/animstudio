using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.AnalyticsModule.Domain.Entities;
using AnimStudio.AnalyticsModule.Domain.Enums;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.AnalyticsModule.Application.Commands.TrackVideoView;

public sealed record TrackVideoViewCommand(
    Guid            EpisodeId,
    Guid            RenderId,
    VideoViewSource Source,
    string?         ViewerIpHash  = null,
    Guid?           ReviewLinkId  = null) : IRequest<Result<bool>>;

public sealed class TrackVideoViewHandler(IVideoViewRepository views)
    : IRequestHandler<TrackVideoViewCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(TrackVideoViewCommand cmd, CancellationToken ct)
    {
        var view = VideoView.Create(cmd.EpisodeId, cmd.RenderId, cmd.Source, cmd.ViewerIpHash, cmd.ReviewLinkId);
        await views.AddAsync(view, ct);
        return Result<bool>.Success(true);
    }
}
