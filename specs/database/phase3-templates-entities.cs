// Phase 3 — Template & Style Library
// EF Core C# entity definitions for AnimStudio.ContentModule

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

// ── Enums ─────────────────────────────────────────────────────────────────────

/// <summary>Episode genre classification.</summary>
public enum Genre
{
    Kids,
    Comedy,
    Drama,
    Horror,
    Romance,
    SciFi,
    Marketing,
    Fantasy
}

/// <summary>Visual rendering style for an episode.</summary>
public enum Style
{
    Pixar3D,
    Anime,
    WatercolorIllustration,
    ComicBook,
    Realistic,
    PhotoStorybook,
    RetroCartoon,
    Cyberpunk
}

// ── EpisodeTemplate ────────────────────────────────────────────────────────────

/// <summary>
/// Starter template for a new episode.
/// Provides genre classification, a structured plot outline, and a default visual style.
/// </summary>
[Table("EpisodeTemplates", Schema = "content")]
public sealed class EpisodeTemplate : Entity<Guid>
{
    /// <summary>Human-readable title, e.g. "Kids Superhero Adventure".</summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Genre classification enumeration.</summary>
    [Required, MaxLength(30)]
    public Genre Genre { get; set; }

    /// <summary>Short description shown on the template card (max 1000 chars).</summary>
    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialised plot structure (acts, beats, scene suggestions).
    /// Stored as nvarchar(max).
    /// </summary>
    [Required]
    public string PlotStructure { get; set; } = "{}";

    /// <summary>Recommended visual style for this template.</summary>
    [Required, MaxLength(30)]
    public Style DefaultStyle { get; set; }

    /// <summary>CDN URL to a short preview video (optional).</summary>
    [MaxLength(2000)]
    public string? PreviewVideoUrl { get; set; }

    /// <summary>CDN URL to a static thumbnail image (optional).</summary>
    [MaxLength(2000)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>Whether this template appears in the picker UI.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Ascending display order in the gallery.</summary>
    public int SortOrder { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}

// ── StylePreset ───────────────────────────────────────────────────────────────

/// <summary>
/// Visual style preset entry.
/// Stores the Flux prompt suffix used to steer the image-generation pipeline.
/// </summary>
[Table("StylePresets", Schema = "content")]
public sealed class StylePreset : Entity<Guid>
{
    /// <summary>Unique style enumeration value.</summary>
    [Required, MaxLength(30)]
    public Style Style { get; set; }

    /// <summary>Display name shown in the style swatches grid.</summary>
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Short description of the visual characteristics.</summary>
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>CDN URL to a sample render showing this style (optional).</summary>
    [MaxLength(2000)]
    public string? SampleImageUrl { get; set; }

    /// <summary>
    /// Suffix appended to every Flux image-generation prompt when this style is active.
    /// Max 500 chars to stay within prompt token budgets.
    /// </summary>
    [Required, MaxLength(500)]
    public string FluxStylePromptSuffix { get; set; } = string.Empty;

    /// <summary>Whether this preset is available for selection.</summary>
    public bool IsActive { get; set; } = true;
}

// ── Episode entity update — TemplateId FK already present from Phase 2 ─────────
// The Episode.Style (string) property stores the Style enum name.
// Validated at the application layer to be a valid Style enum value.
// No structural DB change required — the Phase 3 migration only adds new tables.
