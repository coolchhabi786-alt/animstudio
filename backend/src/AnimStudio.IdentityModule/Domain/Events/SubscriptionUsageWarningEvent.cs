using AnimStudio.SharedKernel;

namespace AnimStudio.IdentityModule.Domain.Events;

public sealed record SubscriptionUsageWarningEvent(
    Guid SubscriptionId,
    Guid TeamId,
    int  UsagePercent) : IDomainEvent;
