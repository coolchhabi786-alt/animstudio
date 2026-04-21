# Plan: Complete Backend Development for AnimStudio Phases 6-12

## TL;DR
Complete the .NET backend architecture to support the full animation pipeline (Phases 6-12). Structure follows the established modular pattern (DDD + MediatR). **Key Insight**: Phases 6-8 are partially done; Phases 9-12 need full implementation. Python pipeline orchestrates asset generation via Service Bus; .NET backend provides APIs, job tracking, and result persistence.

---

## Current State (From Codebase Analysis)

### ✅ Completed
| Phase | Module | What's Done | What's Missing |
|-------|--------|-------------|-----------------|
| 1-5 | IdentityModule, ContentModule (stub) | Domain entities, initial API | Business logic services |
| 6 | Storyboard | Entities done (Storyboard, StoryboardShot) | Services, SignalR notifier, controller routes |
| 7 | Voice | Entities done (VoiceAssignment) | TTS preview service, clone service, batch update |
| 8 | Animation | Controllers + entities done, cost logic | SignalR notifier, Hangfire integration |

### ❌ Missing (0% done)
| Phase | Domain | What's Needed |
|-------|--------|--------------|
| 9 | Render & Delivery | Render entity, CDN service, SRT generator, RenderController |
| 10 | Timeline Editor | 4 entities (Track, Clip, Music, TextOverlay), all services |
| 11 | Sharing & Reviews | 4 entities (ReviewLink, Comment, BrandKit, SocialPublish), OAuth |
| 12 | Analytics | VideoView entity, Notification entity, metering, admin dashboard |

---

## Implementation Plan: 12 Weeks Total

### **Phase 6: Storyboard Studio** (Complete) — 1 week

**Complete These**:
- [ ] `StoryboardService` — orchestrate shot generation, regeneration limits
- [ ] `SignalRStoryboardNotifier` — MediatR handler for ShotUpdated event
- [ ] `StoryboardQueryService` — load storyboard with styles
- [ ] API routes: `GET /storyboard`, `PUT /style`, `POST /regenerate`
- [ ] Add repositories + queries to ContentModule

---

### **Phase 7: Voice Studio** (Complete) — 1.5 weeks

**Complete These**:
- [ ] `VoicePreviewService` — Azure OpenAI TTS + Blob tmp-tts + SAS URL
- [ ] `VoiceCloneService` — stub (tier gate to Studio only)
- [ ] `VoiceBatchUpdateService` — upsert assignments, soft-delete orphans
- [ ] API routes: `GET /voices`, `PUT /voices`, `POST /preview`
- [ ] Repositories + batch update command

---

### **Phase 8: Animation Approval** (Complete) — 1 week

**Complete These**:
- [ ] `SignalRAnimationClipNotifier` — ClipReady event broadcaster
- [ ] Hangfire job handler (calls Python crew or local backend)
- [ ] Verify all 4 controller routes work
- [ ] SignalR contract + tests

---

### **Phase 9: Render & Delivery** (NEW) — 2-3 weeks

**Database**:
- [ ] `Render` aggregate (status, CDN URL, captions, aspect ratio)
- [ ] Update `Episode` with `LatestRenderId`

**Services**:
- [ ] `RenderService` — enqueue FFmpeg post-prod job
- [ ] `SrtGeneratorService` — DialogueLine timings → SRT
- [ ] `AspectRatioService` — enum → FFmpeg params
- [ ] `CdnService` — signed 30-day Blob URLs
- [ ] `SignalRRenderNotifier` — progress + complete events

**API**: `RenderController`
- [ ] `POST /render` → enqueue, 202 response
- [ ] `GET /renders` → history
- [ ] `GET /renders/{id}` → with signed CDN URL
- [ ] `GET /renders/{id}/srt` → download captions

---

### **Phase 10: Timeline Editor** (NEW) — 3-4 weeks

**Database**:
- [ ] `TimelineTrack` (video, audio, music, text lanes)
- [ ] `TimelineClip` (with trim, transition, sort order)
- [ ] `MusicTrack` (stock + custom)
- [ ] `TextOverlay` (position, animation, timing)

**Services**:
- [ ] `TimelineService` — CRUD, ordering, trim validation
- [ ] `TimelineRenderService` — timeline → FFmpeg filter graph
- [ ] `MusicService` — stock library + upload
- [ ] `TextOverlayService` — CRUD

**API**:
- [ ] `TimelineController`: `GET/PUT /timeline`
- [ ] `MusicController`: `GET /stock`, `POST /upload`
- [ ] `TextOverlayController`: CRUD routes

---

### **Phase 11: Sharing & Reviews** (NEW) — 3-4 weeks

