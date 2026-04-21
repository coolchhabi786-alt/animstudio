# Phases 10-12: Implementation Prompts Summary

Due to length constraints, these are comprehensive outlines for phases 10-12. Full implementation follows the established pattern from phases 6-9.

---

# Phase 10: Timeline Editor — Key Implementation Points

## Scope
Timeline editing with shot reordering, trimming, transitions, music, and text overlays. Most complex phase due to FFmpeg filter graph generation.

## Database Schema (4 new tables)

```csharp
// TimelineTrack — container for clips (Video, Audio, Music, Text lanes)
public sealed class TimelineTrack : AggregateRoot<Guid>, ISoftDelete
{
    public Guid EpisodeId { get; set; }
    public TrackType Type { get; set; }                   // Video, Audio, Music, Text
    public int SortOrder { get; set; }
    public ICollection<TimelineClip> Clips { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}

// TimelineClip — individual clip on a track (may be trimmed, have transition)
public sealed class TimelineClip : Entity<Guid>
{
    public Guid TrackId { get; set; }
    public Guid SourceId { get; set; }                      // AnimationClip or AudioFile ID
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public long TrimStartMs { get; set; }                   // How much trimmed from source start
    public long TrimEndMs { get; set; }                     // How much trimmed from source end
    public TransitionType TransitionIn { get; set; }        // Cut, Fade, Dissolve
    public long TransitionDurationMs { get; set; }          // Fade/dissolve length
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
}

// MusicTrack — stock or custom music (separate from timeline tracks)
public sealed class MusicTrack : AggregateRoot<Guid>, ISoftDelete
{
    public Guid TeamId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }                         // Blob path or CDN URL
    public double DurationSeconds { get; set; }
    public bool IsStock { get; set; }                       // Stock library or team-uploaded
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}

// TextOverlay — title cards, subtitles, etc.
public sealed class TextOverlay : AggregateRoot<Guid>, ISoftDelete
{
    public Guid EpisodeId { get; set; }
    public string Text { get; set; }
    public int FontSize { get; set; }
    public string Color { get; set; }                       // Hex: #FFFFFF
    public int PositionX { get; set; }                      // Percentage 0-100
    public int PositionY { get; set; }
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public TextAnimation Animation { get; set; }            // FadeIn, SlideUp, None
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}

public enum TrackType { Video, Audio, Music, Text }
public enum TransitionType { Cut, Fade, Dissolve }
public enum TextAnimation { FadeIn, SlideUp, None }
```

## Key Services

### TimelineService — Business Logic
- CRUD timeline operations
- Validate clip ordering + timings
- Prevent overlapping video clips
- Handle concurrency via RowVersion

### TimelineRenderService — FFmpeg Integration (Hardest Part)
Converts:
```json
{
  "videoTrack": [
    {
      "clipId": "uuid",
      "sourceFile": "s3://animation-clips/clip-1.mp4",
      "startMs": 0,
      "durationMs": 5000,
      "transitionIn": "Fade",
      "transitionDurationMs": 500
    }
  ],
  "audioTrack": [...],
  "musicTrack": [...],
  "textOverlays": [...]
}
```

To FFmpeg complex filter graph:
```bash
ffmpeg \
  -i video1.mp4 -i video2.mp4 -i audio1.mp3 -i music.mp3 \
  -filter_complex "[0:v]fade=t=out:st=4:d=1[v1];
                   [1:v]fade=t=in:st=0:d=1[v2];
                   [v1][v2]concat=n=2:v=1:a=0[out_v];
                   [0:a][1:a]acrossfade=d=1[out_a];
                   [out_v]drawtext=text='My Title':..." \
  -map "[out_v]" -map "[out_a]" output.mp4
```

### MusicService — Stock + Upload
- Seed 10 stock tracks into MusicTrack table
- Handle custom upload (Blob storage)
- Return list with metadata

### TextOverlayService — CRUD
- Create overlays with position, animation
- Update text, timing, styling
- Delete overlays

## API Endpoints

