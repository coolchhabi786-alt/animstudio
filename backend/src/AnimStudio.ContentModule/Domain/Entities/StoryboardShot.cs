using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Single panel in the storyboard grid. Up to 5 shots per scene.
/// Owned by a <see cref="Storyboard"/> aggregate — mutations go through
/// the aggregate to keep invariants consistent.
/// </summary>
public sealed class StoryboardShot : Entity<Guid>
{
    public Guid StoryboardId { get; private set; }
    public int SceneNumber { get; private set; }
    public int ShotIndex { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? StyleOverride { get; private set; }
    public int RegenerationCount { get; private set; }

    private StoryboardShot() { }

    internal static StoryboardShot Create(
        Guid storyboardId,
        int sceneNumber,
        int shotIndex,
        string description,
        string? styleOverride = null)
    {
        return new StoryboardShot
        {
            Id = Guid.NewGuid(),
            StoryboardId = storyboardId,
            SceneNumber = sceneNumber,
            ShotIndex = shotIndex,
            Description = description,
            StyleOverride = styleOverride,
            RegenerationCount = 0,
            ImageUrl = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    internal void UpdateImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty.", nameof(imageUrl));

        ImageUrl = imageUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void IncrementRegeneration()
    {
        RegenerationCount += 1;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void SetStyleOverride(string? style)
    {
        StyleOverride = string.IsNullOrWhiteSpace(style) ? null : style.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
