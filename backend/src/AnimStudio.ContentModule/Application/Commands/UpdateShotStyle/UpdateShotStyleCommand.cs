using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.UpdateShotStyle;

/// <summary>
/// Persists the user's StyleOverride on a single shot and enqueues a
/// StoryboardGen job to re-render the shot with the new style. Returns
/// 202 + JobDto.
/// </summary>
public sealed record UpdateShotStyleCommand(Guid ShotId, string? StyleOverride)
    : IRequest<Result<JobDto>>;

public sealed class UpdateShotStyleValidator : AbstractValidator<UpdateShotStyleCommand>
{
    public UpdateShotStyleValidator()
    {
        RuleFor(x => x.ShotId).NotEmpty();
        RuleFor(x => x.StyleOverride).MaximumLength(500)
            .When(x => x.StyleOverride is not null);
    }
}

public sealed class UpdateShotStyleHandler(
    IStoryboardRepository storyboards,
    IJobRepository jobs)
    : IRequestHandler<UpdateShotStyleCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(UpdateShotStyleCommand cmd, CancellationToken ct)
    {
        var storyboard = await storyboards.GetByShotIdAsync(cmd.ShotId, ct);
        if (storyboard is null)
            return Result<JobDto>.Failure("Shot not found.", "NOT_FOUND");

        var shot = storyboard.Shots.First(s => s.Id == cmd.ShotId);

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
            isStyleChange = true,
        });

        var job = Job.Create(storyboard.EpisodeId, JobType.StoryboardGen, payload, attempt);
        await jobs.AddAsync(job, ct);

        return Result<JobDto>.Success(new JobDto(
            job.Id,
            job.EpisodeId,
            job.Type.ToString(),
            job.Status.ToString(),
            job.Payload,
            null, null,
            job.QueuedAt,
            null, null,
            job.AttemptNumber));
    }
}
