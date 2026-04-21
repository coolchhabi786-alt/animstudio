# Backend Implementation Guide — All Phases

## Overview

You now have **detailed implementation prompts for all 7 phases** (Phases 6-12). This guide explains what's been created and how to use it effectively.

---

## What's Been Created

### Phase-Specific Detailed Prompts (Ready to Hand to Developers)

| Phase | File | Duration | Module | Key Focus |
|-------|------|----------|--------|-----------|
| **6** | `phase6-implementation-prompt.md` | 1 week | ContentModule | Storyboard services, SignalR |
| **7** | `phase7-implementation-prompt.md` | 1.5 weeks | ContentModule | Voice TTS, batch updates |
| **8** | `phase8-implementation-prompt.md` | 1 week | ContentModule | Animation SignalR, Hangfire |
| **9** | `phase9-implementation-prompt.md` | 2-3 weeks | DeliveryModule (NEW) | Render, CDN, SRT |
| **10** | `phases-10-12-prompts.md` (section 1) | 3-4 weeks | ContentModule | Timeline, FFmpeg |
| **11** | `phases-10-12-prompts.md` (section 2) | 3-4 weeks | New Module | Reviews, YouTube OAuth |
| **12** | `phases-10-12-prompts.md` (section 3) | 2-3 weeks | AnalyticsModule | Metering, notifications, analytics |

### Master Planning Documents

- `refined_plan.md` — High-level 12-week architecture + execution order
- `IMPLEMENTATION-GUIDE.md` (this file) — Quick reference for all phases

---

## How Each Prompt Is Structured

Each detailed prompt includes:

1. **Objective** — What phase accomplished
2. **Current State** — What's done vs. missing (✅ vs. ❌)
3. **Implementation Checklist** — Line-by-line code patterns
4. **File Locations** — Exact path where each file goes
5. **Code Examples** — Full class implementations (copy-paste ready)
6. **DTOs & Models** — Database schema, API contracts
7. **Testing Requirements** — Unit, integration, E2E tests
8. **Acceptance Criteria** — Definition of done

**Example**: Phase 6 prompt tells you:
- Create `StoryboardService`, `SignalRStoryboardNotifier`
- Where to put files: `ContentModule/Application/Services/`
- Full implementation code for each service
- API routes to add to StoryboardController
- What tests to write
- When done: "All tests pass, SignalR broadcasts, soft deletes work"

---

## Implementation Workflow (Per Developer)

### For Each Phase (Example: Phase 6)

**Day 1:** Read prompt
```
1. Read phase6-implementation-prompt.md
2. Understand objectives (Storyboard services)
3. Review current state (entities exist, services missing)
4. Note all file locations
```

**Day 1-2:** Create entities + migrations
```csharp
// If migration missing, create:
// Phase6_AddStoryboardTables.cs
// (Prompt shows exact schema)
```

**Day 2-3:** Implement repositories & services
```csharp
// Copy from prompt:
// StoryboardRepository.cs
// StoryboardService.cs
// Update StoryboardRepository interface
```

**Day 3-4:** Controller + MediatR
```csharp
// Verify controller routes exist
// Create Commands: ApplyStyleOverride, RegenerateShot
// Create Queries: GetStoryboard
// Create Handlers (pattern shown in prompt)
```

**Day 4:** SignalR + module registration
```csharp
// Add SignalRStoryboardNotifier handler
// Register in ContentModuleRegistration.cs
// Test broadcasting
```

**Day 5:** Testing
```csharp
// Unit tests (services)
// Integration tests (routes)
// Manual SignalR test
```

**Done:** All acceptance criteria pass

---

## File Organization (By Module)

### ContentModule (Phases 6, 7, 8, 10)

```
ContentModule/
├── Application/
│   ├── Commands/
│   │   ├── ApproveAnimation/  (Phase 8)
│   │   ├── ApplyStyleOverride/  (Phase 6)
│   │   ├── RegenerateShot/  (Phase 6)
│   │   ├── BatchUpdateVoices/  (Phase 7)
│   │   ├── UpdateTimeline/  (Phase 10)
│   │   └── AddTextOverlay/  (Phase 10)
│   ├── Queries/
│   │   ├── GetStoryboard/  (Phase 6)
│   │   ├── GetVoiceAssignments/  (Phase 7)
│   │   ├── GetAnimationClips/  (Phase 8)
│   │   ├── GetTimeline/  (Phase 10)
│   │   └── GetTextOverlays/  (Phase 10)
│   ├── DTOs/
│   │   ├── StoryboardDtos.cs  (Phase 6)
│   │   ├── VoiceDtos.cs  (Phase 7)
│   │   ├── AnimationDtos.cs  (Phase 8)
│   │   └── TimelineDtos.cs  (Phase 10)
│   ├── Services/
│   │   ├── StoryboardService.cs  (Phase 6)
│   │   ├── VoiceBatchUpdateService.cs  (Phase 7)
│   │   ├── TimelineService.cs  (Phase 10)
│   │   ├── TimelineRenderService.cs  (Phase 10, FFmpeg)
│   │   ├── MusicService.cs  (Phase 10)
│   │   └── TextOverlayService.cs  (Phase 10)
│   └── EventHandlers/
│       ├── SignalRStoryboardNotifier.cs  (Phase 6)
│       └── SignalRAnimationClipNotifier.cs  (Phase 8)
├── Infrastructure/
│   ├── Repositories/
│   │   ├── StoryboardRepository.cs  (Phase 6)
│   │   ├── VoiceAssignmentRepository.cs  (Phase 7)
│   │   ├── AnimationClipRepository.cs  (Phase 8)
│   │   ├── TimelineTrackRepository.cs  (Phase 10)
│   │   ├── MusicTrackRepository.cs  (Phase 10)
│   │   └── TextOverlayRepository.cs  (Phase 10)
│   └── Migrations/
│       ├── Phase6_Storyboard.cs  (Phase 6)
│       ├── Phase7_Voice.cs  (Phase 7)
│       ├── Phase8_Animation.cs  (Phase 8)
│       └── Phase10_Timeline.cs  (Phase 10)
```

