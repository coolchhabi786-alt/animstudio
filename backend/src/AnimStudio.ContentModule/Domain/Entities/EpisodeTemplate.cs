using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Starter template for a new episode.
/// Provides genre classification, a structured plot outline, and a default visual style.
/// Not soft-deleted — use <see cref="IsActive"/> = false to retire a template.
/// </summary>
public sealed class EpisodeTemplate : Entity<Guid>
{
    /// <summary>Human-readable title shown on the template card (e.g. "Kids Superhero Adventure").</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Genre classification used for tab filtering in the gallery UI.</summary>
    public Genre Genre { get; private set; }

    /// <summary>Short description of the plot premise (max 1 000 chars).</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialised plot structure containing acts, beats, and suggested scene counts.
    /// Stored as <c>nvarchar(max)</c>.
    /// </summary>
    public string PlotStructure { get; private set; } = "{}";

    /// <summary>Recommended visual style pre-selected when this template is chosen.</summary>
    public Style DefaultStyle { get; private set; }

    /// <summary>CDN URL to a short (~5 s) preview MP4 video. Null if not available.</summary>
    public string? PreviewVideoUrl { get; private set; }

    /// <summary>CDN URL to a static thumbnail image. Null if not available.</summary>
    public string? ThumbnailUrl { get; private set; }

    /// <summary>Controls visibility in the template picker — does not hard-delete rows.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Ascending integer used for display ordering in the gallery.</summary>
    public int SortOrder { get; private set; }

    private EpisodeTemplate() { }

    /// <summary>Factory method used during seeding and future admin operations.</summary>
    public static EpisodeTemplate Create(
        string title,
        Genre genre,
        string description,
        string plotStructure,
        Style defaultStyle,
        int sortOrder,
        string? thumbnailUrl = null,
        string? previewVideoUrl = null)
    {
        return new EpisodeTemplate
        {
            Id = Guid.NewGuid(),
            Title = title,
            Genre = genre,
            Description = description,
            PlotStructure = plotStructure,
            DefaultStyle = defaultStyle,
            SortOrder = sortOrder,
            ThumbnailUrl = thumbnailUrl,
            PreviewVideoUrl = previewVideoUrl,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
