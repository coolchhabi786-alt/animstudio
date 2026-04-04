using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetTeamMembers;

internal sealed class GetTeamMembersQueryHandler(
    ITeamRepository teamRepository) : IRequestHandler<GetTeamMembersQuery, Result<IReadOnlyList<TeamMemberDto>>>
{
    public async Task<Result<IReadOnlyList<TeamMemberDto>>> Handle(
        GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await teamRepository.GetMembersAsync(request.TeamId, cancellationToken);

        var dtos = members.Select(m => new TeamMemberDto(
            UserId: m.UserId,
            Email: m.User?.Email ?? string.Empty,
            DisplayName: m.User?.DisplayName ?? string.Empty,
            AvatarUrl: m.User?.AvatarUrl,
            Role: m.Role.ToString(),
            IsAccepted: m.InviteAcceptedAt.HasValue,
            JoinedAt: m.JoinedAt)).ToList();

        return Result<IReadOnlyList<TeamMemberDto>>.Success(dtos);
    }
}
