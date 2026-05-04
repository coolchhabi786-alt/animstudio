using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// One lane in the timeline (video, audio, music, or text).
/// Owned by <see cref="EpisodeTimeline"/>; cascades deletes.
/// </summary>
public sealed class TimelineTrack : Entity<Guid>
{
    public Guid TimelineId { get; set; }

    /// <summary>"video" | "audio" | "music" | "text"</summary>
    public string TrackType { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsMuted { get; set; }
    public bool IsLocked { get; set; }

    /// <summary>Volume 0–100. Null for non-audio tracks.</summary>
    public int? VolumePercent { get; set; }

    /// <summary>Duck music volume when dialogue is present (music tracks only).</summary>
    public bool? AutoDuck { get; set; }

    public List<TimelineClip> Clips { get; set; } = new();
}
