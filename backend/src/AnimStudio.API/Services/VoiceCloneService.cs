using AnimStudio.ContentModule.Application.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.API.Services;

/// <summary>
/// Implements <see cref="IVoiceCloneService"/> via the ElevenLabs Add Voice API
/// (<c>POST /v1/voices/add</c>). Downloads the audio sample from <paramref name="audioSampleUrl"/>,
/// sends it as multipart form data, and returns the resulting ElevenLabs <c>voice_id</c>.
///
/// <para>Dev fallback: when <c>ElevenLabs:ApiKey</c> is absent, returns a deterministic
/// <c>dev-clone-{characterId}</c> ID so the rest of the flow can be exercised locally.</para>
/// </summary>
public sealed class VoiceCloneService(
    IConfiguration configuration,
    ILogger<VoiceCloneService> logger) : IVoiceCloneService
{
    private const string ElevenLabsBaseUrl = "https://api.elevenlabs.io";

    public async Task<(string? VoiceCloneUrl, string Status)> CloneVoiceAsync(
        Guid characterId, string? audioSampleUrl, CancellationToken ct = default)
    {
        var apiKey = configuration["ElevenLabs:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            var devId = $"dev-clone-{characterId:N}";
            logger.LogInformation(
                "ElevenLabs:ApiKey not configured — returning dev clone ID '{DevId}'", devId);
            return (devId, "Ready");
        }

        if (string.IsNullOrWhiteSpace(audioSampleUrl))
        {
            logger.LogWarning(
                "Voice clone requested for character {CharacterId} but no audio sample URL provided",
                characterId);
            return (null, "Failed");
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);

            // Download the audio sample from blob/CDN URL.
            var sampleBytes = await httpClient.GetByteArrayAsync(audioSampleUrl, ct);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(characterId.ToString("N")), "name");
            form.Add(new StringContent("AnimStudio character voice clone"), "description");

            var fileContent = new ByteArrayContent(sampleBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
            form.Add(fileContent, "files", "sample.mp3");

            var response = await httpClient.PostAsync(
                $"{ElevenLabsBaseUrl}/v1/voices/add", form, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "ElevenLabs voice clone failed for character {CharacterId}: HTTP {StatusCode} — {Body}",
                    characterId, (int)response.StatusCode, body);
                return (null, "Failed");
            }

            var json    = await response.Content.ReadAsStringAsync(ct);
            var result  = JsonSerializer.Deserialize<ElevenLabsAddVoiceResponse>(json);
            var voiceId = result?.VoiceId;

            if (string.IsNullOrWhiteSpace(voiceId))
            {
                logger.LogError(
                    "ElevenLabs response missing voice_id for character {CharacterId}: {Json}",
                    characterId, json);
                return (null, "Failed");
            }

            logger.LogInformation(
                "ElevenLabs voice clone created for character {CharacterId}: voice_id={VoiceId}",
                characterId, voiceId);

            return (voiceId, "Ready");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during ElevenLabs voice cloning for character {CharacterId}", characterId);
            return (null, "Failed");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Timeout during ElevenLabs voice cloning for character {CharacterId}", characterId);
            return (null, "Failed");
        }
    }

    private sealed record ElevenLabsAddVoiceResponse(
        [property: JsonPropertyName("voice_id")] string? VoiceId);
}
