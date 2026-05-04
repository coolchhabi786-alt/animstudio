using AnimStudio.SharedKernel.Interfaces;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AnimStudio.API.Services;

/// <summary>
/// Production implementation of <see cref="IServiceBusPublisher"/> backed by
/// Azure Service Bus with DefaultAzureCredential (Managed Identity in prod, az login in dev).
/// Registered when <c>ServiceBus:Namespace</c> is non-empty in configuration.
/// </summary>
public sealed class AzureServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusPublisher> _logger;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public AzureServiceBusPublisher(
        IConfiguration configuration,
        ILogger<AzureServiceBusPublisher> logger)
    {
        var ns = configuration["ServiceBus:Namespace"]
            ?? throw new InvalidOperationException(
                "ServiceBus:Namespace is required for AzureServiceBusPublisher.");

        // Fully-qualified namespace: "myns.servicebus.windows.net"
        var fqns = ns.Contains('.') ? ns : $"{ns}.servicebus.windows.net";

        _client = new ServiceBusClient(fqns, new DefaultAzureCredential());
        _logger = logger;
    }

    public async Task PublishAsync<T>(
        string            queueName,
        T                 message,
        string?           sessionId = null,
        CancellationToken ct        = default)
    {
        var json   = JsonSerializer.Serialize(message, JsonOpts);
        var sbMsg  = new ServiceBusMessage(BinaryData.FromString(json))
        {
            ContentType = "application/json",
        };

        if (!string.IsNullOrWhiteSpace(sessionId))
            sbMsg.SessionId = sessionId;

        var sender = _senders.GetOrAdd(queueName, q => _client.CreateSender(q));
        await sender.SendMessageAsync(sbMsg, ct);

        _logger.LogDebug(
            "Published message to queue '{Queue}' (sessionId={SessionId}): {Body}",
            queueName, sessionId ?? "(none)", json);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();

        await _client.DisposeAsync();
    }
}
