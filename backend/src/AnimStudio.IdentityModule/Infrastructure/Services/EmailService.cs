using AnimStudio.IdentityModule.Application.Interfaces;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AnimStudio.IdentityModule.Infrastructure.Services;

internal sealed class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    private EmailClient CreateClient()
    {
        var connectionString = configuration["AzureCommunicationServices:ConnectionString"]
            ?? throw new InvalidOperationException("AzureCommunicationServices:ConnectionString is not configured.");
        return new EmailClient(connectionString);
    }

    private string SenderAddress => configuration["AzureCommunicationServices:SenderAddress"]
        ?? "noreply@animstudio.ai";

    public async Task SendTeamInviteAsync(
        string recipientEmail,
        string recipientName,
        string teamName,
        string inviteLink,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        var subject = $"You've been invited to join {teamName} on AnimStudio";
        var htmlBody = $"""
            <h2>You're invited!</h2>
            <p>Hi {recipientName},</p>
            <p><strong>{teamName}</strong> has invited you to collaborate on AnimStudio.</p>
            <p><a href="{inviteLink}" style="background:#4F46E5;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Accept Invitation</a></p>
            <p>This link expires on {expiresAt:MMMM dd, yyyy}.</p>
            <p>If you did not expect this invite, you can safely ignore this email.</p>
            """;

        await SendAsync(recipientEmail, subject, htmlBody, cancellationToken);
    }

    public async Task SendBillingAlertAsync(
        string recipientEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
        => await SendAsync(recipientEmail, subject, $"<p>{body}</p>", cancellationToken);

    private async Task SendAsync(
        string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var message = new EmailMessage(
                senderAddress: SenderAddress,
                recipients: new EmailRecipients([new EmailAddress(to)]),
                content: new EmailContent(subject) { Html = htmlBody });

            var operation = await client.SendAsync(
                Azure.WaitUntil.Started, message, cancellationToken);
            logger.LogInformation("Email queued for {To}, operationId={Id}", to, operation.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
