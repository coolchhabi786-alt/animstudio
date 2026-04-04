namespace AnimStudio.IdentityModule.Application.Interfaces;

/// <summary>Abstraction for sending transactional emails via Azure Communication Services.</summary>
public interface IEmailService
{
    /// <summary>Sends a team invitation email to the specified recipient.</summary>
    Task SendTeamInviteAsync(
        string recipientEmail,
        string recipientName,
        string teamName,
        string inviteLink,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);

    /// <summary>Sends a billing alert email (e.g. 80% usage, payment failed).</summary>
    Task SendBillingAlertAsync(
        string recipientEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
