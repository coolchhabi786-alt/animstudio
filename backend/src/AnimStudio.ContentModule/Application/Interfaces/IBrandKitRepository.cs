using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IBrandKitRepository
{
    Task<BrandKit?> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default);
    Task AddAsync(BrandKit brandKit, CancellationToken ct = default);
    Task UpdateAsync(BrandKit brandKit, CancellationToken ct = default);
}
