using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

public sealed class ProjectRepository(ContentDbContext db) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(List<Project> Items, int TotalCount)> GetByTeamIdAsync(Guid teamId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Projects.Where(p => p.TeamId == teamId).OrderByDescending(p => p.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Project project, CancellationToken ct)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Project project, CancellationToken ct)
    {
        db.Projects.Update(project);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class EpisodeRepository(ContentDbContext db) : IEpisodeRepository
{
    public Task<Episode?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Episodes.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<List<Episode>> GetByProjectIdAsync(Guid projectId, CancellationToken ct)
        => db.Episodes.Where(e => e.ProjectId == projectId).OrderByDescending(e => e.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(Episode episode, CancellationToken ct)
    {
        db.Episodes.Add(episode);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Episode episode, CancellationToken ct)
    {
        db.Episodes.Update(episode);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class JobRepository(ContentDbContext db) : IJobRepository
{
    public Task<Job?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<List<Job>> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct)
        => db.Jobs.Where(j => j.EpisodeId == episodeId).ToListAsync(ct);

    public async Task AddAsync(Job job, CancellationToken ct)
    {
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct)
    {
        db.Jobs.Update(job);
        await db.SaveChangesAsync(ct);
    }
}

/// <summary>
/// SagaStates live in shared.SagaStates (owned by SharedDbContext).
/// ContentModule reads/writes them via SharedDbContext injected here.
/// </summary>
public sealed class SagaStateRepository(SharedDbContext sharedDb) : ISagaStateRepository
{
    public Task<EpisodeSagaState?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct)
        => sharedDb.EpisodeSagaStates.FirstOrDefaultAsync(s => s.EpisodeId == episodeId, ct);

    public async Task AddAsync(EpisodeSagaState state, CancellationToken ct)
    {
        sharedDb.EpisodeSagaStates.Add(state);
        await sharedDb.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(EpisodeSagaState state, CancellationToken ct)
    {
        sharedDb.EpisodeSagaStates.Update(state);
        await sharedDb.SaveChangesAsync(ct);
    }
}

