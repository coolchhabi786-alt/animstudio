using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application;

/// <summary>
/// Extension methods to map domain entities to DTOs.
/// Keeps mapping logic in the Application layer without AutoMapper.
/// </summary>
public static class CharacterMappings
{
    /// <summary>Maps a <see cref="Character"/> aggregate to a <see cref="CharacterDto"/>.</summary>
    public static CharacterDto ToDto(this Character c) =>
        new(
            c.Id,
            c.TeamId,
            c.Name,
            c.Description,
            c.StyleDna,
            c.ImageUrl,
            c.LoraWeightsUrl,
            c.TriggerWord,
            c.TrainingStatus,
            c.TrainingProgressPercent,
            c.CreditsCost,
            c.CreatedAt,
            c.UpdatedAt);
}
