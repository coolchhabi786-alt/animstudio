using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.IdentityModule.Infrastructure.Repositories;

internal sealed class TeamRepository(IdentityDbContext db) : ITeamRepository
{
    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Teams.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Team?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Teams
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Subscription).ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Team?> GetByInviteTokenAsync(string token, CancellationToken cancellationToken = default)
        => await db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Members.Any(m => m.InviteToken == token), cancellationToken);

    public async Task<IReadOnlyList<TeamMember>> GetMembersAsync(Guid teamId, CancellationToken cancellationToken = default)
        => await db.TeamMembers
            .Include(m => m.User)
            .Where(m => m.TeamId == teamId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        await db.Teams.AddAsync(team, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        db.Teams.Update(team);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMemberAsync(TeamMember member, CancellationToken cancellationToken = default)
    {
        await db.TeamMembers.AddAsync(member, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMemberAsync(TeamMember member, CancellationToken cancellationToken = default)
    {
        db.TeamMembers.Update(member);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await db.TeamMembers.FindAsync([teamId, userId], cancellationToken);
        if (member is not null)
        {
            db.TeamMembers.Remove(member);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetMemberCountAsync(Guid teamId, CancellationToken cancellationToken = default)
        => await db.TeamMembers.CountAsync(m => m.TeamId == teamId && m.InviteAcceptedAt.HasValue, cancellationToken);
}
