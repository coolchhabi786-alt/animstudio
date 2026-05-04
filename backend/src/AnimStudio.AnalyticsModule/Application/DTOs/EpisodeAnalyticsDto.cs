namespace AnimStudio.AnalyticsModule.Application.DTOs;

public sealed record EpisodeAnalyticsDto(
    Guid EpisodeId,
    int  TotalViews,
    int  DirectViews,
    int  EmbedViews,
    int  ReviewLinkViews);
