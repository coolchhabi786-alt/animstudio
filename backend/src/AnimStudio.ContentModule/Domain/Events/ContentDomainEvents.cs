using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Events;

public sealed record ProjectCreatedEvent(Guid ProjectId, Guid TeamId, string Name) : IDomainEvent;

public sealed record EpisodeCreatedEvent(Guid EpisodeId, Guid ProjectId, string Name) : IDomainEvent;

public sealed record EpisodeStageAdvancedEvent(Guid EpisodeId, EpisodeStatus NewStage) : IDomainEvent;

public sealed record EpisodeFailedEvent(Guid EpisodeId, EpisodeStatus FailedAtStage, string Error) : IDomainEvent;

public sealed record EpisodeCompletedEvent(Guid EpisodeId) : IDomainEvent;

public sealed record JobQueuedEvent(Guid JobId, Guid EpisodeId, JobType Type, int AttemptNumber) : IDomainEvent;

public sealed record JobCompletedEvent(Guid JobId, Guid EpisodeId, JobType Type, string? Result) : IDomainEvent;
