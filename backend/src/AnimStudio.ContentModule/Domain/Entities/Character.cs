using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// A team-scoped animated character whose visual style is encoded as a LoRA
/// (Low-Rank Adaptation) weight file. Characters progress through a training
/// pipeline before they can be attached to Episodes.
/// </summary>
public sealed class Character : AggregateRoot<Guid>
{
    /// <summary>The team that owns this character.</summary>
    public Guid TeamId { get; private set; }

    /// <summary>Display name shown in the Character Studio. Max 200 chars.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Prose description used as context by the AI pipeline. Max 2000 chars.</summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Free-text style guidance fed directly to the LoRA trainer.
    /// Example: "anime, pastel palette, big expressive eyes". Max 4000 chars.
    /// </summary>
    public string? StyleDna { get; private set; }

    /// <summary>CDN URL of the generated reference image (set after PoseGeneration stage).</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Azure Blob Storage URL of the trained LoRA weights (.safetensors).</summary>
    public string? LoraWeightsUrl { get; private set; }

    /// <summary>
    /// Short LoRA trigger word injected into render prompts at generation time.
    /// Example: "PROF_WHISKERBOLT". Max 100 chars.
    /// </summary>
    public string? TriggerWord { get; private set; }

    /// <summary>Current position in the LoRA training pipeline.</summary>
    public TrainingStatus TrainingStatus { get; private set; } = TrainingStatus.Draft;

    /// <summary>Completion percentage for the current training stage (0–100).</summary>
    public int TrainingProgressPercent { get; private set; }

    /// <summary>Credits consumed (or reserved) for training. Set at creation time.</summary>
    public int CreditsCost { get; private set; }

    // ── Navigation ──────────────────────────────────────────────────────────
    private readonly List<EpisodeCharacter> _episodeCharacters = new();

    /// <summary>Many-to-many links to Episodes this character has been cast in.</summary>
    public IReadOnlyCollection<EpisodeCharacter> EpisodeCharacters => _episodeCharacters.AsReadOnly();

    // ── EF Core parameterless constructor ───────────────────────────────────
    private Character() { }

    // ──────────────────────────────────────── Factory ───────────────────────

    /// <summary>
    /// Creates a new <see cref="Character"/> in <see cref="TrainingStatus.Draft"/> state
    /// and raises <see cref="CharacterCreatedEvent"/>.
    /// </summary>
    /// <param name="teamId">Owning team.</param>
    /// <param name="name">Display name (trimmed).</param>
    /// <param name="description">Optional prose description.</param>
    /// <param name="styleDna">Optional style guidance for LoRA training.</param>
    /// <param name="creditsCost">Credits to charge for training.</param>
    public static Character Create(
        Guid teamId,
        string name,
        string? description,
        string? styleDna,
        int creditsCost)
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

    // ──────────────────────────────────────── Behaviour ─────────────────────

    /// <summary>
    /// Advances the character to a new training stage, optionally updating asset URLs
    /// and the training progress percentage.
    /// </summary>
    public void AdvanceTraining(
        TrainingStatus newStatus,
        int progressPercent = 0,
        string? imageUrl = null,
        string? loraWeightsUrl = null,
        string? triggerWord = null)
    {
        TrainingStatus = newStatus;
        TrainingProgressPercent = Math.Clamp(progressPercent, 0, 100);

        if (imageUrl is not null) ImageUrl = imageUrl;
        if (loraWeightsUrl is not null) LoraWeightsUrl = loraWeightsUrl;
        if (triggerWord is not null) TriggerWord = triggerWord;

        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CharacterTrainingProgressedEvent(Id, TeamId, newStatus, TrainingProgressPercent));

        if (newStatus == TrainingStatus.Ready)
            AddDomainEvent(new CharacterReadyEvent(Id, TeamId, LoraWeightsUrl!, TriggerWord!));
    }

    /// <summary>Marks the character training as failed.</summary>
    public void FailTraining(string? reason = null)
    {
        TrainingStatus = TrainingStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CharacterTrainingFailedEvent(Id, TeamId, reason));
    }

    /// <summary>
    /// Soft-deletes this character. Callers must first verify no active Episodes
    /// reference it (<see cref="EpisodeCharacters"/>).
    /// </summary>
    public void Delete(Guid deletedByUserId)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByUserId = deletedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
