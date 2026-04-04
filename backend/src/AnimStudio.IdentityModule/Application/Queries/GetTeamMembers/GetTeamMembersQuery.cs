using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetTeamMembers;

public sealed record GetTeamMembersQuery(Guid TeamId) : IRequest<Result<IReadOnlyList<TeamMemberDto>>>, ICacheKey
{
    public string Key => $"team:{TeamId}:members";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
