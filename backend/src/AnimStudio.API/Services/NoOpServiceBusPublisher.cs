using AnimStudio.SharedKernel.Interfaces;
using System.Text.Json;

namespace AnimStudio.API.Services;

/// <summary>
/// Local-dev stub for <see cref="IServiceBusPublisher"/> — logs what would be sent
/// instead of talking to Azure. Registered when <c>ServiceBus:Namespace</c> is absent.
/// </summary>
public sealed class NoOpServiceBusPublisher(ILogger<NoOpServiceBusPublisher> logger)
    : IServiceBusPublisher
{
    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web) { WriteIndented = false };

    public Task PublishAsync<T>(
        string            queueName,
        T                 message,
        string?           sessionId = null,
        CancellationToken ct        = default)
    {
        logger.LogInformation(
            "[NoOp Service Bus] Would publish to '{Queue}' (sessionId={SessionId}): {Body}",
            queueName,
            sessionId ?? "(none)",
            JsonSerializer.Serialize(message, JsonOpts));

        return Task.CompletedTask;
    }
}
