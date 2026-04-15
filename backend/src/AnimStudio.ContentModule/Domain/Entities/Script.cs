using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Script aggregate — represents the AI-generated or manually edited screenplay for an episode.
/// Stores the raw JSON of the Screenplay Pydantic model from the Python engine.
/// </summary>
public sealed class Script : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialised Screenplay model produced by the Python ScriptwritingCrew.
    /// Schema: { title, scenes: [{ scene_number, visual_description, emotional_tone,
    ///           dialogue: [{ character, text, start_time, end_time }] }] }
    /// </summary>
    public string RawJson { get; private set; } = "{}";

    public bool IsManuallyEdited { get; private set; }
    public string? DirectorNotes { get; private set; }

    private Script() { }

    public static Script Create(Guid episodeId, string title, string rawJson)
    {
        var script = new Script
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            Title = title,
            RawJson = rawJson,
            IsManuallyEdited = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        script.AddDomainEvent(new ScriptGeneratedEvent(script.Id, episodeId));
        return script;
    }

    /// <summary>Called when the Python engine returns a completed script job.</summary>
    public void UpdateFromJob(string rawJson, string title)
    {
        RawJson = rawJson;
        Title = title;
        IsManuallyEdited = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ScriptUpdatedEvent(Id, EpisodeId));
    }

    /// <summary>Called when a user saves manual edits via PUT /episodes/{id}/script.</summary>
    public void SaveManualEdits(string rawJson)
    {
        RawJson = rawJson;
        IsManuallyEdited = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ScriptManuallyEditedEvent(Id, EpisodeId));
    }

    /// <summary>Sets director notes used when re-queuing a regeneration job.</summary>
    public void SetDirectorNotes(string? notes)
    {
        DirectorNotes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
