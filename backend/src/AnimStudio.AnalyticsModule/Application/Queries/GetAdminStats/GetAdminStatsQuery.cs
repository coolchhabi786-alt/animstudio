using AnimStudio.AnalyticsModule.Application.DTOs;
using AnimStudio.AnalyticsModule.Infrastructure.Persistence;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.AnalyticsModule.Application.Queries.GetAdminStats;

public sealed record GetAdminStatsQuery : IRequest<Result<AdminStatsDto>>;

public sealed record GetAdminUsersQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<List<AdminUserDto>>>;

public sealed class GetAdminStatsHandler(
    IdentityDbContext  identityDb,
    ContentDbContext   contentDb,
    AnalyticsDbContext analyticsDb)
    : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsDto>>
{
    public async Task<Result<AdminStatsDto>> Handle(GetAdminStatsQuery query, CancellationToken ct)
    {
        var totalUsers    = await identityDb.Users.CountAsync(u => !u.IsDeleted, ct);
        var totalTeams    = await identityDb.Teams.CountAsync(t => !t.IsDeleted, ct);
        var totalEpisodes = await contentDb.Episodes.CountAsync(e => !e.IsDeleted, ct);
        var activeJobs    = await contentDb.Jobs
            .CountAsync(j => !j.IsDeleted && j.Status == JobStatus.Running, ct);
        var totalViews    = await analyticsDb.VideoViews.CountAsync(ct);

        return Result<AdminStatsDto>.Success(new AdminStatsDto(
            totalUsers, totalTeams, totalEpisodes, activeJobs, totalViews));
    }
}

public sealed class GetAdminUsersHandler(IdentityDbContext identityDb)
    : IRequestHandler<GetAdminUsersQuery, Result<List<AdminUserDto>>>
{
    public async Task<Result<List<AdminUserDto>>> Handle(GetAdminUsersQuery query, CancellationToken ct)
    {
        var users = await identityDb.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.DisplayName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var result = new List<AdminUserDto>(users.Count);
        foreach (var user in users)
        {
            var membership = await identityDb.TeamMembers
                .FirstOrDefaultAsync(m => m.UserId == user.Id, ct);
            var teamId = membership?.TeamId ?? Guid.Empty;

            Subscription? sub = null;
            if (teamId != Guid.Empty)
                sub = await identityDb.Subscriptions
                    .FirstOrDefaultAsync(s => s.TeamId == teamId, ct);

            result.Add(new AdminUserDto(
                UserId:                  user.Id,
                DisplayName:             user.DisplayName,
                Email:                   user.Email,
                TeamId:                  teamId,
                SubscriptionStatus:      sub?.Status.ToString() ?? "None",
                UsageEpisodesThisMonth:  sub?.UsageEpisodesThisMonth ?? 0));
        }

        return Result<List<AdminUserDto>>.Success(result);
    }
}
