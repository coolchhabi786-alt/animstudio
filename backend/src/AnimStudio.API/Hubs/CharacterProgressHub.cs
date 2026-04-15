using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Hubs;

/// <summary>
/// SignalR hub for real-time character training progress updates.
/// Clients join the group <c>team:{teamId}</c> to receive updates for all
/// characters belonging to their team.
/// 
/// Message method: <c>CharacterTrainingUpdate</c>
/// Payload: <c>{ characterId, status, progressPercent, stage }</c>
/// </summary>
public sealed class CharacterProgressHub : Hub
{
    /// <summary>
    /// Subscribes the calling client to training events for the given team.
    /// Reuses the same group name as <see cref="ProgressHub"/> so a single
    /// frontend <c>JoinTeamGroup</c> call covers both episode and character updates.
    /// </summary>
    public async Task JoinTeamGroup(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId) || !Guid.TryParse(teamId, out _))
            throw new HubException("Invalid team ID.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"team:{teamId}");
    }

    /// <summary>Removes the calling client from the team's training event group.</summary>
    public async Task LeaveTeamGroup(string teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team:{teamId}");
    }
}
