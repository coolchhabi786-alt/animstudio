using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetTeam;

internal sealed class GetTeamQueryHandler(
    ITeamRepository teamRepository) : IRequestHandler<GetTeamQuery, Result<TeamDto>>
{
    public async Task<Result<TeamDto>> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<TeamDto>.Failure("Team not found.", "TEAM_NOT_FOUND");

        SubscriptionDto? subscriptionDto = null;
        if (team.Subscription is { } sub)
        {
            subscriptionDto = new SubscriptionDto(
                Id: sub.Id,
                PlanName: sub.Plan?.Name ?? string.Empty,
                Status: sub.Status.ToString(),
                EpisodesUsedThisMonth: sub.UsageEpisodesThisMonth,
                EpisodesPerMonth: sub.Plan?.EpisodesPerMonth ?? 0,
                CurrentPeriodEnd: sub.CurrentPeriodEnd,
                TrialEndsAt: sub.TrialEndsAt,
                CancelAtPeriodEnd: sub.CancelAtPeriodEnd);
        }

        var dto = new TeamDto(
            Id: team.Id,
            Name: team.Name,
            LogoUrl: team.LogoUrl,
            OwnerId: team.OwnerId,
            CreatedAt: team.CreatedAt,
            MemberCount: team.Members.Count(m => m.InviteAcceptedAt.HasValue),
            Subscription: subscriptionDto);

        return Result<TeamDto>.Success(dto);
    }
}
