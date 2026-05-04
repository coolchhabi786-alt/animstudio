using AnimStudio.AnalyticsModule;
using AnimStudio.API;
using Asp.Versioning;
using AnimStudio.API.Authentication;
using AnimStudio.API.Filters;
using AnimStudio.API.Hosted;
using AnimStudio.API.Hubs;
using AnimStudio.API.Middleware;
using AnimStudio.API.Services;
using AnimStudio.ContentModule;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.DeliveryModule;
using AnimStudio.DeliveryModule.Infrastructure.Persistence;
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
    builder.Services.AddControllers(opts => opts.Filters.Add<ValidationExceptionFilter>())
        .AddJsonOptions(opts =>
            opts.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()));
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
    // Cost estimator, clip-URL signer (legacy — kept for BlobClipUrlSigner consumers),
    // SignalR notifier, and the Hangfire processor that drives local animation.
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IAnimationEstimateService,
        AnimStudio.API.Services.AnimationEstimateService>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IClipUrlSigner,
        AnimStudio.API.Services.BlobClipUrlSigner>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IAnimationClipNotifier,
        AnimStudio.API.Services.SignalRAnimationClipNotifier>();
    // AnimationJobHangfireProcessor must be resolvable from DI for Hangfire to
    // instantiate it when the enqueued background job fires.
    builder.Services.AddScoped<AnimStudio.API.Hosted.AnimationJobHangfireProcessor>();

    // IServiceBusPublisher: Azure (Managed Identity) when namespace configured; NoOp in local dev.
    var sbNamespace = builder.Configuration["ServiceBus:Namespace"];
    if (!string.IsNullOrWhiteSpace(sbNamespace))
    {
        builder.Services.AddSingleton<AnimStudio.SharedKernel.Interfaces.IServiceBusPublisher,
            AnimStudio.API.Services.AzureServiceBusPublisher>();
    }
    else
    {
        builder.Services.AddSingleton<AnimStudio.SharedKernel.Interfaces.IServiceBusPublisher,
            AnimStudio.API.Services.NoOpServiceBusPublisher>();
    }

    // ── Phase 9 — Render & Delivery ────────────────────────────────────────────
    builder.Services.AddScoped<AnimStudio.DeliveryModule.Application.Interfaces.IRenderProgressNotifier,
        AnimStudio.API.Services.SignalRRenderProgressNotifier>();
    builder.Services.AddScoped<AnimStudio.API.Hosted.RenderHangfireProcessor>();

    // ── Phase 11 — Sharing & Review ────────────────────────────────────────────
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IReviewLinkRepository,
        AnimStudio.ContentModule.Infrastructure.Repositories.ReviewLinkRepository>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IReviewCommentRepository,
        AnimStudio.ContentModule.Infrastructure.Repositories.ReviewCommentRepository>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IBrandKitRepository,
        AnimStudio.ContentModule.Infrastructure.Repositories.BrandKitRepository>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IRenderInfoProvider,
        AnimStudio.API.Services.RenderInfoProvider>();

    // ── Phase 12 — Analytics & Admin ──────────────────────────────────────────
    builder.Services.AddDbContext<AnimStudio.AnalyticsModule.Infrastructure.Persistence.AnalyticsDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));
    builder.Services.AddScoped<AnimStudio.AnalyticsModule.Application.Interfaces.IVideoViewRepository,
        AnimStudio.AnalyticsModule.Infrastructure.Repositories.VideoViewRepository>();
    builder.Services.AddScoped<AnimStudio.AnalyticsModule.Application.Interfaces.INotificationRepository,
        AnimStudio.AnalyticsModule.Infrastructure.Repositories.NotificationRepository>();
    builder.Services.AddScoped<AnimStudio.ContentModule.Application.Interfaces.IUsageMeteringService,
        AnimStudio.API.Services.UsageMeteringService>();
    builder.Services.AddScoped<AnimStudio.API.Hosted.TrackVideoViewHangfireProcessor>();

    // ── File Storage — Local (dev) or Azure Blob (prod) ────────────────────────
    // Toggle with FileStorage:Provider in appsettings. LocalFileStorageService serves
    // files via FileStorageController; AzureBlobFileStorageService returns SAS URLs.
    var fileStorageProvider = builder.Configuration["FileStorage:Provider"] ?? "AzureBlob";
    if (string.Equals(fileStorageProvider, "Local", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddSingleton<AnimStudio.ContentModule.Application.Interfaces.IFileStorageService,
            AnimStudio.API.Services.LocalFileStorageService>();
    }
    else
    {
        builder.Services.AddSingleton<AnimStudio.ContentModule.Application.Interfaces.IFileStorageService,
            AnimStudio.API.Services.AzureBlobFileStorageService>();
    }

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
            var analyticsDb = scope.ServiceProvider
                .GetRequiredService<AnimStudio.AnalyticsModule.Infrastructure.Persistence.AnalyticsDbContext>();
            await analyticsDb.Database.MigrateAsync();

            // Phase 9 — Delivery module: raw DDL because EnsureCreatedAsync is a no-op when
            // the AnimStudio DB already exists (Identity/Content migrations created it first).
            var deliveryDb = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
            await deliveryDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'delivery')
                    EXEC('CREATE SCHEMA [delivery]');
                """);
            await deliveryDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'Renders' AND s.name = 'delivery')
                BEGIN
                    CREATE TABLE [delivery].[Renders] (
                        [Id]              UNIQUEIDENTIFIER NOT NULL,
                        [EpisodeId]       UNIQUEIDENTIFIER NOT NULL,
                        [AspectRatio]     NVARCHAR(20)     NOT NULL,
                        [Status]          NVARCHAR(20)     NOT NULL,
                        [FinalVideoUrl]   NVARCHAR(2048)   NULL,
                        [CdnUrl]          NVARCHAR(2048)   NULL,
                        [CaptionsSrtUrl]  NVARCHAR(2048)   NULL,
                        [DurationSeconds] FLOAT            NOT NULL CONSTRAINT [DF_Renders_DurationSeconds] DEFAULT 0,
                        [ErrorMessage]    NVARCHAR(1000)   NULL,
                        [CompletedAt]     DATETIMEOFFSET   NULL,
                        [CreatedAt]       DATETIMEOFFSET   NOT NULL,
                        [UpdatedAt]       DATETIMEOFFSET   NOT NULL,
                        [IsDeleted]       BIT              NOT NULL CONSTRAINT [DF_Renders_IsDeleted] DEFAULT 0,
                        [DeletedAt]       DATETIMEOFFSET   NULL,
                        [DeletedByUserId] UNIQUEIDENTIFIER NULL,
                        [RowVersion]      ROWVERSION       NOT NULL,
                        CONSTRAINT [PK_Renders] PRIMARY KEY ([Id])
                    );
                    CREATE INDEX [IX_Renders_EpisodeId] ON [delivery].[Renders] ([EpisodeId]);
                    CREATE INDEX [IX_Renders_Status]    ON [delivery].[Renders] ([Status]);
                END
                """);
            // Phase 10 — Timeline schema + tables (raw DDL, idempotent)
            await contentDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'timeline')
                    EXEC('CREATE SCHEMA [timeline]');
                """);
            await contentDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'EpisodeTimelines' AND s.name = 'timeline')
                BEGIN
                    CREATE TABLE [timeline].[EpisodeTimelines] (
                        [Id]              UNIQUEIDENTIFIER NOT NULL,
                        [EpisodeId]       UNIQUEIDENTIFIER NOT NULL,
                        [DurationMs]      INT NOT NULL,
                        [Fps]             INT NOT NULL,
                        [CreatedAt]       DATETIMEOFFSET NOT NULL,
                        [UpdatedAt]       DATETIMEOFFSET NOT NULL,
                        [IsDeleted]       BIT NOT NULL CONSTRAINT [DF_EpisodeTimelines_IsDeleted] DEFAULT 0,
                        [DeletedAt]       DATETIMEOFFSET NULL,
                        [DeletedByUserId] UNIQUEIDENTIFIER NULL,
                        [RowVersion]      ROWVERSION NOT NULL,
                        CONSTRAINT [PK_EpisodeTimelines] PRIMARY KEY ([Id])
                    );
                    CREATE UNIQUE INDEX [IX_EpisodeTimelines_EpisodeId]
                        ON [timeline].[EpisodeTimelines] ([EpisodeId]);
                END
                """);
            await contentDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'TimelineTracks' AND s.name = 'timeline')
                BEGIN
                    CREATE TABLE [timeline].[TimelineTracks] (
                        [Id]              UNIQUEIDENTIFIER NOT NULL,
                        [TimelineId]      UNIQUEIDENTIFIER NOT NULL,
                        [TrackType]       NVARCHAR(20) NOT NULL,
                        [Label]           NVARCHAR(100) NOT NULL,
                        [SortOrder]       INT NOT NULL,
                        [IsMuted]         BIT NOT NULL,
                        [IsLocked]        BIT NOT NULL,
                        [VolumePercent]   INT NULL,
                        [AutoDuck]        BIT NULL,
                        [CreatedAt]       DATETIMEOFFSET NOT NULL,
                        [UpdatedAt]       DATETIMEOFFSET NOT NULL,
                        [IsDeleted]       BIT NOT NULL CONSTRAINT [DF_TimelineTracks_IsDeleted] DEFAULT 0,
                        [DeletedAt]       DATETIMEOFFSET NULL,
                        [DeletedByUserId] UNIQUEIDENTIFIER NULL,
                        [RowVersion]      ROWVERSION NOT NULL,
                        CONSTRAINT [PK_TimelineTracks] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_TimelineTracks_EpisodeTimelines]
                            FOREIGN KEY ([TimelineId]) REFERENCES [timeline].[EpisodeTimelines] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_TimelineTracks_TimelineId] ON [timeline].[TimelineTracks] ([TimelineId]);
                END
                """);
            await contentDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'TimelineClips' AND s.name = 'timeline')
                BEGIN
                    CREATE TABLE [timeline].[TimelineClips] (
                        [Id]              UNIQUEIDENTIFIER NOT NULL,
                        [TrackId]         UNIQUEIDENTIFIER NOT NULL,
                        [ClipType]        NVARCHAR(20) NOT NULL,
                        [StartMs]         BIGINT NOT NULL,
                        [DurationMs]      BIGINT NOT NULL,
                        [SortOrder]       INT NOT NULL,
                        [SceneNumber]     INT NULL,
                        [ShotIndex]       INT NULL,
                        [ClipUrl]         NVARCHAR(2048) NULL,
                        [ThumbnailUrl]    NVARCHAR(2048) NULL,
                        [TransitionIn]    NVARCHAR(50) NULL,
                        [Label]           NVARCHAR(200) NULL,
                        [AudioUrl]        NVARCHAR(2048) NULL,
                        [VolumePercent]   INT NULL,
                        [FadeInMs]        INT NULL,
                        [FadeOutMs]       INT NULL,
                        [Text]            NVARCHAR(MAX) NULL,
                        [FontSize]        INT NULL,
                        [Color]           NVARCHAR(50) NULL,
                        [Position]        NVARCHAR(50) NULL,
                        [Animation]       NVARCHAR(50) NULL,
                        [CreatedAt]       DATETIMEOFFSET NOT NULL,
                        [UpdatedAt]       DATETIMEOFFSET NOT NULL,
                        [IsDeleted]       BIT NOT NULL CONSTRAINT [DF_TimelineClips_IsDeleted] DEFAULT 0,
                        [DeletedAt]       DATETIMEOFFSET NULL,
                        [DeletedByUserId] UNIQUEIDENTIFIER NULL,
                        [RowVersion]      ROWVERSION NOT NULL,
                        CONSTRAINT [PK_TimelineClips] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_TimelineClips_TimelineTracks]
                            FOREIGN KEY ([TrackId]) REFERENCES [timeline].[TimelineTracks] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_TimelineClips_TrackId] ON [timeline].[TimelineClips] ([TrackId]);
                END
                """);
            await contentDb.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'TimelineTextOverlays' AND s.name = 'timeline')
                BEGIN
                    CREATE TABLE [timeline].[TimelineTextOverlays] (
                        [Id]              UNIQUEIDENTIFIER NOT NULL,
                        [TimelineId]      UNIQUEIDENTIFIER NOT NULL,
                        [Text]            NVARCHAR(MAX) NOT NULL,
                        [FontSizePixels]  INT NOT NULL,
                        [Color]           NVARCHAR(50) NOT NULL,
                        [PositionX]       INT NOT NULL,
                        [PositionY]       INT NOT NULL,
                        [StartMs]         BIGINT NOT NULL,
                        [DurationMs]      BIGINT NOT NULL,
                        [Animation]       NVARCHAR(50) NULL,
                        [ZIndex]          INT NOT NULL,
                        [CreatedAt]       DATETIMEOFFSET NOT NULL,
                        [UpdatedAt]       DATETIMEOFFSET NOT NULL,
                        [IsDeleted]       BIT NOT NULL CONSTRAINT [DF_TimelineTextOverlays_IsDeleted] DEFAULT 0,
                        [DeletedAt]       DATETIMEOFFSET NULL,
                        [DeletedByUserId] UNIQUEIDENTIFIER NULL,
                        [RowVersion]      ROWVERSION NOT NULL,
                        CONSTRAINT [PK_TimelineTextOverlays] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_TimelineTextOverlays_EpisodeTimelines]
                            FOREIGN KEY ([TimelineId]) REFERENCES [timeline].[EpisodeTimelines] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_TimelineTextOverlays_TimelineId] ON [timeline].[TimelineTextOverlays] ([TimelineId]);
                END
                """);

            Log.Information("Database migrations applied and delivery/timeline schemas provisioned");

            if (app.Environment.IsDevelopment())
            {
                await SeedDevDataAsync(identityDb);
                await SeedDevContentAsync(contentDb);
                await SeedDevDeliveryAsync(deliveryDb);
                var mediaRoot = app.Configuration["FileStorage:LocalRootPath"] ?? @"C:\Users\Vaibhav\cartoon_automation\output";
                await SeedDevTimelineAsync(contentDb, mediaRoot);
            }
        }
        catch (Exception ex)
        {
            // Log at Error so startup failures are visible in the console even in dev.
            Log.Error(ex, "DB startup failed — migrations/seed/DDL error: {Message}", ex.Message);
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

/// <summary>
/// Seeds a dev Project, Episode, and Storyboard with real test images so the
/// storyboard studio page can be tested against the local file storage service.
/// All IDs are fixed to match the FE mock project IDs — idempotent.
/// </summary>
static async Task SeedDevContentAsync(AnimStudio.ContentModule.Infrastructure.Persistence.ContentDbContext db)
{
    // Fixed dev IDs — keep in sync with frontend/src/lib/mock-data/mock-projects.ts
    var devTeamId      = Guid.Parse("00000000-0000-0000-0000-000000000002");
    var devProjectId   = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var devEpisodeId   = Guid.Parse("33333333-3333-3333-3333-333333333333");
    var devStoryboardId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    var now = DateTimeOffset.UtcNow;

    // ── Project ────────────────────────────────────────────────────────────────
    var projectExists = await db.Database.ExecuteSqlRawAsync(
        "SELECT COUNT(1) FROM [content].[Projects] WHERE Id = {0}", devProjectId) == 0;
    _ = projectExists; // suppress unused warning; we use IF NOT EXISTS in the SQL

    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [content].[Projects] WHERE Id = {0})
        INSERT INTO [content].[Projects] (Id, TeamId, Name, Description, ThumbnailUrl, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, {2}, {3}, NULL, {4}, {4}, 0)
        """,
        devProjectId, devTeamId,
        "Neon City Chronicles",
        "A cyberpunk animated series — dev seed project.",
        now);

    // ── Episode ────────────────────────────────────────────────────────────────
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [content].[Episodes] WHERE Id = {0})
        INSERT INTO [content].[Episodes] (Id, ProjectId, Name, Idea, Style, Status, CharacterIds, DirectorNotes, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {8}, 0)
        """,
        devEpisodeId, devProjectId,
        "Episode 1: The Signal",
        "A rogue AI broadcast is detected from an abandoned skyscraper.",
        "Cyberpunk",
        "Animation",
        "[]",
        "Emphasise neon reflections on rain-slicked streets. High contrast cinematography.",
        now);

    // Correct any pre-existing row that has a stale/invalid Status value so EF
    // Core's enum conversion doesn't throw when reading the episode back.
    await db.Database.ExecuteSqlRawAsync("""
        UPDATE [content].[Episodes]
        SET Status = 'Animation', UpdatedAt = {1}
        WHERE Id = {0} AND Status NOT IN ('Idle','CharacterDesign','LoraTraining','Script','Storyboard','Voice','Animation','PostProduction','Done','Failed')
        """,
        devEpisodeId, now);

    // ── Storyboard ─────────────────────────────────────────────────────────────
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [content].[Storyboards] WHERE Id = {0})
        INSERT INTO [content].[Storyboards] (Id, EpisodeId, ScreenplayTitle, RawJson, DirectorNotes, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, {2}, {3}, NULL, {4}, {4}, 0)
        """,
        devStoryboardId, devEpisodeId,
        "The Superpowered Shenanigans of Mr. Whiskers",
        "{}",
        now);

    // ── Storyboard Shots (12 shots with real test images) ─────────────────────
    // Image paths are relative to FileStorage:LocalRootPath (cartoon_automation/output).
    // GetFileUrl() converts them to: http://localhost:5001/api/v1/files/{path}
    var shots = new[]
    {
        (Id: Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001"), Scene: 1, Shot: 1,
         Img: "storyboard/29MarAnimationImages/scene_01_shot_01_6233dc.png",
         Desc: "Wide establishing shot of Mr. Whiskers lounging on a sun-soaked windowsill as Dave the Owner walks in carrying groceries.",
         Style: (string?)null, RegenCount: 1),

        (Id: Guid.Parse("aaaaaaaa-0001-0002-0001-000000000002"), Scene: 1, Shot: 2,
         Img: "storyboard/29MarAnimationImages/scene_01_shot_01_fa1fd5.png",
         Desc: "Alternate take: tighter framing on Mr. Whiskers giving Dave a suspicious side-eye.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0001-0003-0001-000000000003"), Scene: 1, Shot: 3,
         Img: "storyboard/29MarAnimationImages/scene_01_shot_02_3b5d67.png",
         Desc: "Medium shot of Dave setting down bags, noticing Mr. Whiskers has knocked over a plant — again.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0001-0004-0001-000000000004"), Scene: 1, Shot: 4,
         Img: "storyboard/29MarAnimationImages/scene_01_shot_02_5422b4.png",
         Desc: "Close-up on shattered pot and guilty paw — Mr. Whiskers pretends to be asleep.",
         Style: "comic-panel", RegenCount: 1),

        (Id: Guid.Parse("aaaaaaaa-0002-0001-0002-000000000005"), Scene: 2, Shot: 1,
         Img: "storyboard/29MarAnimationImages/scene_02_shot_01_13117c.png",
         Desc: "Professor Paws (neighbour cat) sneaks through the cat flap with a gadget strapped to his back.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0002-0002-0002-000000000006"), Scene: 2, Shot: 2,
         Img: "storyboard/29MarAnimationImages/scene_02_shot_01_313eb6.png",
         Desc: "Alternate angle: Professor Paws presenting a holographic blueprint to Mr. Whiskers.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0002-0003-0002-000000000007"), Scene: 2, Shot: 3,
         Img: "storyboard/29MarAnimationImages/scene_02_shot_02_7b60f5.png",
         Desc: "Two-shot of both cats studying the contraption — Mr. Whiskers looks intrigued, Professor Paws narrates.",
         Style: "pixar3d", RegenCount: 1),

        (Id: Guid.Parse("aaaaaaaa-0002-0004-0002-000000000008"), Scene: 2, Shot: 4,
         Img: "storyboard/29MarAnimationImages/scene_02_shot_03_1f3279.png",
         Desc: "The gadget sparks to life, bathing the room in purple light as Mr. Whiskers gets zapped.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0003-0001-0003-000000000009"), Scene: 3, Shot: 1,
         Img: "storyboard/29MarAnimationImages/scene_03_shot_01_b50a0d.png",
         Desc: "Mr. Whiskers levitates off the couch — eyes glowing purple, fur on end — discovering his new superpower.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0003-0002-0003-000000000010"), Scene: 3, Shot: 2,
         Img: "storyboard/29MarAnimationImages/scene_03_shot_01_bb3785.png",
         Desc: "Dave drops his coffee mug in shock as Mr. Whiskers floats past at eye level.",
         Style: (string?)null, RegenCount: 0),

        (Id: Guid.Parse("aaaaaaaa-0003-0003-0003-000000000011"), Scene: 3, Shot: 3,
         Img: "storyboard/29MarAnimationImages/scene_03_shot_02_480e61.png",
         Desc: "Chaos: Mr. Whiskers zooms around the apartment at super-speed, knocking over everything.",
         Style: (string?)null, RegenCount: 1),

        (Id: Guid.Parse("aaaaaaaa-0003-0004-0003-000000000012"), Scene: 3, Shot: 4,
         Img: "storyboard/29MarAnimationImages/scene_03_shot_03_96a061.png",
         Desc: "Professor Paws furiously scribbles notes while Dave tries to catch Mr. Whiskers with a laundry basket.",
         Style: "comicbook", RegenCount: 0),
    };

    foreach (var s in shots)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [content].[StoryboardShots] WHERE Id = {0})
            INSERT INTO [content].[StoryboardShots]
                (Id, StoryboardId, SceneNumber, ShotIndex, ImageUrl, Description, StyleOverride, RegenerationCount, CreatedAt, UpdatedAt, IsDeleted)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {8}, 0)
            """,
            s.Id, devStoryboardId, s.Scene, s.Shot, s.Img, s.Desc, s.Style, s.RegenCount, now);
    }

    // ── Animation clips (Phase 8) — one Ready clip per available local video ──
    // Clips that have a matching file in 23MarAnimation/ are seeded as Ready.
    // Missing clips are seeded as Pending (will be resolved by AnimationJobHangfireProcessor).
    var devAnimJobId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [content].[AnimationJobs] WHERE Id = {0})
        INSERT INTO [content].[AnimationJobs]
            (Id, EpisodeId, Backend, EstimatedCostUsd, ActualCostUsd, ApprovedByUserId, ApprovedAt, Status, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {8}, 0)
        """,
        devAnimJobId, devEpisodeId,
        "Local",      // AnimationBackend.Local
        0m,           // EstimatedCostUsd
        0m,           // ActualCostUsd
        devTeamId,    // ApprovedByUserId (use teamId as placeholder for dev)
        now,
        "Completed",  // AnimationStatus.Completed
        now);

    // Clips — scene/shot pairs that have real mp4 files in 23MarAnimation/
    var clipSeeds = new[]
    {
        (Id: Guid.Parse("bbbbbbbb-0001-0001-0001-000000000001"), Scene: 1, Shot: 1,
         ShotId: shots[0].Id,
         Url: "animation/23MarAnimation/scene_01_shot_01.mp4", Duration: 4.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0001-0002-0001-000000000002"), Scene: 1, Shot: 2,
         ShotId: shots[2].Id,
         Url: "animation/23MarAnimation/scene_01_shot_02.mp4", Duration: 5.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0002-0001-0002-000000000003"), Scene: 2, Shot: 1,
         ShotId: shots[4].Id,
         Url: "animation/23MarAnimation/scene_02_shot_01.mp4", Duration: 5.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004"), Scene: 2, Shot: 2,
         ShotId: shots[6].Id,
         Url: "animation/23MarAnimation/scene_02_shot_02.mp4", Duration: 6.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0002-0003-0002-000000000005"), Scene: 2, Shot: 3,
         ShotId: shots[7].Id,
         Url: "animation/23MarAnimation/scene_02_shot_03.mp4", Duration: 5.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0003-0001-0003-000000000006"), Scene: 3, Shot: 1,
         ShotId: shots[8].Id,
         Url: "animation/23MarAnimation/scene_03_shot_01.mp4", Duration: 4.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0003-0002-0003-000000000007"), Scene: 3, Shot: 2,
         ShotId: shots[10].Id,
         Url: "animation/23MarAnimation/scene_03_shot_02.mp4", Duration: 5.0, Status: "Ready"),

        (Id: Guid.Parse("bbbbbbbb-0003-0003-0003-000000000008"), Scene: 3, Shot: 3,
         ShotId: shots[11].Id,
         Url: "animation/23MarAnimation/scene_03_shot_03.mp4", Duration: 6.0, Status: "Ready"),
    };

    foreach (var c in clipSeeds)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [content].[AnimationClips] WHERE Id = {0})
            INSERT INTO [content].[AnimationClips]
                (Id, EpisodeId, SceneNumber, ShotIndex, StoryboardShotId, ClipUrl, DurationSeconds, Status, CreatedAt, UpdatedAt, IsDeleted)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {8}, 0)
            """,
            c.Id, devEpisodeId, c.Scene, c.Shot, c.ShotId, c.Url, c.Duration, c.Status, now);
    }

    Log.Information(
        "Dev content seed applied (Project / Episode / Storyboard / {ShotCount} shots / {ClipCount} animation clips)",
        shots.Length, clipSeeds.Length);
}

/// <summary>
/// Seeds dev render rows into delivery.Renders so the Render page has data to display
/// without having to trigger a real render job first. Idempotent — skips rows that exist.
/// </summary>
static async Task SeedDevDeliveryAsync(AnimStudio.DeliveryModule.Infrastructure.Persistence.DeliveryDbContext db)
{
    var devEpisodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    var now = DateTimeOffset.UtcNow;

    // Completed 16:9 render (30 min ago)
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [delivery].[Renders] WHERE Id = {0})
        INSERT INTO [delivery].[Renders]
            (Id, EpisodeId, AspectRatio, Status, FinalVideoUrl, CdnUrl, CaptionsSrtUrl,
             DurationSeconds, ErrorMessage, CompletedAt, CreatedAt, UpdatedAt, IsDeleted, DeletedAt, DeletedByUserId)
        VALUES
            ({0}, {1}, {2}, {3}, {4}, NULL, NULL, {5}, NULL, {6}, {7}, {7}, 0, NULL, NULL)
        """,
        Guid.Parse("cccccccc-0001-0001-0001-000000000001"),
        devEpisodeId,
        "SixteenNine",
        "Complete",
        "http://localhost:5001/api/v1/files/animation/23MarAnimation/scene_01_shot_01.mp4",
        42.0,
        now.AddMinutes(-30),
        now.AddMinutes(-30));

    // Failed 9:16 render (60 min ago)
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [delivery].[Renders] WHERE Id = {0})
        INSERT INTO [delivery].[Renders]
            (Id, EpisodeId, AspectRatio, Status, FinalVideoUrl, CdnUrl, CaptionsSrtUrl,
             DurationSeconds, ErrorMessage, CompletedAt, CreatedAt, UpdatedAt, IsDeleted, DeletedAt, DeletedByUserId)
        VALUES
            ({0}, {1}, {2}, {3}, NULL, NULL, NULL, {4}, {5}, NULL, {6}, {6}, 0, NULL, NULL)
        """,
        Guid.Parse("cccccccc-0002-0001-0001-000000000002"),
        devEpisodeId,
        "NineSixteen",
        "Failed",
        0.0,
        "Render pipeline timed out after 120 s — re-queue to retry.",
        now.AddMinutes(-60));

    Log.Information("Dev delivery seed applied (2 renders for episode 33333333)");
}

/// <summary>
/// Reads the actual duration of a local audio/video file via ffprobe.
/// Returns the measured milliseconds, or <paramref name="fallbackMs"/> when ffprobe is
/// unavailable or the file does not exist.
/// </summary>
static long GetMediaDurationMs(string rootPath, string relativePath, long fallbackMs)
{
    try
    {
        var fullPath = Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath)) return fallbackMs;

        using var proc = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName               = "ffprobe",
                Arguments              = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{fullPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            },
        };
        proc.Start();
        var output = proc.StandardOutput.ReadToEnd().Trim();
        proc.WaitForExit();

        return double.TryParse(output, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out var secs)
            ? (long)Math.Round(secs * 1000)
            : fallbackMs;
    }
    catch
    {
        return fallbackMs;
    }
}

/// <summary>
/// Seeds the dev timeline for episode 33333333 using real local media files.
/// 8 video clips, 3 dialogue clips, 1 music clip, 3 text-track clips, 3 canvas overlays.
/// Relative paths are stored in the DB — resolved to URLs at query time via IFileStorageService.
/// Audio/video durations are read from actual files via ffprobe so the timeline reflects real lengths.
/// </summary>
static async Task SeedDevTimelineAsync(
    AnimStudio.ContentModule.Infrastructure.Persistence.ContentDbContext db,
    string mediaRoot = @"C:\Users\Vaibhav\cartoon_automation\output")
{
    var devEpisodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    var timelineId   = Guid.Parse("dddddddd-0001-0001-0001-000000000001");
    var videoTrackId = Guid.Parse("eeeeeeee-0001-0001-0001-000000000001");
    var audioTrackId = Guid.Parse("eeeeeeee-0002-0002-0001-000000000002");
    var musicTrackId = Guid.Parse("eeeeeeee-0003-0003-0001-000000000003");
    var textTrackId  = Guid.Parse("eeeeeeee-0004-0004-0001-000000000004");
    var now = DateTimeOffset.UtcNow;

    const long SHOT_MS = 5640;
    const long GAP_MS  = 300;
    const long STEP_MS = SHOT_MS + GAP_MS;    // 5940
    var totalMs = 8 * SHOT_MS + 7 * GAP_MS;   // 47220

    // ── EpisodeTimeline ────────────────────────────────────────────────────────
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [timeline].[EpisodeTimelines] WHERE Id = {0})
        INSERT INTO [timeline].[EpisodeTimelines]
            (Id, EpisodeId, DurationMs, Fps, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, {2}, 24, {3}, {3}, 0)
        """,
        timelineId, devEpisodeId, (int)totalMs, now);

    // ── Tracks ─────────────────────────────────────────────────────────────────
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineTracks] WHERE Id = {0})
        INSERT INTO [timeline].[TimelineTracks]
            (Id, TimelineId, TrackType, Label, SortOrder, IsMuted, IsLocked, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, 'video', 'Video', 0, 0, 0, {2}, {2}, 0)
        """,
        videoTrackId, timelineId, now);

    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineTracks] WHERE Id = {0})
        INSERT INTO [timeline].[TimelineTracks]
            (Id, TimelineId, TrackType, Label, SortOrder, IsMuted, IsLocked, VolumePercent, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, 'audio', 'Dialogue', 1, 0, 0, 80, {2}, {2}, 0)
        """,
        audioTrackId, timelineId, now);

    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineTracks] WHERE Id = {0})
        INSERT INTO [timeline].[TimelineTracks]
            (Id, TimelineId, TrackType, Label, SortOrder, IsMuted, IsLocked, VolumePercent, AutoDuck, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, 'music', 'Background Music', 2, 0, 0, 30, 1, {2}, {2}, 0)
        """,
        musicTrackId, timelineId, now);

    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineTracks] WHERE Id = {0})
        INSERT INTO [timeline].[TimelineTracks]
            (Id, TimelineId, TrackType, Label, SortOrder, IsMuted, IsLocked, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, 'text', 'Text Overlays', 3, 0, 0, {2}, {2}, 0)
        """,
        textTrackId, timelineId, now);

    // ── Video clips (8 shots) ──────────────────────────────────────────────────
    var videoClips = new[]
    {
        (Id: Guid.Parse("ffffffff-0001-0001-0001-000000000001"), Scene: 1, Shot: 1,
         Clip: "animation/23MarAnimation/scene_01_shot_01.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_01_shot_01_6233dc.png"),
        (Id: Guid.Parse("ffffffff-0002-0002-0001-000000000002"), Scene: 1, Shot: 2,
         Clip: "animation/23MarAnimation/scene_01_shot_02.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_01_shot_02_3b5d67.png"),
        (Id: Guid.Parse("ffffffff-0003-0003-0002-000000000003"), Scene: 2, Shot: 1,
         Clip: "animation/23MarAnimation/scene_02_shot_01.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_02_shot_01_13117c.png"),
        (Id: Guid.Parse("ffffffff-0004-0004-0002-000000000004"), Scene: 2, Shot: 2,
         Clip: "animation/23MarAnimation/scene_02_shot_02.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_02_shot_02_7b60f5.png"),
        (Id: Guid.Parse("ffffffff-0005-0005-0002-000000000005"), Scene: 2, Shot: 3,
         Clip: "animation/23MarAnimation/scene_02_shot_03.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_02_shot_03_1f3279.png"),
        (Id: Guid.Parse("ffffffff-0006-0006-0003-000000000006"), Scene: 3, Shot: 1,
         Clip: "animation/23MarAnimation/scene_03_shot_01.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_03_shot_01_b50a0d.png"),
        (Id: Guid.Parse("ffffffff-0007-0007-0003-000000000007"), Scene: 3, Shot: 2,
         Clip: "animation/23MarAnimation/scene_03_shot_02.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_03_shot_02_480e61.png"),
        (Id: Guid.Parse("ffffffff-0008-0008-0003-000000000008"), Scene: 3, Shot: 3,
         Clip: "animation/23MarAnimation/scene_03_shot_03.mp4",
         Thumb: "storyboard/29MarAnimationImages/scene_03_shot_03_96a061.png"),
    };

    for (var i = 0; i < videoClips.Length; i++)
    {
        var vc = videoClips[i];
        var startMs = (long)i * STEP_MS;
        var transition = i == 0 ? (object?)null : "cut";
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineClips] WHERE Id = {0})
            INSERT INTO [timeline].[TimelineClips]
                (Id, TrackId, ClipType, StartMs, DurationMs, SortOrder,
                 SceneNumber, ShotIndex, ClipUrl, ThumbnailUrl, TransitionIn,
                 CreatedAt, UpdatedAt, IsDeleted)
            VALUES ({0}, {1}, 'video', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {10}, 0)
            """,
            vc.Id, videoTrackId, startMs, SHOT_MS, i, vc.Scene, vc.Shot, vc.Clip, vc.Thumb, transition, now);
    }

    // ── Dialogue clips (3, one per scene) ─────────────────────────────────────
    // Durations come from the ACTUAL audio files via ffprobe — not from video shot positions.
    // Fallback values (1000, 1200, 2100) are used only when ffprobe is unavailable.
    var audioClips = new[]
    {
        (Id: Guid.Parse("ffffffff-0101-0101-0101-000000000101"),
         StartMs: 0L,
         Duration: GetMediaDurationMs(mediaRoot, "audio/scene_01_mr._whiskers.mp3", 1000),
         Label: "Mr. Whiskers (S1)", Url: "audio/scene_01_mr._whiskers.mp3"),
        (Id: Guid.Parse("ffffffff-0102-0102-0102-000000000102"),
         StartMs: 2 * STEP_MS,
         Duration: GetMediaDurationMs(mediaRoot, "audio/scene_02_professor_paws.mp3", 1200),
         Label: "Prof. Paws (S2)",   Url: "audio/scene_02_professor_paws.mp3"),
        (Id: Guid.Parse("ffffffff-0103-0103-0103-000000000103"),
         StartMs: 5 * STEP_MS,
         Duration: GetMediaDurationMs(mediaRoot, "audio/scene_03_mr._whiskers.mp3", 2100),
         Label: "Mr. Whiskers (S3)", Url: "audio/scene_03_mr._whiskers.mp3"),
    };

    for (var i = 0; i < audioClips.Length; i++)
    {
        var ac = audioClips[i];
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineClips] WHERE Id = {0})
            INSERT INTO [timeline].[TimelineClips]
                (Id, TrackId, ClipType, StartMs, DurationMs, SortOrder,
                 Label, AudioUrl, VolumePercent, FadeInMs, FadeOutMs,
                 CreatedAt, UpdatedAt, IsDeleted)
            VALUES ({0}, {1}, 'audio', {2}, {3}, {4}, {5}, {6}, 80, 200, 200, {7}, {7}, 0)
            """,
            ac.Id, audioTrackId, ac.StartMs, ac.Duration, i, ac.Label, ac.Url, now);
    }

    // ── Music clip — duration from actual file, not timeline total ────────────────
    var musicDurationMs = GetMediaDurationMs(mediaRoot, "audio/scene_01_dave_the_owner.mp3", 2900);
    await db.Database.ExecuteSqlRawAsync("""
        IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineClips] WHERE Id = {0})
        INSERT INTO [timeline].[TimelineClips]
            (Id, TrackId, ClipType, StartMs, DurationMs, SortOrder,
             Label, AudioUrl, VolumePercent, FadeInMs, FadeOutMs,
             CreatedAt, UpdatedAt, IsDeleted)
        VALUES ({0}, {1}, 'music', 0, {2}, 0,
                'Background Music', 'audio/scene_01_dave_the_owner.mp3',
                30, 500, 500, {3}, {3}, 0)
        """,
        Guid.Parse("ffffffff-0201-0201-0201-000000000201"), musicTrackId, musicDurationMs, now);

    // ── Text-track clips (scene title cards) ───────────────────────────────────
    var textClips = new[]
    {
        (Id: Guid.Parse("ffffffff-0301-0301-0301-000000000301"), Text: "Scene 1: Ordinary Day",
         StartMs: 0L),
        (Id: Guid.Parse("ffffffff-0302-0302-0302-000000000302"), Text: "Scene 2: Enter the Professor",
         StartMs: 2 * STEP_MS),
        (Id: Guid.Parse("ffffffff-0303-0303-0303-000000000303"), Text: "Scene 3: Superpowers Unleashed",
         StartMs: 5 * STEP_MS),
    };

    for (var i = 0; i < textClips.Length; i++)
    {
        var tc = textClips[i];
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineClips] WHERE Id = {0})
            INSERT INTO [timeline].[TimelineClips]
                (Id, TrackId, ClipType, StartMs, DurationMs, SortOrder,
                 Text, FontSize, Color, Animation,
                 CreatedAt, UpdatedAt, IsDeleted)
            VALUES ({0}, {1}, 'text', {2}, 3000, {3}, {4}, 24, '#ffffff', 'fadeIn', {5}, {5}, 0)
            """,
            tc.Id, textTrackId, tc.StartMs, i, tc.Text, now);
    }

    // ── Canvas TextOverlays ────────────────────────────────────────────────────
    var overlays = new[]
    {
        (Id: Guid.Parse("ffffffff-0401-0401-0401-000000000401"),
         Text: "Episode 1: The Signal", FontSize: 32, X: 10, Y: 10,
         StartMs: 0L, Duration: 4000L, Animation: "fadeIn", ZIndex: 10),
        (Id: Guid.Parse("ffffffff-0402-0402-0402-000000000402"),
         Text: "Mr. Whiskers", FontSize: 20, X: 60, Y: 80,
         StartMs: 5 * STEP_MS, Duration: 3000L, Animation: "slideUp", ZIndex: 10),
        (Id: Guid.Parse("ffffffff-0403-0403-0403-000000000403"),
         Text: "The End", FontSize: 36, X: 40, Y: 45,
         StartMs: totalMs - 3000, Duration: 3000L, Animation: "fadeIn", ZIndex: 10),
    };

    foreach (var o in overlays)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [timeline].[TimelineTextOverlays] WHERE Id = {0})
            INSERT INTO [timeline].[TimelineTextOverlays]
                (Id, TimelineId, Text, FontSizePixels, Color,
                 PositionX, PositionY, StartMs, DurationMs, Animation, ZIndex,
                 CreatedAt, UpdatedAt, IsDeleted)
            VALUES ({0}, {1}, {2}, {3}, '#ffffff', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {10}, 0)
            """,
            o.Id, timelineId, o.Text, o.FontSize, o.X, o.Y,
            o.StartMs, o.Duration, o.Animation, o.ZIndex, now);
    }

    Log.Information("Dev timeline seed applied (4 tracks / 8 video + 3 dialogue + 1 music + 3 text clips / 3 canvas overlays)");
}
