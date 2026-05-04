using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Tracks a social media publish attempt for a completed render.
/// </summary>
public sealed class SocialPublish : AggregateRoot<Guid>
{
    public Guid EpisodeId { get; private set; }
    public Guid RenderId { get; private set; }
    public SocialPlatform Platform { get; private set; }
    public string? ExternalVideoId { get; private set; }
    public SocialPublishStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private SocialPublish() { }

    public static SocialPublish Create(Guid episodeId, Guid renderId, SocialPlatform platform)
        => new SocialPublish
        {
            Id        = Guid.NewGuid(),
            EpisodeId = episodeId,
            RenderId  = renderId,
            Platform  = platform,
            Status    = SocialPublishStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
}
