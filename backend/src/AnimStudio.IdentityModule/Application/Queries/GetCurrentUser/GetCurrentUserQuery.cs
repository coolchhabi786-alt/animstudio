using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserDto>>, ICacheKey
{
    public string Key => $"user:{UserId}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
