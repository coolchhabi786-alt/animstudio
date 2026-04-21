# Phase 9: Render & Delivery — Implementation Prompt

## Objective
Implement post-production rendering, CDN delivery, and SRT caption generation. This is the foundation for phases 10-11.

---

## Current State
❌ **Missing**:
- `DeliveryModule` — currently a stub
- `Render` entity and related database schema
- All services and controllers

---

## Implementation Checklist

### 1. Create DeliveryModule Structure
**Folder**: `AnimStudio.DeliveryModule/`

```
Domain/
  Entities/
    Render.cs
  Enums/
    RenderStatus.cs
    AspectRatio.cs
  Events/
    RenderDomainEvents.cs
  
Application/
  DTOs/
    RenderDtos.cs
  Interfaces/
    IRenderRepository.cs
    IRenderService.cs
    ISrtGeneratorService.cs
    ICdnService.cs
  Commands/
    StartRender/StartRenderCommand.cs
  Queries/
    GetRenderHistory/GetRenderHistoryQuery.cs
  Services/
    RenderService.cs
    SrtGeneratorService.cs
    AspectRatioService.cs
  EventHandlers/
    SignalRRenderNotifier.cs

Infrastructure/
  Persistence/
    DeliveryDbContext.cs
  Repositories/
    RenderRepository.cs
  Migrations/
    20260421000000_Phase9Render.cs

DeliveryModuleRegistration.cs
```

### 2. Domain Entities
**File**: `AnimStudio.DeliveryModule/Domain/Entities/Render.cs`

```csharp
public sealed class Render : AggregateRoot<Guid>, ISoftDelete
{
    public Guid EpisodeId { get; set; }
    public AspectRatio AspectRatio { get; set; }
    public string? FinalVideoUrl { get; set; }              // Blob path
    public string? CdnUrl { get; set; }                     // Signed Azure CDN URL
    public string? CaptionsSrtUrl { get; set; }             // SRT file URL
    public double DurationSeconds { get; set; }
    public RenderStatus Status { get; set; }               // Pending, Rendering, Complete, Failed
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsDeleted { get; set; }
    
    public static Render Create(Guid id, Guid episodeId, AspectRatio aspectRatio)
    {
        return new Render
        {
            Id = id,
            EpisodeId = episodeId,
            AspectRatio = aspectRatio,
            Status = RenderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

public enum AspectRatio
{
    SixteenNine = 0,    // 16:9
    NineSixteen = 1,    // 9:16
    OneOne = 2          // 1:1
}

public enum RenderStatus
{
    Pending = 0,
    Rendering = 1,
    Complete = 2,
    Failed = 3
}
```

### 3. Domain Events
**File**: `AnimStudio.DeliveryModule/Domain/Events/RenderDomainEvents.cs`

```csharp
public record RenderStartedEvent(Guid RenderId, Guid EpisodeId) : IDomainEvent;
public record RenderProgressEvent(Guid RenderId, Guid EpisodeId, int Percent, string Stage) : IDomainEvent;
public record RenderCompleteEvent(Guid RenderId, Guid EpisodeId, string CdnUrl, string SrtUrl, double DurationSeconds) : IDomainEvent;
public record RenderFailedEvent(Guid RenderId, Guid EpisodeId, string ErrorMessage) : IDomainEvent;
```

### 4. DeliveryDbContext
**File**: `AnimStudio.DeliveryModule/Infrastructure/Persistence/DeliveryDbContext.cs`

```csharp
public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
    }
    
    public DbSet<Render> Renders { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Render>(b =>
        {
            b.ToTable("Renders", "delivery");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.EpisodeId);
            b.HasIndex(x => x.Status);
            b.HasQueryFilter(x => !x.IsDeleted);
            b.Property(x => x.RowVersion).IsRowVersion();
            b.Property(x => x.AspectRatio).HasConversion<int>();
            b.Property(x => x.Status).HasConversion<int>();
        });
    }
}
```

### 5. Repository
**File**: `AnimStudio.DeliveryModule/Infrastructure/Repositories/RenderRepository.cs`

