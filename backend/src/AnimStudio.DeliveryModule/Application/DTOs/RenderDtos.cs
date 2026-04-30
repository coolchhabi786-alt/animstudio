using AnimStudio.DeliveryModule.Domain.Enums;

namespace AnimStudio.DeliveryModule.Application.DTOs;

public sealed record RenderDto(
    Guid Id,
    Guid EpisodeId,
    RenderAspectRatio AspectRatio,
    RenderStatus Status,
    string? FinalVideoUrl,
    string? CdnUrl,
    string? SrtUrl,
    double DurationSeconds,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record StartRenderRequest(RenderAspectRatio AspectRatio);
