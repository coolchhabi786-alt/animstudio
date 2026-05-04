namespace AnimStudio.AnalyticsModule.Application.DTOs;

public sealed record AdminStatsDto(
    int TotalUsers,
    int TotalTeams,
    int TotalEpisodes,
    int ActiveJobs,
    int TotalVideoViews);

public sealed record AdminUserDto(
    Guid   UserId,
    string DisplayName,
    string Email,
    Guid   TeamId,
    string SubscriptionStatus,
    int    UsageEpisodesThisMonth);
