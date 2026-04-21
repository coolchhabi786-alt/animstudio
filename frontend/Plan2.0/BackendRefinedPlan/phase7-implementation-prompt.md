# Phase 7: Voice Studio — Implementation Prompt

## Objective
Complete Voice Studio services: TTS preview generation, batch voice assignment updates, and voice cloning stub.

---

## Current State
✅ **Done**:
- `VoiceAssignment` entity exists
- Basic controller likely exists

❌ **Missing**:
- `VoicePreviewService` (Azure OpenAI TTS)
- `VoiceCloneService` (stub + tier gate)
- `VoiceBatchUpdateService` (upsert logic)
- Repositories
- SignalR is NOT needed (synchronous operation)

---

## Implementation Checklist

### 1. Database Schema Verification
**File**: `AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs`

Verify:
```csharp
modelBuilder.Entity<VoiceAssignment>(b =>
{
    b.ToTable("VoiceAssignments", "content");
    b.HasKey(x => x.Id);
    b.HasIndex(x => new { x.EpisodeId, x.CharacterId }).IsUnique();
    b.HasQueryFilter(x => !x.IsDeleted);
    b.Property(x => x.RowVersion).IsRowVersion();
});
```

### 2. Enums and Value Objects
**File**: `AnimStudio.ContentModule/Domain/Enums/BuiltInVoice.cs`

```csharp
public enum BuiltInVoice
{
    Alloy = 0,    // Neutral, warm
    Echo = 1,     // Deep, resonant
    Fable = 2,    // Warm, friendly
    Onyx = 3,     // Deep, dark
    Nova = 4,     // Bright, energetic
    Shimmer = 5   // High, ethereal
}
```

### 3. Repository Interface
**File**: `AnimStudio.ContentModule/Application/Interfaces/IVoiceAssignmentRepository.cs`

```csharp
public interface IVoiceAssignmentRepository
{
    Task<List<VoiceAssignment>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default);
    Task<VoiceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VoiceAssignment?> GetByEpisodeAndCharacterAsync(Guid episodeId, Guid characterId, CancellationToken ct = default);
    Task AddAsync(VoiceAssignment assignment, CancellationToken ct = default);
    Task UpdateAsync(VoiceAssignment assignment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### 4. Repository Implementation
**File**: `AnimStudio.ContentModule/Infrastructure/Repositories/VoiceAssignmentRepository.cs`

```csharp
public class VoiceAssignmentRepository : IVoiceAssignmentRepository
{
    private readonly ContentDbContext _dbContext;
    
    public VoiceAssignmentRepository(ContentDbContext dbContext) => _dbContext = dbContext;
    
    public async Task<List<VoiceAssignment>> GetByEpisodeAsync(Guid episodeId, CancellationToken ct = default)
    {
        return await _dbContext.VoiceAssignments
            .Where(x => x.EpisodeId == episodeId && !x.IsDeleted)
            .OrderBy(x => x.CharacterId)
            .ToListAsync(cancellationToken: ct);
    }
    
    public async Task<VoiceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.VoiceAssignments
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken: ct);
    }
    
    public async Task<VoiceAssignment?> GetByEpisodeAndCharacterAsync(Guid episodeId, Guid characterId, CancellationToken ct = default)
    {
        return await _dbContext.VoiceAssignments
            .FirstOrDefaultAsync(x => x.EpisodeId == episodeId && x.CharacterId == characterId && !x.IsDeleted, cancellationToken: ct);
    }
    
    public async Task AddAsync(VoiceAssignment assignment, CancellationToken ct = default)
    {
        await _dbContext.VoiceAssignments.AddAsync(assignment, cancellationToken: ct);
    }
    
    public async Task UpdateAsync(VoiceAssignment assignment, CancellationToken ct = default)
    {
        _dbContext.VoiceAssignments.Update(assignment);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await GetByIdAsync(id, ct);
        if (assignment != null)
        {
            _dbContext.VoiceAssignments.Remove(assignment);
        }
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}
```

### 5. Voice Preview Service (Key Service)
**File**: `AnimStudio.API/Services/VoicePreviewService.cs`

```csharp
public interface IVoicePreviewService
{
    Task<Result<string>> GetPreviewUrlAsync(string text, string voiceName, CancellationToken ct = default);
}

