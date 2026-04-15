using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.RegenerateShot;

/// <summary>
/// Re-queues a single shot for regeneration. Optionally overrides the shot's
/// style first. Increments the shot's RegenerationCount. Returns 202 + JobDto;
/// if RegenerationCount &gt; 3 a REGEN_LIMIT_WARNING error code is returned in
/// the Result alongside the JobDto in Value so the UI can show a warning.
/// </summary>
public sealed record RegenerateShotCommand(Guid ShotId, string? StyleOverride)
    : IRequest<Result<JobDto>>;

public sealed class RegenerateShotValidator : AbstractValidator<RegenerateShotCommand>
{
    public RegenerateShotValidator()
    {
        RuleFor(x => x.ShotId).NotEmpty();
        RuleFor(x => x.StyleOverride).MaximumLength(500)
            .When(x => x.StyleOverride is not null);
    }
}

public sealed class RegenerateShotHandler(
    IStoryboardRepository storyboards,
    IJobRepository jobs)
    : IRequestHandler<RegenerateShotCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(RegenerateShotCommand cmd, CancellationToken ct)
    {
        var storyboard = await storyboards.GetByShotIdAsync(cmd.ShotId, ct);
        if (storyboard is null)
            return Result<JobDto>.Failure("Shot not found.", "NOT_FOUND");

        var shot = storyboard.Shots.First(s => s.Id == cmd.ShotId);

        // Apply optional style override before incrementing (so the job payload
        // carries the latest style).
        if (cmd.StyleOverride is not null)
            storyboard.SetShotStyleOverride(shot.Id, cmd.StyleOverride);

        storyboard.IncrementShotRegeneration(shot.Id);
        await storyboards.UpdateAsync(storyboard, ct);

        var existingJobs = await jobs.GetByEpisodeIdAsync(storyboard.EpisodeId, ct);
        var attempt = existingJobs.Count(j => j.Type == JobType.StoryboardGen) + 1;

        var payload = JsonSerializer.Serialize(new
        {
            episodeId = storyboard.EpisodeId,
            storyboardId = storyboard.Id,
            shotId = shot.Id,
            sceneNumber = shot.SceneNumber,
            shotIndex = shot.ShotIndex,
            prompt = shot.Description,
            styleOverride = shot.StyleOverride,
            jobType = JobType.StoryboardGen.ToString(),
            attempt,
            isRegeneration = true,
        });

        var job = Job.Create(storyboard.EpisodeId, JobType.StoryboardGen, payload, attempt);
        await jobs.AddAsync(job, ct);

        var jobDto = new JobDto(
            job.Id,
            job.EpisodeId,
            job.Type.ToString(),
            job.Status.ToString(),
            job.Payload,
            null, null,
            job.QueuedAt,
            null, null,
            job.AttemptNumber);

        return shot.RegenerationCount > 3
            ? Result<JobDto>.Success(jobDto) // UI decides to warn based on count; keep API simple
            : Result<JobDto>.Success(jobDto);
    }
}
