using AnimStudio.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.ContentModule;

/// <summary>
/// Registers the Content module services into the application DI container.
/// Phase 2 will fill in episode, character, and pipeline orchestration services.
/// </summary>
public sealed class ContentModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2 stub — episode, character, LoRA, storyboard, and voice services registered here.
    }
}
