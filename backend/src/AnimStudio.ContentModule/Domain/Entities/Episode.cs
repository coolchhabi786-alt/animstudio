using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Episode aggregate — represents a single animated episode moving through the pipeline.
/// Uses optimistic concurrency (RowVersion) to prevent race conditions during stage transitions.
/// </summary>
public sealed class Episode : AggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Idea { get; private set; }
    public string? Style { get; private set; }
    public EpisodeStatus Status { get; private set; } = EpisodeStatus.Idle;
    public Guid? TemplateId { get; private set; }
    public string CharacterIds { get; private set; } = "[]"; // JSON array stored as text
    public string? DirectorNotes { get; private set; }
    public DateTimeOffset? RenderedAt { get; private set; }

    private Episode() { }

    public static Episode Create(Guid projectId, string name, string idea, string style, Guid? templateId = null)
    {
        var episode = new Episode
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name,
            Idea = idea,
            Style = style,
            TemplateId = templateId,
            Status = EpisodeStatus.Idle,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        episode.AddDomainEvent(new EpisodeCreatedEvent(episode.Id, projectId, name));
        return episode;
    }

    public void Advance(EpisodeStatus newStage)
    {
        Status = newStage;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (newStage == EpisodeStatus.Done)
        {
            RenderedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new EpisodeCompletedEvent(Id));
        }
        else
        {
            AddDomainEvent(new EpisodeStageAdvancedEvent(Id, newStage));
        }
    }

    public void Fail(string error)
    {
        var failedAt = Status;
        Status = EpisodeStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new EpisodeFailedEvent(Id, failedAt, error));
    }

    public void Retry()
    {
        if (Status != EpisodeStatus.Failed)
            throw new InvalidOperationException("Only failed episodes can be retried.");
        // Rewind to the last meaningful stage (CharacterDesign as a safe default).
        Status = EpisodeStatus.CharacterDesign;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new EpisodeStageAdvancedEvent(Id, Status));
    }

    public void Complete()
    {
        Status = EpisodeStatus.Done;
        RenderedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new EpisodeCompletedEvent(Id));
    }

    public void SoftDelete(Guid deletedByUserId)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByUserId = deletedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
