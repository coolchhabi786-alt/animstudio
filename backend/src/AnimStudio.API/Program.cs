using AnimStudio.AnalyticsModule;
using AnimStudio.API;
using Asp.Versioning;
using AnimStudio.API.Authentication;
using AnimStudio.API.Filters;
using AnimStudio.API.Hosted;
using AnimStudio.API.Hubs;
using AnimStudio.API.Middleware;using AnimStudio.API.Services;
using AnimStudio.ContentModule;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.DeliveryModule;
using AnimStudio.IdentityModule;
using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using AnimStudio.SharedKernel;
using AnimStudio.SharedKernel.Behaviours;
using AnimStudio.SharedKernel.Jobs;
using AnimStudio.SharedKernel.Persistence;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Resilience;
using Polly;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;

// ─── Bootstrap logger (before DI is built) ──────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── 1. Serilog ────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "AnimStudio.API")
        .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
        .Destructure.ByTransforming<object>(o => o) // placeholder for custom destructurers
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

    // ── 2. OpenTelemetry (Azure Application Insights) ─────────────────────────
    var appInsightsConnStr = builder.Configuration["ApplicationInsights:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(appInsightsConnStr))
    {
        builder.Services.AddOpenTelemetry().UseAzureMonitor(o =>
        {
            o.ConnectionString = appInsightsConnStr;
        });
    }

    // ── 3. Azure Key Vault ────────────────────────────────────────────────────
    var kvUri = builder.Configuration["Azure:KeyVaultUri"];
    if (!string.IsNullOrWhiteSpace(kvUri))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
        Log.Information("Azure Key Vault connected: {Uri}", kvUri);
    }

    // ── 4. Module registrations ───────────────────────────────────────────────
    IModuleRegistration[] modules = [
        new IdentityModuleRegistration(),
        new ContentModuleRegistration(),
        new DeliveryModuleRegistration(),
        new AnalyticsModuleRegistration(),
    ];
    foreach (var module in modules)
        module.RegisterServices(builder.Services, builder.Configuration);

    // ── 5. SharedKernel DbContext ─────────────────────────────────────────────
    builder.Services.AddDbContext<SharedDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));

    // ── 6. MediatR — global pipeline behaviours (order matters) ──────────────
    //  CorrelationId → Logging → Validation → Caching → Transaction
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(ICacheKey).Assembly,
            typeof(IdentityModuleRegistration).Assembly,
            typeof(ContentModuleRegistration).Assembly,
            typeof(DeliveryModuleRegistration).Assembly,
            typeof(AnalyticsModuleRegistration).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CorrelationIdBehaviour<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehaviour<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
    });

    // ── 7. FluentValidation ───────────────────────────────────────────────────
    // (Validators registered per-module in module registrations via AddValidatorsFromAssembly)

    // ── 8. Hangfire ───────────────────────────────────────────────────────────
    var hangfireConn = builder.Configuration.GetConnectionString("HangfireConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHangfire(cfg => cfg
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(hangfireConn, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
        }));
    builder.Services.AddHangfireServer(opts =>
    {
        opts.WorkerCount = 5;
        opts.Queues = ["critical", "default", "low"]; // 3 queues as per spec
    });
    builder.Services.AddHostedService<RecurringJobsHostedService>();

    // ── 9. Redis ──────────────────────────────────────────────────────────────
    var redisConn = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConn))
        builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
    else
        builder.Services.AddDistributedMemoryCache(); // fallback for local dev

    // ── 10. Azure Service Bus (hosted listeners) ──────────────────────────────
    // CompletionMessageProcessor and DeadLetterProcessor registered below
    var serviceBusConn = builder.Configuration.GetConnectionString("ServiceBus")
        ?? builder.Configuration["Azure:ServiceBusConnectionString"];
    if (!string.IsNullOrWhiteSpace(serviceBusConn))
    {
        builder.Services.AddSingleton(new Azure.Messaging.ServiceBus.ServiceBusClient(serviceBusConn));
        builder.Services.AddHostedService<CompletionMessageProcessor>();
        builder.Services.AddHostedService<DeadLetterProcessor>();
    }

    // ── 11. SignalR ───────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ── 12. Stripe ────────────────────────────────────────────────────────────
    var stripeKey = builder.Configuration["Stripe:SecretKey"];
    if (!string.IsNullOrWhiteSpace(stripeKey))
        Stripe.StripeConfiguration.ApiKey = stripeKey;

    // ── 13. Polly v8 Resilience Pipelines ────────────────────────────────────
    builder.Services.AddResiliencePipeline("service-bus", b => b
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromSeconds(1),
        })
        .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
        }));

    builder.Services.AddResiliencePipeline("http-ai-api", b => b
        .AddTimeout(TimeSpan.FromSeconds(60))
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(2),
        })
        .AddConcurrencyLimiter(10, 20));

    builder.Services.AddResiliencePipeline("http-stripe", b => b
        .AddTimeout(TimeSpan.FromSeconds(15))
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(1),
        })
        .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(60),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(60),
        }));

    builder.Services.AddResiliencePipeline("redis", b => b
        // 2 s is generous for localhost; in prod Redis is co-located so latency is < 1 ms.
        // 500 ms was too tight — StackExchange.Redis refresh can exceed that on first connect.
        .AddTimeout(TimeSpan.FromSeconds(2)));

    // ── 14. Authentication — Dev bypass or real JWT ───────────────────────────
    //  In Development with no Authority set: DevAuthHandler injects synthetic claims.
    //  In Production: real JWT from Entra External ID.
    var isDev = builder.Environment.IsDevelopment();
    var authAuthority = builder.Configuration["Auth:Authority"];
    var useDevAuth = isDev && string.IsNullOrWhiteSpace(authAuthority);

    if (useDevAuth)
    {
        Log.Warning(
            "DEV AUTH is active — all requests auto-authenticated as dev@animstudio.local. " +
            "DO NOT use in production.");
        builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
    }
    else
    {
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", opts =>
            {
                opts.Authority = authAuthority;
                opts.Audience = builder.Configuration["Auth:Audience"];
                opts.RequireHttpsMetadata = builder.Environment.IsProduction();
                opts.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
    }

    // ── 15. Authorization — 4 policies ────────────────────────────────────────
    builder.Services.AddAuthorization(opts =>
    {
        opts.AddPolicy("RequireTeamMember",
            p => p.RequireAuthenticatedUser().RequireClaim("animstudio_team_id"));
        opts.AddPolicy("RequireTeamEditor",
            p => p.RequireAuthenticatedUser()
                  .RequireClaim("animstudio_team_id")
                  .RequireAssertion(ctx =>
                      ctx.User.HasClaim("roles", "AnimStudio.Editor") ||
                      ctx.User.HasClaim("roles", "AnimStudio.Owner")));
        opts.AddPolicy("RequireTeamOwner",
            p => p.RequireAuthenticatedUser()
                  .RequireClaim("animstudio_team_id")
                  .RequireClaim("roles", "AnimStudio.Owner"));
        opts.AddPolicy("RequireAdminRole",
            p => p.RequireAuthenticatedUser().RequireClaim("roles", "AnimStudio.Admin"));
    });

    // ── 16. CORS ──────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000"];
    builder.Services.AddCors(opts => opts.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

    // ── 17. API Versioning ────────────────────────────────────────────────────
    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddMvc(); // registers the {version:apiVersion} route constraint

    // ── 18. Rate Limiting — 5 tiers ───────────────────────────────────────────
    builder.Services.AddRateLimiter(opts =>
    {
        opts.RejectionStatusCode = 429;

        // Anonymous (unauthenticated): 10 req/min per IP
        opts.AddPolicy("anonymous", ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 10,
                }));

        // Authenticated tiers resolved from subscription-tier claim
        opts.AddPolicy("basic", ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.User.FindFirst("animstudio_user_id")?.Value ?? "anon",
                _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 60,
                }));

        opts.AddPolicy("pro", ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.User.FindFirst("animstudio_user_id")?.Value ?? "anon",
                _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 200,
                }));

        opts.AddPolicy("studio", ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.User.FindFirst("animstudio_user_id")?.Value ?? "anon",
                _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 500,
                }));

        // Webhook: no limit for Stripe callbacks
        opts.AddPolicy("webhook", _ => RateLimitPartition.GetNoLimiter("webhook"));

        // Default policy for authenticated requests (tier not resolved from claim)
        opts.AddFixedWindowLimiter("authenticated", o =>
        {
            o.Window = TimeSpan.FromMinutes(1);
            o.PermitLimit = 300;
            o.QueueLimit = 20;
        });
    });

    // ── 19. Swagger / OpenAPI ─────────────────────────────────────────────────
    builder.Services.AddControllers(opts => opts.Filters.Add<ValidationExceptionFilter>());
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opts =>
    {
        opts.SwaggerDoc("v1", new() { Title = "AnimStudio API", Version = "v1" });
        opts.AddSecurityDefinition("Bearer", new()
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT Bearer token",
        });
        opts.AddSecurityRequirement(new()
        {
            {
                new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                []
            }
        });
    });

    // ── 20. Health checks ─────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICorrelationIdProvider, HttpContextCorrelationIdProvider>();
    var sqlConnStr = builder.Configuration.GetConnectionString("DefaultConnection");
    var hc = builder.Services.AddHealthChecks();
    if (!string.IsNullOrWhiteSpace(sqlConnStr))
        hc.AddSqlServer(sqlConnStr, name: "sql", tags: ["ready"]);
    if (!string.IsNullOrWhiteSpace(redisConn))
        hc.AddRedis(redisConn, name: "redis", tags: ["ready"]);

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── Phase 4 — Character Studio ────────────────────────────────────────────
    // Register ICharacterProgressNotifier implementation (SignalR adapter in API layer)
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.ICharacterProgressNotifier,
        AnimStudio.API.Services.SignalRCharacterProgressNotifier>();

    // ── Phase 6 — Storyboard Studio ───────────────────────────────────────────
    // Register IStoryboardShotNotifier implementation (SignalR adapter in API layer)
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IStoryboardShotNotifier,
        AnimStudio.API.Services.SignalRStoryboardShotNotifier>();

    // ── Phase 7 — Voice Studio ─────────────────────────────────────────────────
    // Register voice preview (Azure OpenAI TTS) and voice clone (stub) services
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IVoicePreviewService,
        AnimStudio.API.Services.VoicePreviewService>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IVoiceCloneService,
        AnimStudio.API.Services.VoiceCloneService>();

    // ── Phase 8 — Animation Studio ─────────────────────────────────────────────
    // Cost estimator, clip-URL signer, and SignalR notifier for ClipReady events.
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IAnimationEstimateService,
        AnimStudio.API.Services.AnimationEstimateService>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IClipUrlSigner,
        AnimStudio.API.Services.BlobClipUrlSigner>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IAnimationClipNotifier,
        AnimStudio.API.Services.SignalRAnimationClipNotifier>();

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // Auto-migrate in non-production (dev and preview envs)
    if (!app.Environment.IsProduction())
    {
        await using var scope = app.Services.CreateAsyncScope();
        try
        {
            var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await identityDb.Database.MigrateAsync();
            var sharedDb = scope.ServiceProvider.GetRequiredService<SharedDbContext>();
            await sharedDb.Database.MigrateAsync();
            var contentDb = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
            await contentDb.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");

            if (app.Environment.IsDevelopment())
                await SeedDevDataAsync(identityDb);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "DB migration skipped — database may not be reachable yet");
        }
    }

    // ── Middleware pipeline (order is critical) ────────────────────────────────
    app.UseForwardedHeaders();           // trust X-Forwarded-For from Front Door
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AnimStudio API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors();
    // HSTS and HTTPS redirect are production-only: in Development the Kestrel dev-cert
    // handles TLS and forcing redirects breaks Swagger (which calls the HTTP port).
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<SubscriptionGateMiddleware>();

    if (app.Environment.IsDevelopment())
        app.UseHangfireDashboard("/hangfire");

    app.MapControllers().RequireRateLimiting("authenticated");
    app.MapHub<ProgressHub>("/hubs/progress").RequireAuthorization();
    app.MapHub<CharacterProgressHub>("/hubs/character-training").RequireAuthorization();
    app.MapHealthChecks("/health/live",  new() { Predicate = _ => false }).AllowAnonymous();
    app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") }).AllowAnonymous();
    app.MapHealthChecks("/health").AllowAnonymous();

    Log.Information("AnimStudio API started. DevAuth={DevAuth}", useDevAuth);
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "AnimStudio API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Seeds the minimum dev data needed to match the fixed identities in <see cref="DevAuthHandler"/>.
/// Runs only in Development, after EF migrations. Idempotent — skips rows that already exist.
/// </summary>
static async Task SeedDevDataAsync(IdentityDbContext db)
{
    // These GUIDs are the fixed dev identities — keep in sync with DevAuthHandler
    var devUserId     = Guid.Parse("00000000-0000-0000-0000-000000000001");
    var devTeamId     = Guid.Parse("00000000-0000-0000-0000-000000000002");
    var starterPlanId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    if (!await db.Users.AnyAsync(u => u.Id == devUserId))
    {
        var user = User.Create(devUserId, "dev-001", DevAuthHandler.DevUserEmail, "Dev User");
        db.Users.Add(user);
    }

    if (!await db.Teams.AnyAsync(t => t.Id == devTeamId))
    {
        // Team.Create also adds the owner as a TeamMember — EF tracks it automatically
        var team = Team.Create(devTeamId, "Dev Team", devUserId);
        db.Teams.Add(team);
    }

    if (!await db.Subscriptions.AnyAsync(s => s.TeamId == devTeamId))
    {
        db.Subscriptions.Add(Subscription.Create(
            id: Guid.NewGuid(),
            teamId: devTeamId,
            planId: starterPlanId));
    }
    else
    {
        // If an existing subscription is in a non-active state (e.g., Cancelled from a previous
        // test run), reset it to Active so all API endpoints pass the subscription gate.
        var existingSub = await db.Subscriptions.FirstAsync(s => s.TeamId == devTeamId);
        if (existingSub.Status != SubscriptionStatus.Active &&
            existingSub.Status != SubscriptionStatus.Trialing)
        {
            existingSub.Status = SubscriptionStatus.Active;
            existingSub.CurrentPeriodEnd = DateTimeOffset.UtcNow.AddMonths(1);
            existingSub.CancelAtPeriodEnd = false;
        }
    }

    await db.SaveChangesAsync();
    Log.Information("Dev seed data applied (User / Team / Subscription)");
}
