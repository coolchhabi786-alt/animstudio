namespace AnimStudio.ContentModule.Domain.Enums;

/// <summary>
/// Visual rendering style applied to an episode's image and animation generation pipeline.
/// Each value maps to a <see cref="StylePreset"/> row that carries the Flux prompt suffix.
/// </summary>
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
