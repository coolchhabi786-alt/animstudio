using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.ContentModule.Application.DTOs;

/// <summary>Read-model DTO for a character returned from the API.</summary>
/// <param name="Id">Character identity.</param>
/// <param name="TeamId">Owning team.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">Prose description.</param>
/// <param name="StyleDna">Style guidance used at training time.</param>
/// <param name="ImageUrl">CDN URL of reference image (nullable until PoseGeneration).</param>
/// <param name="LoraWeightsUrl">Blob URL of LoRA weights (nullable until Ready).</param>
/// <param name="TriggerWord">Prompt trigger token (nullable until Ready).</param>
/// <param name="TrainingStatus">Current training lifecycle stage.</param>
/// <param name="TrainingProgressPercent">0–100 percent for current stage.</param>
/// <param name="CreditsCost">Credits reserved for training.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="UpdatedAt">UTC last-updated timestamp.</param>
public sealed record CharacterDto(
    Guid Id,
    Guid TeamId,
    string Name,
    string? Description,
    string? StyleDna,
    string? ImageUrl,
    string? LoraWeightsUrl,
    string? TriggerWord,
    TrainingStatus TrainingStatus,
    int TrainingProgressPercent,
    int CreditsCost,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Paginated response wrapper for the team character library endpoint.
/// </summary>
/// <param name="Items">Characters in the current page.</param>
/// <param name="TotalCount">Total matching characters across all pages.</param>
/// <param name="Page">1-based current page index.</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record PagedCharactersResponse(
    List<CharacterDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>Response payload for a successfully accepted training job.</summary>
/// <param name="JobId">Placeholder job correlation ID.</param>
/// <param name="CharacterId">The character whose training was queued.</param>
/// <param name="Message">Human-readable confirmation.</param>
/// <param name="EstimatedCreditsCost">Credits that will be charged.</param>
public sealed record CharacterJobAcceptedDto(
    Guid JobId,
    Guid CharacterId,
    string Message,
    int EstimatedCreditsCost);
