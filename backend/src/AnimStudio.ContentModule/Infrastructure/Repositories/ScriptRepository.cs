using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IScriptRepository"/>.
/// Scripts are stored in the content.Scripts table, one per episode.
/// </summary>
public sealed class ScriptRepository(ContentDbContext db) : IScriptRepository
{
    public async Task<Script?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => await db.Scripts.FirstOrDefaultAsync(s => s.EpisodeId == episodeId, ct);

    public async Task<Script?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Scripts.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Script script, CancellationToken ct = default)
    {
        await db.Scripts.AddAsync(script, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Script script, CancellationToken ct = default)
    {
        db.Scripts.Update(script);
        await db.SaveChangesAsync(ct);
    }
}
