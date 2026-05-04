using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IReviewCommentRepository
{
    Task<ReviewComment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReviewComment>> GetByReviewLinkIdAsync(Guid reviewLinkId, CancellationToken ct = default);
    Task AddAsync(ReviewComment comment, CancellationToken ct = default);
    Task UpdateAsync(ReviewComment comment, CancellationToken ct = default);
}
