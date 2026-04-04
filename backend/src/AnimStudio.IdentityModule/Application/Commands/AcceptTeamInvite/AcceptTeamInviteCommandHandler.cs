using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.AcceptTeamInvite;

internal sealed class AcceptTeamInviteCommandHandler(
    ITeamRepository teamRepository) : IRequestHandler<AcceptTeamInviteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AcceptTeamInviteCommand request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByInviteTokenAsync(request.InviteToken, cancellationToken);
        if (team is null)
            return Result<Guid>.Failure("Invite token is invalid or has expired.");

        var acceptResult = team.AcceptInvite(request.InviteToken);
        if (!acceptResult.IsSuccess)
            return Result<Guid>.Failure(acceptResult.Error!);

        await teamRepository.UpdateAsync(team, cancellationToken);
        return Result<Guid>.Success(team.Id);
    }
}
