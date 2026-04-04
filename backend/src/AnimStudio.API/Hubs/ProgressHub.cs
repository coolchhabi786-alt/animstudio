using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Hubs;

/// <summary>
/// SignalR hub for real-time episode generation progress updates.
/// Clients join a group named after their team ID to receive updates for all team episodes.
/// </summary>
public sealed class ProgressHub : Hub
{
    /// <summary>Subscribes the caller to progress events for their team.</summary>
    public async Task JoinTeamGroup(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId) || !Guid.TryParse(teamId, out _))
            throw new HubException("Invalid team ID.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"team:{teamId}");
    }

    /// <summary>Removes the caller from their team progress group.</summary>
    public async Task LeaveTeamGroup(string teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team:{teamId}");
    }
}
