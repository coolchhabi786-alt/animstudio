# Phase 6: Storyboard Studio — Implementation Prompt

## Objective
Complete the Storyboard Studio services and API routes. Entities already exist. You need to implement the business logic services, SignalR notifier, and verify controller routes.

---

## Current State
✅ **Done**:
- `Storyboard` and `StoryboardShot` domain entities in ContentModule
- Basic controller routes (likely incomplete)
- Query filters for soft deletes

❌ **Missing**:
- `StoryboardService` (orchestration)
- `SignalRStoryboardNotifier` (real-time events)
- Repository implementations
- Query services
- Migration (if incomplete)

---

## Implementation Checklist

### 1. Database Verification
**File**: `AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs`

Verify these configurations exist:
```csharp
modelBuilder.Entity<Storyboard>(b =>
{
    b.ToTable("Storyboards", "content");
    b.HasKey(x => x.Id);
    b.HasIndex(x => x.EpisodeId).IsUnique();  // One per episode
    b.HasQueryFilter(x => !x.IsDeleted);
    b.Property(x => x.RowVersion).IsRowVersion();
    b.HasMany(x => x.Shots).WithOne().HasForeignKey("StoryboardId").OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<StoryboardShot>(b =>
{
    b.ToTable("StoryboardShots", "content");
    b.HasKey(x => x.Id);
    b.HasIndex(x => new { x.StoryboardId, x.SceneNumber, x.ShotIndex }).IsUnique();
    b.HasQueryFilter(x => !x.IsDeleted);
    b.Property(x => x.RowVersion).IsRowVersion();
    b.HasOne<StoryboardShot>().WithMany().HasForeignKey("StoryboardId").OnDelete(DeleteBehavior.Cascade);
});
```

If missing, create migration: `Add_Phase6_Storyboard_Tables`

### 2. Domain Events (Verify Exist)
**File**: `AnimStudio.ContentModule/Domain/Events/StoryboardDomainEvents.cs`

These events should exist:
```csharp
public record StoryboardCreatedEvent(Guid StoryboardId, Guid EpisodeId, int ShotCount) : IDomainEvent;
public record ShotUpdatedEvent(Guid ShotId, Guid StoryboardId, Guid EpisodeId, string ImageUrl, string? StyleOverride) : IDomainEvent;
public record ShotRegenerationRequestedEvent(Guid ShotId, Guid EpisodeId, int RegenerationCount, string? NewStyle) : IDomainEvent;
```

### 3. Repository Interface
**File**: `AnimStudio.ContentModule/Application/Interfaces/IStoryboardRepository.cs`

```csharp
public interface IStoryboardRepository
{
    Task<Storyboard?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default);
    Task<StoryboardShot?> GetShotByIdAsync(Guid shotId, CancellationToken ct = default);
    Task<List<StoryboardShot>> GetShotsByStoryboardAsync(Guid storyboardId, CancellationToken ct = default);
    Task AddAsync(Storyboard storyboard, CancellationToken ct = default);
    Task UpdateAsync(Storyboard storyboard, CancellationToken ct = default);
    Task UpdateShotAsync(StoryboardShot shot, CancellationToken ct = default);
}
```

### 4. Repository Implementation
**File**: `AnimStudio.ContentModule/Infrastructure/Repositories/StoryboardRepository.cs`

