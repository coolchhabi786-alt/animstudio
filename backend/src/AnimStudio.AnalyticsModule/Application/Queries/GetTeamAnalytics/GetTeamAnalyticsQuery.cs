using AnimStudio.AnalyticsModule.Application.DTOs;
using AnimStudio.AnalyticsModule.Infrastructure.Persistence;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.AnalyticsModule.Application.Queries.GetTeamAnalytics;

public sealed record GetTeamAnalyticsQuery(Guid TeamId) : IRequest<Result<TeamAnalyticsDto>>;

public sealed class GetTeamAnalyticsHandler(
    IdentityDbContext identityDb,
    ContentDbContext  contentDb,
    AnalyticsDbContext analyticsDb)
    : IRequestHandler<GetTeamAnalyticsQuery, Result<TeamAnalyticsDto>>
{
    public async Task<Result<TeamAnalyticsDto>> Handle(GetTeamAnalyticsQuery query, CancellationToken ct)
    {
        var subscription = await identityDb.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.TeamId == query.TeamId, ct);

        var projectIds = await contentDb.Projects
            .Where(p => p.TeamId == query.TeamId && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync(ct);

        int totalEpisodes = 0, completedEpisodes = 0;
        if (projectIds.Count > 0)
        {
            totalEpisodes = await contentDb.Episodes
                .CountAsync(e => projectIds.Contains(e.ProjectId) && !e.IsDeleted, ct);
            completedEpisodes = await contentDb.Episodes
                .CountAsync(e => projectIds.Contains(e.ProjectId) && !e.IsDeleted
                              && e.Status == EpisodeStatus.Done, ct);
        }

        var episodeIds = await contentDb.Episodes
            .Where(e => projectIds.Contains(e.ProjectId) && !e.IsDeleted)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var totalViews = episodeIds.Count > 0
            ? await analyticsDb.VideoViews.CountAsync(v => episodeIds.Contains(v.EpisodeId), ct)
            : 0;

        var dto = new TeamAnalyticsDto(
            TeamId:                    query.TeamId,
            TotalEpisodes:             totalEpisodes,
            CompletedEpisodes:         completedEpisodes,
            TotalVideoViews:           totalViews,
            UsageEpisodesThisMonth:    subscription?.UsageEpisodesThisMonth ?? 0,
            MonthlyQuota:              subscription?.Plan?.EpisodesPerMonth ?? 0);

        return Result<TeamAnalyticsDto>.Success(dto);
    }
}
