using Microsoft.AspNetCore.SignalR;

namespace AnimStudio.API.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery.
/// Clients join a group named after their user ID to receive personal notifications.
/// The backend pushes "NewNotification" events when a notification is created.
/// </summary>
public sealed class NotificationsHub : Hub
{
    /// <summary>Subscribes the caller to their personal notification stream.</summary>
    public async Task JoinUserGroup(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
            throw new HubException("Invalid user ID.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }

    /// <summary>Removes the caller from their notification group.</summary>
    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
    }
}
