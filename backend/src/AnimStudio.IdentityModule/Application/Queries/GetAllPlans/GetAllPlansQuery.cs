using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetAllPlans;

public sealed record GetAllPlansQuery : IRequest<Result<IReadOnlyList<PlanDto>>>, ICacheKey
{
    public string Key => "plans:all";
    public TimeSpan CacheDuration => TimeSpan.FromHours(1);
}
