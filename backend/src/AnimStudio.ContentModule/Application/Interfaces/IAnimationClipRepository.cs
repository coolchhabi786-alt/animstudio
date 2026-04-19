using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IAnimationClipRepository
{
    Task<AnimationClip?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AnimationClip>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task<AnimationClip?> GetByEpisodeAndPositionAsync(Guid episodeId, int sceneNumber, int shotIndex, CancellationToken ct = default);
    Task AddAsync(AnimationClip clip, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<AnimationClip> clips, CancellationToken ct = default);
    Task UpdateAsync(AnimationClip clip, CancellationToken ct = default);
}
