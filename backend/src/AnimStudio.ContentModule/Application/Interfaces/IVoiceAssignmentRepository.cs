using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IVoiceAssignmentRepository
{
    Task<List<VoiceAssignment>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task<VoiceAssignment?> GetByEpisodeAndCharacterAsync(Guid episodeId, Guid characterId, CancellationToken ct = default);
    Task AddAsync(VoiceAssignment assignment, CancellationToken ct = default);
    Task UpdateAsync(VoiceAssignment assignment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
