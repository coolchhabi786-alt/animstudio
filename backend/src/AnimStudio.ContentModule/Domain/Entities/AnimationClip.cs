using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// One rendered (or in-flight) video clip, keyed off the storyboard grid
/// position (scene + shot). FK to the source <see cref="StoryboardShot"/> is
/// nullable with ON DELETE SET NULL — deleting a storyboard shot must not
/// destroy already-rendered footage.
/// </summary>
public sealed class AnimationClip : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public int SceneNumber { get; private set; }
    public int ShotIndex { get; private set; }
    public Guid? StoryboardShotId { get; private set; }
    public string? ClipUrl { get; private set; }
    public double? DurationSeconds { get; private set; }
    public ClipStatus Status { get; private set; }

    private AnimationClip() { }

    public static AnimationClip CreatePending(
        Guid episodeId,
        int sceneNumber,
        int shotIndex,
        Guid? storyboardShotId)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));
        if (sceneNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(sceneNumber), "Scene number is 1-based.");
        if (shotIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(shotIndex), "Shot index is 0-based.");

        return new AnimationClip
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            SceneNumber = sceneNumber,
            ShotIndex = shotIndex,
            StoryboardShotId = storyboardShotId,
            Status = ClipStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void MarkRendering()
    {
        if (Status is ClipStatus.Ready or ClipStatus.Failed)
            throw new InvalidOperationException($"Clip already terminal ({Status}).");
        Status = ClipStatus.Rendering;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkReady(string clipUrl, double durationSeconds)
    {
        if (string.IsNullOrWhiteSpace(clipUrl))
            throw new ArgumentException("Clip URL is required.", nameof(clipUrl));
        if (durationSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be positive.");

        ClipUrl = clipUrl;
        DurationSeconds = durationSeconds;
        Status = ClipStatus.Ready;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AnimationClipReadyEvent(Id, EpisodeId, SceneNumber, ShotIndex, clipUrl));
    }

    public void MarkFailed()
    {
        Status = ClipStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
