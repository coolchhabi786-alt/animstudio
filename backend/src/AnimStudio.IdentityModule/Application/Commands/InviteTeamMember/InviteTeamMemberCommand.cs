using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.InviteTeamMember;

public sealed record InviteTeamMemberCommand(
    Guid TeamId,
    Guid InvitedByUserId,
    string InviteeEmail,
    string Role) : IRequest<Result<string>>;  // returns the invite token
