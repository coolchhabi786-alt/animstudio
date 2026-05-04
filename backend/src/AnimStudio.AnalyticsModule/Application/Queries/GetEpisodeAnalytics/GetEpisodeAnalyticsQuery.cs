using AnimStudio.AnalyticsModule.Application.DTOs;
using AnimStudio.AnalyticsModule.Domain.Enums;
using AnimStudio.AnalyticsModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.AnalyticsModule.Application.Queries.GetEpisodeAnalytics;

public sealed record GetEpisodeAnalyticsQuery(Guid EpisodeId) : IRequest<Result<EpisodeAnalyticsDto>>;

public sealed class GetEpisodeAnalyticsHandler(AnalyticsDbContext db)
    : IRequestHandler<GetEpisodeAnalyticsQuery, Result<EpisodeAnalyticsDto>>
{
    public async Task<Result<EpisodeAnalyticsDto>> Handle(GetEpisodeAnalyticsQuery query, CancellationToken ct)
    {
        var views = await db.VideoViews
            .Where(v => v.EpisodeId == query.EpisodeId)
            .Select(v => v.Source)
            .ToListAsync(ct);

        var dto = new EpisodeAnalyticsDto(
            EpisodeId:        query.EpisodeId,
            TotalViews:       views.Count,
            DirectViews:      views.Count(s => s == VideoViewSource.Direct),
            EmbedViews:       views.Count(s => s == VideoViewSource.Embed),
            ReviewLinkViews:  views.Count(s => s == VideoViewSource.ReviewLink));

        return Result<EpisodeAnalyticsDto>.Success(dto);
    }
}
