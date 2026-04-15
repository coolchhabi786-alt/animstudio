using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Read-only repository for <see cref="EpisodeTemplate"/> lookup operations.
/// Only rows where <see cref="EpisodeTemplate.IsActive"/> is <see langword="true"/> are ever returned;
/// retired templates are excluded at the infrastructure layer via a global query filter.
/// </summary>
public interface IEpisodeTemplateRepository
{
    /// <summary>
    /// Returns all active <see cref="EpisodeTemplate"/> rows, optionally narrowed to a single genre.
    /// </summary>
    /// <param name="genre">
    /// Case-insensitive genre name to filter by (e.g. <c>"Action"</c>).
    /// Pass <see langword="null"/> to return templates across all genres.
    /// </param>
    /// <param name="ct">Token used to cancel the asynchronous database read.</param>
    /// <returns>
    /// A list of active <see cref="EpisodeTemplate"/> instances ordered by
    /// <see cref="EpisodeTemplate.SortOrder"/> ascending. Returns an empty list when no
    /// templates match the requested genre.
    /// </returns>
    Task<List<EpisodeTemplate>> GetAllAsync(string? genre, CancellationToken ct);

    /// <summary>
    /// Returns the active <see cref="EpisodeTemplate"/> with the specified primary key,
    /// or <see langword="null"/> if it does not exist or has been retired
    /// (<see cref="EpisodeTemplate.IsActive"/> is <see langword="false"/>).
    /// </summary>
    /// <param name="id">The <see cref="Guid"/> primary key of the template to retrieve.</param>
    /// <param name="ct">Token used to cancel the asynchronous database read.</param>
    /// <returns>
    /// The matching <see cref="EpisodeTemplate"/>, or <see langword="null"/> when not found.
    /// </returns>
    Task<EpisodeTemplate?> GetByIdAsync(Guid id, CancellationToken ct);
}

/// <summary>
/// Read-only repository for <see cref="StylePreset"/> lookup operations.
/// Only rows where <see cref="StylePreset.IsActive"/> is <see langword="true"/> are returned.
/// Each preset carries a <see cref="StylePreset.FluxStylePromptSuffix"/> consumed server-side
/// by the image-generation pipeline; the suffix is never exposed to end-users.
/// </summary>
public interface IStylePresetRepository
{
    /// <summary>
    /// Returns all active <see cref="StylePreset"/> rows ordered by <see cref="StylePreset.Style"/>.
    /// </summary>
    /// <param name="ct">Token used to cancel the asynchronous database read.</param>
    /// <returns>
    /// A list of active <see cref="StylePreset"/> instances sorted by
    /// <see cref="StylePreset.Style"/> ascending. Returns an empty list when no presets are
    /// configured.
    /// </returns>
    Task<List<StylePreset>> GetAllAsync(CancellationToken ct);
}
