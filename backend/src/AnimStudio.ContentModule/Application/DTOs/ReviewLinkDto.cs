namespace AnimStudio.ContentModule.Application.DTOs;

public sealed record ReviewLinkDto(
    Guid           Id,
    string         Token,
    string         ShareUrl,
    Guid           EpisodeId,
    DateTime?      ExpiresAt,
    bool           IsRevoked,
    int            ViewCount,
    DateTimeOffset CreatedAt);

public sealed record ReviewLinkDetailDto(
    Guid                        Id,
    string                      Token,
    string                      ShareUrl,
    Guid                        EpisodeId,
    Guid                        RenderId,
    DateTime?                   ExpiresAt,
    bool                        IsRevoked,
    int                         ViewCount,
    DateTimeOffset              CreatedAt,
    RenderInfo?                 RenderInfo,
    IReadOnlyList<ReviewCommentDto> Comments);

/// <summary>Render metadata embedded in a review link detail response.</summary>
public sealed record RenderInfo(string? VideoUrl, double DurationSeconds);
