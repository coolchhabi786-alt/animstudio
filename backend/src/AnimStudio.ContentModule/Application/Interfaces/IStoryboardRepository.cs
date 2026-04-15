using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Repository port for the Storyboard aggregate. Shots are loaded and
/// saved transitively through the aggregate — there is no separate shot
/// repository.
/// </summary>
public interface IStoryboardRepository
{
    /// <summary>Returns the storyboard for an episode with its shots eagerly loaded, or null.</summary>
    Task<Storyboard?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);

    /// <summary>Returns the storyboard by its own id with shots eagerly loaded, or null.</summary>
    Task<Storyboard?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns the storyboard that contains the given shot (loaded with all shots),
    /// or null if the shot is not found. Used by shot-level endpoints to traverse
    /// back to the aggregate.
    /// </summary>
    Task<Storyboard?> GetByShotIdAsync(Guid shotId, CancellationToken ct = default);

    Task AddAsync(Storyboard storyboard, CancellationToken ct = default);
    Task UpdateAsync(Storyboard storyboard, CancellationToken ct = default);
}
