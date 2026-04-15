using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStoryboardRepository"/>. Shots are
/// eagerly loaded because the aggregate API (IncrementShotRegeneration,
/// SetShotStyleOverride, SetShotImage) operates on the in-memory collection.
/// </summary>
public sealed class StoryboardRepository(ContentDbContext db) : IStoryboardRepository
{
    public async Task<Storyboard?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => await db.Storyboards
            .Include(s => s.Shots)
            .FirstOrDefaultAsync(s => s.EpisodeId == episodeId, ct);

    public async Task<Storyboard?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Storyboards
            .Include(s => s.Shots)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Storyboard?> GetByShotIdAsync(Guid shotId, CancellationToken ct = default)
    {
        var storyboardId = await db.StoryboardShots
            .Where(s => s.Id == shotId)
            .Select(s => (Guid?)s.StoryboardId)
            .FirstOrDefaultAsync(ct);

        if (storyboardId is null)
            return null;

        return await db.Storyboards
            .Include(s => s.Shots)
            .FirstOrDefaultAsync(s => s.Id == storyboardId.Value, ct);
    }

    public async Task AddAsync(Storyboard storyboard, CancellationToken ct = default)
    {
        await db.Storyboards.AddAsync(storyboard, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Storyboard storyboard, CancellationToken ct = default)
    {
        db.Storyboards.Update(storyboard);
        await db.SaveChangesAsync(ct);
    }
}