```csharp
public class StoryboardRepository : IStoryboardRepository
{
    private readonly ContentDbContext _dbContext;
    
    public StoryboardRepository(ContentDbContext dbContext) => _dbContext = dbContext;
    
    public async Task<Storyboard?> GetByEpisodeIdAsync(Guid episodeId, CancellationToken ct = default)
    {
        return await _dbContext.Storyboards
            .Include(x => x.Shots)
            .FirstOrDefaultAsync(x => x.EpisodeId == episodeId && !x.IsDeleted, cancellationToken: ct);
    }
    
    public async Task<StoryboardShot?> GetShotByIdAsync(Guid shotId, CancellationToken ct = default)
    {
        return await _dbContext.StoryboardShots
            .FirstOrDefaultAsync(x => x.Id == shotId && !x.IsDeleted, cancellationToken: ct);
    }
    
    public async Task<List<StoryboardShot>> GetShotsByStoryboardAsync(Guid storyboardId, CancellationToken ct = default)
    {
        return await _dbContext.StoryboardShots
            .Where(x => x.StoryboardId == storyboardId && !x.IsDeleted)
            .OrderBy(x => x.SceneNumber)
            .ThenBy(x => x.ShotIndex)
            .ToListAsync(cancellationToken: ct);
    }
    
    public async Task AddAsync(Storyboard storyboard, CancellationToken ct = default)
    {
        await _dbContext.Storyboards.AddAsync(storyboard, cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task UpdateAsync(Storyboard storyboard, CancellationToken ct = default)
    {
        _dbContext.Storyboards.Update(storyboard);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task UpdateShotAsync(StoryboardShot shot, CancellationToken ct = default)
    {
        _dbContext.StoryboardShots.Update(shot);
        await _dbContext.SaveChangesAsync(ct);
    }
}
```

### 5. Application Service
**File**: `AnimStudio.ContentModule/Application/Services/StoryboardService.cs`

```csharp
public interface IStoryboardService
{
    Task<Result<StoryboardDto>> GetStoryboardAsync(Guid episodeId, CancellationToken ct = default);
    Task<Result> ApplyStyleOverrideAsync(Guid shotId, string newStyle, CancellationToken ct = default);
    Task<Result> RegenerateShotAsync(Guid shotId, string? newStyle, CancellationToken ct = default);
}

public class StoryboardService : IStoryboardService
{
    private readonly IStoryboardRepository _repository;
    private readonly IJobEnqueuer _jobEnqueuer;
    private readonly ILogger<StoryboardService> _logger;
    
    private const int MaxRegenerations = 3;
    
    public StoryboardService(
        IStoryboardRepository repository,
        IJobEnqueuer jobEnqueuer,
        ILogger<StoryboardService> logger)
    {
        _repository = repository;
        _jobEnqueuer = jobEnqueuer;
        _logger = logger;
    }
    
    public async Task<Result<StoryboardDto>> GetStoryboardAsync(Guid episodeId, CancellationToken ct = default)
    {
        var storyboard = await _repository.GetByEpisodeIdAsync(episodeId, ct);
        if (storyboard is null)
            return Result<StoryboardDto>.Failure("STORYBOARD_NOT_FOUND", "Storyboard not found");
        
        var shots = await _repository.GetShotsByStoryboardAsync(storyboard.Id, ct);
        var dto = new StoryboardDto
        {
            Id = storyboard.Id,
            EpisodeId = storyboard.EpisodeId,
            Shots = shots.Select(s => new StoryboardShotDto
            {
                Id = s.Id,
                SceneNumber = s.SceneNumber,
                ShotIndex = s.ShotIndex,
                ImageUrl = s.ImageUrl,
                StyleOverride = s.StyleOverride,
                RegenerationCount = s.RegenerationCount,
                CreatedAt = s.CreatedAt
            }).ToList()
        };
        
        return Result<StoryboardDto>.Success(dto);
    }
    
    public async Task<Result> ApplyStyleOverrideAsync(Guid shotId, string newStyle, CancellationToken ct = default)
    {
        var shot = await _repository.GetShotByIdAsync(shotId, ct);
        if (shot is null)
            return Result.Failure("SHOT_NOT_FOUND", "Shot not found");
        
        shot.StyleOverride = newStyle;
        shot.RegenerationCount++;
        
        await _repository.UpdateShotAsync(shot, ct);
        
        // Publish event for SignalR broadcast
        var evt = new ShotUpdatedEvent(shot.Id, shot.StoryboardId, shot.EpisodeId, shot.ImageUrl, newStyle);
        // Handler will publish via MediatR
        
        _logger.LogInformation("Style override applied to shot {ShotId}: {Style}", shotId, newStyle);
        
        return Result.Success();
    }
    
    public async Task<Result> RegenerateShotAsync(Guid shotId, string? newStyle, CancellationToken ct = default)
    {
        var shot = await _repository.GetShotByIdAsync(shotId, ct);
        if (shot is null)
            return Result.Failure("SHOT_NOT_FOUND", "Shot not found");
        
        if (shot.RegenerationCount >= MaxRegenerations)
        {
            _logger.LogWarning("Shot {ShotId} has reached max regenerations ({Max})", shotId, MaxRegenerations);
            // Return warning but allow (cost will apply)
        }
        
        shot.RegenerationCount++;
        if (!string.IsNullOrEmpty(newStyle))
            shot.StyleOverride = newStyle;
        
        await _repository.UpdateShotAsync(shot, ct);
        
        // Enqueue regeneration job to Service Bus
        await _jobEnqueuer.EnqueueStoryboardRegenerationAsync(
            new StoryboardRegenerationJobMessage
            {
                CorrelationId = Guid.NewGuid().ToString(),
                EpisodeId = shot.EpisodeId,
                ShotId = shot.Id,
                Style = newStyle
            },
            ct);
        
        _logger.LogInformation("Storyboard regeneration enqueued for shot {ShotId}", shotId);
        
        return Result.Success();
    }
}
```