**Database**:
- [ ] `ReviewLink` (token, expiry, password)
- [ ] `ReviewComment` (thread, timestamp)
- [ ] `BrandKit` (logo, colors, watermark)
- [ ] `SocialPublish` (YouTube integration)

**Services**:
- [ ] `ReviewLinkService` — create, validate, revoke
- [ ] `BrandKitService` — CRUD, logo upload
- [ ] `YouTubePublishService` — OAuth + upload
- [ ] `WatermarkService` — FFmpeg watermark

**API**:
- [ ] `ReviewController`: public + auth routes
- [ ] `BrandKitController`: CRUD
- [ ] `PublishController`: YouTube OAuth

---

### **Phase 12: Analytics & Admin** (NEW) — 2-3 weeks

**Database**:
- [ ] `VideoView` (track view events)
- [ ] `Notification` (job alerts, billing, etc.)
- [ ] Update `Subscription` (usage fields)

**Services**:
- [ ] `UsageMeteringService` — enforce tier limits
- [ ] `VideoViewTrackingService` — CDN webhook
- [ ] `AnalyticsQueryService` — stats aggregation
- [ ] `AdminStatsService` — DAU, costs, errors

**API**:
- [ ] `AnalyticsController`: episode + team stats
- [ ] `AdminController`: DAU, jobs, users [AdminRole]
- [ ] `NotificationController`: CRUD + read
- [ ] `POST /webhooks/cdn-views` — CDN view tracking

---

## Architecture Pattern (Enforce Consistently)

```
Module/
├── Domain/
│   ├── Entities/          # Aggregates, owned entities
│   ├── ValueObjects/      # Enums, immutable types
│   ├── Events/            # Domain events
│   └── Interfaces/        # Repository contracts
├── Application/
│   ├── Commands/          # Write operations
│   ├── Queries/           # Read operations
│   ├── DTOs/              # Request/response objects
│   └── Services/          # Business logic
├── Infrastructure/
│   ├── Repositories/      # EF Core implementations
│   ├── Migrations/        # Database changes
│   └── External/          # API clients, Blob, Service Bus
└── ModuleRegistration.cs  # DI setup
```

**MediatR Pipeline** (in Program.cs):
1. CorrelationId → Logging → Validation → Caching → Transaction

**SignalR Hub** (existing):
- `ProgressHub` at `/hubs/progress`
- Group: `team:{teamId}`
- Create `IHubContext<ProgressHub>` injected notifiers

---

## Database Schema Checklist

| Phase | Tables | New Indices | Migrations |
|-------|--------|------------|-----------|
| 6-8 | (existing) | (existing) | (existing or verify) |
| 9 | Render | EpisodeId, Status | New migration |
| 10 | Track, Clip, Music, TextOverlay | (multiple) | New migration |
| 11 | ReviewLink, Comment, BrandKit, Publish | (multiple) | New migration |
| 12 | VideoView, Notification | UserId, RenderId | Update Subscription |

**Pattern** (all tables):
- `Id` (Guid PK)
- `CreatedAt`, `UpdatedAt` (audit)
- `RowVersion` (optimistic concurrency)
- `IsDeleted` (soft delete with query filter)

---

## Python Pipeline Integration Points

### What Python Handles
- Character design + LoRA training
- Screenplay generation with dialogue timings
- Storyboard planning + image generation
- Animation video generation (Kling.ai or local)
- Voice generation (TTS)

### Service Bus Contract

**Message Format** (all stages):
```json
{
  "correlationId": "uuid",
  "episodeId": "guid",
  "stage": "storyboard|animation|voice",
  "tier": "free|pro|studio",
  "payload": { ... stage-specific data },
  "webhookUrl": "https://backend/api/jobs/{correlationId}/complete"
}
```

### Integration Steps
1. Update `service_bus_listener.py` to route by stage
2. Add `ResultWriter` utility (→ Blob + webhook)
3. Update agent YAML configs to accept metadata
4. Implement webhook handler in .NET: `POST /api/jobs/complete`

---

## Execution Order (Recommended)

**Week 1-3**: Phases 6-8 completion (parallel work possible on partial tasks)
**Week 4-6**: Phase 9 (foundation for delivery, unblocks Phase 11)
**Week 7-10**: Phase 10 (heavy lifting, complex FFmpeg integration)
**Week 10-12**: Phase 11 (needs Phase 9) + Phase 12 (parallel)

---

## Critical Success Factors

1. **Maintain modular structure** — each phase = separate module or sub-folder
2. **Establish spec-first approach** — create architecture docs before coding
3. **SignalR consistency** — all real-time events follow same pattern
4. **Service Bus routing** — clear contract between .NET and Python
5. **Test coverage** — unit + integration + E2E tests per phase
6. **Database migrations** — backward compatible, versioned properly
