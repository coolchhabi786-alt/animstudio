namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Stub interface for voice cloning integration (ElevenLabs / Azure Custom Neural Voice).
/// Studio tier only.
/// </summary>
public interface IVoiceCloneService
{
    /// <summary>
    /// Initiates a voice cloning process from an audio sample.
    /// Returns the URL to the cloned voice model and its processing status.
    /// </summary>
    Task<(string? VoiceCloneUrl, string Status)> CloneVoiceAsync(
        Guid characterId,
        string? audioSampleUrl,
        CancellationToken ct = default);
}
