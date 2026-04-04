using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl) : IRequest<Result<bool>>;
