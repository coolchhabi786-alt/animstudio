using AnimStudio.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.AnalyticsModule;

public sealed class AnalyticsModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // MediatR handlers (commands, queries, event handlers) in this assembly are
        // registered automatically by Program.cs via AddMediatR(...FromAssemblies...).
        // Repositories and DbContext are registered in Program.cs (Phase 12 section)
        // since they require the connection string from configuration.
    }
}
