using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.CreateTeam;

internal sealed class CreateTeamCommandHandler(
    IUserRepository userRepository,
    ITeamRepository teamRepository) : IRequestHandler<CreateTeamCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var owner = await userRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner is null)
            return Result<Guid>.Failure("User not found.");

        var teamId = Guid.NewGuid();
        var team = Team.Create(id: teamId, name: request.Name, ownerId: request.OwnerId, logoUrl: request.LogoUrl);

        await teamRepository.AddAsync(team, cancellationToken);
        return Result<Guid>.Success(teamId);
    }
}
