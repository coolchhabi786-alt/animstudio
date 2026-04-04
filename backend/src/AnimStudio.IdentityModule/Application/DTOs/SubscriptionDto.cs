namespace AnimStudio.IdentityModule.Application.DTOs;

public sealed record SubscriptionDto(
    Guid Id,
    string PlanName,
    string Status,
    int EpisodesUsedThisMonth,
    int EpisodesPerMonth,
    DateTimeOffset? CurrentPeriodEnd,
    DateTimeOffset? TrialEndsAt,
    bool CancelAtPeriodEnd,
    string StripeCustomerId = "");
