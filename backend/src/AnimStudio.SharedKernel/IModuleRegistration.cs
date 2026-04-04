using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Interface for registering services within a module.
    /// </summary>
    public interface IModuleRegistration
    {
        /// <summary>
        /// Configures services for the module.
        /// </summary>
        /// <param name="services">The service collection to register services into.</param>
        /// <param name="configuration">The application configuration.</param>
        void RegisterServices(IServiceCollection services, IConfiguration configuration);
    }
}