using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events;

public sealed record SubscriptionQuotaExceededEvent(
    Guid SubscriptionId,
    Guid TeamId,
    int  UsageCount,
    int  Quota) : IDomainEvent;