### DeliveryModule (Phase 9, 11)

```
DeliveryModule/
├── Application/
│   ├── Commands/
│   │   ├── StartRender/  (Phase 9)
│   │   ├── CreateReviewLink/  (Phase 11)
│   │   └── PublishToYouTube/  (Phase 11)
│   ├── Queries/
│   │   ├── GetRender/  (Phase 9)
│   │   ├── GetReviewPage/  (Phase 11)
│   │   └── GetBrandKit/  (Phase 11)
│   ├── DTOs/
│   │   ├── RenderDtos.cs  (Phase 9)
│   │   ├── ReviewDtos.cs  (Phase 11)
│   │   └── BrandKitDtos.cs  (Phase 11)
│   ├── Services/
│   │   ├── RenderService.cs  (Phase 9)
│   │   ├── SrtGeneratorService.cs  (Phase 9)
│   │   ├── CdnService.cs  (Phase 9)
│   │   ├── ReviewLinkService.cs  (Phase 11)
│   │   ├── BrandKitService.cs  (Phase 11)
│   │   ├── YouTubePublishService.cs  (Phase 11)
│   │   └── WatermarkService.cs  (Phase 11)
│   └── EventHandlers/
│       ├── SignalRRenderProgressNotifier.cs  (Phase 9)
│       ├── SignalRRenderCompleteNotifier.cs  (Phase 9)
├── Infrastructure/
│   ├── Repositories/
│   │   ├── RenderRepository.cs  (Phase 9)
│   │   ├── ReviewLinkRepository.cs  (Phase 11)
│   │   ├── BrandKitRepository.cs  (Phase 11)
│   │   └── SocialPublishRepository.cs  (Phase 11)
│   ├── Persistence/
│   │   └── DeliveryDbContext.cs  (Phase 9)
│   └── Migrations/
│       ├── Phase9_Render.cs  (Phase 9)
│       └── Phase11_SharingReview.cs  (Phase 11)
```

### AnalyticsModule (Phase 12)

```
AnalyticsModule/
├── Application/
│   ├── Queries/
│   │   ├── GetEpisodeAnalytics/
│   │   ├── GetTeamAnalytics/
│   │   ├── GetAdminStats/
│   │   └── GetUserNotifications/
│   ├── DTOs/
│   │   ├── AnalyticsDtos.cs
│   │   ├── NotificationDtos.cs
│   │   └── AdminStatsDtos.cs
│   └── Services/
│       ├── UsageMeteringService.cs
│       ├── VideoViewTrackingService.cs
│       ├── AnalyticsQueryService.cs
│       └── AdminStatsService.cs
├── Infrastructure/
│   ├── Repositories/
│   │   ├── VideoViewRepository.cs
│   │   ├── NotificationRepository.cs
│   │   └── AnalyticsQueryRepository.cs
│   ├── Migrations/
│   │   └── Phase12_Analytics.cs
│   └── HostedServices/
│       ├── UsageResetService.cs
│       └── NotificationCleanupService.cs
```

---

## Execution Plan (Recommended)

### Week 1-2: Phases 6-8 (Existing ContentModule)
**1 Developer**
- Day 1-2: Complete Phase 6 (Storyboard services)
- Day 3-4: Complete Phase 7 (Voice services)
- Day 5-8: Complete Phase 8 (Animation SignalR)
- Day 9-10: Integration testing across 6-8

**Goal**: All three phases fully working + tested

### Week 3-4: Phase 9 (DeliveryModule Foundation)
**1 Developer**
- Create `DeliveryModule` folder structure
- Implement Render entity + database
- Build RenderService, SrtGenerator, CdnService
- Add RenderController routes
- Test signalR rendering

**Goal**: Rendering pipeline working end-to-end

### Week 5-7: Phase 10 (Timeline — Complex)
**1 Developer**
- Phase 10 is the most complex (FFmpeg integration)
- Build 4 table models + repositories
- Implement TimelineService + TimelineRenderService (FFmpeg)
- Add MusicService + TextOverlayService
- Test FFmpeg filter graph generation

