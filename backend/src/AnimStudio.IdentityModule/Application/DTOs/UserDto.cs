namespace AnimStudio.IdentityModule.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string ExternalId,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTimeOffset CreatedAt);
