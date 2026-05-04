using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.AnalyticsModule.Application.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(
    Guid NotificationId,
    Guid UserId) : IRequest<Result>;

public sealed class MarkNotificationReadHandler(INotificationRepository notifications)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async Task<Result> Handle(MarkNotificationReadCommand cmd, CancellationToken ct)
    {
        var notification = await notifications.GetByIdAsync(cmd.NotificationId, ct);
        if (notification is null)
            return Result.Failure("Notification not found", "NOT_FOUND");
        if (notification.UserId != cmd.UserId)
            return Result.Failure("Forbidden", "FORBIDDEN");

        notification.MarkRead();
        await notifications.UpdateAsync(notification, ct);
        return Result.Success();
    }
}
