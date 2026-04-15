using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

public enum JobStatus { Pending, Running, Completed, Failed }

/// <summary>
/// Represents a single unit of work dispatched to the Python rendering engine.
/// </summary>
public sealed class Job : Entity<Guid>
{
    public Guid EpisodeId { get; private set; }
    public JobType Type { get; private set; }
    public JobStatus Status { get; private set; } = JobStatus.Pending;
    public string? Payload { get; private set; }  // JSON
    public string? Result { get; private set; }   // JSON
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset QueuedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int AttemptNumber { get; private set; }

    private Job() { }

    public static Job Create(Guid episodeId, JobType type, string? payload, int attemptNumber)
        => new()
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            Type = type,
            Status = JobStatus.Pending,
            Payload = payload,
            AttemptNumber = attemptNumber,
            QueuedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

    public void Start()
    {
        Status = JobStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(string? result)
    {
        Status = JobStatus.Completed;
        Result = result;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string error)
    {
        Status = JobStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