```csharp
public interface IRenderRepository
{
    Task<Render?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Render>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task<Render?> GetLatestByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task AddAsync(Render render, CancellationToken ct = default);
    Task UpdateAsync(Render render, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public class RenderRepository : IRenderRepository
{
    private readonly DeliveryDbContext _dbContext;
    
    public RenderRepository(DeliveryDbContext dbContext) => _dbContext = dbContext;
    
    public async Task<Render?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Renders
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken: ct);
    }
    
    public async Task<List<Render>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
    {
        return await _dbContext.Renders
            .Where(x => x.EpisodeId == episodeId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }
    
    public async Task<Render?> GetLatestByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
    {
        return await _dbContext.Renders
            .Where(x => x.EpisodeId == episodeId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: ct);
    }
    
    public async Task AddAsync(Render render, CancellationToken ct = default)
    {
        await _dbContext.Renders.AddAsync(render, cancellationToken: ct);
    }
    
    public async Task UpdateAsync(Render render, CancellationToken ct = default)
    {
        _dbContext.Renders.Update(render);
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}
```

### 6. SRT Generator Service (Key)
**File**: `AnimStudio.DeliveryModule/Application/Services/SrtGeneratorService.cs`

```csharp
public interface ISrtGeneratorService
{
    Task<Result<string>> GenerateSrtAsync(
        Guid episodeId,
        List<DialogueLineDto> dialogueLines,
        string outputPath,
        CancellationToken ct = default);
}

public class SrtGeneratorService : ISrtGeneratorService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<SrtGeneratorService> _logger;
    
    public SrtGeneratorService(BlobContainerClient blobContainerClient, ILogger<SrtGeneratorService> logger)
    {
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }
    
    public async Task<Result<string>> GenerateSrtAsync(
        Guid episodeId,
        List<DialogueLineDto> dialogueLines,
        string outputPath,
        CancellationToken ct = default)
    {
        try
        {
            var srtBuilder = new StringBuilder();
            int sequenceNumber = 1;
            
            foreach (var dialogue in dialogueLines.OrderBy(x => x.StartTimeSeconds))
            {
                var startTime = FormatSrtTime(dialogue.StartTimeSeconds);
                var endTime = FormatSrtTime(dialogue.EndTimeSeconds);
                
                srtBuilder.AppendLine(sequenceNumber.ToString());
                srtBuilder.AppendLine($"{startTime} --> {endTime}");
                srtBuilder.AppendLine(dialogue.Text);
                srtBuilder.AppendLine();
                
                sequenceNumber++;
            }
            
            var srtContent = srtBuilder.ToString();
            
            // Upload to Blob
            var blobClient = _blobContainerClient.GetBlobClient(outputPath);
            await blobClient.UploadAsync(
                new BinaryData(Encoding.UTF8.GetBytes(srtContent)),
                overwrite: true,
                cancellationToken: ct);
            
            _logger.LogInformation("Generated SRT with {LineCount} captions for episode {EpisodeId}", dialogueLines.Count, episodeId);
            
            return Result<string>.Success(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SRT for episode {EpisodeId}", episodeId);
            return Result<string>.Failure("SRT_ERROR", "Failed to generate captions");
        }
    }
    
    private static string FormatSrtTime(double totalSeconds)
    {
        var hours = (int)(totalSeconds / 3600);
        var minutes = (int)((totalSeconds % 3600) / 60);
        var seconds = (int)(totalSeconds % 60);
        var milliseconds = (int)((totalSeconds % 1) * 1000);
        
        return $"{hours:D2}:{minutes:D2}:{seconds:D2},{milliseconds:D3}";
    }
}

public record DialogueLineDto(string Character, string Text, double StartTimeSeconds, double EndTimeSeconds);
```

### 7. Aspect Ratio Service
**File**: `AnimStudio.DeliveryModule/Application/Services/AspectRatioService.cs`

