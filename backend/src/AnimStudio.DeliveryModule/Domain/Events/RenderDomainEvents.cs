using AnimStudio.SharedKernel;
using AnimStudio.DeliveryModule.Domain.Enums;

namespace AnimStudio.DeliveryModule.Domain.Events;

public record RenderStartedEvent(Guid RenderId, Guid EpisodeId) : IDomainEvent;

public record RenderProgressEvent(
    Guid RenderId,
    Guid EpisodeId,
    int Percent,
    string Stage) : IDomainEvent;

public record RenderCompleteEvent(
    Guid RenderId,
    Guid EpisodeId,
    string? CdnUrl,
    string? SrtUrl,
    double DurationSeconds) : IDomainEvent;

public record RenderFailedEvent(
    Guid RenderId,
    Guid EpisodeId,
    string ErrorMessage) : IDomainEvent;
