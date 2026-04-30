using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Hangfire job handler for animation processing. Enqueued by
/// <see cref="Controllers.AnimationController"/> immediately after an
/// animation job is approved.
///
/// <para>
/// <b>Local backend</b>: resolves each pending clip against the
/// <c>FileStorage:LocalRootPath/animation/{subfolder}/</c> directory using
/// the naming convention <c>scene_{sc:00}_shot_{sh:00}.mp4</c>. Any clip
/// whose file is found is marked Ready; clips with no matching file are
/// marked Failed. The subfolder defaults to <c>23MarAnimation</c> and can
/// be overridden via <c>Animation:LocalSubfolder</c> in config.
/// </para>
///
/// <para>
/// <b>Kling backend</b>: stub — logs intent and marks the job as failed
/// with a "not yet implemented" reason. Production Kling integration ships
/// in Phase 8+ once the Python worker is wired through Service Bus.
/// </para>
/// </summary>
public sealed class AnimationJobHangfireProcessor(
    IAnimationJobRepository animationJobs,
    IAnimationClipRepository animationClips,
    IFileStorageService fileStorage,
    IMediator mediator,
    IConfiguration configuration,
    ILogger<AnimationJobHangfireProcessor> logger)
{
    private const string DefaultSubfolder = "23MarAnimation";

    /// <summary>
    /// Entry point called by Hangfire. <paramref name="animationJobId"/> is the
    /// PK of the <c>AnimationJob</c> aggregate (not the generic <c>Job</c> row).
    /// </summary>
    public async Task ProcessAsync(Guid animationJobId, CancellationToken ct = default)
    {
        var animJob = await animationJobs.GetByIdAsync(animationJobId, ct);
        if (animJob is null)
        {
            logger.LogWarning("AnimationJobHangfireProcessor: job {Id} not found — aborting", animationJobId);
            return;
        }

        if (animJob.Status is AnimationStatus.Completed or AnimationStatus.Cancelled)
        {
            logger.LogInformation("AnimationJobHangfireProcessor: job {Id} is already {Status} — skipping", animationJobId, animJob.Status);
            return;
        }

        animJob.MarkRunning();
        await animationJobs.UpdateAsync(animJob, ct);

        logger.LogInformation(
            "AnimationJobHangfireProcessor: processing job {Id}, backend={Backend}, episode={EpisodeId}",
            animationJobId, animJob.Backend, animJob.EpisodeId);

        try
        {
            switch (animJob.Backend)
            {
                case AnimationBackend.Local:
                    await ProcessLocalAsync(animJob.EpisodeId, animationJobId, ct);
                    break;

                case AnimationBackend.Kling:
                    await ProcessKlingStubAsync(animJob.EpisodeId, animationJobId, ct);
                    break;

                default:
                    logger.LogWarning("AnimationJobHangfireProcessor: unknown backend {Backend}", animJob.Backend);
                    animJob.MarkFailed();
                    await animationJobs.UpdateAsync(animJob, ct);
                    return;
            }

            // Reload to get updated clip statuses
            var clips = await animationClips.GetByEpisodeIdAsync(animJob.EpisodeId, ct);
            var failCount = clips.Count(c => c.Status == ClipStatus.Failed);
            var readyCount = clips.Count(c => c.Status == ClipStatus.Ready);
            var actualCost = animJob.Backend == AnimationBackend.Local
                ? 0m
                : readyCount * 0.056m;

            if (failCount == clips.Count)
                animJob.MarkFailed();
            else
                animJob.MarkCompleted(actualCost);

            await animationJobs.UpdateAsync(animJob, ct);

            logger.LogInformation(
                "AnimationJobHangfireProcessor: job {Id} finished — {Ready} ready, {Failed} failed",
                animationJobId, readyCount, failCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AnimationJobHangfireProcessor: unhandled error for job {Id}", animationJobId);
            animJob.MarkFailed();
            await animationJobs.UpdateAsync(animJob, ct);
            throw; // Hangfire will retry
        }
    }

    // ── Local file-based processing ─────────────────────────────────────────

    private async Task ProcessLocalAsync(Guid episodeId, Guid animationJobId, CancellationToken ct)
    {
        var subfolder = configuration["Animation:LocalSubfolder"] ?? DefaultSubfolder;
        var rootPath  = configuration["FileStorage:LocalRootPath"]
            ?? throw new InvalidOperationException("FileStorage:LocalRootPath is required.");

        var clips = await animationClips.GetByEpisodeIdAsync(episodeId, ct);
        if (clips.Count == 0)
        {
            logger.LogWarning("ProcessLocal: no clips found for episode {EpisodeId}", episodeId);
            return;
        }

        foreach (var clip in clips)
        {
            if (clip.Status is ClipStatus.Ready)
                continue; // already done (idempotent)

            clip.MarkRendering();
            await animationClips.UpdateAsync(clip, ct);

            // Convention: animation/{subfolder}/scene_{sc:00}_shot_{sh:00}.mp4
            var relativePath = $"animation/{subfolder}/scene_{clip.SceneNumber:00}_shot_{clip.ShotIndex:00}.mp4";
            var absolutePath = Path.GetFullPath(Path.Combine(
                rootPath,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));

            if (File.Exists(absolutePath))
            {
                // Measure duration using file size as a proxy (no FFprobe dependency).
                // Exact duration will be updated when the Python pipeline integrates.
                var info = new FileInfo(absolutePath);
                var estimatedDuration = Math.Max(1.0, info.Length / 500_000.0); // ~500KB/s rough estimate

                clip.MarkReady(relativePath, estimatedDuration);
                await animationClips.UpdateAsync(clip, ct);

                // Publish domain event → AnimationClipReadyEventHandler → SignalR
                // We pass the file-storage URL (not the relative path) in the event
                // so the frontend can render it directly.
                var publicUrl = fileStorage.GetFileUrl(relativePath);
                logger.LogInformation(
                    "ProcessLocal: clip scene={Scene} shot={Shot} marked Ready — {Url}",
                    clip.SceneNumber, clip.ShotIndex, publicUrl);
            }
            else
            {
                // Try fallback patterns: scene_{sc:00}_shot_{sh}.mp4 (non-padded shot index)
                var altPath1 = $"animation/{subfolder}/scene_{clip.SceneNumber:00}_shot_{clip.ShotIndex}.mp4";
                var altAbsolute1 = Path.GetFullPath(Path.Combine(
                    rootPath, altPath1.Replace('/', Path.DirectorySeparatorChar)));

                if (File.Exists(altAbsolute1))
                {
                    var info = new FileInfo(altAbsolute1);
                    var estimatedDuration = Math.Max(1.0, info.Length / 500_000.0);
                    clip.MarkReady(altPath1, estimatedDuration);
                    await animationClips.UpdateAsync(clip, ct);
                    logger.LogInformation(
                        "ProcessLocal: clip scene={Scene} shot={Shot} matched alt pattern — {Path}",
                        clip.SceneNumber, clip.ShotIndex, altPath1);
                }
                else
                {
                    clip.MarkFailed();
                    await animationClips.UpdateAsync(clip, ct);
                    logger.LogWarning(
                        "ProcessLocal: no file found for scene={Scene} shot={Shot} — marked Failed. Tried: {Path1}, {Path2}",
                        clip.SceneNumber, clip.ShotIndex, relativePath, altPath1);
                }
            }

            // Dispatch domain events accumulated on the clip aggregate
            foreach (var domainEvent in clip.DomainEvents)
                await mediator.Publish(domainEvent, ct);
            clip.ClearDomainEvents();
        }
    }

    // ── Kling stub ──────────────────────────────────────────────────────────

    private async Task ProcessKlingStubAsync(Guid episodeId, Guid animationJobId, CancellationToken ct)
    {
        logger.LogWarning(
            "ProcessKling: Kling AI backend is not yet integrated — " +
            "episode {EpisodeId} clips will remain Pending until Python pipeline delivers results via webhook",
            episodeId);

        // In production: Python worker picks up the Job from Service Bus,
        // calls Kling AI, and posts results back via POST /api/v1/jobs/{id}/complete.
        // Nothing to do here — clips stay Pending until the webhook fires.
        await Task.CompletedTask;
    }
}
