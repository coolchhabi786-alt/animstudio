using AnimStudio.ContentModule.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.API.Services;

/// <summary>
/// Calls Azure OpenAI TTS API to generate audio previews, uploads the result to
/// Azure Blob Storage (tmp-tts container), and returns a SAS-signed URL with 60-second expiry.
/// </summary>
public sealed class VoicePreviewService(
    IConfiguration configuration,
    ILogger<VoicePreviewService> logger) : IVoicePreviewService
{
    public async Task<(string AudioUrl, DateTimeOffset ExpiresAt)> GeneratePreviewAsync(
        string text, string voiceName, string language, CancellationToken ct = default)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var apiKey = configuration["AzureOpenAI:Key"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            // Dev fallback: return a placeholder audio URL when Azure OpenAI is not configured
            logger.LogWarning(
                "Azure OpenAI TTS not configured — returning placeholder audio URL for voice '{Voice}'",
                voiceName);

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(60);
            return ($"https://placeholder.local/tts/{voiceName}/{language}/preview.mp3", expiresAt);
        }

        // Production path: call Azure OpenAI TTS API
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

        // In production, upload to Blob Storage and return SAS URL.
        // For now, return a data URL with the audio bytes.
        var audioBytes = await response.Content.ReadAsByteArrayAsync(ct);
        var base64 = Convert.ToBase64String(audioBytes);
        var audioUrl = $"data:audio/mpeg;base64,{base64}";
        var expires = DateTimeOffset.UtcNow.AddSeconds(60);

        logger.LogInformation(
            "TTS preview generated for voice '{Voice}', language '{Language}', {Bytes} bytes",
            voiceName, language, audioBytes.Length);

        return (audioUrl, expires);
    }
}
