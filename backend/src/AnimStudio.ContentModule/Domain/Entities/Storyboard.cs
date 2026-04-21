using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Storyboard aggregate — one per episode. Owns a collection of
/// <see cref="StoryboardShot"/> children (up to 5 per scene).
/// Stores the raw JSON of the Python engine's StoryboardPlan model.
/// </summary>
public sealed class Storyboard : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public string ScreenplayTitle { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialised StoryboardPlan model produced by the Python StoryboardCrew.
    /// Schema: { screenplayTitle, scenePlans: [{ sceneNumber, shotPrompts: [...] }] }
    /// </summary>
    public string RawJson { get; private set; } = "{}";

    public string? DirectorNotes { get; private set; }

    // Backing field keeps the collection encapsulated.
    private readonly List<StoryboardShot> _shots = new();
    public IReadOnlyCollection<StoryboardShot> Shots => _shots.AsReadOnly();

    private Storyboard() { }

    public static Storyboard Create(Guid episodeId, string screenplayTitle, string rawJson)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));
        if (string.IsNullOrWhiteSpace(screenplayTitle))
            throw new ArgumentException("Screenplay title is required.", nameof(screenplayTitle));

        var storyboard = new Storyboard
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            ScreenplayTitle = screenplayTitle,
            RawJson = string.IsNullOrWhiteSpace(rawJson) ? "{}" : rawJson,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        storyboard.AddDomainEvent(new StoryboardCreatedEvent(storyboard.Id, episodeId));
        return storyboard;
    }

    /// <summary>Called when the StoryboardPlan job returns — replaces raw JSON and shot list.</summary>
    public void UpdateFromJob(string rawJson, string screenplayTitle)
    {
        RawJson = string.IsNullOrWhiteSpace(rawJson) ? "{}" : rawJson;
        if (!string.IsNullOrWhiteSpace(screenplayTitle))
            ScreenplayTitle = screenplayTitle;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StoryboardUpdatedEvent(Id, EpisodeId));
    }

    public void SetDirectorNotes(string? notes)
    {
        DirectorNotes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Replaces the entire shot set. Used when the StoryboardPlan job returns
    /// a new plan — previous shots are discarded and new ones seeded.
    /// </summary>
    public void SeedShots(IEnumerable<(int SceneNumber, int ShotIndex, string Description)> plannedShots)
    {
        _shots.Clear();
        foreach (var p in plannedShots.OrderBy(p => p.SceneNumber).ThenBy(p => p.ShotIndex))
            _shots.Add(StoryboardShot.Create(Id, p.SceneNumber, p.ShotIndex, p.Description));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Records that a shot has been queued for regeneration.</summary>
    public void IncrementShotRegeneration(Guid shotId)
    {
        var shot = FindShotOrThrow(shotId);
        shot.IncrementRegeneration();
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StoryboardShotRegeneratedEvent(Id, shot.Id, EpisodeId, shot.RegenerationCount));
    }

    /// <summary>Updates the style override for a shot and records the change.</summary>
    public void SetShotStyleOverride(Guid shotId, string? styleOverride)
    {
        var shot = FindShotOrThrow(shotId);
        shot.SetStyleOverride(styleOverride);
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StoryboardShotStyleOverriddenEvent(Id, shot.Id, EpisodeId, shot.StyleOverride));
    }

    /// <summary>Records the CDN URL produced by a completed StoryboardGen job.</summary>
    public void SetShotImage(Guid shotId, string imageUrl)
    {
        var shot = FindShotOrThrow(shotId);
        shot.UpdateImage(imageUrl);
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StoryboardShotImageUpdatedEvent(Id, shot.Id, EpisodeId, imageUrl, shot.RegenerationCount));
    }

    private StoryboardShot FindShotOrThrow(Guid shotId)
    {
        var shot = _shots.FirstOrDefault(s => s.Id == shotId)
            ?? throw new InvalidOperationException(
                $"Shot {shotId} is not part of storyboard {Id}.");
        return shot;
    }
}
