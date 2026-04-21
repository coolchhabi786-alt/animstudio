# Phase 8: Animation Approval — Implementation Prompt

## Objective
Complete animation job handling: SignalR notifications, Hangfire integration, and verify all controller routes work.

---

## Current State
✅ **Done**:
- `AnimationJob` and `AnimationClip` entities complete
- `AnimationController` with all 4 routes
- Cost estimation logic

❌ **Missing**:
- `SignalRAnimationClipNotifier` (broadcast ClipReady events)
- Hangfire job handler (calls Python or local backend)
- Domain event handlers wired up

---

## Implementation Checklist

### 1. Migration Verification
Ensure these tables exist with proper schema (from phase8-animation-manifest.md):

```csharp
// In ContentDbContext.OnModelCreating():
modelBuilder.Entity<AnimationJob>(b =>
{
    b.ToTable("AnimationJobs", "content");
    b.HasKey(x => x.Id);
    b.HasIndex(x => x.EpisodeId);
    b.HasQueryFilter(x => !x.IsDeleted);
    b.Property(x => x.EstimatedCostUsd).HasPrecision(10, 4);
    b.Property(x => x.ActualCostUsd).HasPrecision(10, 4);
    b.Property(x => x.RowVersion).IsRowVersion();
});

modelBuilder.Entity<AnimationClip>(b =>
{
    b.ToTable("AnimationClips", "content");
    b.HasIndex(x => new { x.EpisodeId, x.SceneNumber, x.ShotIndex }).IsUnique();
    b.HasQueryFilter(x => !x.IsDeleted);
    b.HasOne<StoryboardShot>()
        .WithMany()
        .HasForeignKey(x => x.StoryboardShotId)
        .OnDelete(DeleteBehavior.SetNull);
    b.Property(x => x.RowVersion).IsRowVersion();
});
```

### 2. SignalR Animation Notifier (Key Component)
**File**: `AnimStudio.ContentModule/Application/EventHandlers/SignalRAnimationClipNotifier.cs`

```csharp
public class SignalRAnimationClipNotifier : INotificationHandler<AnimationClipReadyEvent>
{
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly ILogger<SignalRAnimationClipNotifier> _logger;
    
    public SignalRAnimationClipNotifier(
        IHubContext<ProgressHub> hubContext,
        IEpisodeRepository episodeRepository,
        ILogger<SignalRAnimationClipNotifier> logger)
    {
        _hubContext = hubContext;
        _episodeRepository = episodeRepository;
        _logger = logger;
    }
    
    public async Task Handle(AnimationClipReadyEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get episode to find team
            var episode = await _episodeRepository.GetByIdAsync(notification.EpisodeId, cancellationToken);
            if (episode is null)
            {
                _logger.LogWarning("Episode {EpisodeId} not found for clip event", notification.EpisodeId);
                return;
            }
            
            // Generate signed URL for clip (60-second TTL)
            var signedClipUrl = await GenerateSignedClipUrlAsync(notification.ClipUrl, cancellationToken);
            
            var payload = new
            {
                episodeId = notification.EpisodeId,
                sceneNumber = notification.SceneNumber,
                shotIndex = notification.ShotIndex,
                clipId = notification.ClipId,
                clipUrl = signedClipUrl,
                durationSeconds = notification.DurationSeconds
            };
            
            var groupName = $"team:{episode.TeamId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync(
                "ClipReady",
                payload,
                cancellationToken);
            
            _logger.LogInformation(
                "ClipReady broadcast to {Group}: scene {Scene}, shot {Shot}",
                groupName,
                notification.SceneNumber,
                notification.ShotIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting ClipReady event");
        }
    }
    
    private async Task<string> GenerateSignedClipUrlAsync(string clipPath, CancellationToken ct)
    {
        // Extract blob URI from clip path and generate SAS URL
        // Example: clipPath = "animation-clips/episode-123/scene-1-shot-0.mp4"
        var blobClient = new BlobClient(
            new Uri($"https://YOUR_STORAGE_ACCOUNT.blob.core.windows.net/{clipPath}"),
            new StorageSharedKeyCredential("ACCOUNT", "KEY"));
        
        var sasUri = blobClient.GenerateSasUri(
            BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddSeconds(60));
        
        return sasUri.ToString();
    }
}
```

