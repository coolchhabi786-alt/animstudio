namespace AnimStudio.AnalyticsModule.Application.DTOs;

public sealed record TeamAnalyticsDto(
    Guid TeamId,
    int  TotalEpisodes,
    int  CompletedEpisodes,
    int  TotalVideoViews,
    int  UsageEpisodesThisMonth,
    int  MonthlyQuota);
