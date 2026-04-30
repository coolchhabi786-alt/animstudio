using AnimStudio.DeliveryModule.Domain.Entities;

namespace AnimStudio.DeliveryModule.Application.Interfaces;

public interface IRenderRepository
{
    Task<Render?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Render>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task<Render?> GetLatestByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(Render render, CancellationToken ct = default);
    Task UpdateAsync(Render render, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
