using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetTimeline;

public sealed record GetTimelineQuery(Guid EpisodeId) : IRequest<Result<TimelineDto>>;

public sealed class GetTimelineHandler(ITimelineRepository repo, IFileStorageService files)
    : IRequestHandler<GetTimelineQuery, Result<TimelineDto>>
{
    public async Task<Result<TimelineDto>> Handle(GetTimelineQuery query, CancellationToken ct)
    {
        var timeline = await repo.GetByEpisodeIdAsync(query.EpisodeId, ct);
        if (timeline is null)
            return Result<TimelineDto>.Failure("Timeline not found.", "NOT_FOUND");

        return Result<TimelineDto>.Success(MapDto(timeline, files));
    }

    internal static TimelineDto MapDto(EpisodeTimeline t, IFileStorageService files) => new(
        t.Id,
        t.EpisodeId,
        t.DurationMs,
        t.Fps,
        t.Tracks.OrderBy(tr => tr.SortOrder).Select(tr => MapTrack(tr, files)).ToList(),
        t.TextOverlays.Select(o => MapOverlay(o, t.EpisodeId)).ToList(),
        t.UpdatedAt);

    private static TimelineTrackDto MapTrack(TimelineTrack tr, IFileStorageService files) => new(
        tr.Id,
        tr.TrackType,
        tr.Label,
        tr.IsMuted,
        tr.IsLocked,
        tr.VolumePercent,
        tr.AutoDuck,
        tr.Clips.OrderBy(c => c.SortOrder).Select(c => MapClip(c, files)).ToList());

    private static TimelineClipDto MapClip(TimelineClip c, IFileStorageService files) => new(
        c.Id,
        c.TrackId,
        c.ClipType,
        c.StartMs,
        c.DurationMs,
        c.SceneNumber,
        c.ShotIndex,
        c.ClipUrl is null      ? null : files.GetFileUrl(c.ClipUrl),
        c.ThumbnailUrl is null ? null : files.GetFileUrl(c.ThumbnailUrl),
        c.TransitionIn,
        c.Label,
        c.AudioUrl is null     ? null : files.GetFileUrl(c.AudioUrl),
        c.VolumePercent,
        c.FadeInMs,
        c.FadeOutMs,
        c.Text,
        c.FontSize,
        c.Color,
        c.Position,
        c.Animation);

    // Frontend expects `episodeId` key on TextOverlay, so we pass the timeline's EpisodeId.
    private static TextOverlayDto MapOverlay(TimelineTextOverlay o, Guid episodeId) => new(
        o.Id,
        episodeId,
        o.Text,
        o.FontSizePixels,
        o.Color,
        o.PositionX,
        o.PositionY,
        o.StartMs,
        o.DurationMs,
        o.Animation,
        o.ZIndex);
}
