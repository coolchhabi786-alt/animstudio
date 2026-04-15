namespace AnimStudio.ContentModule.Application.DTOs;

// ── Screenplay model — mirrors the Python Pydantic models in models.py ─────────

public sealed record DialogueLineDto(
    string Character,
    string Text,
    double StartTime,
    double EndTime
);

public sealed record SceneDto(
    int SceneNumber,
    string VisualDescription,
    string EmotionalTone,
    List<DialogueLineDto> Dialogue
);

public sealed record ScreenplayDto(
    string Title,
    List<SceneDto> Scenes
);

// ── Script DTO — wraps the Script entity + parsed screenplay ──────────────────

public sealed record ScriptDto(
    Guid Id,
    Guid EpisodeId,
    string Title,
    ScreenplayDto Screenplay,
    bool IsManuallyEdited,
    string? DirectorNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

// ── Request bodies ─────────────────────────────────────────────────────────────

public sealed record GenerateScriptRequest(string? DirectorNotes = null);

public sealed record SaveScriptRequest(ScreenplayDto Screenplay);

public sealed record RegenerateScriptRequest(string? DirectorNotes = null);
