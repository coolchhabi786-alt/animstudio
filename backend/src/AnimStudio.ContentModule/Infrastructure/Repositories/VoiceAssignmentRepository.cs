using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IVoiceAssignmentRepository"/>.
/// Voice assignments are stored in the content.VoiceAssignments table.
/// </summary>
public sealed class VoiceAssignmentRepository(ContentDbContext db) : IVoiceAssignmentRepository
{
    public async Task<List<VoiceAssignment>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
        => await db.VoiceAssignments
            .Where(v => v.EpisodeId == episodeId)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync(ct);

    public async Task<VoiceAssignment?> GetByEpisodeAndCharacterAsync(
        Guid episodeId, Guid characterId, CancellationToken ct = default)
        => await db.VoiceAssignments
            .FirstOrDefaultAsync(v => v.EpisodeId == episodeId && v.CharacterId == characterId, ct);

    public async Task AddAsync(VoiceAssignment assignment, CancellationToken ct = default)
    {
        await db.VoiceAssignments.AddAsync(assignment, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VoiceAssignment assignment, CancellationToken ct = default)
    {
        db.VoiceAssignments.Update(assignment);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