**Goal**: Timeline editing + FFmpeg rendering working

### Week 8-9: Phase 11 (Sharing & YouTube)
**1 Developer**
- Build ReviewLink + BrandKit entities
- Implement ReviewLinkService + BrandKitService
- Set up YouTube OAuth flow
- Add YouTubePublishService
- Test OAuth + video upload

**Goal**: Review links + YouTube publishing working

### Week 10-11: Phase 12 (Analytics)
**2 Developers** (can parallelize with Phase 11)
- Build VideoView + Notification tables
- Implement UsageMeteringService (tier limits)
- Build AnalyticsQueryService + AdminStatsService
- Add webhook for CDN view tracking
- Test usage metering + billing alerts

**Goal**: Analytics dashboard + metering working

### Week 12: Integration & Polish
- E2E testing all phases
- Performance optimization (indices, queries)
- Documentation

---

## Python Pipeline Integration (Parallel)

**Sync with .NET development**:
- Week 1-2: Update `service_bus_listener.py` to handle multi-stage messages
- Week 3: Add `ResultWriter` utility (Blob upload + webhook)
- Week 4-5: Test full Python → .NET flow (via Service Bus + webhook)
- All other weeks: Update agent configs as .NET APIs are ready

**Key Message Schema** (all stages):
```json
{
  "correlationId": "uuid-unique",
  "episodeId": "guid",
  "stage": "storyboard|animation|voice",
  "tier": "free|pro|studio",
  "payload": { /* stage-specific */ },
  "webhookUrl": "https://backend/api/jobs/complete"
}
```

---

## Infrastructure Checklist

### Azure Services (Setup Early)

**Blob Storage Containers**:
- [ ] `storyboard-shots` — Phase 6
- [ ] `animation-clips` — Phase 8
- [ ] `tmp-tts` — Phase 7 (1-hour lifecycle delete)
- [ ] `renders` — Phase 9
- [ ] `logos` — Phase 11
- [ ] `music` — Phase 10

**Service Bus Queues**:
- [ ] `storyboard-jobs`
- [ ] `animation-jobs`
- [ ] `voice-jobs`
- [ ] `character-jobs`
- [ ] `screenplay-jobs`
- [ ] Dead-letter queues for all

**Key Vault Secrets**:
- [ ] `AzureOpenAIKey`, `AzureOpenAIEndpoint`
- [ ] `YouTubeClientId`, `YouTubeClientSecret`
- [ ] Service Bus connection string
- [ ] Blob Storage connection string

**Databases**:
- [ ] Create `content` schema (Phases 6-10)
- [ ] Create `delivery` schema (Phases 9, 11)
- [ ] Extend `IdentityModule` schema with usage fields (Phase 12)

---

## Team Role Suggestions

**Backend Developers** (3 total recommended):
- **Dev 1**: Phases 6-8 (ContentModule completion) — weeks 1-2
- **Dev 2**: Phases 9-10 (DeliveryModule) — weeks 3-7
- **Dev 3**: Phases 11-12 (YouTube OAuth, Analytics) — weeks 8-11

**Python Engineer** (1):
- Parallel: Integrate Service Bus messaging
- Coordinate with .NET on webhook contracts
- Test end-to-end flows

**DevOps** (1):
- Setup Azure Blob, Service Bus, Key Vault
- Configure CI/CD pipelines
- Monitor Hangfire job queue

---

## How to Use These Prompts

### Give to a Developer

> "You're implementing Phase 6. Here's your prompt:"
> 
> See: `phase6-implementation-prompt.md` → Copy file location → Start building

### During Implementation

Developer gets stuck on FFmpeg filter syntax?

> "Check Phase 10 prompt section on TimelineRenderService — shows example FFmpeg calls"

Questions on database schema?

> "See phases-10-12-prompts.md section on Phase 10 — full schema with constraints"

### PR Review Checklist

Use phase prompts as acceptance criteria:

> "This Phase 8 PR should have:
> - ✅ SignalRAnimationClipNotifier
> - ✅ Hangfire job handler
> - ✅ All 4 controller routes
> - ✅ Integration tests
> - See: phase8-implementation-prompt.md acceptance criteria"

---

## Common Questions?

**Q: Can phases run in parallel?**  
A: Phases 6-8 are independent (can overlap). Phase 9 foundation needed before 10-11. Phase 12 can start week 10.

**Q: What if a developer falls behind?**  
A: Reallocate. Phase 9-10 can be split (Render vs Timeline).

**Q: How much time for testing?**  
A: ~20% of development time. E2E tests run across all phases in week 12.

**Q: Can we skip phases?**  
A: Not recommended. Phase 9 blocks 11. Each phase builds on prior API contracts.

---

## Success Metrics (Week 12)

- [ ] All 7 phases implemented
- [ ] All integration tests pass
- [ ] SignalR broadcasts working (4 event types)
- [ ] Python → .NET pipeline tested end-to-end
- [ ] Performance meets thresholds (queries < 100ms)
- [ ] Documentation complete
- [ ] Ready for frontend team integration
