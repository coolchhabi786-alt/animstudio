using AnimStudio.AnalyticsModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.AnalyticsModule.Domain.Entities;

public sealed class Notification : AggregateRoot<Guid>
{
    public Guid             UserId            { get; private set; }
    public NotificationType Type              { get; private set; }
    public string           Title             { get; private set; } = string.Empty;
    public string           Body              { get; private set; } = string.Empty;
    public bool             IsRead            { get; private set; }
    public DateTimeOffset?  ReadAt            { get; private set; }
    public Guid?            RelatedEntityId   { get; private set; }
    public string?          RelatedEntityType { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        return new Notification
        {
            Id                = Guid.NewGuid(),
            UserId            = userId,
            Type              = type,
            Title             = title,
            Body              = body,
            IsRead            = false,
            RelatedEntityId   = relatedEntityId,
            RelatedEntityType = relatedEntityType,
        };
    }

    public void MarkRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }
}
