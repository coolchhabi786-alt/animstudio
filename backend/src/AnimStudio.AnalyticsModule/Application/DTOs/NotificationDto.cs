using AnimStudio.AnalyticsModule.Domain.Enums;

namespace AnimStudio.AnalyticsModule.Application.DTOs;

public sealed record NotificationDto(
    Guid             Id,
    NotificationType Type,
    string           Title,
    string           Body,
    bool             IsRead,
    DateTimeOffset?  ReadAt,
    Guid?            RelatedEntityId,
    string?          RelatedEntityType,
    DateTimeOffset   CreatedAt);
