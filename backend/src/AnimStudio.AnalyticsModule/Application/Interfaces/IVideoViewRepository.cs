using AnimStudio.AnalyticsModule.Domain.Entities;

namespace AnimStudio.AnalyticsModule.Application.Interfaces;

public interface IVideoViewRepository
{
    Task AddAsync(VideoView view, CancellationToken ct = default);
    Task<int> GetViewCountByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task<int> GetViewCountByRenderAsync(Guid renderId, CancellationToken ct = default);
}
