using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IAnimationJobRepository
{
    Task<AnimationJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AnimationJob?> GetLatestByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task<bool> HasActiveJobAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(AnimationJob job, CancellationToken ct = default);
    Task UpdateAsync(AnimationJob job, CancellationToken ct = default);
}