### 3. AnimationClip Repository
**File**: `AnimStudio.ContentModule/Infrastructure/Repositories/AnimationClipRepository.cs`

```csharp
public interface IAnimationClipRepository
{
    Task<AnimationClip?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AnimationClip>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task<AnimationClip?> GetBySceneAndShotAsync(Guid episodeId, int sceneNumber, int shotIndex, CancellationToken ct = default);
    Task AddAsync(AnimationClip clip, CancellationToken ct = default);
    Task UpdateAsync(AnimationClip clip, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public class AnimationClipRepository : IAnimationClipRepository
{
    private readonly ContentDbContext _dbContext;
    
    public AnimationClipRepository(ContentDbContext dbContext) => _dbContext = dbContext;
    
    public async Task<AnimationClip?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.AnimationClips
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken: ct);
    }
    
    public async Task<List<AnimationClip>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
    {
        return await _dbContext.AnimationClips
            .Where(x => x.EpisodeId == episodeId && !x.IsDeleted)
            .OrderBy(x => x.SceneNumber)
            .ThenBy(x => x.ShotIndex)
            .ToListAsync(cancellationToken: ct);
    }
    
    public async Task<AnimationClip?> GetBySceneAndShotAsync(Guid episodeId, int sceneNumber, int shotIndex, CancellationToken ct = default)
    {
        return await _dbContext.AnimationClips
            .FirstOrDefaultAsync(
                x => x.EpisodeId == episodeId && x.SceneNumber == sceneNumber && x.ShotIndex == shotIndex && !x.IsDeleted,
                cancellationToken: ct);
    }
    
    public async Task AddAsync(AnimationClip clip, CancellationToken ct = default)
    {
        await _dbContext.AnimationClips.AddAsync(clip, cancellationToken: ct);
    }
    
    public async Task UpdateAsync(AnimationClip clip, CancellationToken ct = default)
    {
        _dbContext.AnimationClips.Update(clip);
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}
```

### 4. Hangfire Job Handler
**File**: `AnimStudio.API/Hosted/AnimationJobHandler.cs`

```csharp
public class AnimationJobHandler : IJobHandler
{
    private readonly IAnimationClipRepository _clipRepository;
    private readonly IAzureBlobService _blobService;
    private readonly IMediator _mediator;
    private readonly ILogger<AnimationJobHandler> _logger;
    
    public AnimationJobHandler(
        IAnimationClipRepository clipRepository,
        IAzureBlobService blobService,
        IMediator mediator,
        ILogger<AnimationJobHandler> logger)
    {
        _clipRepository = clipRepository;
        _blobService = blobService;
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task HandleAsync(JobMessage message, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting animation job for episode {EpisodeId}", message.EpisodeId);
            
            // Get all clips to render for this episode
            var clips = await _clipRepository.GetByEpisodeAsync(message.EpisodeId, ct);
            
            if (clips.Count == 0)
            {
                _logger.LogWarning("No animation clips found for episode {EpisodeId}", message.EpisodeId);
                return;
            }
            
            // For each clip, call Python pipeline (via Service Bus) or local backend
            // Example: Call local backend or enqueue to Python
            foreach (var clip in clips)
            {
                await RenderClipAsync(clip, message, ct);
            }
            
            _logger.LogInformation("Animation job completed for episode {EpisodeId}", message.EpisodeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing animation job for episode {EpisodeId}", message.EpisodeId);
            throw;
        }
    }
    
    private async Task RenderClipAsync(AnimationClip clip, JobMessage job, CancellationToken ct)
    {
        // Call Python via Service Bus OR local backend
        // For now, simulate rendering by setting clip status to Ready
        
        clip.Status = ClipStatus.Rendering;
        await _clipRepository.UpdateAsync(clip, ct);
        await _clipRepository.SaveChangesAsync(ct);
        
        _logger.LogInformation(
            "Rendering clip: episode {EpisodeId}, scene {Scene}, shot {Shot}",
            clip.EpisodeId,
            clip.SceneNumber,
            clip.ShotIndex);
        
        // Simulate rendering delay (replace with actual API call)
        await Task.Delay(TimeSpan.FromSeconds(5), ct);
        
        // Generate mock video URL (in real implementation, this comes from Python)
        clip.ClipUrl = $"animation-clips/episode-{clip.EpisodeId}/scene-{clip.SceneNumber}-shot-{clip.ShotIndex}.mp4";
        clip.Status = ClipStatus.Ready;
        clip.DurationSeconds = 8.0;
        clip.UpdatedAt = DateTime.UtcNow;
        
        await _clipRepository.UpdateAsync(clip, ct);
        await _clipRepository.SaveChangesAsync(ct);
        
        // Publish domain event → SignalR notifier
        var evt = new AnimationClipReadyEvent(
            clip.Id,
            clip.EpisodeId,
            clip.SceneNumber,
            clip.ShotIndex,
            clip.ClipUrl,
            clip.DurationSeconds ?? 0);
        
        await _mediator.Publish(evt, ct);
    }
}
```

