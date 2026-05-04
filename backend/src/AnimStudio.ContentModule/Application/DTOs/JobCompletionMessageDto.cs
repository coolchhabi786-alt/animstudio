using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.ContentModule.Application.DTOs;

/// <summary>
/// Shared contract for completion messages published to the "completions" Service Bus queue
/// by the Python cartoon_automation pipeline.
/// </summary>
public sealed record JobCompletionMessageDto(
    [property: JsonPropertyName("jobId")]        Guid              JobId,
    [property: JsonPropertyName("episodeId")]    Guid              EpisodeId,
    [property: JsonPropertyName("jobType")]      string            JobType,
    [property: JsonPropertyName("status")]       string            Status,
    [property: JsonPropertyName("result")]       JsonElement?      Result,
    [property: JsonPropertyName("errorMessage")] string?           ErrorMessage,
    [property: JsonPropertyName("completedAt")]  DateTimeOffset    CompletedAt);

// ── Per-type result shapes (deserialised from JobCompletionMessageDto.Result) ─────────────

public sealed record CharacterDesignResult(
    [property: JsonPropertyName("imageUrl")] string ImageUrl);

public sealed record LoraTrainingResult(
    [property: JsonPropertyName("loraWeightsUrl")] string LoraWeightsUrl,
    [property: JsonPropertyName("triggerWord")]    string TriggerWord);

public sealed record ScriptResult(
    [property: JsonPropertyName("screenplay")] JsonElement Screenplay);

public sealed record StoryboardPlanJobResult(
    [property: JsonPropertyName("screenplayTitle")] string? ScreenplayTitle,
    [property: JsonPropertyName("shots")]            List<StoryboardPlanShotDto> Shots);

public sealed record StoryboardPlanShotDto(
    [property: JsonPropertyName("sceneNumber")] int    SceneNumber,
    [property: JsonPropertyName("shotIndex")]   int    ShotIndex,
    [property: JsonPropertyName("description")] string? Description);

public sealed record StoryboardGenJobResult(
    [property: JsonPropertyName("shots")] List<StoryboardGenShotDto> Shots);

public sealed record StoryboardGenShotDto(
    [property: JsonPropertyName("sceneNumber")] int    SceneNumber,
    [property: JsonPropertyName("shotIndex")]   int    ShotIndex,
    [property: JsonPropertyName("imageUrl")]    string ImageUrl);

public sealed record AnimationJobResult(
    [property: JsonPropertyName("clips")]         List<AnimationClipResultDto> Clips,
    [property: JsonPropertyName("actualCostUsd")] decimal?                     ActualCostUsd);

public sealed record AnimationClipResultDto(
    [property: JsonPropertyName("sceneNumber")]     int    SceneNumber,
    [property: JsonPropertyName("shotIndex")]       int    ShotIndex,
    [property: JsonPropertyName("clipUrl")]         string ClipUrl,
    [property: JsonPropertyName("durationSeconds")] double DurationSeconds);

public sealed record PostProdResult(
    [property: JsonPropertyName("videoUrl")]         string  VideoUrl,
    [property: JsonPropertyName("srtUrl")]           string? SrtUrl,
    [property: JsonPropertyName("durationSeconds")]  double  DurationSeconds);
