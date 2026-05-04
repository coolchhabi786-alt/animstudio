using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.ContentModule.Application.Commands.HandleJobCompletion;

public sealed record HandleJobCompletionCommand(
    Guid    JobId,
    bool    IsSuccess,
    string? Result,
    string? Error) : IRequest<Result<bool>>;

public sealed class HandleJobCompletionHandler(
    IJobRepository          jobs,
    IEpisodeRepository      episodes,
    ISagaStateRepository    sagas,
    ICharacterRepository    characters,
    IScriptRepository       scripts,
    IStoryboardRepository   storyboards,
    IAnimationJobRepository animationJobs,
    IAnimationClipRepository animationClips,
    IPublisher              publisher,
    IUsageMeteringService   usageMetering,
    ILogger<HandleJobCompletionHandler> logger)
    : IRequestHandler<HandleJobCompletionCommand, Result<bool>>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<bool>> Handle(HandleJobCompletionCommand cmd, CancellationToken ct)
    {
        var job = await jobs.GetByIdAsync(cmd.JobId, ct);
        if (job is null)
        {
            logger.LogWarning("HandleJobCompletion: job {JobId} not found", cmd.JobId);
            return Result<bool>.Failure("Job not found", "NOT_FOUND");
        }

        var episode = await episodes.GetByIdAsync(job.EpisodeId, ct);
        if (episode is null)
        {
            logger.LogWarning("HandleJobCompletion: episode {EpisodeId} not found for job {JobId}", job.EpisodeId, cmd.JobId);
            return Result<bool>.Failure("Episode not found", "NOT_FOUND");
        }

        if (cmd.IsSuccess)
        {
            await DispatchSuccessHandlerAsync(job, cmd.Result, ct);

            job.Complete(cmd.Result);
            await jobs.UpdateAsync(job, ct);

            var nextStage = NextStageAfter(job.Type);
            episode.Advance(nextStage);
            await episodes.UpdateAsync(episode, ct);
            // Episode domain events (EpisodeStageAdvancedEvent, EpisodeCompletedEvent) are
            // drained by TransactionBehaviour → outbox → OutboxPublisherJob.

            var saga = await sagas.GetByEpisodeIdAsync(episode.Id, ct);
            if (saga is not null)
            {
                saga.CurrentStage = (PipelineStage)(int)nextStage;
                saga.UpdatedAt = DateTimeOffset.UtcNow;
                saga.IsCompensating = false;
                await sagas.UpdateAsync(saga, ct);
            }
        }
        else
        {
            job.Fail(cmd.Error ?? "Unknown error");
            await jobs.UpdateAsync(job, ct);

            episode.Fail(cmd.Error ?? "Job failed");
            await episodes.UpdateAsync(episode, ct);

            var saga = await sagas.GetByEpisodeIdAsync(episode.Id, ct);
            if (saga is not null)
            {
                saga.LastError = cmd.Error;
                saga.UpdatedAt = DateTimeOffset.UtcNow;
                saga.IsCompensating = true;
                await sagas.UpdateAsync(saga, ct);
            }
        }

        return Result<bool>.Success(true);
    }

    // ── Type dispatch ──────────────────────────────────────────────────────────

    private Task DispatchSuccessHandlerAsync(Job job, string? resultJson, CancellationToken ct)
        => job.Type switch
        {
            JobType.CharacterDesign => HandleCharacterDesignAsync(job.EpisodeId, resultJson, ct),
            JobType.LoraTraining    => HandleLoraTrainingAsync(job.EpisodeId, resultJson, ct),
            JobType.Script          => HandleScriptAsync(job.EpisodeId, resultJson, ct),
            JobType.StoryboardPlan  => HandleStoryboardPlanAsync(job.EpisodeId, resultJson, ct),
            JobType.StoryboardGen   => HandleStoryboardGenAsync(job.EpisodeId, resultJson, ct),
            JobType.Animation       => HandleAnimationAsync(job.EpisodeId, resultJson, ct),
            JobType.PostProd        => HandlePostProdAsync(job.EpisodeId, ct),
            _                       => Task.CompletedTask,
        };

    // ── CharacterDesign ────────────────────────────────────────────────────────
    // Python returns: { "imageUrl": "string" }
    // All characters linked to the episode are advanced to TrainingQueued with the reference image.

    private async Task HandleCharacterDesignAsync(Guid episodeId, string? resultJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            logger.LogWarning("CharacterDesign result for episode {EpisodeId} had no payload", episodeId);
            return;
        }
        try
        {
            var result = JsonSerializer.Deserialize<CharacterDesignResult>(resultJson, JsonOpts);
            if (result is null || string.IsNullOrWhiteSpace(result.ImageUrl))
            {
                logger.LogWarning("CharacterDesign result for episode {EpisodeId} had no imageUrl", episodeId);
                return;
            }

            var episodeChars = await characters.GetByEpisodeIdAsync(episodeId, ct);
            if (episodeChars.Count == 0)
            {
                logger.LogWarning("CharacterDesign: no characters found for episode {EpisodeId}", episodeId);
                return;
            }

            foreach (var character in episodeChars)
            {
                // BOLA: character already verified to belong to this episode via EpisodeCharacter join
                character.AdvanceTraining(
                    TrainingStatus.TrainingQueued,
                    progressPercent: 50,
                    imageUrl: result.ImageUrl);
                await characters.UpdateAsync(character, ct);

                foreach (var evt in character.DomainEvents)
                    await publisher.Publish(evt, ct);
                character.ClearDomainEvents();
            }

            logger.LogInformation(
                "CharacterDesign completed for episode {EpisodeId}: {Count} character(s) advanced to TrainingQueued",
                episodeId, episodeChars.Count);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse CharacterDesign result for episode {EpisodeId}", episodeId);
        }
    }

    // ── LoraTraining ───────────────────────────────────────────────────────────
    // Python returns: { "loraWeightsUrl": "string", "triggerWord": "string" }

    private async Task HandleLoraTrainingAsync(Guid episodeId, string? resultJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            logger.LogWarning("LoraTraining result for episode {EpisodeId} had no payload", episodeId);
            return;
        }
        try
        {
            var result = JsonSerializer.Deserialize<LoraTrainingResult>(resultJson, JsonOpts);
            if (result is null || string.IsNullOrWhiteSpace(result.LoraWeightsUrl))
            {
                logger.LogWarning("LoraTraining result for episode {EpisodeId} missing loraWeightsUrl", episodeId);
                return;
            }

            var episodeChars = await characters.GetByEpisodeIdAsync(episodeId, ct);
            var trainingChars = episodeChars
                .Where(c => c.TrainingStatus is TrainingStatus.TrainingQueued or TrainingStatus.Training)
                .ToList();

            if (trainingChars.Count == 0)
            {
                logger.LogWarning("LoraTraining: no characters in training state for episode {EpisodeId}", episodeId);
                return;
            }

            foreach (var character in trainingChars)
            {
                character.AdvanceTraining(
                    TrainingStatus.Ready,
                    progressPercent: 100,
                    loraWeightsUrl: result.LoraWeightsUrl,
                    triggerWord: result.TriggerWord);
                await characters.UpdateAsync(character, ct);

                foreach (var evt in character.DomainEvents)
                    await publisher.Publish(evt, ct);
                character.ClearDomainEvents();
            }

            logger.LogInformation(
                "LoraTraining completed for episode {EpisodeId}: {Count} character(s) marked Ready",
                episodeId, trainingChars.Count);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse LoraTraining result for episode {EpisodeId}", episodeId);
        }
    }

    // ── Script ─────────────────────────────────────────────────────────────────
    // Python returns: { "screenplay": { ...Screenplay model... } }

    private async Task HandleScriptAsync(Guid episodeId, string? resultJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            logger.LogWarning("Script result for episode {EpisodeId} had no payload", episodeId);
            return;
        }
        try
        {
            var result = JsonSerializer.Deserialize<ScriptResult>(resultJson, JsonOpts);
            if (result is null)
            {
                logger.LogWarning("Script result for episode {EpisodeId} could not be deserialized", episodeId);
                return;
            }

            var screenplayJson = result.Screenplay.GetRawText();
            var title = result.Screenplay.TryGetProperty("title", out var titleProp)
                ? titleProp.GetString() ?? "Untitled"
                : "Untitled";

            var existing = await scripts.GetByEpisodeIdAsync(episodeId, ct);
            if (existing is not null)
            {
                existing.UpdateFromJob(screenplayJson, title);
                await scripts.UpdateAsync(existing, ct);
            }
            else
            {
                var created = Script.Create(episodeId, title, screenplayJson);
                await scripts.AddAsync(created, ct);
            }

            logger.LogInformation("Script result persisted for episode {EpisodeId}, title={Title}", episodeId, title);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse Script result for episode {EpisodeId}", episodeId);
        }
    }

    // ── StoryboardPlan ─────────────────────────────────────────────────────────
    // Python returns: { "screenplayTitle": "...", "shots": [{ sceneNumber, shotIndex, description }] }

    private async Task HandleStoryboardPlanAsync(Guid episodeId, string? resultJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            logger.LogWarning("StoryboardPlan result for episode {EpisodeId} had no payload", episodeId);
            return;
        }
        try
        {
            var plan = JsonSerializer.Deserialize<StoryboardPlanJobResult>(resultJson, JsonOpts);
            if (plan?.Shots is null || plan.Shots.Count == 0)
            {
                logger.LogWarning("StoryboardPlan result for episode {EpisodeId} had no shots", episodeId);
                return;
            }

            var title = plan.ScreenplayTitle ?? "Untitled";
            var shotTuples = plan.Shots
                .Select(s => (s.SceneNumber, s.ShotIndex, s.Description ?? string.Empty));

            var existing = await storyboards.GetByEpisodeIdAsync(episodeId, ct);
            if (existing is not null)
            {
                existing.UpdateFromJob(resultJson, title);
                existing.SeedShots(shotTuples);
                await storyboards.UpdateAsync(existing, ct);
            }
            else
            {
                var created = Storyboard.Create(episodeId, title, resultJson);
                created.SeedShots(shotTuples);
                await storyboards.AddAsync(created, ct);
            }

            logger.LogInformation(
                "Storyboard seeded for episode {EpisodeId}: {Count} shots, title={Title}",
                episodeId, plan.Shots.Count, title);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse StoryboardPlan result for episode {EpisodeId}", episodeId);
        }
    }

    // ── StoryboardGen ─────────────────────────────────────────────────────────
    // Python returns: { "shots": [{ sceneNumber, shotIndex, imageUrl }] }

    private async Task HandleStoryboardGenAsync(Guid episodeId, string? resultJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            logger.LogWarning("StoryboardGen result for episode {EpisodeId} had no payload", episodeId);
            return;
        }
        try
        {
            var genResult = JsonSerializer.Deserialize<StoryboardGenJobResult>(resultJson, JsonOpts);
            if (genResult?.Shots is null || genResult.Shots.Count == 0)
            {
                logger.LogWarning("StoryboardGen result for episode {EpisodeId} had no shots", episodeId);
                return;
            }

            var storyboard = await storyboards.GetByEpisodeIdAsync(episodeId, ct);
            if (storyboard is null)
            {
                logger.LogWarning("StoryboardGen: no storyboard found for episode {EpisodeId}", episodeId);
                return;
            }

            foreach (var shot in genResult.Shots)
            {
                if (string.IsNullOrWhiteSpace(shot.ImageUrl))
                {
                    logger.LogWarning(
                        "StoryboardGen: skipping shot scene={Scene} index={Index} — imageUrl empty",
                        shot.SceneNumber, shot.ShotIndex);
                    continue;
                }
                try
                {
                    storyboard.SetShotImageByPosition(shot.SceneNumber, shot.ShotIndex, shot.ImageUrl);
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning(ex,
                        "StoryboardGen: shot scene={Scene} index={Index} not found in storyboard {Id}",
                        shot.SceneNumber, shot.ShotIndex, storyboard.Id);
                }
            }

            await storyboards.UpdateAsync(storyboard, ct);

            logger.LogInformation(
                "StoryboardGen: {Count} shot image(s) set for episode {EpisodeId}",
                genResult.Shots.Count, episodeId);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse StoryboardGen result for episode {EpisodeId}", episodeId);
        }
    }

    // ── Animation ─────────────────────────────────────────────────────────────
    // Python returns: { "clips": [{ sceneNumber, shotIndex, clipUrl, durationSeconds }], "actualCostUsd": 0.672 }

    private async Task HandleAnimationAsync(Guid episodeId, string? resultJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            logger.LogWarning("Animation result for episode {EpisodeId} had no payload", episodeId);
            return;
        }
        try
        {
            var result = JsonSerializer.Deserialize<AnimationJobResult>(resultJson, JsonOpts);
            if (result?.Clips is null || result.Clips.Count == 0)
            {
                logger.LogWarning("Animation result for episode {EpisodeId} had no clips", episodeId);
                return;
            }

            foreach (var item in result.Clips)
            {
                var clip = await animationClips.GetByEpisodeAndPositionAsync(
                    episodeId, item.SceneNumber, item.ShotIndex, ct);

                if (clip is null)
                {
                    logger.LogWarning(
                        "Animation result: no clip found for episode {EpisodeId} scene {Scene} shot {Shot}",
                        episodeId, item.SceneNumber, item.ShotIndex);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.ClipUrl))
                    clip.MarkFailed();
                else
                    clip.MarkReady(item.ClipUrl, item.DurationSeconds);

                await animationClips.UpdateAsync(clip, ct);

                foreach (var evt in clip.DomainEvents)
                    await publisher.Publish(evt, ct);
                clip.ClearDomainEvents();
            }

            var animJob = await animationJobs.GetLatestByEpisodeIdAsync(episodeId, ct);
            if (animJob is not null && !animJob.Status.Equals(AnimationStatus.Completed))
            {
                animJob.MarkCompleted(result.ActualCostUsd ?? animJob.EstimatedCostUsd);
                await animationJobs.UpdateAsync(animJob, ct);
            }

            logger.LogInformation(
                "Animation result processed for episode {EpisodeId}: {Count} clip(s) updated",
                episodeId, result.Clips.Count);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse Animation result for episode {EpisodeId}", episodeId);
        }
    }

    // ── PostProd ──────────────────────────────────────────────────────────────

    private async Task HandlePostProdAsync(Guid episodeId, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(episodeId, ct);
        if (episode is null) return;

        await usageMetering.IncrementEpisodeUsageAsync(episodeId, episode.ProjectId, ct);
    }

    // ── Stage progression ─────────────────────────────────────────────────────

    private static EpisodeStatus NextStageAfter(JobType type) => type switch
    {
        JobType.CharacterDesign => EpisodeStatus.LoraTraining,
        JobType.LoraTraining    => EpisodeStatus.Script,
        JobType.Script          => EpisodeStatus.Storyboard,
        JobType.StoryboardPlan  => EpisodeStatus.Storyboard,
        JobType.StoryboardGen   => EpisodeStatus.Voice,
        JobType.Voice           => EpisodeStatus.Animation,
        JobType.Animation       => EpisodeStatus.PostProduction,
        JobType.PostProd        => EpisodeStatus.Done,
        _                       => EpisodeStatus.Done,
    };
}
