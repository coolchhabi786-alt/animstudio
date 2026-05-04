using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// A canvas-positioned text overlay rendered on top of the video during playback.
/// Stored separately from track clips — these are positioned by X/Y percentage, not
/// constrained to a track lane.
/// </summary>
public sealed class TimelineTextOverlay : Entity<Guid>
{
    public Guid TimelineId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int FontSizePixels { get; set; }
    public string Color { get; set; } = "#ffffff";
    /// <summary>Horizontal position as a percentage 0–100.</summary>
    public int PositionX { get; set; }
    /// <summary>Vertical position as a percentage 0–100.</summary>
    public int PositionY { get; set; }
    public long StartMs { get; set; }
    public long DurationMs { get; set; }
    /// <summary>"none" | "fadeIn" | "slideUp" | "slideDown"</summary>
    public string Animation { get; set; } = "none";
    public int ZIndex { get; set; }
}
