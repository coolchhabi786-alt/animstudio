using AnimStudio.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.DeliveryModule;

/// <summary>
/// Registers the Delivery module services into the application DI container.
/// Phase 2 will fill in CDN, Azure Blob Storage, and video delivery services.
/// </summary>
public sealed class DeliveryModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2 stub — Azure Blob Storage and CDN delivery services registered here.
    }
}
