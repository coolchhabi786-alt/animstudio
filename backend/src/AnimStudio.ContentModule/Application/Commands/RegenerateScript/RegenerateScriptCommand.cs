using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.RegenerateScript;

/// <summary>
/// Re-enqueues a Script writing job, optionally with updated director notes.
/// Overwrites the existing script when the job completes.
/// </summary>
public sealed record RegenerateScriptCommand(Guid EpisodeId, string? DirectorNotes) : IRequest<Result<JobDto>>;

public sealed class RegenerateScriptValidator : AbstractValidator<RegenerateScriptCommand>
{
    public RegenerateScriptValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.DirectorNotes).MaximumLength(5000)
            .When(x => x.DirectorNotes is not null);
    }
}

public sealed class RegenerateScriptHandler(
    IEpisodeRepository episodes,
    IJobRepository jobs,
    IScriptRepository scripts,
    ICharacterRepository characters)
    : IRequestHandler<RegenerateScriptCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(RegenerateScriptCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<JobDto>.Failure("Episode not found.", "NOT_FOUND");

        // Characters must be ready before regenerating
        var roster = await characters.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        if (!roster.Any(c => c.TrainingStatus == TrainingStatus.Ready))
            return Result<JobDto>.Failure(
                "At least one character must be in Ready status before regenerating a script.",
                "CHARACTERS_NOT_READY");

        // Store director notes on the existing script (if any) for the Python engine to use
        var script = await scripts.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        if (script is not null)
        {
            script.SetDirectorNotes(cmd.DirectorNotes);
            await scripts.UpdateAsync(script, ct);
        }

        var existingJobs = await jobs.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var attempt = existingJobs.Count(j => j.Type == JobType.Script) + 1;

        var payload = JsonSerializer.Serialize(new
        {
            episodeId = cmd.EpisodeId,
            jobType = JobType.Script.ToString(),
            attempt,
            directorNotes = cmd.DirectorNotes,
            isRegeneration = true,
        });

        var job = Job.Create(cmd.EpisodeId, JobType.Script, payload, attempt);
        await jobs.AddAsync(job, ct);

        episode.Advance(EpisodeStatus.Script);
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
