using AnimStudio.IdentityModule.Application.Commands.RegisterUser;
using AnimStudio.IdentityModule.Application.Interfaces;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.IdentityModule.Infrastructure;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using AnimStudio.IdentityModule.Infrastructure.Repositories;
using AnimStudio.IdentityModule.Infrastructure.Services;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimStudio.IdentityModule;

/// <summary>
/// Wires the Identity Module into the DI container.
/// Implements <see cref="IModuleRegistration"/> so Program.cs can call it generically.
/// </summary>
public sealed class IdentityModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core ──────────────────────────────────────────────────────────
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql
                    .MigrationsAssembly(typeof(IdentityModuleRegistration).Assembly.FullName)
                    .EnableRetryOnFailure(maxRetryCount: 5)));

        // ── MediatR (scoped to this assembly) ─────────────────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IdentityModuleRegistration).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        // ── FluentValidation ──────────────────────────────────────────────
        services.AddValidatorsFromAssembly(
            typeof(RegisterUserCommandValidator).Assembly,
            includeInternalTypes: true);

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // ── Application / Infrastructure services ────────────────────────
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICacheService, CacheService>();

        // ── Domain event collector (used by TransactionBehaviour outbox flush) ─
        services.AddScoped<IDomainEventCollector, IdentityDomainEventCollector>();
    }
}
