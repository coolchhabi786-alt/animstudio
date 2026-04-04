using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetTeam;

public sealed record GetTeamQuery(Guid TeamId) : IRequest<Result<TeamDto>>, ICacheKey
{
    public string Key => $"team:{TeamId}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
