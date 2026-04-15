namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Join table linking an <see cref="Episode"/> to the <see cref="Character"/> instances
/// that appear in it. Composite primary key: (EpisodeId, CharacterId).
/// Only <see cref="Enums.TrainingStatus.Ready"/> characters may be attached.
/// </summary>
public sealed class EpisodeCharacter
{
    /// <summary>FK to <see cref="Episode"/>.</summary>
    public Guid EpisodeId { get; set; }

    /// <summary>FK to <see cref="Character"/>.</summary>
    public Guid CharacterId { get; set; }

    /// <summary>Timestamp when the character was cast into the episode.</summary>
    public DateTimeOffset AttachedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Navigation ──────────────────────────────────────────────────────────
    /// <summary>The episode the character is cast in.</summary>
    public Episode Episode { get; set; } = null!;

    /// <summary>The character that has been cast.</summary>
    public Character Character { get; set; } = null!;
}
