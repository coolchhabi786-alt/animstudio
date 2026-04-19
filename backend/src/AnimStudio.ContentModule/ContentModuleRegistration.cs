using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.ContentModule.Infrastructure.Repositories;
using AnimStudio.SharedKernel;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.ContentModule;

/// <summary>
/// Registers all ContentModule services into the application DI container.
/// </summary>
public sealed class ContentModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── DbContext ──────────────────────────────────────────────────────────
        services.AddDbContext<ContentDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));

        // ── Repositories ───────────────────────────────────────────────────────
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<ISagaStateRepository, SagaStateRepository>();

        // Phase 3 — Template & Style Library
        services.AddScoped<IEpisodeTemplateRepository, EpisodeTemplateRepository>();
        services.AddScoped<IStylePresetRepository, StylePresetRepository>();

        // Phase 4 — Character Studio
        services.AddScoped<ICharacterRepository, CharacterRepository>();

        // Phase 5 — Script Workshop
        services.AddScoped<IScriptRepository, ScriptRepository>();

        // Phase 6 — Storyboard Studio
        services.AddScoped<IStoryboardRepository, StoryboardRepository>();

        // Phase 7 — Voice Studio
        services.AddScoped<IVoiceAssignmentRepository, VoiceAssignmentRepository>();

        // Phase 8 — Animation Studio
        services.AddScoped<IAnimationJobRepository, AnimationJobRepository>();
        services.AddScoped<IAnimationClipRepository, AnimationClipRepository>();

        // ── FluentValidation — scan this module's validators ───────────────────
        services.AddValidatorsFromAssembly(typeof(ContentModuleRegistration).Assembly, includeInternalTypes: true);
    }
}
