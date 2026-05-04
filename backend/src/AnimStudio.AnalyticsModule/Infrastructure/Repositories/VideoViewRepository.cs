using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.AnalyticsModule.Domain.Entities;
using AnimStudio.AnalyticsModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.AnalyticsModule.Infrastructure.Repositories;

public sealed class VideoViewRepository(AnalyticsDbContext db) : IVideoViewRepository
{
    public async Task AddAsync(VideoView view, CancellationToken ct = default)
    {
        db.VideoViews.Add(view);
        await db.SaveChangesAsync(ct);
    }

    public Task<int> GetViewCountByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
        => db.VideoViews.CountAsync(v => v.EpisodeId == episodeId, ct);

    public Task<int> GetViewCountByRenderAsync(Guid renderId, CancellationToken ct = default)
        => db.VideoViews.CountAsync(v => v.RenderId == renderId, ct);
}
