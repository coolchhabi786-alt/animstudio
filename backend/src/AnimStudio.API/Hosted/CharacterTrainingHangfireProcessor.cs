using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel.Interfaces;
using System.Text.Json.Serialization;

namespace AnimStudio.API.Hosted;

/// <summary>
/// Hangfire job processor that dispatches character training jobs to the Python
/// worker via Azure Service Bus.
///
/// CharacterDesign  — generates reference images from the character's description.
/// LoraTraining     — trains a LoRA weight file from the generated reference images.
///
/// Both jobs use the character's ID as both jobId and episodeId in the Service Bus
/// message (episodeId is repurposed since characters are team-scoped, not episode-scoped).
/// CompletionMessageProcessor routes completions back to CompleteCharacterJobCommand.
/// </summary>
public sealed class CharacterTrainingHangfireProcessor(
    ICharacterRepository characters,
    IServiceBusPublisher serviceBusPublisher,
    ILogger<CharacterTrainingHangfireProcessor> logger)
{
    private const string JobsQueue = "jobs-queue";

    // ── CharacterDesign dispatch ──────────────────────────────────────────────

    public async Task DispatchCharacterDesignAsync(Guid characterId, CancellationToken ct = default)
    {
        var character = await characters.GetByIdAsync(characterId, ct);
        if (character is null)
        {
            logger.LogWarning("DispatchCharacterDesign: character {Id} not found — aborting", characterId);
            return;
        }

        var message = new CharacterJobMessage(
            JobId:       characterId,
            EpisodeId:   characterId,
            JobType:     "CharacterDesign",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload:     new CharacterDesignPayload(
                             character.Name,
                             character.StyleDna ?? string.Empty,
                             character.Description));

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: characterId.ToString(), ct: ct);

        logger.LogInformation(
            "CharacterDesign dispatched — character={Id}, name={Name}",
            characterId, character.Name);
    }

    // ── LoraTraining dispatch ─────────────────────────────────────────────────

    public async Task DispatchLoraTrainingAsync(Guid characterId, CancellationToken ct = default)
    {
        var character = await characters.GetByIdAsync(characterId, ct);
        if (character is null)
        {
            logger.LogWarning("DispatchLoraTraining: character {Id} not found — aborting", characterId);
            return;
        }

        if (string.IsNullOrWhiteSpace(character.ImageUrl))
        {
            logger.LogWarning(
                "DispatchLoraTraining: character {Id} has no imageUrl yet — cannot train LoRA",
                characterId);
            return;
        }

        // Build a minimal CharacterRoster payload matching the Python model format.
        var rosterDump = new
        {
            characters = new[]
            {
                new
                {
                    name               = character.Name,
                    style_dna          = character.StyleDna ?? string.Empty,
                    character_image_url = character.ImageUrl,
                    lora_weights_url   = string.Empty,
                    trigger_word       = string.Empty,
                },
            },
        };

        var message = new CharacterJobMessage(
            JobId:       characterId,
            EpisodeId:   characterId,
            JobType:     "LoraTraining",
            RequestedAt: DateTimeOffset.UtcNow,
            Payload:     new LoraTrainingPayload(rosterDump, character.Description));

        await serviceBusPublisher.PublishAsync(
            JobsQueue, message, sessionId: characterId.ToString(), ct: ct);

        logger.LogInformation(
            "LoraTraining dispatched — character={Id}, name={Name}",
            characterId, character.Name);
    }

    // ── Service Bus message types ─────────────────────────────────────────────

    private sealed record CharacterJobMessage(
        [property: JsonPropertyName("jobId")]       Guid           JobId,
        [property: JsonPropertyName("episodeId")]   Guid           EpisodeId,
        [property: JsonPropertyName("jobType")]     string         JobType,
        [property: JsonPropertyName("requestedAt")] DateTimeOffset RequestedAt,
        [property: JsonPropertyName("payload")]     object         Payload);

    private sealed record CharacterDesignPayload(
        [property: JsonPropertyName("name")]        string  Name,
        [property: JsonPropertyName("styleDna")]    string  StyleDna,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record LoraTrainingPayload(
        [property: JsonPropertyName("fullRosterDump")] object  FullRosterDump,
        [property: JsonPropertyName("description")]    string? Description = null);
}
