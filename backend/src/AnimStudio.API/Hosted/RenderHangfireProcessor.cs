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
                // Strategy 1: episode-specific render at renders/{episodeId}/{Ratio}.mp4
                // This is where production output (or a manually placed dev file) lives.
                var ratioFileName = $"{render.AspectRatio}.mp4";
                var episodePath = Path.GetFullPath(Path.Combine(
                    rootPath, "renders", render.EpisodeId.ToString(), ratioFileName));

                string? foundPath = null;
                string? foundRelative = null;

                if (File.Exists(episodePath))
                {
                    foundPath     = episodePath;
                    foundRelative = $"renders/{render.EpisodeId}/{ratioFileName}";
                }
                else
                {
                    // Strategy 2: fall back to any .mp4 in the final/ directory.
                    // In local dev the pipeline writes finished episodes here with
                    // descriptive names (e.g. The_Superpowered_Shenanigans_of_Mr._Whiskers_episode.mp4).
                    var finalDir = Path.Combine(rootPath, "final");
                    var candidate = Directory.Exists(finalDir)
                        ? Directory.EnumerateFiles(finalDir, "*.mp4")
                              .OrderByDescending(f => new FileInfo(f).Length)
                              .FirstOrDefault()
                        : null;

                    if (candidate is not null)
                    {
                        foundPath     = candidate;
                        foundRelative = $"final/{Path.GetFileName(candidate)}";
                        logger.LogInformation(
                            "RenderHangfireProcessor: no episode-specific file at {Primary} — " +
                            "using fallback final/ file {Fallback}", episodePath, candidate);
                    }
                }

                if (foundPath is not null && foundRelative is not null)
                {
                    var info = new FileInfo(foundPath);
                    durationSeconds = GetActualDurationSeconds(foundPath);
                    cdnUrl = $"{backendBase}/api/v1/files/{foundRelative}";
                    logger.LogInformation(
                        "RenderHangfireProcessor: serving render from {Path} ({Dur:F1}s)", foundPath, durationSeconds);
                }
                else
                {
                    logger.LogWarning(
                        "RenderHangfireProcessor: no render file found — completing without video URL. " +
                        "Place a file at {Expected} or add any .mp4 to {FinalDir}",
                        episodePath, Path.Combine(rootPath, "final"));
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

    /// <summary>
    /// Uses ffprobe to read the exact video duration. Falls back to a file-size
    /// heuristic when ffprobe is not installed (same estimate as the old code).
    /// </summary>
    private static double GetActualDurationSeconds(string path)
    {
        try
        {
            using var proc = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName               = "ffprobe",
                    Arguments              = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                },
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit();
            if (double.TryParse(output, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out var secs))
                return secs;
        }
        catch { /* ffprobe not available */ }

        // Fallback: rough estimate from file size (500 KB/s ≈ 4 Mbps bitrate)
        return Math.Max(1.0, new FileInfo(path).Length / 500_000.0);
    }
}
