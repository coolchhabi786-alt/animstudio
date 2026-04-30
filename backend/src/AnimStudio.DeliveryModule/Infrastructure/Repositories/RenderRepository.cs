using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.DeliveryModule.Domain.Entities;
using AnimStudio.DeliveryModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.DeliveryModule.Infrastructure.Repositories;

public sealed class RenderRepository(DeliveryDbContext db) : IRenderRepository
{
    public Task<Render?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Renders.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<Render>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
        => db.Renders
            .Where(r => r.EpisodeId == episodeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public Task<Render?> GetLatestByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
        => db.Renders
            .Where(r => r.EpisodeId == episodeId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Render render, CancellationToken ct = default)
        => await db.Renders.AddAsync(render, ct);

    public Task UpdateAsync(Render render, CancellationToken ct = default)
    {
        db.Renders.Update(render);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
