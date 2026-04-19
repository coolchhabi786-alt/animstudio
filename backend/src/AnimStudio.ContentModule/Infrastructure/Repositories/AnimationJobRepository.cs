using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IAnimationJobRepository"/>.</summary>
public sealed class AnimationJobRepository(ContentDbContext db) : IAnimationJobRepository
{
    public Task<AnimationJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.AnimationJobs.FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<AnimationJob?> GetLatestByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => db.AnimationJobs
            .Where(j => j.EpisodeId == episodeId)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public Task<bool> HasActiveJobAsync(Guid episodeId, CancellationToken ct = default)
        => db.AnimationJobs.AnyAsync(
            j => j.EpisodeId == episodeId
                && j.Status != AnimationStatus.Failed
                && j.Status != AnimationStatus.Cancelled,
            ct);

    public async Task AddAsync(AnimationJob job, CancellationToken ct = default)
    {
        await db.AnimationJobs.AddAsync(job, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AnimationJob job, CancellationToken ct = default)
    {
        db.AnimationJobs.Update(job);
        await db.SaveChangesAsync(ct);
    }
}
