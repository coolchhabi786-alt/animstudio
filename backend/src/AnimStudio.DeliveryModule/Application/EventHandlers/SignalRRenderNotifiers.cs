using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.DeliveryModule.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimStudio.DeliveryModule.Application.EventHandlers;

public sealed class RenderProgressEventHandler(
    IRenderProgressNotifier notifier,
    ILogger<RenderProgressEventHandler> logger)
    : INotificationHandler<RenderProgressEvent>
{
    public async Task Handle(RenderProgressEvent notification, CancellationToken ct)
    {
        try
        {
            await notifier.NotifyProgressAsync(
                notification.RenderId,
                notification.EpisodeId,
                notification.Percent,
                notification.Stage,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error broadcasting RenderProgress for render {Id}", notification.RenderId);
        }
    }
}

public sealed class RenderCompleteEventHandler(
    IRenderProgressNotifier notifier,
    ILogger<RenderCompleteEventHandler> logger)
    : INotificationHandler<RenderCompleteEvent>
{
    public async Task Handle(RenderCompleteEvent notification, CancellationToken ct)
    {
        try
        {
            await notifier.NotifyCompleteAsync(
                notification.RenderId,
                notification.EpisodeId,
                notification.CdnUrl,
                notification.SrtUrl,
                notification.DurationSeconds,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error broadcasting RenderComplete for render {Id}", notification.RenderId);
        }
    }
}

public sealed class RenderFailedEventHandler(
    IRenderProgressNotifier notifier,
    ILogger<RenderFailedEventHandler> logger)
    : INotificationHandler<RenderFailedEvent>
{
    public async Task Handle(RenderFailedEvent notification, CancellationToken ct)
    {
        try
        {
            await notifier.NotifyFailedAsync(
                notification.RenderId,
                notification.EpisodeId,
                notification.ErrorMessage,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error broadcasting RenderFailed for render {Id}", notification.RenderId);
        }
    }
}
