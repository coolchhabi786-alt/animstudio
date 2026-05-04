namespace AnimStudio.ContentModule.Application.DTOs;

public sealed record ReviewCommentDto(
    Guid           Id,
    string         AuthorName,
    string         Text,
    double         TimestampSeconds,
    bool           IsResolved,
    DateTimeOffset CreatedAt);
