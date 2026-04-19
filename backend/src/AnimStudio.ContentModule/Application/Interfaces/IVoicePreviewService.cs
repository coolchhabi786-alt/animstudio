namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Generates TTS audio previews via Azure OpenAI and uploads to temporary Blob storage.
/// </summary>
public interface IVoicePreviewService
{
    /// <summary>
    /// Generates a TTS audio preview and returns a signed Blob URL with 60-second expiry.
    /// </summary>
    Task<(string AudioUrl, DateTimeOffset ExpiresAt)> GeneratePreviewAsync(
        string text,
        string voiceName,
        string language,
        CancellationToken ct = default);
}
