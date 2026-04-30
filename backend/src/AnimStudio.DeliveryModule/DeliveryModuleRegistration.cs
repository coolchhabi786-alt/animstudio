using AnimStudio.DeliveryModule.Application.EventHandlers;
using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.DeliveryModule.Infrastructure.Persistence;
using AnimStudio.DeliveryModule.Infrastructure.Repositories;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.DeliveryModule;

/// <summary>
/// Registers all Delivery module services into the application DI container.
/// Phase 9: Render entity, repository, and SignalR event handlers.
/// IRenderProgressNotifier implementation is registered by the API layer.
/// </summary>
public sealed class DeliveryModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── DbContext ──────────────────────────────────────────────────────────
        services.AddDbContext<DeliveryDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));

        // ── Repository ─────────────────────────────────────────────────────────
        services.AddScoped<IRenderRepository, RenderRepository>();

        // ── Domain event handlers ──────────────────────────────────────────────
        services.AddScoped<INotificationHandler<AnimStudio.DeliveryModule.Domain.Events.RenderProgressEvent>, RenderProgressEventHandler>();
        services.AddScoped<INotificationHandler<AnimStudio.DeliveryModule.Domain.Events.RenderCompleteEvent>, RenderCompleteEventHandler>();
        services.AddScoped<INotificationHandler<AnimStudio.DeliveryModule.Domain.Events.RenderFailedEvent>, RenderFailedEventHandler>();

        // ── FluentValidation ───────────────────────────────────────────────────
        services.AddValidatorsFromAssembly(typeof(DeliveryModuleRegistration).Assembly, includeInternalTypes: true);
    }
}
