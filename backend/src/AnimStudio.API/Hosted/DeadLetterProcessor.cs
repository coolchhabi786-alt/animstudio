using Azure.Messaging.ServiceBus;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Monitors the Service Bus dead-letter queue and logs poison messages to
/// Application Insights for alerting and manual remediation.
/// 
/// Not registered in local development (no ServiceBusClient configured).
/// </summary>
public sealed class DeadLetterProcessor(
    ServiceBusClient serviceBusClient,
    ILogger<DeadLetterProcessor> logger) : BackgroundService, IAsyncDisposable
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Listen on the dead-letter sub-queue of the "completions" queue
        _processor = serviceBusClient.CreateProcessor(
            "completions",
            new ServiceBusProcessorOptions
            {
                SubQueue = SubQueue.DeadLetter,
                MaxConcurrentCalls = 2,
                AutoCompleteMessages = false,
            });

        _processor.ProcessMessageAsync += OnDeadLetterAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        logger.LogInformation("DeadLetterProcessor started — monitoring dead-letter queue");

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
    }

    private async Task OnDeadLetterAsync(ProcessMessageEventArgs args)
    {
        // Log with enough detail for an Application Insights alert to fire
        logger.LogError(
            "DEAD LETTER MESSAGE detected — MessageId: {MessageId}, " +
            "DeadLetterReason: {Reason}, DeadLetterErrorDescription: {Description}, " +
            "Body: {Body}",
            args.Message.MessageId,
            args.Message.DeadLetterReason,
            args.Message.DeadLetterErrorDescription,
            args.Message.Body.ToString());

        // Complete to remove from DLQ after logging (prevents infinitely growing DLQ)
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception,
            "DeadLetterProcessor error on {EntityPath}", args.EntityPath);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync();
            await _processor.DisposeAsync();
        }
        base.Dispose();
    }
}
