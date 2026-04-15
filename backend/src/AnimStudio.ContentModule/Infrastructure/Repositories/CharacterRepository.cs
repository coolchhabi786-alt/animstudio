using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICharacterRepository"/>.
/// Uses the <see cref="ContentDbContext"/>; all queries respect the
/// global soft-delete query filter applied in <c>OnModelCreating</c>.
/// </summary>
public sealed class CharacterRepository(ContentDbContext db) : ICharacterRepository
{
    public async Task<Character?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Characters.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<(List<Character> Items, int TotalCount)> GetByTeamIdAsync(
        Guid teamId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Characters
            .Where(c => c.TeamId == teamId)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<List<Character>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => await db.EpisodeCharacters
            .Where(ec => ec.EpisodeId == episodeId)
            .Include(ec => ec.Character)
            .Select(ec => ec.Character)
            .ToListAsync(ct);

    public async Task AddAsync(Character character, CancellationToken ct = default)
    {
        await db.Characters.AddAsync(character, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Character character, CancellationToken ct = default)
    {
        db.Characters.Update(character);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsUsedInActiveEpisodeAsync(Guid characterId, CancellationToken ct = default)
    {
        // Use a List so EF Core can translate the Contains to SQL IN(...)
        var terminalStatuses = new List<EpisodeStatus> { EpisodeStatus.Done, EpisodeStatus.Failed };

        return await db.EpisodeCharacters
            .Where(ec => ec.CharacterId == characterId)
            .Join(db.Episodes,
                ec => ec.EpisodeId,
                e => e.Id,
                (ec, e) => e)
            .AnyAsync(e => !terminalStatuses.Contains(e.Status), ct);
    }

    public async Task AttachToEpisodeAsync(EpisodeCharacter link, CancellationToken ct = default)
    {
        await db.EpisodeCharacters.AddAsync(link, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<EpisodeCharacter?> GetEpisodeCharacterAsync(
        Guid episodeId, Guid characterId, CancellationToken ct = default)
        => await db.EpisodeCharacters.FindAsync(
            new object[] { episodeId, characterId }, ct);

    public async Task DetachFromEpisodeAsync(EpisodeCharacter link, CancellationToken ct = default)
    {
        db.EpisodeCharacters.Remove(link);
        await db.SaveChangesAsync(ct);
    }
}
