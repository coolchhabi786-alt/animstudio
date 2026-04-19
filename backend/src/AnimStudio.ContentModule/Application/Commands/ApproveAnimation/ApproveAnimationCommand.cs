using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.ApproveAnimation;

/// <summary>
/// Approves the animation cost estimate for an episode, persists an
/// <see cref="AnimationJob"/> in Approved state, advances the episode into
/// Animation status, and enqueues a <see cref="JobType.Animation"/> worker job.
/// Idempotent-by-rejection: returns 409 CONFLICT if a non-terminal AnimationJob
/// already exists.
/// </summary>
public sealed record ApproveAnimationCommand(
    Guid EpisodeId,
    AnimationBackend Backend,
    Guid ApprovedByUserId) : IRequest<Result<AnimationJobDto>>;

public sealed class ApproveAnimationValidator : AbstractValidator<ApproveAnimationCommand>
{
    public ApproveAnimationValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.Backend).IsInEnum();
    }
}

public sealed class ApproveAnimationHandler(
    IEpisodeRepository episodes,
    IAnimationJobRepository animationJobs,
    IAnimationClipRepository animationClips,
    IAnimationEstimateService estimator,
    IJobRepository jobs)
    : IRequestHandler<ApproveAnimationCommand, Result<AnimationJobDto>>
{
    public async Task<Result<AnimationJobDto>> Handle(
        ApproveAnimationCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<AnimationJobDto>.Failure("Episode not found.", "NOT_FOUND");

        if (await animationJobs.HasActiveJobAsync(cmd.EpisodeId, ct))
            return Result<AnimationJobDto>.Failure(
                "Animation is already in progress or completed for this episode.",
                "ANIMATION_ALREADY_ACTIVE");

        var estimate = await estimator.EstimateAsync(cmd.EpisodeId, cmd.Backend, ct);
        if (estimate is null)
            return Result<AnimationJobDto>.Failure(
                "No storyboard found; generate a storyboard before approving animation.",
                "STORYBOARD_NOT_READY");

        if (estimate.ShotCount == 0)
            return Result<AnimationJobDto>.Failure(
                "Storyboard has no shots to animate.",
                "STORYBOARD_EMPTY");

        var animationJob = AnimationJob.Approve(
            cmd.EpisodeId, cmd.Backend, estimate.TotalCostUsd, cmd.ApprovedByUserId);
        await animationJobs.AddAsync(animationJob, ct);

        // Seed one Pending clip per shot so the UI has an immediate grid.
        var clips = estimate.Breakdown
            .Select(li => AnimationClip.CreatePending(
                cmd.EpisodeId, li.SceneNumber, li.ShotIndex, li.StoryboardShotId))
            .ToList();
        await animationClips.AddRangeAsync(clips, ct);

        var existingJobs = await jobs.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var attempt = existingJobs.Count(j => j.Type == JobType.Animation) + 1;

        var payload = JsonSerializer.Serialize(new
        {
            episodeId = cmd.EpisodeId,
            animationJobId = animationJob.Id,
            backend = cmd.Backend.ToString(),
            estimatedCostUsd = estimate.TotalCostUsd,
            shotCount = estimate.ShotCount,
            attempt,
        });
        var workerJob = Job.Create(cmd.EpisodeId, JobType.Animation, payload, attempt);
        await jobs.AddAsync(workerJob, ct);

        episode.Advance(EpisodeStatus.Animation);
        await episodes.UpdateAsync(episode, ct);

        return Result<AnimationJobDto>.Success(new AnimationJobDto(
            animationJob.Id,
            animationJob.EpisodeId,
            animationJob.Backend,
            animationJob.EstimatedCostUsd,
            animationJob.ActualCostUsd,
            animationJob.ApprovedByUserId,
            animationJob.ApprovedAt,
            animationJob.Status,
            animationJob.CreatedAt));
    }
}
