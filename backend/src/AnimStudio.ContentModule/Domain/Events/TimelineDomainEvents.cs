using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Events;

public sealed record TimelineCreatedEvent(Guid TimelineId, Guid EpisodeId) : IDomainEvent;
public sealed record TimelineSavedEvent(Guid TimelineId, Guid EpisodeId) : IDomainEvent;
