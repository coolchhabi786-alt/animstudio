using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.ContentModule.Application.Commands.HandleJobCompletion;

public sealed record HandleJobCompletionCommand(Guid JobId, bool IsSuccess, string? Result, string? Error) : IRequest<Result<bool>>;

public sealed class HandleJobCompletionHandler(
    IJobRepository jobs,
    IEpisodeRepository episodes,
    ISagaStateRepository sagas,
    IStoryboardRepository storyboards,
    ILogger<HandleJobCompletionHandler> logger)
    : IRequestHandler<HandleJobCompletionCommand, Result<bool>>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<bool>> Handle(HandleJobCompletionCommand cmd, CancellationToken ct)
    {
        var job = await jobs.GetByIdAsync(cmd.JobId, ct);
        if (job is null) return Result<bool>.Failure("Job not found", "NOT_FOUND");

        var episode = await episodes.GetByIdAsync(job.EpisodeId, ct);
        if (episode is null) return Result<bool>.Failure("Episode not found", "NOT_FOUND");

        if (cmd.IsSuccess)
        {
            // ── Storyboard-specific result processing ────────────────────────
            if (job.Type == JobType.StoryboardPlan && cmd.Result is not null)
                await HandleStoryboardPlanResultAsync(job.EpisodeId, cmd.Result, ct);

            if (job.Type == JobType.StoryboardGen && cmd.Result is not null)
                await HandleStoryboardGenResultAsync(job.EpisodeId, cmd.Result, ct);

            job.Complete(cmd.Result);
            await jobs.UpdateAsync(job, ct);

            var nextStage = NextStageAfter(job.Type);
            episode.Advance(nextStage);
            await episodes.UpdateAsync(episode, ct);

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

    // ── StoryboardPlan ────────────────────────────────────────────────────────
    // Python returns: { "screenplayTitle": "...", "shots": [{ "sceneNumber": 1, "shotIndex": 1, "description": "..." }] }

    private async Task HandleStoryboardPlanResultAsync(Guid episodeId, string resultJson, CancellationToken ct)
    {
        try
        {
            var plan = JsonSerializer.Deserialize<StoryboardPlanResult>(resultJson, JsonOpts);
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
                "Storyboard seeded for episode {EpisodeId}: {Count} shots",
                episodeId, plan.Shots.Count);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse StoryboardPlan result for episode {EpisodeId}", episodeId);
        }
    }

    // ── StoryboardGen ─────────────────────────────────────────────────────────
    // Python returns: { "shotId": "guid", "imageUrl": "https://..." }

    private async Task HandleStoryboardGenResultAsync(Guid episodeId, string resultJson, CancellationToken ct)
    {
        try
        {
            var genResult = JsonSerializer.Deserialize<StoryboardGenResult>(resultJson, JsonOpts);
            if (genResult is null || string.IsNullOrWhiteSpace(genResult.ImageUrl))
            {
                logger.LogWarning("StoryboardGen result for episode {EpisodeId} had no imageUrl", episodeId);
                return;
            }

            var storyboard = await storyboards.GetByEpisodeIdAsync(episodeId, ct);
            if (storyboard is null)
            {
                logger.LogWarning("No storyboard found for episode {EpisodeId} when processing StoryboardGen", episodeId);
                return;
            }

            storyboard.SetShotImage(genResult.ShotId, genResult.ImageUrl);
            await storyboards.UpdateAsync(storyboard, ct);

            logger.LogInformation(
                "Shot {ShotId} image set for episode {EpisodeId}",
                genResult.ShotId, episodeId);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse StoryboardGen result for episode {EpisodeId}", episodeId);
        }
    }

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

    // ── Private result DTOs ───────────────────────────────────────────────────

    private sealed record StoryboardPlanResult(
        [property: JsonPropertyName("screenplayTitle")] string? ScreenplayTitle,
        [property: JsonPropertyName("shots")] List<PlannedShot> Shots);

    private sealed record PlannedShot(
        [property: JsonPropertyName("sceneNumber")] int SceneNumber,
        [property: JsonPropertyName("shotIndex")] int ShotIndex,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record StoryboardGenResult(
        [property: JsonPropertyName("shotId")] Guid ShotId,
        [property: JsonPropertyName("imageUrl")] string ImageUrl);
}
