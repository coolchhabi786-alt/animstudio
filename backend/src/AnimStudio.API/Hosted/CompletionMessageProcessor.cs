using Azure.Messaging.ServiceBus;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Listens to the "completions" Service Bus queue and publishes received messages
/// as MediatR notifications so module handlers can react to pipeline completion events.
/// 
/// In local development this service is not registered (ServiceBusClient is not
/// configured when no connection string is present in appsettings.Development.json).
/// </summary>
public sealed class CompletionMessageProcessor(
    ServiceBusClient serviceBusClient,
    ILogger<CompletionMessageProcessor> logger) : BackgroundService, IAsyncDisposable
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = serviceBusClient.CreateProcessor("completions", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false,
        });

        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        logger.LogInformation("CompletionMessageProcessor started — listening on 'completions'");

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            // Message body is expected to be a JSON-serialized INotification
            logger.LogInformation("Received completion message {MessageId}", args.Message.MessageId);

            // Phase 2+ will implement concrete notification deserialization here
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing completion message {MessageId}", args.Message.MessageId);
            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception,
            "Service Bus error on {EntityPath}: {ErrorSource}",
            args.EntityPath, args.ErrorSource);
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
