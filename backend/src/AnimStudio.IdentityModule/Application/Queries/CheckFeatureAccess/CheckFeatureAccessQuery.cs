using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.CheckFeatureAccess;

public sealed record CheckFeatureAccessQuery(Guid TeamId, string Feature)
    : IRequest<Result<bool>>, ICacheKey
{
    public string Key => $"feature:{TeamId}:{Feature}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