### 5. Job Cost Service
**File**: `AnimStudio.API/Services/AnimationCostService.cs`

```csharp
public interface IAnimationCostService
{
    decimal CalculateCost(AnimationBackend backend, int shotCount);
    (decimal unit, decimal total) GetDetailedCost(AnimationBackend backend, int shotCount);
}

public class AnimationCostService : IAnimationCostService
{
    private readonly IConfiguration _config;
    
    public AnimationCostService(IConfiguration config) => _config = config;
    
    public decimal CalculateCost(AnimationBackend backend, int shotCount)
    {
        var unitCost = backend switch
        {
            AnimationBackend.Kling => decimal.Parse(_config["Animation:Rates:Kling"] ?? "0.056"),
            AnimationBackend.Local => decimal.Parse(_config["Animation:Rates:Local"] ?? "0.000"),
            _ => 0m
        };
        
        return shotCount * unitCost;
    }
    
    public (decimal unit, decimal total) GetDetailedCost(AnimationBackend backend, int shotCount)
    {
        var unitCost = backend switch
        {
            AnimationBackend.Kling => decimal.Parse(_config["Animation:Rates:Kling"] ?? "0.056"),
            AnimationBackend.Local => decimal.Parse(_config["Animation:Rates:Local"] ?? "0.000"),
            _ => 0m
        };
        
        return (unitCost, shotCount * unitCost);
    }
}
```

### 6. Verify Controller Routes
**File**: `AnimStudio.API/Controllers/AnimationController.cs`

Verify these are complete (should already exist from specs):

```csharp
[HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/animation/estimate")]
[HttpPost("api/v{version:apiVersion}/episodes/{id:guid}/animation")]
[HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/animation")]
[HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/animation/clips/{clipId:guid}")]
```

All should return appropriate status codes and DTOs.

### 7. Module Registration
**File**: `AnimStudio.ContentModule/ContentModuleRegistration.cs`

```csharp
services.AddScoped<IAnimationClipRepository, AnimationClipRepository>();
services.AddScoped<IAnimationCostService, AnimationCostService>();
services.AddScoped<INotificationHandler<AnimationClipReadyEvent>, SignalRAnimationClipNotifier>();
```

---

## Testing Requirements

### Unit Tests
- `AnimationCostService`: Calculate correct costs per backend
- `AnimationJobHandler`: Mark clips as Ready, publish events

### Integration Tests
- `GET /animation/estimate` — returns correct cost breakdown
- `POST /animation` — creates job, enqueues Hangfire, returns 202
- `GET /animation` — lists clips with status
- `GET /animation/clips/{id}` — returns signed URL
- SignalR: `ClipReady` event broadcasts to team group

### End-to-End Tests
1. Approve animation animation  
2. Hangfire job processes
3. Clips marked Ready + SignalR broadcasts
4. Frontend receives ClipReady events

---

## Acceptance Criteria

- [ ] All 4 animation controller routes work
- [ ] Cost estimation accurate per backend + shot count
- [ ] SignalR broadcasts ClipReady with signed URL (60s TTL)
- [ ] Hangfire job handles animation processing
- [ ] Animation clips load and display correctly
- [ ] Concurrency: RowVersion prevents conflicts
- [ ] Soft deletes respected
- [ ] All tests pass
