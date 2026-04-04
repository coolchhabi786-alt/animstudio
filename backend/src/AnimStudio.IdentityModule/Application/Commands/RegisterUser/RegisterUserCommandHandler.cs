using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Idempotency: return existing user if already registered
        var existing = await userRepository.GetByExternalIdAsync(request.ExternalId, cancellationToken);
        if (existing is not null)
            return Result<Guid>.Success(existing.Id);

        var user = User.Create(
            id: Guid.NewGuid(),
            externalId: request.ExternalId,
            email: request.Email,
            displayName: request.DisplayName,
            avatarUrl: request.AvatarUrl);

        await userRepository.AddAsync(user, cancellationToken);
        return Result<Guid>.Success(user.Id);
    }
}
