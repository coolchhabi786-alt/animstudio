using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Enums;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.HandleJobCompletion;

public sealed record HandleJobCompletionCommand(Guid JobId, bool IsSuccess, string? Result, string? Error) : IRequest<Result<bool>>;

public sealed class HandleJobCompletionHandler(
    IJobRepository jobs,
    IEpisodeRepository episodes,
    ISagaStateRepository sagas)
    : IRequestHandler<HandleJobCompletionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(HandleJobCompletionCommand cmd, CancellationToken ct)
    {
        var job = await jobs.GetByIdAsync(cmd.JobId, ct);
        if (job is null) return Result<bool>.Failure("Job not found", "NOT_FOUND");

        var episode = await episodes.GetByIdAsync(job.EpisodeId, ct);
        if (episode is null) return Result<bool>.Failure("Episode not found", "NOT_FOUND");

        if (cmd.IsSuccess)
        {
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
