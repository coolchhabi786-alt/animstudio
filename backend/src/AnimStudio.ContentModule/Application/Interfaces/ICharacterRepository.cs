using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Persistence contract for <see cref="Character"/> aggregate operations.
/// All queries implicitly exclude soft-deleted records via the global query filter.
/// </summary>
public interface ICharacterRepository
{
    /// <summary>Returns a character by ID, or null if not found / soft-deleted.</summary>
    Task<Character?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of characters belonging to <paramref name="teamId"/>.
    /// </summary>
    Task<(List<Character> Items, int TotalCount)> GetByTeamIdAsync(
        Guid teamId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all characters attached to a specific episode (non-deleted, any training status).
    /// </summary>
    Task<List<Character>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);

    /// <summary>Persists a new character record.</summary>
    Task AddAsync(Character character, CancellationToken ct = default);

    /// <summary>Persists changes to an existing character record.</summary>
    Task UpdateAsync(Character character, CancellationToken ct = default);

    /// <summary>
    /// Returns true if any non-deleted EpisodeCharacter row links
    /// <paramref name="characterId"/> to an episode in a non-terminal state
    /// (i.e., not Done or Failed).
    /// </summary>
    Task<bool> IsUsedInActiveEpisodeAsync(Guid characterId, CancellationToken ct = default);

    /// <summary>Adds an EpisodeCharacter link.</summary>
    Task AttachToEpisodeAsync(EpisodeCharacter link, CancellationToken ct = default);

    /// <summary>
    /// Returns the EpisodeCharacter join record, or null if not present.
    /// </summary>
    Task<EpisodeCharacter?> GetEpisodeCharacterAsync(Guid episodeId, Guid characterId, CancellationToken ct = default);

    /// <summary>Removes an EpisodeCharacter link.</summary>
    Task DetachFromEpisodeAsync(EpisodeCharacter link, CancellationToken ct = default);
}