```csharp
public interface IAspectRatioService
{
    (int width, int height) GetResolution(AspectRatio aspectRatio, int basePixels = 1080);
    string GetFfmpegScaleFilter(AspectRatio aspectRatio, int basePixels = 1080);
}

public class AspectRatioService : IAspectRatioService
{
    public (int width, int height) GetResolution(AspectRatio aspectRatio, int basePixels = 1080)
    {
        return aspectRatio switch
        {
            AspectRatio.SixteenNine => (basePixels * 16 / 9, basePixels),
            AspectRatio.NineSixteen => (basePixels, basePixels * 16 / 9),
            AspectRatio.OneOne => (basePixels, basePixels),
            _ => (1920, 1080)
        };
    }
    
    public string GetFfmpegScaleFilter(AspectRatio aspectRatio, int basePixels = 1080)
    {
        var (width, height) = GetResolution(aspectRatio, basePixels);
        return $"scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:(ow-iw)/2:(oh-ih)/2";
    }
}
```

### 8. CDN Service
**File**: `AnimStudio.DeliveryModule/Application/Services/CdnService.cs`

```csharp
public interface ICdnService
{
    Task<string> GenerateSignedUrlAsync(string blobPath, int expiryDays = 30, CancellationToken ct = default);
}

public class CdnService : ICdnService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<CdnService> _logger;
    
    public CdnService(BlobContainerClient blobContainerClient, ILogger<CdnService> logger)
    {
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }
    
    public async Task<string> GenerateSignedUrlAsync(string blobPath, int expiryDays = 30, CancellationToken ct = default)
    {
        try
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobPath);
            
            var sasUri = blobClient.GenerateSasUri(
                BlobSasPermissions.Read,
                DateTimeOffset.UtcNow.AddDays(expiryDays));
            
            _logger.LogInformation("Generated signed URL for {BlobPath} with {Days} day expiry", blobPath, expiryDays);
            
            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signed URL for {BlobPath}", blobPath);
            throw;
        }
    }
}
```

### 9. Render Service (Orchestration)
**File**: `AnimStudio.DeliveryModule/Application/Services/RenderService.cs`

```csharp
public interface IRenderService
{
    Task<Result<RenderDto>> StartRenderAsync(Guid episodeId, AspectRatio aspectRatio, Guid userId, CancellationToken ct = default);
    Task<Result<RenderDto>> GetRenderAsync(Guid renderId, CancellationToken ct = default);
    Task<Result<List<RenderDto>>> GetRenderHistoryAsync(Guid episodeId, CancellationToken ct = default);
    Task<Result<string>> GetSignedRenderUrlAsync(Guid renderId, int expiryDays = 30, CancellationToken ct = default);
}

public class RenderService : IRenderService
{
    private readonly IRenderRepository _repository;
    private readonly IJobEnqueuer _jobEnqueuer;
    private readonly ICdnService _cdnService;
    private readonly ILogger<RenderService> _logger;
    
    public RenderService(
        IRenderRepository repository,
        IJobEnqueuer jobEnqueuer,
        ICdnService cdnService,
        ILogger<RenderService> logger)
    {
        _repository = repository;
        _jobEnqueuer = jobEnqueuer;
        _cdnService = cdnService;
        _logger = logger;
    }
    
    public async Task<Result<RenderDto>> StartRenderAsync(
        Guid episodeId,
        AspectRatio aspectRatio,
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Create Render entity
            var render = Render.Create(Guid.NewGuid(), episodeId, aspectRatio);
            render.Status = RenderStatus.Pending;
            
            await _repository.AddAsync(render, ct);
            await _repository.SaveChangesAsync(ct);
            
            // Enqueue Hangfire job
            await _jobEnqueuer.EnqueueRenderJobAsync(
                new RenderJobMessage
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    RenderId = render.Id,
                    EpisodeId = episodeId,
                    AspectRatio = (int)aspectRatio
                },
                ct);
            
            _logger.LogInformation("Render job started for episode {EpisodeId}: {RenderId}", episodeId, render.Id);
            
            return Result<RenderDto>.Success(new RenderDto
            {
                Id = render.Id,
                EpisodeId = render.EpisodeId,
                AspectRatio = render.AspectRatio,
                Status = render.Status,
                CreatedAt = render.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting render for episode {EpisodeId}", episodeId);
            return Result<RenderDto>.Failure("RENDER_ERROR", "Failed to start render");
        }
    }
    
    public async Task<Result<RenderDto>> GetRenderAsync(Guid renderId, CancellationToken ct = default)
    {
        var render = await _repository.GetByIdAsync(renderId, ct);
        if (render is null)
            return Result<RenderDto>.Failure("RENDER_NOT_FOUND", "Render not found");
        
        return Result<RenderDto>.Success(MapToDto(render));
    }
    
    public async Task<Result<List<RenderDto>>> GetRenderHistoryAsync(Guid episodeId, CancellationToken ct = default)
    {
        var renders = await _repository.GetByEpisodeAsync(episodeId, ct);
        return Result<List<RenderDto>>.Success(renders.Select(MapToDto).ToList());
    }
    
    public async Task<Result<string>> GetSignedRenderUrlAsync(Guid renderId, int expiryDays = 30, CancellationToken ct = default)
    {
        var render = await _repository.GetByIdAsync(renderId, ct);
        if (render is null)
            return Result<string>.Failure("RENDER_NOT_FOUND", "Render not found");
        
        if (string.IsNullOrEmpty(render.CdnUrl))
            return Result<string>.Failure("NO_VIDEO", "Render is not yet complete");
        
        var signedUrl = await _cdnService.GenerateSignedUrlAsync(render.CdnUrl, expiryDays, ct);
        return Result<string>.Success(signedUrl);
    }
    
    private static RenderDto MapToDto(Render render)
    {
        return new RenderDto
        {
            Id = render.Id,
            EpisodeId = render.EpisodeId,
            AspectRatio = render.AspectRatio,
            Status = render.Status,
            CdnUrl = render.CdnUrl,
            SrtUrl = render.CaptionsSrtUrl,
            Duration = render.DurationSeconds,
            CreatedAt = render.CreatedAt,
            CompletedAt = render.CompletedAt
        };
    }
}
```

