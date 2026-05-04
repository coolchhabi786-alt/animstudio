using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface ITimelineRepository
{
    Task<EpisodeTimeline?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(EpisodeTimeline timeline, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
