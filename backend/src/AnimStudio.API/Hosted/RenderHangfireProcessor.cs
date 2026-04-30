using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.DeliveryModule.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Hangfire job that processes a render asynchronously after it is created by
/// <see cref="Controllers.RenderController"/>.
///
/// <para>
/// <b>Local backend</b>: looks for a pre-rendered MP4 at
/// <c>{FileStorage:LocalRootPath}/renders/{episodeId}/{aspectRatio}.mp4</c>
/// and marks the render Complete if found. If no file exists, marks it Failed
/// (no actual video transcoding is performed in dev).
/// </para>
///
/// <para>
/// <b>Production</b>: the Python pipeline picks up the render job from Service Bus,
/// runs FFmpeg, uploads to Blob Storage, and posts results back via a webhook.
/// This processor is not invoked in that flow.
/// </para>
/// </summary>
public sealed class RenderHangfireProcessor(
    IRenderRepository renders,
    IMediator mediator,
    IConfiguration configuration,
    ILogger<RenderHangfireProcessor> logger)
{
    public async Task ProcessAsync(Guid renderId, CancellationToken ct = default)
    {
        var render = await renders.GetByIdAsync(renderId, ct);
        if (render is null)
        {
            logger.LogWarning("RenderHangfireProcessor: render {Id} not found — aborting", renderId);
            return;
        }

        if (render.Status is RenderStatus.Complete or RenderStatus.Failed)
        {
            logger.LogInformation("RenderHangfireProcessor: render {Id} already terminal — skipping", renderId);
            return;
        }

        render.MarkRendering();
        await renders.UpdateAsync(render, ct);
        await renders.SaveChangesAsync(ct);

        logger.LogInformation(
            "RenderHangfireProcessor: processing render {Id}, episode={EpisodeId}, ratio={Ratio}",
            renderId, render.EpisodeId, render.AspectRatio);

        try
        {
            var rootPath = configuration["FileStorage:LocalRootPath"];
            var backendBase = (configuration["FileStorage:BackendBaseUrl"] ?? "http://localhost:5001").TrimEnd('/');

            string? cdnUrl = null;
            double durationSeconds = 0;

            if (!string.IsNullOrWhiteSpace(rootPath))
            {
                // Look for: {rootPath}/renders/{episodeId}/{SixteenNine|NineSixteen|OneOne}.mp4
                var ratioFileName = $"{render.AspectRatio}.mp4";
                var localPath = Path.GetFullPath(Path.Combine(
                    rootPath, "renders", render.EpisodeId.ToString(), ratioFileName));

                if (File.Exists(localPath))
                {
                    var info = new FileInfo(localPath);
                    durationSeconds = Math.Max(1.0, info.Length / 500_000.0);
                    var relativePath = $"renders/{render.EpisodeId}/{ratioFileName}";
                    cdnUrl = $"{backendBase}/api/v1/files/{relativePath}";
                    logger.LogInformation(
                        "RenderHangfireProcessor: found local render file — {Path}", localPath);
                }
                else
                {
                    logger.LogWarning(
                        "RenderHangfireProcessor: no local render file at {Path} — completing without video",
                        localPath);
                }
            }

            render.MarkComplete(cdnUrl, cdnUrl, srtUrl: null, durationSeconds);
            await renders.UpdateAsync(render, ct);
            await renders.SaveChangesAsync(ct);

            // Publish domain events (SignalR notifications)
            foreach (var evt in render.DomainEvents)
                await mediator.Publish(evt, ct);
            render.ClearDomainEvents();

            logger.LogInformation(
                "RenderHangfireProcessor: render {Id} completed — cdnUrl={Url}", renderId, cdnUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RenderHangfireProcessor: unhandled error for render {Id}", renderId);
            render.MarkFailed(ex.Message);
            await renders.UpdateAsync(render, ct);
            await renders.SaveChangesAsync(ct);
            throw; // Hangfire will retry
        }
    }
}
