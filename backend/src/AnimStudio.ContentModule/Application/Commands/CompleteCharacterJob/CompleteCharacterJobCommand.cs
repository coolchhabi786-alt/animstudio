using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.ContentModule.Application.Commands.CompleteCharacterJob;

/// <summary>
/// Updates a character's training state when a CharacterDesign or LoraTraining
/// job completes (or fails) on the Python worker.
///
/// Character training is team-scoped (not episode-scoped) so it bypasses the
/// normal Job/Episode tracking path in HandleJobCompletionCommand.
/// </summary>
public sealed record CompleteCharacterJobCommand(
    Guid    CharacterId,
    bool    IsSuccess,
    string? ResultJson,
    string? ErrorMessage,
    string  JobType) : IRequest<Result<Unit>>;

public sealed class CompleteCharacterJobHandler(
    ICharacterRepository       characters,
    ICharacterProgressNotifier notifier,
    ILogger<CompleteCharacterJobHandler> logger)
    : IRequestHandler<CompleteCharacterJobCommand, Result<Unit>>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<Unit>> Handle(CompleteCharacterJobCommand cmd, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(cmd.CharacterId, ct);
        if (character is null)
        {
            logger.LogWarning("CompleteCharacterJob: character {CharacterId} not found", cmd.CharacterId);
            return Result<Unit>.Failure($"Character {cmd.CharacterId} not found.", "NOT_FOUND");
        }

        if (!cmd.IsSuccess)
        {
            character.FailTraining(cmd.ErrorMessage);
            await characters.UpdateAsync(character, ct);
            await notifier.NotifyAsync(
                character.TeamId, character.Id,
                character.TrainingStatus.ToString(), 0,
                cmd.JobType, ct);
            logger.LogWarning(
                "Character {CharacterId} {JobType} failed: {Error}",
                cmd.CharacterId, cmd.JobType, cmd.ErrorMessage);
            return Result<Unit>.Success(Unit.Value);
        }

        if (string.Equals(cmd.JobType, "CharacterDesign", StringComparison.OrdinalIgnoreCase))
        {
            string? imageUrl = null;
            int datasetImageCount = 0;
            if (cmd.ResultJson is not null)
            {
                try
                {
                    var r = JsonSerializer.Deserialize<DesignResult>(cmd.ResultJson, JsonOpts);
                    imageUrl = r?.ImageUrl;
                    datasetImageCount = r?.DatasetImageCount ?? 0;
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(ex, "Failed to parse CharacterDesign result for {CharacterId}", cmd.CharacterId);
                }
            }

            character.SetDatasetImageCount(datasetImageCount);
            character.AdvanceTraining(TrainingStatus.Training, progressPercent: 50, imageUrl: imageUrl);
            await characters.UpdateAsync(character, ct);
            await notifier.NotifyAsync(
                character.TeamId, character.Id,
                character.TrainingStatus.ToString(),
                character.TrainingProgressPercent,
                "CharacterDesign", ct);
            logger.LogInformation(
                "Character {CharacterId} design complete — imageUrl={Url}",
                cmd.CharacterId, imageUrl);
        }
        else if (string.Equals(cmd.JobType, "LoraTraining", StringComparison.OrdinalIgnoreCase))
        {
            string? loraUrl = null;
            string? triggerWord = null;
            if (cmd.ResultJson is not null)
            {
                try
                {
                    var r = JsonSerializer.Deserialize<LoraResult>(cmd.ResultJson, JsonOpts);
                    loraUrl = r?.LoraWeightsUrl;
                    triggerWord = r?.TriggerWord;
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(ex, "Failed to parse LoraTraining result for {CharacterId}", cmd.CharacterId);
                }
            }

            character.AdvanceTraining(
                TrainingStatus.Ready,
                progressPercent: 100,
                loraWeightsUrl: loraUrl,
                triggerWord: triggerWord);
            await characters.UpdateAsync(character, ct);
            await notifier.NotifyAsync(
                character.TeamId, character.Id,
                character.TrainingStatus.ToString(),
                character.TrainingProgressPercent,
                "LoraTraining", ct);
            logger.LogInformation(
                "Character {CharacterId} LoRA training complete — trigger={Trigger}",
                cmd.CharacterId, triggerWord);
        }

        return Result<Unit>.Success(Unit.Value);
    }

    private sealed record DesignResult(
        [property: JsonPropertyName("imageUrl")]          string? ImageUrl,
        [property: JsonPropertyName("datasetImageCount")] int     DatasetImageCount);

    private sealed record LoraResult(
        [property: JsonPropertyName("loraWeightsUrl")] string? LoraWeightsUrl,
        [property: JsonPropertyName("triggerWord")]    string? TriggerWord);
}
