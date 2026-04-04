using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.UpdateUserProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    ICacheService cacheService) : IRequestHandler<UpdateUserProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<bool>.Failure("User not found.");

        user.UpdateProfile(request.DisplayName, request.AvatarUrl);
        await userRepository.UpdateAsync(user, cancellationToken);

        await cacheService.RemoveAsync($"user:{request.UserId}", cancellationToken);

        return Result<bool>.Success(true);
    }
}
