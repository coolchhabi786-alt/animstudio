namespace AnimStudio.ContentModule.Application.DTOs;

// ── Timeline ──────────────────────────────────────────────────────────────────

public sealed record TimelineDto(
    Guid Id,
    Guid EpisodeId,
    int DurationMs,
    int Fps,
    List<TimelineTrackDto> Tracks,
    List<TextOverlayDto> TextOverlays,
    DateTimeOffset UpdatedAt);

// ── Track ─────────────────────────────────────────────────────────────────────

public sealed record TimelineTrackDto(
    Guid Id,
    string TrackType,
    string Label,
    bool IsMuted,
    bool IsLocked,
    int? VolumePercent,
    bool? AutoDuck,
    List<TimelineClipDto> Clips);

// ── Clip (all types share one DTO — unused fields are null) ───────────────────

public sealed record TimelineClipDto(
    Guid Id,
    Guid TrackId,
    string Type,
    long StartMs,
    long DurationMs,
    // video
    int? SceneNumber,
    int? ShotIndex,
    string? ClipUrl,
    string? ThumbnailUrl,
    string? TransitionIn,
    // audio / music
    string? Label,
    string? AudioUrl,
    int? VolumePercent,
    int? FadeInMs,
    int? FadeOutMs,
    // text clip (track lane)
    string? Text,
    int? FontSize,
    string? Color,
    string? Position,
    string? Animation);

// ── Canvas text overlay ───────────────────────────────────────────────────────
// NOTE: frontend TextOverlay type uses `episodeId` (not `timelineId`),
// so we expose EpisodeId here to match the JSON key the FE expects.

public sealed record TextOverlayDto(
    Guid Id,
    Guid EpisodeId,
    string Text,
    int FontSizePixels,
    string Color,
    int PositionX,
    int PositionY,
    long StartMs,
    long DurationMs,
    string Animation,
    int ZIndex);

// ── Save request (PUT body) ───────────────────────────────────────────────────

public sealed record SaveTimelineRequest(
    int DurationMs,
    int Fps,
    List<TimelineTrackDto> Tracks,
    List<TextOverlayDto> TextOverlays);
