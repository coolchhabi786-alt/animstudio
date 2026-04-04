namespace AnimStudio.IdentityModule.Application.DTOs;

public sealed record TeamMemberDto(
    Guid UserId,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    bool IsAccepted,
    DateTimeOffset JoinedAt);
