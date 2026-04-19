using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Events;

/// <summary>Raised when a voice is first assigned to a character in an episode.</summary>
public sealed record VoiceAssignedEvent(
    Guid VoiceAssignmentId,
    Guid EpisodeId,
    Guid CharacterId,
    string VoiceName) : IDomainEvent;

/// <summary>Raised when an existing voice assignment is updated (voice name, language, or clone URL changed).</summary>
public sealed record VoiceAssignmentUpdatedEvent(
    Guid VoiceAssignmentId,
    Guid EpisodeId,
    Guid CharacterId,
    string VoiceName) : IDomainEvent;
