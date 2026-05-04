using AnimStudio.AnalyticsModule.Application.DTOs;
using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.AnalyticsModule.Application.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    int  Page     = 1,
    int  PageSize = 20) : IRequest<Result<List<NotificationDto>>>;

public sealed class GetNotificationsHandler(INotificationRepository notifications)
    : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery query, CancellationToken ct)
    {
        var items = await notifications.GetByUserIdAsync(query.UserId, query.Page, query.PageSize, ct);
        var dtos = items.Select(n => new NotificationDto(
            n.Id, n.Type, n.Title, n.Body, n.IsRead, n.ReadAt,
            n.RelatedEntityId, n.RelatedEntityType, n.CreatedAt)).ToList();
        return Result<List<NotificationDto>>.Success(dtos);
    }
}
