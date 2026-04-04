using AnimStudio.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.AnalyticsModule;

/// <summary>
/// Registers the Analytics module services into the application DI container.
/// Phase 2 will fill in usage tracking, event ingestion, and reporting services.
/// </summary>
public sealed class AnalyticsModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2 stub — usage metrics, event ingestion, and dashboards registered here.
    }
}
