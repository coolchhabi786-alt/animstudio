using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AnimStudio.DeliveryModule.Application.Commands.CompleteRenderFromJob;

/// <summary>
/// Dispatched by <c>CompletionMessageProcessor</c> (API layer) when a PostProd job
/// completes. Finds the latest pending Render for the episode, stamps it with the
/// output URLs, and emits <c>RenderCompleteEvent</c> → SignalR notification.
/// </summary>
public sealed record CompleteRenderFromJobCommand(
    Guid    EpisodeId,
    string  ResultJson) : IRequest<Result<bool>>;

public sealed class CompleteRenderFromJobHandler(
    IRenderRepository renders,
    ILogger<CompleteRenderFromJobHandler> logger)
    : IRequestHandler<CompleteRenderFromJobCommand, Result<bool>>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<bool>> Handle(CompleteRenderFromJobCommand cmd, CancellationToken ct)
    {
        var render = await renders.GetLatestByEpisodeAsync(cmd.EpisodeId, ct);
        if (render is null)
        {
            logger.LogWarning(
                "CompleteRenderFromJob: no Render found for episode {EpisodeId}", cmd.EpisodeId);
            return Result<bool>.Failure("Render not found for episode", "NOT_FOUND");
        }

        PostProdPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<PostProdPayload>(cmd.ResultJson, JsonOpts);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "CompleteRenderFromJob: failed to parse PostProd result for episode {EpisodeId}",
                cmd.EpisodeId);
            return Result<bool>.Failure("PostProd result payload could not be parsed", "BAD_PAYLOAD");
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.VideoUrl))
        {
            logger.LogWarning(
                "CompleteRenderFromJob: PostProd result for episode {EpisodeId} missing videoUrl",
                cmd.EpisodeId);
            return Result<bool>.Failure("PostProd result missing videoUrl", "BAD_PAYLOAD");
        }

        render.MarkComplete(
            finalVideoUrl:   payload.VideoUrl,
            cdnUrl:          payload.VideoUrl,   // Python pipeline provides the CDN/blob URL directly
            srtUrl:          payload.SrtUrl,
            durationSeconds: payload.DurationSeconds);

        await renders.UpdateAsync(render, ct);
        await renders.SaveChangesAsync(ct);

        logger.LogInformation(
            "Render {RenderId} for episode {EpisodeId} marked Complete — duration={Duration:F1}s",
            render.Id, cmd.EpisodeId, payload.DurationSeconds);

        return Result<bool>.Success(true);
    }

    // RenderCompleteEvent is raised inside render.MarkComplete() and will be dispatched
    // by TransactionBehaviour / OutboxPublisherJob via MediatR IPublisher.

    private sealed record PostProdPayload(
        [property: System.Text.Json.Serialization.JsonPropertyName("videoUrl")]        string  VideoUrl,
        [property: System.Text.Json.Serialization.JsonPropertyName("srtUrl")]          string? SrtUrl,
        [property: System.Text.Json.Serialization.JsonPropertyName("durationSeconds")] double  DurationSeconds);
}
