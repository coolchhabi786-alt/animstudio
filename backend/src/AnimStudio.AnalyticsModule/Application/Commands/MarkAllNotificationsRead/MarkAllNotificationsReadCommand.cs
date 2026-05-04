using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.AnalyticsModule.Application.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(Guid UserId) : IRequest<Result>;

public sealed class MarkAllNotificationsReadHandler(INotificationRepository notifications)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand cmd, CancellationToken ct)
    {
        await notifications.MarkAllReadAsync(cmd.UserId, ct);
        return Result.Success();
    }
}