### 6. DTOs
**File**: `AnimStudio.ContentModule/Application/DTOs/StoryboardDtos.cs`

```csharp
public class StoryboardDto
{
    public Guid Id { get; set; }
    public Guid EpisodeId { get; set; }
    public List<StoryboardShotDto> Shots { get; set; } = new();
}

public class StoryboardShotDto
{
    public Guid Id { get; set; }
    public int SceneNumber { get; set; }
    public int ShotIndex { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? StyleOverride { get; set; }
    public int RegenerationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApplyStyleRequest
{
    public string Style { get; set; } = string.Empty;  // Cartoon, Anime, etc.
}

public class RegenerateShotRequest
{
    public string? NewStyle { get; set; }
}
```

### 7. SignalR Notifier
**File**: `AnimStudio.ContentModule/Application/EventHandlers/SignalRStoryboardNotifier.cs`

```csharp
public class SignalRStoryboardNotifier : INotificationHandler<ShotUpdatedEvent>
{
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly ILogger<SignalRStoryboardNotifier> _logger;
    
    public SignalRStoryboardNotifier(IHubContext<ProgressHub> hubContext, ILogger<SignalRStoryboardNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task Handle(ShotUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                shotId = notification.ShotId,
                storyboardId = notification.StoryboardId,
                episodeId = notification.EpisodeId,
                imageUrl = notification.ImageUrl,
                styleOverride = notification.StyleOverride
            };
            
            // Get team from episode context (you'll need IEpisodeRepository or context)
            var groupName = $"team:{notification.EpisodeId}";  // Placeholder
            
            await _hubContext.Clients.Group(groupName).SendAsync(
                "ShotUpdated",
                payload,
                cancellationToken);
            
            _logger.LogInformation("ShotUpdated broadcast to {Group}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting ShotUpdated");
        }
    }
}
```

### 8. Controller Routes
**File**: `AnimStudio.API/Controllers/StoryboardController.cs`

