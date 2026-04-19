using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Tracks the cost + approval lifecycle of an animation run for one episode.
/// The generic <see cref="Job"/> row tracks engine progress; this aggregate owns
/// the finance / governance side (estimated vs. actual spend, approver, status).
/// </summary>
public sealed class AnimationJob : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public AnimationBackend Backend { get; private set; }
    public decimal EstimatedCostUsd { get; private set; }
    public decimal? ActualCostUsd { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public AnimationStatus Status { get; private set; }

    private AnimationJob() { }

    public static AnimationJob Approve(
        Guid episodeId,
        AnimationBackend backend,
        decimal estimatedCostUsd,
        Guid approvedByUserId)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));
        if (estimatedCostUsd < 0)
            throw new ArgumentOutOfRangeException(nameof(estimatedCostUsd), "Cost must be non-negative.");
        if (approvedByUserId == Guid.Empty)
            throw new ArgumentException("Approver user ID is required.", nameof(approvedByUserId));

        var job = new AnimationJob
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            Backend = backend,
            EstimatedCostUsd = estimatedCostUsd,
            ApprovedByUserId = approvedByUserId,
            ApprovedAt = DateTimeOffset.UtcNow,
            Status = AnimationStatus.Approved,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        job.AddDomainEvent(new AnimationJobApprovedEvent(
            job.Id, episodeId, backend, estimatedCostUsd, approvedByUserId));
        return job;
    }

    public void MarkRunning()
    {
        if (Status != AnimationStatus.Approved)
            throw new InvalidOperationException($"Cannot transition to Running from {Status}.");
        Status = AnimationStatus.Running;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCompleted(decimal actualCostUsd)
    {
        if (Status is AnimationStatus.Completed or AnimationStatus.Failed or AnimationStatus.Cancelled)
            throw new InvalidOperationException($"Job already terminal ({Status}).");
        Status = AnimationStatus.Completed;
        ActualCostUsd = actualCostUsd;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimationJobCompletedEvent(Id, EpisodeId, Status, actualCostUsd));
    }

    public void MarkFailed()
    {
        if (Status is AnimationStatus.Completed or AnimationStatus.Failed or AnimationStatus.Cancelled)
            return;
        Status = AnimationStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimationJobCompletedEvent(Id, EpisodeId, Status, ActualCostUsd));
    }

    public void Cancel()
    {
        if (Status is AnimationStatus.Completed or AnimationStatus.Failed or AnimationStatus.Cancelled)
            return;
        Status = AnimationStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimationJobCompletedEvent(Id, EpisodeId, Status, ActualCostUsd));
    }
}
