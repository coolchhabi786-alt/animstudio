namespace AnimStudio.IdentityModule.Application.DTOs;

public sealed record TeamDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    Guid OwnerId,
    DateTimeOffset CreatedAt,
    int MemberCount,
    SubscriptionDto? Subscription);
