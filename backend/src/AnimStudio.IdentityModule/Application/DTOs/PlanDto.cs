namespace AnimStudio.IdentityModule.Application.DTOs;

public sealed record PlanDto(
    Guid Id,
    string Name,
    string StripePriceId,
    int EpisodesPerMonth,
    int MaxCharacters,
    int MaxTeamMembers,
    decimal Price,
    bool IsDefault);
