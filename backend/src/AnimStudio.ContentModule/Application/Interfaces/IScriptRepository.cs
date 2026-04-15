using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IScriptRepository
{
    Task<Script?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task<Script?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Script script, CancellationToken ct = default);
    Task UpdateAsync(Script script, CancellationToken ct = default);
}
