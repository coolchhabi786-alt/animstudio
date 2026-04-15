using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Project> Items, int TotalCount)> GetByTeamIdAsync(Guid teamId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
}

public interface IEpisodeRepository
{
    Task<Episode?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Episode>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task AddAsync(Episode episode, CancellationToken ct = default);
    Task UpdateAsync(Episode episode, CancellationToken ct = default);
}

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Job>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(Job job, CancellationToken ct = default);
    Task UpdateAsync(Job job, CancellationToken ct = default);
}

public interface ISagaStateRepository
{
    Task<EpisodeSagaState?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(EpisodeSagaState state, CancellationToken ct = default);
    Task UpdateAsync(EpisodeSagaState state, CancellationToken ct = default);
}
