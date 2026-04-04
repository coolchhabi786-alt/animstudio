using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.AcceptTeamInvite;

public sealed record AcceptTeamInviteCommand(string InviteToken) : IRequest<Result<Guid>>;  // returns TeamId
