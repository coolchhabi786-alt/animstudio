namespace AnimStudio.SharedKernel.Interfaces;

/// <summary>
/// Abstraction for publishing messages to Azure Service Bus queues.
/// Inject <see cref="AzureServiceBusPublisher"/> in production and
/// <see cref="NoOpServiceBusPublisher"/> in local dev (no connection string).
/// </summary>
public interface IServiceBusPublisher
{
    /// <summary>
    /// Serialises <paramref name="message"/> as JSON and sends it to the
    /// specified Service Bus queue.
    /// </summary>
    /// <typeparam name="T">Message body type — must be JSON-serialisable.</typeparam>
    /// <param name="queueName">Target queue name (e.g. "jobs-queue").</param>
    /// <param name="message">The message payload.</param>
    /// <param name="sessionId">
    /// Optional Service Bus session identifier for ordered, session-aware queues.
    /// Use the <c>episodeId</c> string to group all episode messages in one session.
    /// The queue must have sessions enabled in Azure for this to take effect.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<T>(
        string           queueName,
        T                message,
        string?          sessionId = null,
        CancellationToken ct       = default);
}
