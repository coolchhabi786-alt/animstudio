namespace AnimStudio.ContentModule.Application.DTOs;

public sealed record ProjectDto(
    Guid Id,
    Guid TeamId,
    string Name,
    string Description,
    string? ThumbnailUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record EpisodeDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Idea,
    string? Style,
    string Status,
    Guid? TemplateId,
    string? DirectorNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RenderedAt);

public sealed record JobDto(
    Guid Id,
    Guid EpisodeId,
    string Type,
    string Status,
    string? Payload,
    string? Result,
    string? ErrorMessage,
    DateTimeOffset QueuedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    int AttemptNumber);

public sealed record SagaStateDto(
    Guid Id,
    Guid EpisodeId,
    string CurrentStage,
    int RetryCount,
    string? LastError,
    DateTimeOffset StartedAt,
    DateTimeOffset UpdatedAt,
    bool IsCompensating);
