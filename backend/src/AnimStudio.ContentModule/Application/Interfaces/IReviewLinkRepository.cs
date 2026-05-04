using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IReviewLinkRepository
{
    Task<ReviewLink?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReviewLink?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<List<ReviewLink>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(ReviewLink reviewLink, CancellationToken ct = default);
    Task UpdateAsync(ReviewLink reviewLink, CancellationToken ct = default);
}
