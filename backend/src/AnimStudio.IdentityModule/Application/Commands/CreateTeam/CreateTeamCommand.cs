using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.CreateTeam;

public sealed record CreateTeamCommand(
    Guid OwnerId,
    string Name,
    string? LogoUrl) : IRequest<Result<Guid>>;