public class VoicePreviewService : IVoicePreviewService
{
    private readonly Azure.AI.OpenAI.OpenAIClient _openAiClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<VoicePreviewService> _logger;
    
    public VoicePreviewService(
        Azure.AI.OpenAI.OpenAIClient openAiClient,
        BlobContainerClient blobContainerClient,
        ILogger<VoicePreviewService> logger)
    {
        _openAiClient = openAiClient;
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }
    
    public async Task<Result<string>> GetPreviewUrlAsync(string text, string voiceName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 5000)
            return Result<string>.Failure("INVALID_TEXT", "Text must be 1-5000 characters");
        
        if (!IsValidVoiceName(voiceName))
            return Result<string>.Failure("INVALID_VOICE", $"Voice '{voiceName}' not supported");
        
        try
        {
            // Call Azure OpenAI TTS API
            var speechSynthesizationOptions = new SpeechSynthesisOptions
            {
                VoiceName = voiceName,  // e.g., "en-US-AvaNeural"
                InputText = text,
            };
            
            var response = await _openAiClient.GetSpeechSynthesizationAsync(speechSynthesizationOptions, ct);
            
            // Read audio bytes
            using var memoryStream = new MemoryStream();
            response.Value.ContentStream.CopyTo(memoryStream);
            var audioBytes = memoryStream.ToArray();
            
            // Upload to tmp-tts Blob container
            var blobName = $"{Guid.NewGuid()}.mp3";
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(new BinaryData(audioBytes), overwrite: true, cancellationToken: ct);
            
            // Generate SAS URL with 60-second expiry
            var sasUri = _blobContainerClient.GenerateSasUri(
                BlobSasPermissions.Read,
                DateTimeOffset.UtcNow.AddSeconds(60));
            
            var fullUrl = new UriBuilder(sasUri)
            {
                Path = _blobContainerClient.Uri.AbsolutePath + "/" + blobName
            }.Uri.ToString();
            
            _logger.LogInformation("TTS preview generated for voice {Voice}: {Url}", voiceName, blobName);
            
            return Result<string>.Success(fullUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating TTS preview");
            return Result<string>.Failure("TTS_ERROR", "Failed to generate preview");
        }
    }
    
    private static bool IsValidVoiceName(string voiceName)
    {
        return voiceName switch
        {
            "Alloy" or "Echo" or "Fable" or "Onyx" or "Nova" or "Shimmer" => true,
            _ => false
        };
    }
}
```

### 6. Voice Batch Update Service
**File**: `AnimStudio.ContentModule/Application/Services/VoiceBatchUpdateService.cs`

```csharp
public interface IVoiceBatchUpdateService
{
    Task<Result> BatchUpdateAssignmentsAsync(
        Guid episodeId,
        List<VoiceAssignmentDto> assignments,
        CancellationToken ct = default);
}

public class VoiceBatchUpdateService : IVoiceBatchUpdateService
{
    private readonly IVoiceAssignmentRepository _repository;
    private readonly ILogger<VoiceBatchUpdateService> _logger;
    
