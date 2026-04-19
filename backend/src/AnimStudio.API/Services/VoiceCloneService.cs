using AnimStudio.ContentModule.Application.Interfaces;

namespace AnimStudio.API.Services;

/// <summary>
/// Stub implementation of <see cref="IVoiceCloneService"/>.
/// Voice cloning integration (ElevenLabs / Azure Custom Neural Voice) is planned for a future release.
/// Returns a "not available" response until the integration is implemented.
/// </summary>
public sealed class VoiceCloneService(ILogger<VoiceCloneService> logger) : IVoiceCloneService
{
    public Task<(string? VoiceCloneUrl, string Status)> CloneVoiceAsync(
        Guid characterId, string? audioSampleUrl, CancellationToken ct = default)
    {
        logger.LogWarning(
            "Voice cloning requested for character {CharacterId} but the feature is not yet implemented",
            characterId);

        // Stub: return a placeholder indicating the feature is not available
        return Task.FromResult<(string?, string)>((null, "NotAvailable"));
    }
}
