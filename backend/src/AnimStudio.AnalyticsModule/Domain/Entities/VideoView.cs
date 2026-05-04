using AnimStudio.AnalyticsModule.Domain.Enums;

namespace AnimStudio.AnalyticsModule.Domain.Entities;

public sealed class VideoView
{
    public Guid             Id             { get; private set; }
    public Guid             EpisodeId      { get; private set; }
    public Guid             RenderId       { get; private set; }
    public string?          ViewerIpHash   { get; private set; }
    public VideoViewSource  Source         { get; private set; }
    public Guid?            ReviewLinkId   { get; private set; }
    public DateTimeOffset   ViewedAt       { get; private set; }

    private VideoView() { }

    public static VideoView Create(
        Guid episodeId,
        Guid renderId,
        VideoViewSource source,
        string? viewerIpHash = null,
        Guid? reviewLinkId = null)
    {
        return new VideoView
        {
            Id           = Guid.NewGuid(),
            EpisodeId    = episodeId,
            RenderId     = renderId,
            Source       = source,
            ViewerIpHash = viewerIpHash,
            ReviewLinkId = reviewLinkId,
            ViewedAt     = DateTimeOffset.UtcNow,
        };
    }
}
