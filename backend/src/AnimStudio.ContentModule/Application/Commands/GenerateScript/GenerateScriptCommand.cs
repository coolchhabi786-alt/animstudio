using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.GenerateScript;

/// <summary>Enqueues a Script writing job for an episode. Returns 202 + JobDto.</summary>
public sealed record GenerateScriptCommand(Guid EpisodeId, string? DirectorNotes) : IRequest<Result<JobDto>>;

public sealed class GenerateScriptValidator : AbstractValidator<GenerateScriptCommand>
{
    public GenerateScriptValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
    }
}

public sealed class GenerateScriptHandler(
    IEpisodeRepository episodes,
    IJobRepository jobs,
    ICharacterRepository characters)
    : IRequestHandler<GenerateScriptCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(GenerateScriptCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<JobDto>.Failure("Episode not found.", "NOT_FOUND");

        // Require at least one ready character before scripting
        var roster = await characters.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        if (!roster.Any(c => c.TrainingStatus == TrainingStatus.Ready))
            return Result<JobDto>.Failure(
                "At least one character must be in Ready status before generating a script.",
                "CHARACTERS_NOT_READY");

        var existingJobs = await jobs.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var attempt = existingJobs.Count(j => j.Type == JobType.Script) + 1;

        var payload = JsonSerializer.Serialize(new
        {
            episodeId = cmd.EpisodeId,
            jobType = JobType.Script.ToString(),
            attempt,
            directorNotes = cmd.DirectorNotes,
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
