using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.SharedKernel.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Hangfire processor that dispatches episode-scoped AI jobs to the Python worker
/// via Azure Service Bus. Handles Script, StoryboardPlan, StoryboardGen, and PostProd.
///
/// Separation of concerns:
///   • MediatR commands (GenerateScript, GenerateStoryboard, etc.) create Job entities
///     in the database and return the jobId.
///   • Controllers enqueue a Hangfire job passing that jobId.
///   • This processor loads all required data from the DB, builds the correctly
///     formatted payload, and publishes to "jobs-queue".
///
/// In local dev (NoOpServiceBusPublisher registered), PublishAsync is a no-op that
/// logs what would be sent. No code-path changes are needed between environments.
/// </summary>
public sealed class EpisodeJobDispatchHangfireProcessor(
    IEpisodeRepository       episodes,
    IJobRepository           jobs,
    IScriptRepository        scripts,
    IStoryboardRepository    storyboards,
    ICharacterRepository     characters,
    IAnimationClipRepository animationClips,
    IServiceBusPublisher     serviceBusPublisher,
    IRenderRepository        renders,
    ILogger<EpisodeJobDispatchHangfireProcessor> logger)
{
    private const string JobsQueue = "jobs-queue";

    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web) { WriteIndented = false };

    // ── Script ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Dispatches an existing Script <see cref="Job"/> (created by GenerateScriptCommand
    /// or RegenerateScriptCommand) to the Python worker.
    /// Payload: { idea, rosterJson, characterAliasesJson }
    /// </summary>
    public async Task DispatchScriptJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await jobs.GetByIdAsync(jobId, ct);
        if (job is null)
        {
            logger.LogWarning("DispatchScriptJob: job {JobId} not found — aborting", jobId);
            return;
        }

        var episode = await episodes.GetByIdAsync(job.EpisodeId, ct);
        if (episode is null)
        {
            logger.LogWarning("DispatchScriptJob: episode {EpisodeId} not found — aborting", job.EpisodeId);
            return;
        }

        var roster = await characters.GetByEpisodeIdAsync(job.EpisodeId, ct);
        var rosterJson   = BuildRosterJson(roster);

        var message = new EpisodeJobMessage(
            JobId:       jobId,
            EpisodeId:   job.EpisodeId,
            JobType:     "Script",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload: new
            {
                idea                  = episode.Idea ?? episode.Name,
                rosterJson,
                characterAliasesJson  = "{}",
                characterPreferences  = episode.CharacterPreferences,
            });

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: job.EpisodeId.ToString(), ct: ct);

        logger.LogInformation(
            "Script job dispatched — jobId={JobId}, episodeId={EpisodeId}, idea={Idea}",
            jobId, job.EpisodeId,
            (episode.Idea ?? episode.Name)?[..Math.Min(60, (episode.Idea ?? episode.Name)!.Length)]);
    }

    // ── StoryboardPlan ────────────────────────────────────────────────────────

    /// <summary>
    /// Dispatches an existing StoryboardPlan <see cref="Job"/> to the Python worker.
    /// Payload: { screenplayJson, rosterJson, characterAliasesJson }
    /// </summary>
    public async Task DispatchStoryboardPlanJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await jobs.GetByIdAsync(jobId, ct);
        if (job is null)
        {
            logger.LogWarning("DispatchStoryboardPlanJob: job {JobId} not found — aborting", jobId);
            return;
        }

        var script = await scripts.GetByEpisodeIdAsync(job.EpisodeId, ct);
        if (script is null)
        {
            logger.LogWarning(
                "DispatchStoryboardPlanJob: no script found for episode {EpisodeId} — aborting",
                job.EpisodeId);
            return;
        }

        var roster     = await characters.GetByEpisodeIdAsync(job.EpisodeId, ct);
        var rosterJson = BuildRosterJson(roster);

        var message = new EpisodeJobMessage(
            JobId:       jobId,
            EpisodeId:   job.EpisodeId,
            JobType:     "StoryboardPlan",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload: new
            {
                screenplayJson       = script.RawJson,
                rosterJson,
                characterAliasesJson = "{}",
            });

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: job.EpisodeId.ToString(), ct: ct);

        logger.LogInformation(
            "StoryboardPlan job dispatched — jobId={JobId}, episodeId={EpisodeId}",
            jobId, job.EpisodeId);
    }

    // ── StoryboardGen (auto-dispatch after StoryboardPlan completes) ──────────

    /// <summary>
    /// Creates a new StoryboardGen <see cref="Job"/> and dispatches it to the Python worker.
    /// Called automatically by <see cref="CompletionMessageProcessor"/> after a
    /// StoryboardPlan job succeeds.
    /// Payload: { storyboardPlanJson, rosterJson, characterAliasesJson, characterLorasJson }
    /// </summary>
    public async Task DispatchStoryboardGenJobAsync(Guid episodeId, CancellationToken ct = default)
    {
        var storyboard = await storyboards.GetByEpisodeIdAsync(episodeId, ct);
        if (storyboard is null)
        {
            logger.LogWarning(
                "DispatchStoryboardGenJob: no storyboard found for episode {EpisodeId} — aborting",
                episodeId);
            return;
        }

        // Create a new Job entity so Python's completion can be matched back.
        var existingJobs = await jobs.GetByEpisodeIdAsync(episodeId, ct);
        var attempt      = existingJobs.Count(j => j.Type == JobType.StoryboardGen) + 1;

        var newJob = Job.Create(episodeId, JobType.StoryboardGen, payload: null, attempt);
        await jobs.AddAsync(newJob, ct);

        var roster            = await characters.GetByEpisodeIdAsync(episodeId, ct);
        var rosterJson        = BuildRosterJson(roster);
        var characterLorasJson = BuildCharacterLorasJson(roster);
        var storyboardPlanJson = BuildStoryboardPlanJson(storyboard);

        var message = new EpisodeJobMessage(
            JobId:       newJob.Id,
            EpisodeId:   episodeId,
            JobType:     "StoryboardGen",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload: new
            {
                storyboardPlanJson,
                rosterJson,
                characterAliasesJson  = "{}",
                characterLorasJson,
            });

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: episodeId.ToString(), ct: ct);

        logger.LogInformation(
            "StoryboardGen job dispatched — jobId={JobId}, episodeId={EpisodeId}, shots={ShotCount}",
            newJob.Id, episodeId, storyboard.Shots.Count);
    }

    // ── StoryboardGen (single-shot regeneration) ──────────────────────────────

    /// <summary>
    /// Dispatches an existing StoryboardGen <see cref="Job"/> created by
    /// RegenerateShotCommand or UpdateShotStyleCommand (single-shot re-render).
    /// Payload: { storyboardPlanJson, rosterJson, characterAliasesJson, characterLorasJson }
    /// </summary>
    public async Task DispatchShotRegenJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await jobs.GetByIdAsync(jobId, ct);
        if (job is null)
        {
            logger.LogWarning("DispatchShotRegenJob: job {JobId} not found — aborting", jobId);
            return;
        }

        var storyboard = await storyboards.GetByEpisodeIdAsync(job.EpisodeId, ct);
        if (storyboard is null)
        {
            logger.LogWarning(
                "DispatchShotRegenJob: no storyboard for episode {EpisodeId} — aborting",
                job.EpisodeId);
            return;
        }

        var roster             = await characters.GetByEpisodeIdAsync(job.EpisodeId, ct);
        var rosterJson         = BuildRosterJson(roster);
        var characterLorasJson = BuildCharacterLorasJson(roster);
        var storyboardPlanJson = BuildStoryboardPlanJson(storyboard);

        var message = new EpisodeJobMessage(
            JobId:       jobId,
            EpisodeId:   job.EpisodeId,
            JobType:     "StoryboardGen",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload: new
            {
                storyboardPlanJson,
                rosterJson,
                characterAliasesJson  = "{}",
                characterLorasJson,
            });

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: job.EpisodeId.ToString(), ct: ct);

        logger.LogInformation(
            "StoryboardGen (shot regen) dispatched — jobId={JobId}, episodeId={EpisodeId}",
            jobId, job.EpisodeId);
    }

    // ── PostProd ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a PostProd <see cref="Job"/> and dispatches it to the Python worker.
    /// Called by <see cref="Controllers.RenderController"/> after a Render record is created.
    /// Payload: { animationMetadataJson, voiceOutputJson, renderId }
    /// </summary>
    public async Task DispatchPostProdAsync(Guid renderId, CancellationToken ct = default)
    {
        var render = await renders.GetByIdAsync(renderId, ct);
        if (render is null)
        {
            logger.LogWarning("DispatchPostProd: render {RenderId} not found — aborting", renderId);
            return;
        }

        var episodeId = render.EpisodeId;

        // Require Ready animation clips before dispatching.
        var clips = (await animationClips.GetByEpisodeIdAsync(episodeId, ct))
            .Where(c => c.Status == ClipStatus.Ready && !string.IsNullOrWhiteSpace(c.ClipUrl))
            .ToList();

        if (clips.Count == 0)
        {
            logger.LogWarning(
                "DispatchPostProd: no Ready clips found for episode {EpisodeId} — aborting",
                episodeId);
            return;
        }

        var script = await scripts.GetByEpisodeIdAsync(episodeId, ct);
        var title  = script?.Title ?? "Untitled";

        var animationMetadataJson = BuildAnimationMetadataJson(clips, title);

        // Voice not yet implemented — send empty track list so the mixer just
        // assembles video clips without audio.
        var voiceOutputJson = BuildEmptyVoiceOutputJson(title);

        // Create a PostProd Job entity so the completion can be matched back.
        var existingJobs = await jobs.GetByEpisodeIdAsync(episodeId, ct);
        var attempt      = existingJobs.Count(j => j.Type == JobType.PostProd) + 1;

        var newJob = Job.Create(episodeId, JobType.PostProd, payload: null, attempt);
        await jobs.AddAsync(newJob, ct);

        var message = new EpisodeJobMessage(
            JobId:       newJob.Id,
            EpisodeId:   episodeId,
            JobType:     "PostProd",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload: new
            {
                animationMetadataJson,
                voiceOutputJson,
                renderId = renderId.ToString(),
            });

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: episodeId.ToString(), ct: ct);

        logger.LogInformation(
            "PostProd job dispatched — jobId={JobId}, renderId={RenderId}, episodeId={EpisodeId}, clips={ClipCount}",
            newJob.Id, renderId, episodeId, clips.Count);
    }

    // ── Payload builders ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds the roster JSON string matching Python's CharacterRoster.model_dump() format.
    /// Only includes Ready characters that have LoRA weights (needed for image generation).
    /// </summary>
    private static string BuildRosterJson(IEnumerable<Character> roster)
    {
        var chars = roster
            .Where(c => c.TrainingStatus == TrainingStatus.Ready)
            .Select(c => new
            {
                name                = c.Name,
                style_dna           = c.StyleDna ?? string.Empty,
                character_image_url = c.ImageUrl ?? string.Empty,
                lora_weights_url    = c.LoraWeightsUrl ?? string.Empty,
                trigger_word        = c.TriggerWord ?? string.Empty,
            })
            .ToList();

        return JsonSerializer.Serialize(new { characters = chars }, JsonOpts);
    }

    /// <summary>
    /// Builds characterLorasJson: { "CharacterName": { "lora_weights_url": "...", "trigger_word": "..." } }
    /// </summary>
    private static string BuildCharacterLorasJson(IEnumerable<Character> roster)
    {
        var loras = roster
            .Where(c => c.TrainingStatus == TrainingStatus.Ready
                     && !string.IsNullOrWhiteSpace(c.LoraWeightsUrl))
            .ToDictionary(
                c => c.Name,
                c => new { lora_weights_url = c.LoraWeightsUrl!, trigger_word = c.TriggerWord ?? string.Empty });

        return JsonSerializer.Serialize(loras, JsonOpts);
    }

    /// <summary>
    /// Reconstructs a StoryboardPlan JSON string (Python StoryboardPlan model format) from
    /// the shot entities stored in the Storyboard aggregate.
    /// Format: { screenplay_title, scene_plans: [{ scene_number, shot_descriptions: [...] }] }
    /// </summary>
    private static string BuildStoryboardPlanJson(Storyboard storyboard)
    {
        var scenePlans = storyboard.Shots
            .GroupBy(s => s.SceneNumber)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                scene_number      = g.Key,
                shot_descriptions = g.OrderBy(s => s.ShotIndex)
                                     .Select(s => s.Description ?? string.Empty)
                                     .ToList(),
            })
            .ToList();

        return JsonSerializer.Serialize(new
        {
            screenplay_title = storyboard.ScreenplayTitle,
            scene_plans      = scenePlans,
        }, JsonOpts);
    }

    /// <summary>
    /// Builds animationMetadataJson matching Python's AnimationMetadata model format.
    /// Format: { screenplay_title, scene_count, scene_shots: { "1": [url1, url2], "2": [...] }, metadata_for_generation: {} }
    /// </summary>
    private static string BuildAnimationMetadataJson(IEnumerable<AnimationClip> clips, string title)
    {
        var sceneShots = clips
            .GroupBy(c => c.SceneNumber)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.OrderBy(c => c.ShotIndex)
                       .Select(c => c.ClipUrl!)
                       .ToList());

        return JsonSerializer.Serialize(new
        {
            screenplay_title           = title,
            scene_count                = sceneShots.Count,
            scene_shots                = sceneShots,
            metadata_for_generation    = new { },
        }, JsonOpts);
    }

    /// <summary>
    /// Builds an AudioOutput JSON with an empty track list (voice not yet implemented).
    /// Format: { screenplay_title, tracks: [] }
    /// </summary>
    private static string BuildEmptyVoiceOutputJson(string title)
        => JsonSerializer.Serialize(new { screenplay_title = title, tracks = Array.Empty<object>() }, JsonOpts);

    // ── Shared message type ───────────────────────────────────────────────────

    private sealed record EpisodeJobMessage(
        [property: JsonPropertyName("jobId")]       Guid           JobId,
        [property: JsonPropertyName("episodeId")]   Guid           EpisodeId,
        [property: JsonPropertyName("jobType")]     string         JobType,
        [property: JsonPropertyName("requestedAt")] DateTimeOffset RequestedAt,
        [property: JsonPropertyName("payload")]     object         Payload);
}