Verify these routes exist and work:
```csharp
[HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/storyboard")]
public async Task<IActionResult> GetStoryboard(Guid id, CancellationToken ct)
{
    var result = await _mediator.Send(new GetStoryboardQuery(id), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
}

[HttpPut("api/v{version:apiVersion}/storyboard/shots/{shotId:guid}/style")]
public async Task<IActionResult> ApplyStyle(Guid shotId, [FromBody] ApplyStyleRequest req, CancellationToken ct)
{
    var result = await _mediator.Send(new ApplyStyleOverrideCommand(shotId, req.Style), ct);
    return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
}

[HttpPost("api/v{version:apiVersion}/storyboard/shots/{shotId:guid}/regenerate")]
public async Task<IActionResult> RegenerateShot(Guid shotId, [FromBody] RegenerateShotRequest req, CancellationToken ct)
{
    var result = await _mediator.Send(new RegenerateShotCommand(shotId, req.NewStyle), ct);
    return result.IsSuccess ? Accepted() : BadRequest(new { error = result.Error });
}
```

### 9. MediatR Commands/Queries
**Files**:
- `AnimStudio.ContentModule/Application/Commands/ApplyStyleOverride/ApplyStyleOverrideCommand.cs`
- `AnimStudio.ContentModule/Application/Commands/RegenerateShot/RegenerateShotCommand.cs`
- `AnimStudio.ContentModule/Application/Queries/GetStoryboard/GetStoryboardQuery.cs`

Pattern:
```csharp
public record GetStoryboardQuery(Guid EpisodeId) : IRequest<Result<StoryboardDto>>;

public class GetStoryboardQueryHandler : IRequestHandler<GetStoryboardQuery, Result<StoryboardDto>>
{
    private readonly IStoryboardService _service;
    
    public GetStoryboardQueryHandler(IStoryboardService service) => _service = service;
    
    public async Task<Result<StoryboardDto>> Handle(GetStoryboardQuery request, CancellationToken ct)
        => await _service.GetStoryboardAsync(request.EpisodeId, ct);
}
```

### 10. Module Registration
**File**: `AnimStudio.ContentModule/ContentModuleRegistration.cs`

Add to `RegisterServices`:
```csharp
services.AddScoped<IStoryboardRepository, StoryboardRepository>();
services.AddScoped<IStoryboardService, StoryboardService>();
services.AddScoped<INotificationHandler<ShotUpdatedEvent>, SignalRStoryboardNotifier>();
```

---

## Testing Requirements

### Unit Tests
- `StoryboardService.GetStoryboardAsync()` — returns correct DTO with shots ordered by scene/shot index
- `StoryboardService.ApplyStyleOverrideAsync()` — updates shot style + increments regen count
- `StoryboardService.RegenerateShotAsync()` — respects max regen limit, enqueues job

### Integration Tests
- `GET /storyboard` — returns full storyboard with all shots
- `PUT /storyboard/shots/{id}/style` — persists style override, broadcasts SignalR event
- `POST /storyboard/shots/{id}/regenerate` — enqueues job, returns 202

### Manual Testing
1. Create episode with script
2. Manually insert Storyboard + StoryboardShot rows (or trigger from Python)
3. Call `GET /storyboard` — verify shots load correctly
4. Call `PUT /style` — verify shot updates + SignalR event fires
5. Call `POST /regenerate` — verify job enqueued + 202 returned

---

## API Contract (OpenAPI)

**GET** `/api/v1/episodes/{id}/storyboard`
- Response: `StoryboardDto`
- Status: 200 OK, 404 Not Found

**PUT** `/api/v1/storyboard/shots/{shotId}/style`
- Body: `{ "style": "Cartoon" }`
- Status: 200 OK, 400 Bad Request

**POST** `/api/v1/storyboard/shots/{shotId}/regenerate`
- Body: `{ "newStyle": null }`
- Status: 202 Accepted, 400 Bad Request

---

## Acceptance Criteria

- [ ] Storyboard + shots load from database correctly
- [ ] Style override persists and increments regen count
- [ ] Regeneration respects max limit (3)
- [ ] SignalR broadcasts ShotUpdated on style change
- [ ] Service Bus message enqueued on regeneration
- [ ] All tests pass (unit + integration)
- [ ] No query N+1 issues (use query filters, includes)
- [ ] Soft deletes respected on all queries