```csharp
// Timeline CRUD
[HttpGet("api/v1/episodes/{id}/timeline")]              // Full timeline
[HttpPut("api/v1/episodes/{id}/timeline")]              // Save entire timeline
[HttpPost("api/v1/episodes/{id}/timeline/preview")]     // Queue low-res preview

// Music Management
[HttpGet("api/v1/music/stock")]                         // List stock tracks
[HttpPost("api/v1/music/upload")]                       // Upload custom track

// Text Overlays CRUD
[HttpPost("api/v1/text-overlays")]
[HttpPatch("api/v1/text-overlays/{id}")]
[HttpDelete("api/v1/text-overlays/{id}")]
```

## Challenge Areas
1. **FFmpeg Filter Graph Generation** — Complex string building for transitions, audio mixing, text overlays
2. **Concurrency** — Multiple users editing timeline simultaneously (use RowVersion)
3. **Clip Duration Calculation** — Account for trim + transitions in timeline math
4. **Music Auto-Duck** — Reduce music volume when dialogue present (optional Phase 2 feature)

---

# Phase 11: Sharing & Review Links — Key Points

## Database Schema (4 new tables)

```csharp
public sealed class ReviewLink : AggregateRoot<Guid>, ISoftDelete
{
    public Guid RenderId { get; set; }
    public string Token { get; set; }                       // Unique, URL-safe
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? PasswordHash { get; set; }               // PBKDF2
    public Guid CreatedByUserId { get; set; }
    public ICollection<ReviewComment> Comments { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class ReviewComment : Entity<Guid>
{
    public Guid ReviewLinkId { get; set; }
    public string AuthorName { get; set; }
    public string Text { get; set; }
    public double TimestampSeconds { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; }
    public byte[] RowVersion { get; set; }
}

public sealed class BrandKit : AggregateRoot<Guid>, ISoftDelete
{
    public Guid TeamId { get; set; }
    public string? LogoUrl { get; set; }                     // Blob path
    public string PrimaryColor { get; set; }                // #RRGGBB
    public string SecondaryColor { get; set; }
    public WatermarkPosition WatermarkPosition { get; set; }
    public float WatermarkOpacity { get; set; }             // 0.0-1.0
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class SocialPublish : AggregateRoot<Guid>, ISoftDelete
{
    public Guid RenderId { get; set; }
    public SocialPlatform Platform { get; set; }            // YouTube
    public string? ExternalVideoId { get; set; }
    public PublishStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}

public enum WatermarkPosition { BottomRight, BottomLeft, TopRight }
public enum SocialPlatform { YouTube }
public enum PublishStatus { Pending, Published, Failed }
```

## Key Services

### ReviewLinkService
- Create review link with expiry + optional password
- Validate token + check expiry/revocation
- Soft-delete (revoke) link

### BrandKitService
- CRUD brand kit per team
- Upload logo image to Blob
- Return with SAS URL

### YouTubePublishService (Complex)
1. Start OAuth flow → redirect to Google consent screen
2. Handle OAuth callback → exchange code for token
3. Upload video to YouTube via Data API v3
4. Store ExternalVideoId + PublishStatus

### WatermarkService
- Apply brand logo to FFmpeg render
- Draw logo at selected position with opacity
- Add primary/secondary colors (overlay)

## API Endpoints (3 Controllers)

```csharp
// ReviewController (public + auth routes)
[HttpPost("api/v1/renders/{id}/review-links")]          // Auth, tier gate (Pro+)
[HttpGet("api/v1/review/{token}")]                      // No auth (public)
[HttpGet("api/v1/review/{token}/comments")]             // No auth
[HttpPost("api/v1/review/{token}/comments")]            // No auth  
[HttpPatch("api/v1/review/{token}/comments/{id}/resolve")] // Creator only

// BrandKitController
[HttpGet("api/v1/teams/{id}/brand-kit")]                // Auth
[HttpPut("api/v1/teams/{id}/brand-kit")]
[HttpPost("api/v1/brand-kit/logo")]                     // File upload

// PublishController
[HttpPost("api/v1/renders/{id}/publish/youtube")]       // Studio tier, OAuth start
[HttpGet("api/v1/publish/youtube/callback")]            // OAuth redirect (no auth)
```

## Challenge Areas
1. **OAuth Flow** — State validation, PKCE, token refresh
2. **YouTube Data API** — Video upload protocol (resumable)
3. **Public Endpoints** — `/review/{token}/*` are unauthenticated (separate auth via token)
4. **Tier Gating** — Pro tier for review links, Studio for YouTube

