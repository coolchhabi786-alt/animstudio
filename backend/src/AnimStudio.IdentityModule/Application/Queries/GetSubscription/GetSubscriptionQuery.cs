using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetSubscription;

public sealed record GetSubscriptionQuery(Guid TeamId) : IRequest<Result<SubscriptionDto>>, ICacheKey
{
    public string Key => $"subscription:{TeamId}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
