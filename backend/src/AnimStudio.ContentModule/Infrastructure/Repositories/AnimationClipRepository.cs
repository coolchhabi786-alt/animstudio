using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>EF Core implementation of <see cref="IAnimationClipRepository"/>.</summary>
public sealed class AnimationClipRepository(ContentDbContext db) : IAnimationClipRepository
{
    public Task<AnimationClip?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.AnimationClips.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<AnimationClip>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => await db.AnimationClips
            .Where(c => c.EpisodeId == episodeId)
            .OrderBy(c => c.SceneNumber)
            .ThenBy(c => c.ShotIndex)
            .ToListAsync(ct);

    public Task<AnimationClip?> GetByEpisodeAndPositionAsync(
        Guid episodeId, int sceneNumber, int shotIndex, CancellationToken ct = default)
        => db.AnimationClips.FirstOrDefaultAsync(
            c => c.EpisodeId == episodeId
                && c.SceneNumber == sceneNumber
                && c.ShotIndex == shotIndex,
            ct);

    public async Task AddAsync(AnimationClip clip, CancellationToken ct = default)
    {
        await db.AnimationClips.AddAsync(clip, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<AnimationClip> clips, CancellationToken ct = default)
    {
        await db.AnimationClips.AddRangeAsync(clips, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AnimationClip clip, CancellationToken ct = default)
    {
        db.AnimationClips.Update(clip);
        await db.SaveChangesAsync(ct);
    }
}
