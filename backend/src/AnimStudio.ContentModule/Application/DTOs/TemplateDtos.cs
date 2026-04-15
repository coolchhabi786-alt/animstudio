using System.Text.Json;

namespace AnimStudio.ContentModule.Application.DTOs;

/// <summary>Read-model DTO for an episode template returned by the API.</summary>
public sealed record TemplateDto(
    Guid Id,
    string Title,
    string Genre,
    string Description,
    JsonElement PlotStructure,
    string DefaultStyle,
    string? PreviewVideoUrl,
    string? ThumbnailUrl,
    bool IsActive,
    int SortOrder);

/// <summary>Read-model DTO for a visual style preset returned by the API.</summary>
public sealed record StylePresetDto(
    Guid Id,
    string Style,
    string DisplayName,
    string Description,
    string? SampleImageUrl,
    string FluxStylePromptSuffix,
    bool IsActive);