### 10. SignalR Render Notifier
**File**: `AnimStudio.DeliveryModule/Application/EventHandlers/SignalRRenderNotifier.cs`

```csharp
public class SignalRRenderProgressNotifier : INotificationHandler<RenderProgressEvent>
{
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly ILogger<SignalRRenderProgressNotifier> _logger;
    
    public SignalRRenderProgressNotifier(IHubContext<ProgressHub> hubContext, ILogger<SignalRRenderProgressNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task Handle(RenderProgressEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                episodeId = notification.EpisodeId,
                renderId = notification.RenderId,
                percent = notification.Percent,
                stage = notification.Stage
            };
            
            var groupName = $"team:{notification.EpisodeId}";  // Get actual teamId from context
            
            await _hubContext.Clients.Group(groupName).SendAsync(
                "RenderProgress",
                payload,
                cancellationToken);
            
            _logger.LogInformation("RenderProgress: {Stage} {Percent}%", notification.Stage, notification.Percent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting RenderProgress");
        }
    }
}

public class SignalRRenderCompleteNotifier : INotificationHandler<RenderCompleteEvent>
{
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly ILogger<SignalRRenderCompleteNotifier> _logger;
    
    public SignalRRenderCompleteNotifier(IHubContext<ProgressHub> hubContext, ILogger<SignalRRenderCompleteNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task Handle(RenderCompleteEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                episodeId = notification.EpisodeId,
                renderId = notification.RenderId,
                cdnUrl = notification.CdnUrl,
                srtUrl = notification.SrtUrl,
                durationSeconds = notification.DurationSeconds
            };
            
            var groupName = $"team:{notification.EpisodeId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync(
                "RenderComplete",
                payload,
                cancellationToken);
            
            _logger.LogInformation("RenderComplete broadcast for episode {EpisodeId}", notification.EpisodeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting RenderComplete");
        }
    }
}
```

### 11. DTOs
**File**: `AnimStudio.DeliveryModule/Application/DTOs/RenderDtos.cs`

```csharp
public class RenderDto
{
    public Guid Id { get; set; }
    public Guid EpisodeId { get; set; }
    public AspectRatio AspectRatio { get; set; }
    public RenderStatus Status { get; set; }
    public string? CdnUrl { get; set; }
    public string? SrtUrl { get; set; }
    public double Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class StartRenderRequest
{
    public AspectRatio AspectRatio { get; set; }
}
```

