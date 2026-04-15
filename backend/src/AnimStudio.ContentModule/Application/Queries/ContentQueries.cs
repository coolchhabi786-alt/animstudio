using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries;

// ── GetProject ────────────────────────────────────────────────────────────────

public sealed record GetProjectQuery(Guid Id) : IRequest<Result<ProjectDto>>, ICacheKey
{
    public string Key => $"project:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

public sealed class GetProjectHandler(IProjectRepository projects)
    : IRequestHandler<GetProjectQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(GetProjectQuery q, CancellationToken ct)
    {
        var project = await projects.GetByIdAsync(q.Id, ct);
        return project is null
            ? Result<ProjectDto>.Failure("Project not found", "NOT_FOUND")
            : Result<ProjectDto>.Success(project.ToDto());
    }
}

// ── GetProjects ───────────────────────────────────────────────────────────────

public sealed record GetProjectsQuery(Guid TeamId, int Page = 1, int PageSize = 20) : IRequest<Result<PaginatedList<ProjectDto>>>;

public sealed class GetProjectsHandler(IProjectRepository projects)
    : IRequestHandler<GetProjectsQuery, Result<PaginatedList<ProjectDto>>>
{
    public async Task<Result<PaginatedList<ProjectDto>>> Handle(GetProjectsQuery q, CancellationToken ct)
    {
        var (items, total) = await projects.GetByTeamIdAsync(q.TeamId, q.Page, q.PageSize, ct);
        var dto = new PaginatedList<ProjectDto>(items.Select(p => p.ToDto()).ToList(), total, q.Page, q.PageSize);
        return Result<PaginatedList<ProjectDto>>.Success(dto);
    }
}

// ── GetEpisode ────────────────────────────────────────────────────────────────

public sealed record GetEpisodeQuery(Guid Id) : IRequest<Result<EpisodeDto>>, ICacheKey
{
    public string Key => $"episode:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(2);
}

public sealed class GetEpisodeHandler(IEpisodeRepository episodes)
    : IRequestHandler<GetEpisodeQuery, Result<EpisodeDto>>
{
    public async Task<Result<EpisodeDto>> Handle(GetEpisodeQuery q, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(q.Id, ct);
        return episode is null
            ? Result<EpisodeDto>.Failure("Episode not found", "NOT_FOUND")
            : Result<EpisodeDto>.Success(episode.ToDto());
    }
}

// ── GetEpisodes ───────────────────────────────────────────────────────────────

public sealed record GetEpisodesQuery(Guid ProjectId) : IRequest<Result<List<EpisodeDto>>>;

public sealed class GetEpisodesHandler(IEpisodeRepository episodes)
    : IRequestHandler<GetEpisodesQuery, Result<List<EpisodeDto>>>
{
    public async Task<Result<List<EpisodeDto>>> Handle(GetEpisodesQuery q, CancellationToken ct)
    {
        var list = await episodes.GetByProjectIdAsync(q.ProjectId, ct);
        return Result<List<EpisodeDto>>.Success(list.Select(e => e.ToDto()).ToList());
    }
}

// ── GetJob ────────────────────────────────────────────────────────────────────

public sealed record GetJobQuery(Guid Id) : IRequest<Result<JobDto>>;

public sealed class GetJobHandler(IJobRepository jobs)
    : IRequestHandler<GetJobQuery, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(GetJobQuery q, CancellationToken ct)
    {
        var job = await jobs.GetByIdAsync(q.Id, ct);
        return job is null
            ? Result<JobDto>.Failure("Job not found", "NOT_FOUND")
            : Result<JobDto>.Success(job.ToDto());
    }
}

// ── GetSagaState ──────────────────────────────────────────────────────────────

public sealed record GetSagaStateQuery(Guid EpisodeId) : IRequest<Result<SagaStateDto>>;

public sealed class GetSagaStateHandler(ISagaStateRepository sagas)
    : IRequestHandler<GetSagaStateQuery, Result<SagaStateDto>>
{
    public async Task<Result<SagaStateDto>> Handle(GetSagaStateQuery q, CancellationToken ct)
    {
        var saga = await sagas.GetByEpisodeIdAsync(q.EpisodeId, ct);
        if (saga is null) return Result<SagaStateDto>.Failure("Saga state not found", "NOT_FOUND");

        var dto = new SagaStateDto(
            saga.Id,
            saga.EpisodeId,
            saga.CurrentStage.ToString(),
            saga.RetryCount,
            saga.LastError,
            saga.StartedAt,
            saga.UpdatedAt,
            saga.IsCompensating);

        return Result<SagaStateDto>.Success(dto);
    }
}
