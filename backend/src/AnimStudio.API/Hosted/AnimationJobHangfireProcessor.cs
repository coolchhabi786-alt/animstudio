using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

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
/// <b>Kling backend</b>: publishes an <see cref="AnimationJobMessage"/> to the
/// Azure Service Bus <c>jobs-queue</c> so the Python cartoon_automation worker
/// picks it up. The Hangfire job then exits; clips remain Pending until the Python
/// worker posts results back via the <c>completions-queue</c>.
/// </para>
/// </summary>
public sealed class AnimationJobHangfireProcessor(
    IAnimationJobRepository  animationJobs,
    IAnimationClipRepository animationClips,
    IJobRepository           jobs,
    IStoryboardRepository    storyboards,
    IScriptRepository        scripts,
    IFileStorageService      fileStorage,
    IServiceBusPublisher     serviceBusPublisher,
    IMediator                mediator,
    IConfiguration           configuration,
    ILogger<AnimationJobHangfireProcessor> logger)
{
    private const string DefaultSubfolder = "23MarAnimation";
    private const string JobsQueue        = "jobs-queue";

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
            logger.LogInformation(
                "AnimationJobHangfireProcessor: job {Id} is already {Status} — skipping",
                animationJobId, animJob.Status);
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

                    // Reload clips and finalise AnimationJob status synchronously.
                    var clips     = await animationClips.GetByEpisodeIdAsync(animJob.EpisodeId, ct);
                    var failCount = clips.Count(c => c.Status == ClipStatus.Failed);
                    var readyCount = clips.Count(c => c.Status == ClipStatus.Ready);

                    if (failCount == clips.Count)
                        animJob.MarkFailed();
                    else
                        animJob.MarkCompleted(0m);

                    await animationJobs.UpdateAsync(animJob, ct);

                    logger.LogInformation(
                        "AnimationJobHangfireProcessor: local job {Id} finished — {Ready} ready, {Failed} failed",
                        animationJobId, readyCount, failCount);
                    break;

                case AnimationBackend.Kling:
                    // Fire-and-forget: publish message to Service Bus then exit.
                    // AnimationJob stays Running; HandleJobCompletionHandler finalises it
                    // when the Python pipeline posts to completions-queue.
                    await ProcessKlingAsync(animJob, ct);
                    break;

                default:
                    logger.LogWarning(
                        "AnimationJobHangfireProcessor: unknown backend {Backend}", animJob.Backend);
                    animJob.MarkFailed();
                    await animationJobs.UpdateAsync(animJob, ct);
                    break;
            }
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

    // ── Kling / Service Bus dispatch ────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web) { WriteIndented = false };

    private async Task ProcessKlingAsync(AnimationJob animJob, CancellationToken ct)
    {
        var episodeId = animJob.EpisodeId;

        // Find the generic Job row — its ID is echoed back by Python in the completion message.
        var allJobs   = await jobs.GetByEpisodeIdAsync(episodeId, ct);
        var workerJob = allJobs
            .Where(j => j.Type == JobType.Animation)
            .MaxBy(j => j.CreatedAt);

        var storyboard = await storyboards.GetByEpisodeIdAsync(episodeId, ct);
        var script     = await scripts.GetByEpisodeIdAsync(episodeId, ct);

        if (storyboard is null)
        {
            logger.LogWarning("ProcessKling: no storyboard found for episode {EpisodeId} — aborting", episodeId);
            animJob.MarkFailed();
            await animationJobs.UpdateAsync(animJob, ct);
            return;
        }

        if (script is null)
        {
            logger.LogWarning("ProcessKling: no script found for episode {EpisodeId} — aborting", episodeId);
            animJob.MarkFailed();
            await animationJobs.UpdateAsync(animJob, ct);
            return;
        }

        // Rebuild the Storyboard model JSON that Python's execute_animation expects.
        // Python: Storyboard { screenplay_title, prompts: [{ scene_number, prompt,
        //   character_description, background_description, image_urls }] }
        var promptsByScene = storyboard.Shots
            .GroupBy(s => s.SceneNumber)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var orderedShots = g.OrderBy(s => s.ShotIndex).ToList();
                return new
                {
                    scene_number            = g.Key,
                    prompt                  = orderedShots.FirstOrDefault()?.Description ?? string.Empty,
                    character_description   = string.Empty,
                    background_description  = string.Empty,
                    image_urls              = orderedShots
                                                 .Select(s => s.ImageUrl ?? string.Empty)
                                                 .ToList(),
                };
            })
            .ToList();

        var storyboardJson = JsonSerializer.Serialize(new
        {
            screenplay_title = storyboard.ScreenplayTitle,
            prompts          = promptsByScene,
        }, JsonOpts);

        var message = new AnimationJobMessage(
            JobId:       workerJob?.Id ?? Guid.NewGuid(),
            EpisodeId:   episodeId,
            JobType:     "Animation",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload: new
            {
                storyboardJson,
                screenplayJson               = script.RawJson,
                animationDurationInstruction = "Use '5' for simple scenes, '10' for complex scenes.",
            });

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: episodeId.ToString(), ct: ct);

        logger.LogInformation(
            "ProcessKling: dispatched Animation job to '{Queue}' — episode={EpisodeId}, jobId={JobId}, scenes={SceneCount}",
            JobsQueue, episodeId, message.JobId, promptsByScene.Count);
    }

    // ── Message DTO ───────────────────────────────────────────────────────────

    private sealed record AnimationJobMessage(
        [property: JsonPropertyName("jobId")]       Guid           JobId,
        [property: JsonPropertyName("episodeId")]   Guid           EpisodeId,
        [property: JsonPropertyName("jobType")]     string         JobType,
        [property: JsonPropertyName("requestedAt")] DateTimeOffset RequestedAt,
        [property: JsonPropertyName("payload")]     object         Payload);
}