### 12. Controller
**File**: `AnimStudio.API/Controllers/RenderController.cs`

```csharp
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public class RenderController(ISender mediator) : ControllerBase
{
    [HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/render")]
    public async Task<IActionResult> StartRender(
        Guid id,
        [FromBody] StartRenderRequest req,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var result = await mediator.Send(new StartRenderCommand(id, req.AspectRatio, userId), ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status202Accepted, result.Value)
            : BadRequest(new { error = result.Error });
    }
    
    [HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/renders")]
    public async Task<IActionResult> GetRenderHistory(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRenderHistoryQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
    
    [HttpGet("api/v{version:apiVersion}/renders/{id:guid}")]
    public async Task<IActionResult> GetRender(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRenderQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
    
    [HttpGet("api/v{version:apiVersion}/renders/{id:guid}/srt")]
    public async Task<IActionResult> DownloadSrt(Guid id, CancellationToken ct)
    {
        // Return SRT file download (implementation details)
        var result = await mediator.Send(new GetRenderQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound();
        
        // Redirect to SRT URL or stream from Blob
        return Redirect(result.Value.SrtUrl);
    }
}
```

### 13. Module Registration
**File**: `AnimStudio.DeliveryModule/DeliveryModuleRegistration.cs`

```csharp
public class DeliveryModuleRegistration : IModuleRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<DeliveryDbContext>(options =>
            options.UseSqlServer(connectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));
        
        // Repositories
        services.AddScoped<IRenderRepository, RenderRepository>();
        
        // Services
        services.AddScoped<IRenderService, RenderService>();
        services.AddScoped<ISrtGeneratorService, SrtGeneratorService>();
        services.AddScoped<IAspectRatioService, AspectRatioService>();
        services.AddScoped<ICdnService, CdnService>();
        
        // Event handlers
        services.AddScoped<INotificationHandler<RenderProgressEvent>, SignalRRenderProgressNotifier>();
        services.AddScoped<INotificationHandler<RenderCompleteEvent>, SignalRRenderCompleteNotifier>();
    }
}
```

### 14. MediatR Commands/Queries
Pattern (Commands/Queries/Handlers):
```csharp
public record StartRenderCommand(Guid EpisodeId, AspectRatio AspectRatio, Guid UserId) : IRequest<Result<RenderDto>>;
public record GetRenderQuery(Guid RenderId) : IRequest<Result<RenderDto>>;
public record GetRenderHistoryQuery(Guid EpisodeId) : IRequest<Result<List<RenderDto>>>;
```

---

## Database Migration
**File**: `AnimStudio.DeliveryModule/Infrastructure/Migrations/20260421000000_Phase9Render.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.EnsureSchema("delivery");
    
    migrationBuilder.CreateTable(
        name: "Renders",
        schema: "delivery",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            EpisodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            AspectRatio = table.Column<int>(type: "int", nullable: false),
            FinalVideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
            CdnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
            CaptionsSrtUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
            DurationSeconds = table.Column<double>(type: "float", nullable: false),
            Status = table.Column<int>(type: "int", nullable: false),
            ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            IsDeleted = table.Column<bool>(type: "bit", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Renders", x => x.Id);
        });
    
    migrationBuilder.CreateIndex(
        name: "IX_Renders_EpisodeId",
        schema: "delivery",
        table: "Renders",
        column: "EpisodeId");
    
    migrationBuilder.CreateIndex(
        name: "IX_Renders_Status",
        schema: "delivery",
        table: "Renders",
        column: "Status");
}
```

---

## Azure Infrastructure Setup

**Blob Storage**:
- Container: `renders` — for final rendered videos

**Key Vault**:
- Connection strings for all Blob containers

---

## Testing & Acceptance Criteria

- [ ] Start Render: creates record, enqueues Hangfire, returns 202
- [ ] SignalR broadcasts RenderProgress + RenderComplete
- [ ] SRT generation: DialogueLine timings → valid SRT format
- [ ] Signed URLs: 30-day expiry, not publicly accessible
- [ ] Aspect ratio: correctly scales to target resolution
- [ ] All tests pass
