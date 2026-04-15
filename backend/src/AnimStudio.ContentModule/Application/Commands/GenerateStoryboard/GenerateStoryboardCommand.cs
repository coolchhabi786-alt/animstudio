using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.GenerateStoryboard;

/// <summary>
/// Enqueues a StoryboardPlan job for the episode. The Python engine plans the
/// shots per scene, and its completion handler (HandleJobCompletion) creates
/// the Storyboard row and seeds its shots. Returns 202 + JobDto.
/// </summary>
public sealed record GenerateStoryboardCommand(Guid EpisodeId, string? DirectorNotes)
    : IRequest<Result<JobDto>>;

public sealed class GenerateStoryboardValidator : AbstractValidator<GenerateStoryboardCommand>
{
    public GenerateStoryboardValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.DirectorNotes).MaximumLength(5000)
            .When(x => x.DirectorNotes is not null);
    }
}

public sealed class GenerateStoryboardHandler(
    IEpisodeRepository episodes,
    IScriptRepository scripts,
    IStoryboardRepository storyboards,
    IJobRepository jobs)
    : IRequestHandler<GenerateStoryboardCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(GenerateStoryboardCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<JobDto>.Failure("Episode not found.", "NOT_FOUND");

        // Script must exist before the storyboard can be planned.
        var script = await scripts.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        if (script is null)
            return Result<JobDto>.Failure(
                "A script must be generated before creating a storyboard.",
                "SCRIPT_NOT_READY");

        // If a storyboard already exists, record director notes against it so the
        // Python engine receives them; persistence flushes in UpdateAsync.
        var storyboard = await storyboards.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        if (storyboard is not null)
        {
            storyboard.SetDirectorNotes(cmd.DirectorNotes);
            await storyboards.UpdateAsync(storyboard, ct);
        }

        var existingJobs = await jobs.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var attempt = existingJobs.Count(j =>
            j.Type == JobType.StoryboardPlan || j.Type == JobType.StoryboardGen) + 1;

        var payload = JsonSerializer.Serialize(new
        {
            episodeId = cmd.EpisodeId,
            jobType = JobType.StoryboardPlan.ToString(),
            attempt,
            directorNotes = cmd.DirectorNotes,
            screenplayTitle = script.Title,
        });

        var job = Job.Create(cmd.EpisodeId, JobType.StoryboardPlan, payload, attempt);
        await jobs.AddAsync(job, ct);

        episode.Advance(EpisodeStatus.Storyboard);
        await episodes.UpdateAsync(episode, ct);

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
