using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// One timeline per episode — the authoritative edit state for the timeline editor.
/// Tracks, clips, and text overlays are owned children that are always replaced together
/// on <see cref="ReplaceContent"/>.
/// </summary>
public sealed class EpisodeTimeline : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; set; }
    public int DurationMs { get; set; }
    public int Fps { get; set; }

    // EF Core needs a public setter or backing-field access for navigation collections.
    public List<TimelineTrack> Tracks { get; set; } = new();
    public List<TimelineTextOverlay> TextOverlays { get; set; } = new();

    // Parameterless constructor for EF Core
    private EpisodeTimeline() { }

    /// <summary>Creates a new, empty timeline for an episode.</summary>
    public static EpisodeTimeline Create(Guid episodeId, int durationMs = 60_000, int fps = 24)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));

        var tl = new EpisodeTimeline
        {
            Id         = Guid.NewGuid(),
            EpisodeId  = episodeId,
            DurationMs = durationMs,
            Fps        = fps,
            CreatedAt  = DateTimeOffset.UtcNow,
            UpdatedAt  = DateTimeOffset.UtcNow,
        };
        tl.AddDomainEvent(new TimelineCreatedEvent(tl.Id, episodeId));
        return tl;
    }

    /// <summary>
    /// Replaces all tracks, clips, and overlays with the new state coming from the frontend.
    /// EF Core change-tracking will diff the collections on SaveChanges.
    /// </summary>
    public void ReplaceContent(List<TimelineTrack> tracks, List<TimelineTextOverlay> overlays, int durationMs)
    {
        Tracks        = tracks;
        TextOverlays  = overlays;
        DurationMs    = durationMs;
        UpdatedAt     = DateTimeOffset.UtcNow;
        AddDomainEvent(new TimelineSavedEvent(Id, EpisodeId));
    }
}
