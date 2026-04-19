using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Maps a character to a voice within an episode.
/// One assignment per (EpisodeId, CharacterId) pair.
/// VoiceCloneUrl is nullable — populated only for Studio-tier custom voice clones.
/// </summary>
public sealed class VoiceAssignment : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public Guid CharacterId { get; private set; }

    /// <summary>Built-in voice name (e.g. "Alloy") or custom clone name.</summary>
    public string VoiceName { get; private set; } = string.Empty;

    /// <summary>BCP-47 language tag, e.g. "en-US".</summary>
    public string Language { get; private set; } = "en-US";

    /// <summary>
    /// Signed Blob URL to a custom voice clone audio sample.
    /// Null for built-in voices; populated only for Studio-tier subscribers.
    /// </summary>
    public string? VoiceCloneUrl { get; private set; }

    private VoiceAssignment() { }

    public static VoiceAssignment Create(
        Guid episodeId,
        Guid characterId,
        string voiceName,
        string language,
        string? voiceCloneUrl = null)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));
        if (characterId == Guid.Empty)
            throw new ArgumentException("Character ID is required.", nameof(characterId));
        if (string.IsNullOrWhiteSpace(voiceName))
            throw new ArgumentException("Voice name is required.", nameof(voiceName));

        var assignment = new VoiceAssignment
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            CharacterId = characterId,
            VoiceName = voiceName,
            Language = string.IsNullOrWhiteSpace(language) ? "en-US" : language,
            VoiceCloneUrl = voiceCloneUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        assignment.AddDomainEvent(new VoiceAssignedEvent(assignment.Id, episodeId, characterId, voiceName));
        return assignment;
    }

    /// <summary>Updates the voice assignment with new values.</summary>
    public void Update(string voiceName, string language, string? voiceCloneUrl)
    {
        if (string.IsNullOrWhiteSpace(voiceName))
            throw new ArgumentException("Voice name is required.", nameof(voiceName));

        VoiceName = voiceName;
        Language = string.IsNullOrWhiteSpace(language) ? "en-US" : language;
        VoiceCloneUrl = voiceCloneUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new VoiceAssignmentUpdatedEvent(Id, EpisodeId, CharacterId, voiceName));
    }
}
