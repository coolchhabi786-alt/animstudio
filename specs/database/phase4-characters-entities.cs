// ============================================================
// Phase 4 — Character Studio
// EF Core C# entity definitions with data annotations,
// navigation properties, and XML doc comments.
// ============================================================

using AnimStudio.SharedKernel;
using System.ComponentModel.DataAnnotations;

namespace AnimStudio.ContentModule.Domain.Entities;

// ── TrainingStatus enum ──────────────────────────────────────────────────────

/// <summary>
/// LoRA training lifecycle states for a <see cref="Character"/>.
/// </summary>
public enum TrainingStatus
{
    /// <summary>Character created but training has not started.</summary>
    Draft,
    /// <summary>AI pipeline is generating reference poses.</summary>
    PoseGeneration,
    /// <summary>Training job is queued in Azure Service Bus.</summary>
    TrainingQueued,
    /// <summary>GPU training is actively running.</summary>
    Training,
    /// <summary>LoRA weights are available; character is usable in episodes.</summary>
    Ready,
    /// <summary>Training failed; can be retried.</summary>
    Failed,
}

// ── Character ────────────────────────────────────────────────────────────────

/// <summary>
/// A team-scoped animated character whose visual style is encoded as a LoRA
/// (Low-Rank Adaptation) weight file. Characters progress through a training
/// pipeline before they can be attached to Episodes.
/// </summary>
public sealed class Character : AggregateRoot<Guid>
{
    /// <summary>The team that owns this character.</summary>
    [Required]
    public Guid TeamId { get; private set; }

    /// <summary>Display name. Max 200 chars.</summary>
    [Required, MaxLength(200)]
    public string Name { get; private set; } = string.Empty;

    /// <summary>Prose description used as context for the AI pipeline. Max 2000 chars.</summary>
    [MaxLength(2000)]
    public string? Description { get; private set; }

    /// <summary>
    /// Free-text style DNA / prompt guidance fed directly to the LoRA trainer.
    /// Examples: "anime, pastel palette, big expressive eyes". Max 4000 chars.
    /// </summary>
    [MaxLength(4000)]
    public string? StyleDna { get; private set; }

    /// <summary>CDN URL of the generated reference image (set after PoseGeneration).</summary>
    [MaxLength(2048)]
    public string? ImageUrl { get; private set; }

    /// <summary>Azure Blob Storage URL of the trained LoRA weights (.safetensors).</summary>
    [MaxLength(2048)]
    public string? LoraWeightsUrl { get; private set; }

    /// <summary>
    /// Short LoRA trigger word injected into prompts at render time.
    /// Example: "PROF_WHISKERBOLT". Max 100 chars.
    /// </summary>
    [MaxLength(100)]
    public string? TriggerWord { get; private set; }

    /// <summary>Current position in the LoRA training pipeline.</summary>
    [Required]
    public TrainingStatus TrainingStatus { get; private set; } = TrainingStatus.Draft;

    /// <summary>0-100 percent completion of the current training stage.</summary>
    public int TrainingProgressPercent { get; private set; }

    /// <summary>Credits consumed (or to be consumed) for training. Set at creation time.</summary>
    public int CreditsCost { get; private set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    private readonly List<EpisodeCharacter> _episodeCharacters = new();
    /// <summary>Many-to-many links to Episodes.</summary>
    public IReadOnlyCollection<EpisodeCharacter> EpisodeCharacters => _episodeCharacters.AsReadOnly();

    private Character() { }

    // ── Factory ────────────────────────────────────────────────────────────

    /// <summary>Creates a new Draft character and raises <see cref="CharacterCreatedEvent"/>.</summary>
    public static Character Create(Guid teamId, string name, string? description, string? styleDna, int creditsCost)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var character = new Character
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            Name = name.Trim(),
            Description = description?.Trim(),
            StyleDna = styleDna?.Trim(),
            CreditsCost = creditsCost,
            TrainingStatus = TrainingStatus.Draft,
            TrainingProgressPercent = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        character.AddDomainEvent(new CharacterCreatedEvent(character.Id, teamId, name));
        return character;
    }

    // ── Behaviour ──────────────────────────────────────────────────────────

    /// <summary>Advance to a new training stage with an optional progress snapshot.</summary>
    public void AdvanceTraining(TrainingStatus newStatus, int progressPercent = 0,
        string? imageUrl = null, string? loraWeightsUrl = null, string? triggerWord = null)
    {
        TrainingStatus = newStatus;
        TrainingProgressPercent = Math.Clamp(progressPercent, 0, 100);
        if (imageUrl is not null) ImageUrl = imageUrl;
        if (loraWeightsUrl is not null) LoraWeightsUrl = loraWeightsUrl;
        if (triggerWord is not null) TriggerWord = triggerWord;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CharacterTrainingProgressedEvent(Id, TeamId, newStatus, TrainingProgressPercent));

        if (newStatus == TrainingStatus.Ready)
            AddDomainEvent(new CharacterReadyEvent(Id, TeamId));
    }

    /// <summary>Mark the character as failed with an optional reason.</summary>
    public void FailTraining(string? reason = null)
    {
        TrainingStatus = TrainingStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CharacterTrainingFailedEvent(Id, TeamId, reason));
    }

    /// <summary>Soft-delete guard — cannot delete if attached to an active episode.</summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

// ── EpisodeCharacter (many-to-many join) ────────────────────────────────────

/// <summary>
/// Join table linking an <see cref="Episode"/> to the <see cref="Character"/> instances
/// that appear in it. No additional payload — the relationship itself is the fact.
/// </summary>
public sealed class EpisodeCharacter
{
    /// <summary>FK to <see cref="Episode.Id"/>.</summary>
    [Required]
    public Guid EpisodeId { get; set; }

    /// <summary>FK to <see cref="Character.Id"/>.</summary>
    [Required]
    public Guid CharacterId { get; set; }

    /// <summary>When was the character attached.</summary>
    public DateTimeOffset AttachedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Navigation ──────────────────────────────────────────────────────────
    public Episode Episode { get; set; } = null!;
    public Character Character { get; set; } = null!;
}
