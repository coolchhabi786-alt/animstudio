namespace AnimStudio.ContentModule.Application.DTOs;

/// <summary>Voice assignment DTO returned by GET /episodes/{id}/voices.</summary>
public sealed record VoiceAssignmentDto(
    Guid Id,
    Guid EpisodeId,
    Guid CharacterId,
    string CharacterName,
    string VoiceName,
    string Language,
    string? VoiceCloneUrl,
    DateTimeOffset UpdatedAt);

/// <summary>Single assignment in the batch update request body.</summary>
public sealed record VoiceAssignmentRequest(
    Guid CharacterId,
    string VoiceName,
    string Language,
    string? VoiceCloneUrl = null);

/// <summary>Batch update request body for PUT /episodes/{id}/voices.</summary>
public sealed record BatchUpdateVoicesRequest(
    List<VoiceAssignmentRequest> Assignments);

/// <summary>Request body for POST /voices/preview.</summary>
public sealed record VoicePreviewRequest(
    string Text,
    string VoiceName,
    string Language = "en-US");

/// <summary>Response for POST /voices/preview.</summary>
public sealed record VoicePreviewResponse(
    string AudioUrl,
    DateTimeOffset ExpiresAt);

/// <summary>Request body for POST /voices/clone.</summary>
public sealed record VoiceCloneRequest(
    Guid CharacterId,
    string? AudioSampleUrl = null);

/// <summary>Response for POST /voices/clone.</summary>
public sealed record VoiceCloneResponse(
    string? VoiceCloneUrl,
    string Status);
