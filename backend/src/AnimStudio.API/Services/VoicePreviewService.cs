using AnimStudio.ContentModule.Application.Interfaces;

namespace AnimStudio.API.Services;

/// <summary>
/// Calls Azure OpenAI TTS API to synthesize speech, uploads the audio stream to Blob Storage
/// via <see cref="IFileStorageService.SavePreviewAsync"/>, and returns a 1-hour SAS URL.
/// Falls back to a placeholder URL when <c>AzureOpenAI:Endpoint</c> / <c>AzureOpenAI:Key</c>
/// are absent (local dev without Azure).
/// </summary>
public sealed class VoicePreviewService(
    IConfiguration       configuration,
    IFileStorageService  fileStorage,
    ILogger<VoicePreviewService> logger) : IVoicePreviewService
{
    public async Task<(string AudioUrl, DateTimeOffset ExpiresAt)> GeneratePreviewAsync(
        string text, string voiceName, string language, CancellationToken ct = default)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var apiKey   = configuration["AzureOpenAI:Key"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning(
                "Azure OpenAI TTS not configured — returning placeholder URL for voice '{Voice}'",
                voiceName);
            return (
                $"https://placeholder.local/tts/{voiceName}/{language}/preview.mp3",
                DateTimeOffset.UtcNow.AddHours(1));
        }

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

        var requestBody = new
        {
            model = "tts-1",
            input = text,
            voice = voiceName.ToLowerInvariant(),
        };

        var response = await httpClient.PostAsJsonAsync(
            $"{endpoint}/openai/deployments/tts-1/audio/speech?api-version=2024-02-15-preview",
            requestBody, ct);

        response.EnsureSuccessStatusCode();

        await using var audioStream = await response.Content.ReadAsStreamAsync(ct);

        var timestamp   = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
        var previewPath = $"previews/tts/{voiceName}/{timestamp}.mp3";

        var audioUrl = await fileStorage.SavePreviewAsync(audioStream, previewPath, "audio/mpeg", ct);
        var expires  = DateTimeOffset.UtcNow.AddHours(1);

        logger.LogInformation(
            "TTS preview generated for voice '{Voice}', language '{Language}', uploaded to {Path}",
            voiceName, language, previewPath);

        return (audioUrl, expires);
    }
}