    public VoiceBatchUpdateService(
        IVoiceAssignmentRepository repository,
        ILogger<VoiceBatchUpdateService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<Result> BatchUpdateAssignmentsAsync(
        Guid episodeId,
        List<VoiceAssignmentDto> assignments,
        CancellationToken ct = default)
    {
        try
        {
            // Get existing assignments
            var existing = await _repository.GetByEpisodeAsync(episodeId, ct);
            
            // Identify which to delete (in existing but not in new list)
            var incomingCharacterIds = assignments.Select(x => x.CharacterId).ToHashSet();
            var toDelete = existing.Where(x => !incomingCharacterIds.Contains(x.CharacterId)).ToList();
            
            // Soft-delete orphaned assignments
            foreach (var assignment in toDelete)
            {
                await _repository.DeleteAsync(assignment.Id, ct);
            }
            
            // Upsert incoming assignments
            foreach (var dto in assignments)
            {
                var existing_assignment = await _repository.GetByEpisodeAndCharacterAsync(episodeId, dto.CharacterId, ct);
                
                if (existing_assignment is null)
                {
                    // Create new
                    var assignment = new VoiceAssignment
                    {
                        Id = Guid.NewGuid(),
                        EpisodeId = episodeId,
                        CharacterId = dto.CharacterId,
                        VoiceName = dto.VoiceName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _repository.AddAsync(assignment, ct);
                }
                else
                {
                    // Update existing
                    existing_assignment.VoiceName = dto.VoiceName;
                    existing_assignment.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(existing_assignment, ct);
                }
            }
            
            // Commit all changes
            await _repository.SaveChangesAsync(ct);
            
            _logger.LogInformation("Batch updated {Count} voice assignments for episode {EpisodeId}", assignments.Count, episodeId);
            
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error during batch update");
            return Result.Failure("CONCURRENCY", "Another user updated voices; please refresh and try again");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch updating voice assignments");
            return Result.Failure("UPDATE_ERROR", "Failed to update voice assignments");
        }
    }
}
```

### 7. Voice Clone Service (Stub)
**File**: `AnimStudio.ContentModule/Application/Services/VoiceCloneService.cs`

```csharp
public interface IVoiceCloneService
{
    Task<Result<string>> CloneVoiceAsync(Guid userId, string cloneName, byte[] audioData, CancellationToken ct = default);
}

public class VoiceCloneService : IVoiceCloneService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<VoiceCloneService> _logger;
    
    public VoiceCloneService(ICurrentUserService currentUserService, ILogger<VoiceCloneService> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    public async Task<Result<string>> CloneVoiceAsync(Guid userId, string cloneName, byte[] audioData, CancellationToken ct = default)
    {
        // Check user tier
        var userTier = await _currentUserService.GetUserTierAsync(userId, ct);
        if (userTier != SubscriptionTier.Studio)
        {
            _logger.LogWarning("Non-Studio user {UserId} attempted voice cloning", userId);
            return Result<string>.Failure("CLONE_NOT_AVAILABLE", "Voice cloning is only available for Studio tier subscribers");
        }
        
        // Validate audio
        if (audioData is null || audioData.Length == 0 || audioData.Length > 10_000_000) // 10MB max
        {
            return Result<string>.Failure("INVALID_AUDIO", "Audio must be 1-10MB");
        }
        
        _logger.LogInformation("Voice clone requested by Studio user {UserId}: {CloneName}", userId, cloneName);
        
        // TODO: Integrate with ElevenLabs or Azure Custom Neural Voice
        // For now, return not implemented
        return Result<string>.Failure("NOT_IMPLEMENTED", "Voice cloning will be available in Q3 2026");
    }
}
```

### 8. DTOs
**File**: `AnimStudio.ContentModule/Application/DTOs/VoiceDtos.cs`

```csharp
public class VoiceAssignmentDto
{
    public Guid CharacterId { get; set; }
    public string VoiceName { get; set; } = string.Empty;  // "Alloy", "Echo", etc.
}

public class VoiceAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid EpisodeId { get; set; }
    public Guid CharacterId { get; set; }
    public string VoiceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class VoicePreviewRequest
{
    public string Text { get; set; } = string.Empty;
    public string VoiceName { get; set; } = string.Empty;
}

public class VoicePreviewResponse
{
    public string PreviewUrl { get; set; } = string.Empty;
    public int ExpirySeconds { get; set; } = 60;
}

public class BatchUpdateVoicesRequest
{
    public List<VoiceAssignmentDto> Assignments { get; set; } = new();
}

public class VoiceCloneRequest
{
    public string CloneName { get; set; } = string.Empty;
    // File upload via form-data
}
```

### 9. Controller Routes
**File**: `AnimStudio.API/Controllers/VoiceController.cs`

```csharp
[HttpGet("api/v{version:apiVersion}/episodes/{id:guid}/voices")]
public async Task<IActionResult> GetVoiceAssignments(Guid id, CancellationToken ct)
{
    var result = await _mediator.Send(new GetVoiceAssignmentsQuery(id), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound();
}

[HttpPut("api/v{version:apiVersion}/episodes/{id:guid}/voices")]
public async Task<IActionResult> BatchUpdateVoices(
    Guid id,
    [FromBody] BatchUpdateVoicesRequest req,
    CancellationToken ct)
{
    var result = await _mediator.Send(new BatchUpdateVoicesCommand(id, req.Assignments), ct);
    return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
}

[HttpPost("api/v{version:apiVersion}/voices/preview")]
[ProducesResponseType(typeof(VoicePreviewResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> GetVoicePreview(
    [FromBody] VoicePreviewRequest req,
    CancellationToken ct)
{
    var result = await _mediator.Send(new GetVoicePreviewQuery(req.Text, req.VoiceName), ct);
    
    if (!result.IsSuccess)
        return BadRequest(new { error = result.Error });
    
    return Ok(new VoicePreviewResponse { PreviewUrl = result.Value });
}

[HttpPost("api/v{version:apiVersion}/voices/clone")]
[Authorize(Policy = "RequireStudioTier")]
public async Task<IActionResult> CloneVoice(
    [FromForm] string cloneName,
    [FromForm] IFormFile audioFile,
    CancellationToken ct)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(userId, out var userGuid))
        return Unauthorized();
    
    var audioBytes = new byte[audioFile.Length];
    using (var stream = audioFile.OpenReadStream())
        await stream.ReadAsync(audioBytes, ct);
    
    var result = await _mediator.Send(new CloneVoiceCommand(userGuid, cloneName, audioBytes), ct);
    
    return result.IsSuccess
        ? Ok(new { cloneId = result.Value })
        : BadRequest(new { error = result.Error });
}
```

### 10. MediatR Commands/Queries

Pattern (similar to Phase 6):
```csharp
public record GetVoiceAssignmentsQuery(Guid EpisodeId) : IRequest<Result<List<VoiceAssignmentResponseDto>>>;
public record BatchUpdateVoicesCommand(Guid EpisodeId, List<VoiceAssignmentDto> Assignments) : IRequest<Result>;
public record GetVoicePreviewQuery(string Text, string VoiceName) : IRequest<Result<string>>;
public record CloneVoiceCommand(Guid UserId, string CloneName, byte[] AudioData) : IRequest<Result<string>>;
```

### 11. Module Registration
**File**: `AnimStudio.ContentModule/ContentModuleRegistration.cs`

```csharp
services.AddScoped<IVoiceAssignmentRepository, VoiceAssignmentRepository>();
services.AddScoped<IVoiceBatchUpdateService, VoiceBatchUpdateService>();
services.AddScoped<IVoiceCloneService, VoiceCloneService>();

// Add to API Services
services.AddScoped<IVoicePreviewService, VoicePreviewService>();
```

---

## Infrastructure Setup

**Azure Blob Storage**:
- Container: `tmp-tts` with lifecycle policy:
  - Delete blobs after 1 hour
  - Keep only last 100 items (optional limit)

**Azure KeyVault**:
- Store Azure OpenAI endpoint and key

---

## Testing Requirements

### Unit Tests
- `VoicePreviewService`: Generate URL for valid voice + text, reject invalid inputs
- `VoiceBatchUpdateService`: Upsert existing + new, soft-delete orphans
- `VoiceCloneService`: Reject non-Studio users

### Integration Tests
- `GET /voices` — returns all assignments for episode
- `PUT /voices` — batch updates persist correctly
- `POST /preview` — returns valid SAS URL with 60s expiry
- `POST /clone` — returns 403 for non-Studio users

---

## Acceptance Criteria

- [ ] Voice assignments load and display correctly
- [ ] Batch update upserts + soft-deletes orphaned assignments
- [ ] TTS preview Returns valid SAS URL with 60-second TTL
- [ ] Voice clone returns tier gate error for non-Studio
- [ ] No N+1 queries
- [ ] Soft deletes respected
- [ ] All tests pass
