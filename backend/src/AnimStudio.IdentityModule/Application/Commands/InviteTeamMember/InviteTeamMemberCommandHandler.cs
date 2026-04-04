using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.IdentityModule.Domain.ValueObjects;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.IdentityModule.Application.Commands.InviteTeamMember;

internal sealed class InviteTeamMemberCommandHandler(
    ITeamRepository teamRepository,
    IUserRepository userRepository,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<InviteTeamMemberCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InviteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdWithMembersAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<string>.Failure("Team not found.");

        if (!Enum.TryParse<TeamRole>(request.Role, ignoreCase: true, out var role))
            return Result<string>.Failure($"Invalid role '{request.Role}'. Must be Admin or Member.");

        // Invitee must already be registered
        var invitee = await userRepository.GetByEmailAsync(request.InviteeEmail, cancellationToken);
        if (invitee is null)
            return Result<string>.Failure("No account found for this email. Ask the user to register first.");

        var inviteResult = team.InviteMember(invitee.Id, role);
        if (!inviteResult.IsSuccess)
            return Result<string>.Failure(inviteResult.Error!);

        await teamRepository.UpdateAsync(team, cancellationToken);

        var member = team.Members.First(m => m.UserId == invitee.Id && m.InviteToken is not null);
        var appBaseUrl = configuration["AppSettings:BaseUrl"] ?? "https://animstudio.ai";
        var inviteLink = $"{appBaseUrl}/accept-invite?token={member.InviteToken}";

        await emailService.SendTeamInviteAsync(
            recipientEmail: request.InviteeEmail,
            recipientName: invitee.DisplayName,
            teamName: team.Name,
            inviteLink: inviteLink,
            expiresAt: member.InviteExpiresAt!.Value,
            cancellationToken: cancellationToken);

        return Result<string>.Success(member.InviteToken!);
    }
}
