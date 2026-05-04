using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

public sealed class ReviewLinkRepository(ContentDbContext db) : IReviewLinkRepository
{
    public Task<ReviewLink?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ReviewLinks.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<ReviewLink?> GetByTokenAsync(string token, CancellationToken ct = default)
        => db.ReviewLinks.FirstOrDefaultAsync(r => r.Token == token, ct);

    public Task<List<ReviewLink>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => db.ReviewLinks.Where(r => r.EpisodeId == episodeId).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(ReviewLink reviewLink, CancellationToken ct = default)
    {
        db.ReviewLinks.Add(reviewLink);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReviewLink reviewLink, CancellationToken ct = default)
    {
        db.ReviewLinks.Update(reviewLink);
        await db.SaveChangesAsync(ct);
    }
}
