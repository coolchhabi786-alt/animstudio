using AnimStudio.DeliveryModule.Domain.Enums;
using AnimStudio.DeliveryModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.DeliveryModule.Domain.Entities;

/// <summary>
/// Tracks the lifecycle and output of a single post-production render job.
/// One render = one output video at a chosen aspect ratio.
/// </summary>
public sealed class Render : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public RenderAspectRatio AspectRatio { get; private set; }
    public RenderStatus Status { get; private set; }

    /// <summary>Relative path or blob path to the final rendered MP4.</summary>
    public string? FinalVideoUrl { get; private set; }

    /// <summary>Signed CDN URL (Azure Blob SAS) — populated once render is complete.</summary>
    public string? CdnUrl { get; private set; }

    /// <summary>URL to the generated SRT captions file.</summary>
    public string? CaptionsSrtUrl { get; private set; }

    public double DurationSeconds { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private Render() { }

    public static Render Create(Guid episodeId, RenderAspectRatio aspectRatio)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));

        var render = new Render
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            AspectRatio = aspectRatio,
            Status = RenderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        render.AddDomainEvent(new RenderStartedEvent(render.Id, episodeId));
        return render;
    }

    public void MarkRendering()
    {
        if (Status is RenderStatus.Complete or RenderStatus.Failed)
            throw new InvalidOperationException($"Render is already terminal ({Status}).");
        Status = RenderStatus.Rendering;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkComplete(string? finalVideoUrl, string? cdnUrl, string? srtUrl, double durationSeconds)
    {
        if (Status is RenderStatus.Complete or RenderStatus.Failed)
            return;
        FinalVideoUrl = finalVideoUrl;
        CdnUrl = cdnUrl;
        CaptionsSrtUrl = srtUrl;
        DurationSeconds = durationSeconds;
        Status = RenderStatus.Complete;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new RenderCompleteEvent(Id, EpisodeId, cdnUrl, srtUrl, durationSeconds));
    }

    public void MarkFailed(string errorMessage)
    {
        if (Status is RenderStatus.Complete or RenderStatus.Failed)
            return;
        ErrorMessage = errorMessage;
        Status = RenderStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new RenderFailedEvent(Id, EpisodeId, errorMessage));
    }
}
