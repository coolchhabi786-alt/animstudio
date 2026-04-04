using AnimStudio.IdentityModule.Domain.Entities;

namespace AnimStudio.IdentityModule.Domain.Interfaces;

/// <summary>Repository contract for <see cref="Team"/> aggregate persistence.</summary>
public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Team?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Finds the team that owns the pending invite matching <paramref name="token"/>.</summary>
    Task<Team?> GetByInviteTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamMember>> GetMembersAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);
    Task AddMemberAsync(TeamMember member, CancellationToken cancellationToken = default);
    Task UpdateMemberAsync(TeamMember member, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetMemberCountAsync(Guid teamId, CancellationToken cancellationToken = default);
}
