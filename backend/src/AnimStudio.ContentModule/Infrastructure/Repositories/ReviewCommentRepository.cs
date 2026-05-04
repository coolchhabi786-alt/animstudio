using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

public sealed class ReviewCommentRepository(ContentDbContext db) : IReviewCommentRepository
{
    public Task<ReviewComment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ReviewComments.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<ReviewComment>> GetByReviewLinkIdAsync(Guid reviewLinkId, CancellationToken ct = default)
        => db.ReviewComments
            .Where(c => c.ReviewLinkId == reviewLinkId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(ReviewComment comment, CancellationToken ct = default)
    {
        db.ReviewComments.Add(comment);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReviewComment comment, CancellationToken ct = default)
    {
        db.ReviewComments.Update(comment);
        await db.SaveChangesAsync(ct);
    }
}
