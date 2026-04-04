using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure("User not found.", "USER_NOT_FOUND");

        var dto = new UserDto(
            Id: user.Id,
            ExternalId: user.ExternalId,
            Email: user.Email,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl,
            CreatedAt: user.CreatedAt);

        return Result<UserDto>.Success(dto);
    }
}
