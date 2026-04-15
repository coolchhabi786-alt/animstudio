using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Enums;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.DispatchEpisodeJob;

public sealed record DispatchEpisodeJobCommand(Guid EpisodeId, JobType JobType) : IRequest<Result<JobDto>>;

public sealed class DispatchEpisodeJobValidator : AbstractValidator<DispatchEpisodeJobCommand>
{
    public DispatchEpisodeJobValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
    }
}

public sealed class DispatchEpisodeJobHandler(
    IEpisodeRepository episodes,
    IJobRepository jobs,
    ISagaStateRepository sagas)
    : IRequestHandler<DispatchEpisodeJobCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(DispatchEpisodeJobCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null) return Result<JobDto>.Failure("Episode not found", "NOT_FOUND");

        if (episode.Status is EpisodeStatus.Done or EpisodeStatus.Failed)
            return Result<JobDto>.Failure($"Cannot dispatch a job for an episode in '{episode.Status}' state.", "INVALID_STATE");

        // Calculate the next attempt number for idempotent Service Bus MessageId
        var existingJobs = await jobs.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var attemptNumber = existingJobs.Count(j => j.Type == cmd.JobType) + 1;

        // Build job payload (stage-specific enrichment can be added later)
        var payload = JsonSerializer.Serialize(new
        {
            episodeId = cmd.EpisodeId,
            jobType = cmd.JobType.ToString(),
            attempt = attemptNumber,
        });

        var job = Job.Create(cmd.EpisodeId, cmd.JobType, payload, attemptNumber);
        await jobs.AddAsync(job, ct);

        // Advance episode stage to the corresponding status
        var newStatus = JobTypeToEpisodeStatus(cmd.JobType);
        episode.Advance(newStatus);
        await episodes.UpdateAsync(episode, ct);

        // Update or create the saga state row
        var saga = await sagas.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var stage = (PipelineStage)(int)newStatus;
        if (saga is null)
        {
            saga = new EpisodeSagaState { EpisodeId = cmd.EpisodeId, CurrentStage = stage, StartedAt = DateTimeOffset.UtcNow };
            await sagas.AddAsync(saga, ct);
        }
        else
        {
            saga.CurrentStage = stage;
            saga.UpdatedAt = DateTimeOffset.UtcNow;
            await sagas.UpdateAsync(saga, ct);
        }

        return Result<JobDto>.Success(job.ToDto());
    }

    private static EpisodeStatus JobTypeToEpisodeStatus(JobType type) => type switch
    {
        JobType.CharacterDesign => EpisodeStatus.CharacterDesign,
        JobType.LoraTraining    => EpisodeStatus.LoraTraining,
        JobType.Script          => EpisodeStatus.Script,
        JobType.StoryboardPlan  => EpisodeStatus.Storyboard,
        JobType.StoryboardGen   => EpisodeStatus.Storyboard,
        JobType.Voice           => EpisodeStatus.Voice,
        JobType.Animation       => EpisodeStatus.Animation,
        JobType.PostProd        => EpisodeStatus.PostProduction,
        _                       => throw new ArgumentOutOfRangeException(nameof(type)),
    };
}
