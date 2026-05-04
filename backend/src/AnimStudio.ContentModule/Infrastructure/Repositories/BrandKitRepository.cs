using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

public sealed class BrandKitRepository(ContentDbContext db) : IBrandKitRepository
{
    public Task<BrandKit?> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default)
        => db.BrandKits.FirstOrDefaultAsync(b => b.TeamId == teamId, ct);

    public async Task AddAsync(BrandKit brandKit, CancellationToken ct = default)
    {
        db.BrandKits.Add(brandKit);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BrandKit brandKit, CancellationToken ct = default)
    {
        db.BrandKits.Update(brandKit);
        await db.SaveChangesAsync(ct);
    }
}
