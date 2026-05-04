using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// One clip on a <see cref="TimelineTrack"/>. All clip types (video, audio, text) share
/// this single table via a flat nullable-column design (table-per-hierarchy lite).
/// Columns irrelevant to a given type are NULL.
/// </summary>
public sealed class TimelineClip : Entity<Guid>
{
    public Guid TrackId { get; set; }

    /// <summary>"video" | "audio" | "text"</summary>
    public string ClipType { get; set; } = string.Empty;

    public long StartMs { get; set; }
    public long DurationMs { get; set; }
    public int SortOrder { get; set; }

    // ── Video fields ───────────────────────────────────────────────────────────
    public int? SceneNumber { get; set; }
    public int? ShotIndex { get; set; }

    /// <summary>Relative path stored in DB; resolved to a full URL at query time.</summary>
    public string? ClipUrl { get; set; }

    /// <summary>Relative path stored in DB; resolved to a full URL at query time.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>"cut" | "fade" | "dissolve" | "slide-left" | "slide-right" | "zoom"</summary>
    public string? TransitionIn { get; set; }

    // ── Audio / Music fields ───────────────────────────────────────────────────
    public string? Label { get; set; }

    /// <summary>Relative path stored in DB; resolved to a full URL at query time.</summary>
    public string? AudioUrl { get; set; }

    public int? VolumePercent { get; set; }
    public int? FadeInMs { get; set; }
    public int? FadeOutMs { get; set; }

    // ── Text-clip fields (shown in the text track lane) ────────────────────────
    public string? Text { get; set; }
    public int? FontSize { get; set; }
    public string? Color { get; set; }

    /// <summary>"top-left" | "center" | "bottom-right" etc.</summary>
    public string? Position { get; set; }

    /// <summary>"none" | "fadeIn" | "slideUp" | "slideDown"</summary>
    public string? Animation { get; set; }
}
