using AnimStudio.ContentModule.Application.Commands.CompleteCharacterJob;
using AnimStudio.ContentModule.Application.Commands.HandleJobCompletion;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.DeliveryModule.Application.Commands.CompleteRenderFromJob;
using Azure.Messaging.ServiceBus;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Listens on the "completions" Service Bus queue, deserialises each message to
/// <see cref="JobCompletionMessageDto"/>, and dispatches the appropriate MediatR commands.
///
/// Retry policy: abandon on first/second failure (Service Bus re-delivers);
/// dead-letter on the third delivery attempt.
///
/// Not registered in local development (no ServiceBusClient connection string).
/// </summary>
public sealed class CompletionMessageProcessor(
    ServiceBusClient     serviceBusClient,
    IServiceScopeFactory scopeFactory,
    IBackgroundJobClient backgroundJobs,
    ILogger<CompletionMessageProcessor> logger) : BackgroundService, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private const int MaxDeliveryAttempts = 3;

    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = serviceBusClient.CreateProcessor("completions-queue", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls    = 5,
            AutoCompleteMessages  = false,
        });

        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync   += OnErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        logger.LogInformation("CompletionMessageProcessor started — listening on 'completions-queue'");

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var ct = args.CancellationToken;
        JobCompletionMessageDto? dto = null;

        // BackgroundService runs in the root (singleton) scope.  MediatR pipeline
        // behaviours depend on scoped services (ICorrelationIdProvider etc.), so we
        // must create a fresh DI scope per message rather than using the injected ISender.
        await using var scope   = scopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            var body = args.Message.Body.ToString();

            dto = JsonSerializer.Deserialize<JobCompletionMessageDto>(body, JsonOpts);
            if (dto is null)
            {
                logger.LogWarning(
                    "CompletionMessageProcessor: message {MessageId} body could not be parsed — dead-lettering",
                    args.Message.MessageId);
                await args.DeadLetterMessageAsync(
                    args.Message, "DeserializationFailed", "Body was null after deserialization", ct);
                return;
            }

            var isSuccess  = string.Equals(dto.Status, "Completed", StringComparison.OrdinalIgnoreCase);
            var resultJson = dto.Result?.GetRawText();

            // ── Character training jobs are team-scoped, not episode-scoped ──────────
            // jobId == characterId (episodeId is repurposed the same way).
            if (dto.JobType is "CharacterDesign" or "LoraTraining")
            {
                var charId = dto.JobId; // jobId == characterId for character training jobs

                logger.LogInformation(
                    "Received {JobType} completion for character {CharId} — status={Status}",
                    dto.JobType, charId, dto.Status);

                var charResult = await mediator.Send(
                    new CompleteCharacterJobCommand(charId, isSuccess, resultJson, dto.ErrorMessage, dto.JobType), ct);

                if (!charResult.IsSuccess)
                    logger.LogWarning(
                        "CompleteCharacterJob returned failure for {CharId}: {Error}",
                        charId, charResult.Error);

                // After CharacterDesign succeeds, enqueue LoRA training dispatch
                if (isSuccess && dto.JobType == "CharacterDesign")
                    backgroundJobs.Enqueue<CharacterTrainingHangfireProcessor>(
                        x => x.DispatchLoraTrainingAsync(charId, CancellationToken.None));

                await args.CompleteMessageAsync(args.Message, ct);
                return;
            }

            logger.LogInformation(
                "Received {JobType} completion for episode {EpisodeId} — job={JobId}, status={Status}, " +
                "pipelineDuration={PipelineMs:N0}ms, deliveryCount={DeliveryCount}",
                dto.JobType, dto.EpisodeId, dto.JobId, dto.Status,
                (DateTimeOffset.UtcNow - dto.CompletedAt).TotalMilliseconds,
                args.Message.DeliveryCount);

            // ── Step 1: update Job + Episode + type-specific entities (ContentModule) ──
            var contentResult = await mediator.Send(
                new HandleJobCompletionCommand(dto.JobId, isSuccess, resultJson, dto.ErrorMessage), ct);

            if (!contentResult.IsSuccess)
            {
                logger.LogWarning(
                    "HandleJobCompletion returned failure for job {JobId}: {Error}",
                    dto.JobId, contentResult.Error);
            }

            // ── Step 2: auto-dispatch the next stage where needed ─────────────────────
            if (isSuccess)
            {
                // After StoryboardPlan completes, automatically dispatch StoryboardGen
                // so the user doesn't have to trigger a second API call.
                if (string.Equals(dto.JobType, "StoryboardPlan", StringComparison.OrdinalIgnoreCase))
                {
                    backgroundJobs.Enqueue<EpisodeJobDispatchHangfireProcessor>(
                        x => x.DispatchStoryboardGenJobAsync(dto.EpisodeId, CancellationToken.None));

                    logger.LogInformation(
                        "StoryboardPlan completed — enqueued StoryboardGen dispatch for episode {EpisodeId}",
                        dto.EpisodeId);
                }

                // After PostProd completes, update the Render aggregate (DeliveryModule).
                if (string.Equals(dto.JobType, "PostProd", StringComparison.OrdinalIgnoreCase)
                    && resultJson is not null)
                {
                    var renderResult = await mediator.Send(
                        new CompleteRenderFromJobCommand(dto.EpisodeId, resultJson), ct);

                    if (!renderResult.IsSuccess)
                    {
                        logger.LogWarning(
                            "CompleteRenderFromJob returned failure for episode {EpisodeId}: {Error}",
                            dto.EpisodeId, renderResult.Error);
                    }
                }
            }

            await args.CompleteMessageAsync(args.Message, ct);
        }
        catch (Exception ex)
        {
            if (args.Message.DeliveryCount >= MaxDeliveryAttempts)
            {
                logger.LogError(ex,
                    "Message {MessageId} exceeded {Max} delivery attempts — dead-lettering " +
                    "(job={JobId}, type={JobType}, episode={EpisodeId})",
                    args.Message.MessageId, MaxDeliveryAttempts,
                    dto?.JobId, dto?.JobType, dto?.EpisodeId);

                await args.DeadLetterMessageAsync(
                    args.Message, "ExceededRetries", ex.Message, ct);
            }
            else
            {
                logger.LogWarning(ex,
                    "Message {MessageId} failed on attempt {Attempt}/{Max} — abandoning for retry " +
                    "(job={JobId}, type={JobType})",
                    args.Message.MessageId, args.Message.DeliveryCount, MaxDeliveryAttempts,
                    dto?.JobId, dto?.JobType);

                await args.AbandonMessageAsync(args.Message, cancellationToken: ct);
            }
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
