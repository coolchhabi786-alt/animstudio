using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Visual style preset entry. Carries the Flux prompt suffix that steers
/// the image-generation and animation pipeline for each supported style.
/// Not soft-deleted — use <see cref="IsActive"/> = false to disable a preset.
/// </summary>
public sealed class StylePreset : Entity<Guid>
{
    /// <summary>Unique style enumeration value — one row per <see cref="Style"/> variant.</summary>
    public Style Style { get; private set; }

    /// <summary>Label shown in the style swatches grid (e.g. "Pixar 3D").</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Short prose description of the visual characteristics (max 500 chars).</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>CDN URL to a sample render illustrating the style. Null if not available.</summary>
    public string? SampleImageUrl { get; private set; }

    /// <summary>
    /// Suffix appended verbatim to every Flux.1 image-generation prompt when this style is active.
    /// Never echoed back to end-users; used server-side only.
    /// </summary>
    public string FluxStylePromptSuffix { get; private set; } = string.Empty;

    /// <summary>Controls visibility in the style picker UI.</summary>
    public bool IsActive { get; private set; } = true;

    private StylePreset() { }

    /// <summary>Factory method used during seeding and future admin operations.</summary>
    public static StylePreset Create(
        Style style,
        string displayName,
        string description,
        string fluxStylePromptSuffix,
        string? sampleImageUrl = null)
    {
        return new StylePreset
        {
            Id = Guid.NewGuid(),
            Style = style,
            DisplayName = displayName,
            Description = description,
            FluxStylePromptSuffix = fluxStylePromptSuffix,
            SampleImageUrl = sampleImageUrl,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
