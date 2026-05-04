using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetTimeline;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.SaveTimeline;

public sealed record SaveTimelineCommand(
    Guid EpisodeId,
    SaveTimelineRequest Body,
    Guid RequestedByUserId) : IRequest<Result<TimelineDto>>;

public sealed class SaveTimelineValidator : AbstractValidator<SaveTimelineCommand>
{
    public SaveTimelineValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.Body.DurationMs).GreaterThan(0);
        RuleFor(x => x.Body.Fps).InclusiveBetween(1, 120);
        RuleFor(x => x.Body.Tracks).NotNull();
    }
}

public sealed class SaveTimelineHandler(ITimelineRepository repo, IFileStorageService files)
    : IRequestHandler<SaveTimelineCommand, Result<TimelineDto>>
{
    public async Task<Result<TimelineDto>> Handle(SaveTimelineCommand cmd, CancellationToken ct)
    {
        var existing = await repo.GetByEpisodeIdAsync(cmd.EpisodeId, ct);

        var tracks = cmd.Body.Tracks.Select((tDto, i) => new TimelineTrack
        {
            Id           = tDto.Id == Guid.Empty ? Guid.NewGuid() : tDto.Id,
            TimelineId   = existing?.Id ?? Guid.Empty, // filled after we know the aggregate Id
            TrackType    = tDto.TrackType,
            Label        = tDto.Label,
            SortOrder    = i,
            IsMuted      = tDto.IsMuted,
            IsLocked     = tDto.IsLocked,
            VolumePercent = tDto.VolumePercent,
            AutoDuck     = tDto.AutoDuck,
            Clips        = tDto.Clips.Select((cDto, ci) => MapClip(cDto, ci)).ToList(),
            CreatedAt    = DateTimeOffset.UtcNow,
            UpdatedAt    = DateTimeOffset.UtcNow,
        }).ToList();

        var overlays = cmd.Body.TextOverlays.Select(oDto => new TimelineTextOverlay
        {
            Id            = oDto.Id == Guid.Empty ? Guid.NewGuid() : oDto.Id,
            TimelineId    = existing?.Id ?? Guid.Empty,
            Text          = oDto.Text,
            FontSizePixels = oDto.FontSizePixels,
            Color         = oDto.Color,
            PositionX     = oDto.PositionX,
            PositionY     = oDto.PositionY,
            StartMs       = oDto.StartMs,
            DurationMs    = oDto.DurationMs,
            Animation     = oDto.Animation,
            ZIndex        = oDto.ZIndex,
            CreatedAt     = DateTimeOffset.UtcNow,
            UpdatedAt     = DateTimeOffset.UtcNow,
        }).ToList();

        if (existing is null)
        {
            // First save — create new aggregate
            var timeline = EpisodeTimeline.Create(cmd.EpisodeId, cmd.Body.DurationMs, cmd.Body.Fps);

            // Fix forward references now that we know the aggregate Id
            foreach (var tr in tracks) { tr.TimelineId = timeline.Id; foreach (var c in tr.Clips) c.TrackId = tr.Id; }
            foreach (var o in overlays) o.TimelineId = timeline.Id;

            timeline.Tracks       = tracks;
            timeline.TextOverlays = overlays;

            await repo.AddAsync(timeline, ct);
            return Result<TimelineDto>.Success(GetTimelineHandler.MapDto(timeline, files));
        }

        // Update path — replace all content via the aggregate method
        // Fix track/clip/overlay foreign keys to existing IDs
        foreach (var tr in tracks)
        {
            tr.TimelineId = existing.Id;
            foreach (var c in tr.Clips) c.TrackId = tr.Id;
        }
        foreach (var o in overlays) o.TimelineId = existing.Id;

        existing.ReplaceContent(tracks, overlays, cmd.Body.DurationMs);
        await repo.SaveChangesAsync(ct);

        return Result<TimelineDto>.Success(GetTimelineHandler.MapDto(existing, files));
    }

    private static TimelineClip MapClip(TimelineClipDto dto, int sortOrder) => new()
    {
        Id           = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
        TrackId      = dto.TrackId,  // corrected by caller after track Id is known
        ClipType     = dto.Type,
        StartMs      = dto.StartMs,
        DurationMs   = dto.DurationMs,
        SortOrder    = sortOrder,
        SceneNumber  = dto.SceneNumber,
        ShotIndex    = dto.ShotIndex,
        ClipUrl      = dto.ClipUrl,
        ThumbnailUrl = dto.ThumbnailUrl,
        TransitionIn = dto.TransitionIn,
        Label        = dto.Label,
        AudioUrl     = dto.AudioUrl,
        VolumePercent = dto.VolumePercent,
        FadeInMs     = dto.FadeInMs,
        FadeOutMs    = dto.FadeOutMs,
        Text         = dto.Text,
        FontSize     = dto.FontSize,
        Color        = dto.Color,
        Position     = dto.Position,
        Animation    = dto.Animation,
        CreatedAt    = DateTimeOffset.UtcNow,
        UpdatedAt    = DateTimeOffset.UtcNow,
    };
}