---

# Phase 12: Analytics & Admin Dashboard — Key Points

## Database Schema (Update + 2 new tables)

```csharp
// New: VideoView — track view events
public sealed class VideoView : AggregateRoot<Guid>
{
    public Guid RenderId { get; set; }
    public string ViewerIpHash { get; set; }                // SHA256(IP)
    public ViewSource Source { get; set; }                  // Direct, Embed, Review
    public DateTime ViewedAt { get; set; }
    public DateTime CreatedAt { get; }
}

// New: Notification — job alerts, billing, invites
public sealed class Notification : AggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }              // JobComplete, BillingAlert, TeamInvite
    public string Title { get; set; }
    public string Body { get; set; }
    public bool IsRead { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }          // "Episode", "Render", etc.
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
}

// Update Subscription:
public decimal UsageEpisodesThisMonth { get; set; }
public DateTime UsageResetAt { get; set; }

public enum ViewSource { Direct, Embed, Review }
public enum NotificationType { JobComplete, BillingAlert, TeamInvite, SystemMessage }
```

## Key Services

### UsageMeteringService
- Increment UsageEpisodesThisMonth on job complete
- Enforce tier limits (Free 5, Pro 50, Studio unlimited)
- Send BillingAlert notifications at 80%, 100%
- Prevent over-quota job submission

### VideoViewTrackingService
- Record view events from CDN webhooks
- Hash IP addresses for privacy
- Aggregate by episode + source

### AnalyticsQueryService
- Views per episode (total + by source)
- Renders per episode (count + avg time)
- Exports per episode
- Team-level aggregates

### AdminStatsService
- DAU/MAU metrics
- Active subscriptions by tier
- Job queue depth + avg processing time
- Cost per episode + per tier
- Error rate per stage

## API Endpoints (4 Controllers)

```csharp
// AnalyticsController
[HttpGet("api/v1/episodes/{id}/analytics")]              // Auth
[HttpGet("api/v1/teams/{id}/analytics")]

// AdminController [AdminRole]
[HttpGet("api/v1/admin/stats")]
[HttpGet("api/v1/admin/users")]
[HttpGet("api/v1/admin/jobs")]

// NotificationController
[HttpGet("api/v1/notifications")]                        // Auth
[HttpPatch("api/v1/notifications/{id}/read")]
[HttpPatch("api/v1/notifications/read-all")]

// Webhook (IP whitelist, no auth)
[HttpPost("api/v1/webhooks/cdn-views")]
```

## Background Services

### UsageResetService
- Hosted background service
- Runs daily at subscription renewal time
- Resets UsageEpisodesThisMonth counter

### NotificationCleanupService (Optional)
- Delete read notifications after 30 days
- Archive old view events

## Challenge Areas
1. **Metering Logic** — Exact tier limit enforcement + alert timing
2. **View Tracking** — CDN webhook parsing, deduplication
3. **Performance** — Analytics queries at scale (indices, aggregation queries)
4. **Background Services** — Async job scheduling, distributed setup

---

## Common Patterns (All Phases)

### Module Structure
```
Module/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── Events/
│   └── Interfaces/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   ├── Services/
│   └── EventHandlers/
├── Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   └── Migrations/
└── ModuleRegistration.cs
```

### MediatR Pattern
- Commands → Database changes + events
- Queries → Read-only
- Handlers → Business logic
- Event handlers → Side effects (SignalR, notifications)

### Database Pattern
- Soft delete: `IsDeleted` with query filter
- Concurrency: `RowVersion` on update-heavy tables
- Audit: `CreatedAt`, `UpdatedAt`
- Performance: Strategic indices on FK + query columns

### SignalR Delivery
- Hub: `ProgressHub` at `/hubs/progress`
- Group: `team:{teamId}`
- Events: Typed payloads, 60-second TTL on URLs

---

## Testing Strategy (All Phases)

**Unit Tests**:
- Service methods in isolation
- Mock dependencies
- Business rule validation

**Integration Tests**:
- MediatR pipeline
- Database persistence
- Repository queries

**E2E Tests**:
- API endpoints
- SignalR broadcasting
- Frontend hooks

**Load Tests**:
- Concurrent edits (Phase 10)
- View tracking at scale (Phase 12)
