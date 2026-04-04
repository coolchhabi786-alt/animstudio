using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string ExternalId,
    string Email,
    string DisplayName,
    string? AvatarUrl) : IRequest<Result<Guid>>;
