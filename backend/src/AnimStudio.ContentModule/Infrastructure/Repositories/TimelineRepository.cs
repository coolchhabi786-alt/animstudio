using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="ITimelineRepository"/>.</summary>
public sealed class TimelineRepository(ContentDbContext db) : ITimelineRepository
{
    public Task<EpisodeTimeline?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => db.EpisodeTimelines
            .Include(t => t.Tracks).ThenInclude(tr => tr.Clips)
            .Include(t => t.TextOverlays)
            .FirstOrDefaultAsync(t => t.EpisodeId == episodeId, ct);

    public async Task AddAsync(EpisodeTimeline timeline, CancellationToken ct = default)
    {
        await db.EpisodeTimelines.AddAsync(timeline, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
