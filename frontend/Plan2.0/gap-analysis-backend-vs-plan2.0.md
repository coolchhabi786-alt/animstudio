# Gap Analysis: Plan 2.0 vs Backend Status vs Phase Documents

**Generated**: April 18, 2026  
**Scope**: Phases 6-12 (Frontend + Backend)  
**Status**: Plan 2.0 is frontend-heavy; backend needs parallel development

---

## Executive Summary

| Aspect | Frontend (Plan 2.0) | Backend (Current) | Gap | Action |
|--------|-------------------|------------------|-----|--------|
| **Phases 6-8** | ✅ Fully specified | ✅ Controllers exist | ✓ | **Continue** |
| **Phase 9 (Render)** | ✅ Fully specified | ❌ No controller | **BACKEND NEEDED** | **Code backend** |
| **Phase 10 (Timeline)** | ✅ Fully specified | ❌ No controller | **BACKEND NEEDED** | **Code backend** |
| **Phase 11 (Sharing)** | ✅ Fully specified | ❌ No controller | **BACKEND NEEDED** | **Code backend** |
| **Phase 12 (Analytics)** | ✅ Specified | ❌ Module is stub | **BACKEND NEEDED** | **Code backend** |

**Bottom Line**: **Plan 2.0 covers frontend 100%, but backend development for phases 9-12 is entirely separate and needed in parallel**.

---

## PHASE-BY-PHASE BREAKDOWN

### Phase 6: Storyboard Studio ✅ IN PROGRESS

**Backend Status**: Partially Ready
```csharp
// ✅ EXISTS:
- StoryboardController (API layer)
- StoryboardShot model
- SignalR: ShotUpdated event

// ❌ MISSING/INCOMPLETE:
- StoryboardService (business logic)
- Storyboard planning orchestration
- Shot regeneration queue integration
- StyleOverride persistence
- Image generation calls (Imagen/FAL.ai)
```

**What Plan 2.0 Says**:
```
"Modify use-storyboard.ts hook"
"Add types to src/types/index.ts"
"SignalR integration: ShotUpdated events"
```

**Differentiator**: Plan 2.0 focuses on **FRONTEND hooks + React Query**. Backend services for **storyboard orchestration** (calling AI image gen, managing regenerations, handling StyleOverride business logic) are **not in Plan 2.0**.

**Frontend Dev Dependency**: Frontend can't proceed without:
- `GET /episodes/{id}/storyboard` endpoint working
- `PUT /storyboard/shots/{shotId}/style` persisting correctly
- `POST /storyboard/shots/{shotId}/regenerate` queuing jobs
- SignalR `ShotUpdated` events firing on image completion

---

### Phase 7: Voice Studio ✅ IN PROGRESS

**Backend Status**: Partially Ready
```csharp
// ✅ EXISTS:
- VoiceController (API layer)
- VoiceAssignment model
- TTS integration (Azure OpenAI)

// ❌ MISSING/INCOMPLETE:
- Voice cloning service (ElevenLabs stub)
- TTS preview Blob URL generation
- Language-based voice routing logic
- Character-to-voice persistence layer
```

**What Plan 2.0 Says**:
```
"Modify use-voice-assignments.ts hook"
"VoiceTalentPicker component"
"AudioPreviewPlayer component"
```

**Differentiator**: Plan 2.0 focuses on **FRONTEND components**. Backend **voice services** (preview URL generation, clone upload handling, tier validation) are not detailed.

**Frontend Dev Dependency**:
- `GET /episodes/{id}/voices` returning assignments
- `POST /voices/preview` generating playable URLs
- `PUT /episodes/{id}/voices` saving assignments
- Azure OpenAI TTS working end-to-end

---

### Phase 8: Animation Approval ✅ IN PROGRESS

**Backend Status**: Partially Ready
```csharp
// ✅ EXISTS:
- AnimationController (API layer)
- AnimationClip model
- AnimationJob model
- Cost estimation logic

// ❌ MISSING/INCOMPLETE:
- Kling.ai integration (API calls)
- MiniMax integration (API calls)
- Job queue orchestration
- Backend model routing (kling vs local)
- SignalR ClipReady event broadcasting
- Approval workflow state machine
```

**What Plan 2.0 Says**:
```
"RenderEstimateCard component"
"RenderApprovalDialog component"
"RenderProgressComponent + SignalR"
```

**Differentiator**: Plan 2.0 focuses on **FRONTEND approval UX + progress UI**. Backend **animation service** (calling Kling/MiniMax APIs, managing job queue, broadcasting progress) is not covered.

**Frontend Dev Dependency**:
- `GET /episodes/{id}/animation/estimate` returning accurate cost
- `POST /episodes/{id}/animation` queuing jobs successfully
- `GET /episodes/{id}/animation` returning clip statuses
- SignalR `ClipReady` events firing on completion

---

## PHASES 9-12: MISSING BACKEND

---

### **Phase 9: Render & Delivery** ❌ NOT IN BACKEND

**Backend Missing**:

```csharp
// ❌ DOES NOT EXIST:
- RenderController
- Render database model
- RenderService (orchestration)
- AspectRatioService
- SrtGeneratorService
- CdnService
- SignalR: RenderProgress, RenderComplete
```

**What Phase Document Says**:
```
POST /episodes/{id}/render → enqueue PostProd job
GET /episodes/{id}/renders → render history list
GET /renders/{id} → signed CDN URL (30-day expiry)
GET /renders/{id}/srt → download SRT captions
SignalR: RenderProgress(episodeId, percent, stage)
SignalR: RenderComplete(episodeId, cdnUrl)
```

**What Plan 2.0 Says**:
```
Frontend components: AspectRatioPicker, DirectDownloadBar, RenderHistoryTable
Hook: use-renders.ts
Handles SignalR RenderProgress + RenderComplete
```

**Differentiator**: Plan 2.0 **completely skips backend for Phase 9**. It assumes APIs exist.

**What You Need to Develop**:
- ✅ Render database schema (with AspectRatio enum)
- ✅ RenderController with all 4 endpoints
- ✅ RenderService orchestrating FFmpeg post-production
- ✅ CdnService generating signed Azure Blob URLs (30-day token)
- ✅ SrtGeneratorService converting Timeline → SRT subtitles
- ✅ AspectRatioService translating enum to FFmpeg parameters
- ✅ SignalR hub for RenderProgress + RenderComplete events
- ✅ Background job for post-production processing

**Estimated Effort**: 3-4 weeks (backend + API integration + testing)

---

### **Phase 10: Timeline Editor** ❌ NOT IN BACKEND

**Backend Missing**:

```csharp
// ❌ DOES NOT EXIST:
- TimelineController
- TimelineTrack model
- TimelineClip model
- MusicTrack model
- TextOverlay model
- TimelineService (orchestration)
- TimelineRenderService (translate to FFmpeg)
```

**What Phase Document Says**:
```
GET /episodes/{id}/timeline → full timeline with all tracks + clips
PUT /episodes/{id}/timeline → save reordered/trimmed timeline
POST /episodes/{id}/timeline/preview → queue low-res preview render

MusicController: GET /music/stock, POST /music/upload
TextOverlayController: CRUD for text overlays

TimelineRenderService: translates timeline model → ffmpeg filter graph
```

**What Plan 2.0 Says**:
```
Frontend: TimelineCanvas (Konva.js), TrackPanel, TimelineRuler
Hooks: use-timeline.ts (state management)
Uses @dnd-kit/core for drag-drop
```

**Differentiator**: Plan 2.0 **heavily focuses on frontend Konva.js rendering**. Backend **timeline persistence + rendering** is completely absent.

**What You Need to Develop**:
- ✅ Timeline database schema (tracks, clips, text overlays, music)
- ✅ TimelineController with GET/PUT endpoints
- ✅ MusicController for stock music + upload
- ✅ TextOverlayController for CRUD
- ✅ TimelineService for state management
- ✅ TimelineRenderService translating user edits → FFmpeg filter graph
- ✅ SignalR integration for real-time multi-user edits (future)
- ✅ Timeline preview rendering (low-res for quick feedback)

**Estimated Effort**: 4-5 weeks (backend + database schema + FFmpeg integration)

---

### **Phase 11: Sharing & Review Links** ❌ NOT IN BACKEND

**Backend Missing**:

```csharp
// ❌ DOES NOT EXIST:
- ReviewController
- ReviewLink model
- ReviewComment model
- BrandKit model
- BrandKitController
- PublishController
- YouTube OAuth callback handler
- YouTube Data API v3 integration
- WatermarkService
```

**What Phase Document Says**:
```
ReviewController:
  POST /renders/{id}/review-links → create review link (Pro/Studio tier)
  GET /review/{token} → validate token + return render info (no auth)
  GET /review/{token}/comments → list comments (no auth)
  POST /review/{token}/comments → add timestamped comment (no auth)

BrandKitController: GET/PUT /teams/{id}/brand-kit, POST /brand-kit/logo

PublishController:
  POST /renders/{id}/publish/youtube → OAuth flow start (Studio tier)
  GET /publish/youtube/callback → OAuth callback, upload via YouTube Data API v3

WatermarkService: applies brand logo overlay in post-production
```

**What Plan 2.0 Says**:
```
Frontend components: ReviewLinkGenerator, CommentPanel, YouTubePublish, BrandKitEditor
Public page: /review/[token] (no auth required)
Hooks: use-review.ts, use-brand-kit.ts
```

**Differentiator**: Plan 2.0 **only covers frontend UI**. Backend **tier-based access control, OAuth flow, YouTube integration, public endpoints** are completely absent.

**What You Need to Develop**:
- ✅ ReviewLink + ReviewComment database models
- ✅ ReviewController with 4 endpoints (no auth for /review/{token}/**)
- ✅ BrandKit database model + controller
- ✅ Brand kit logo upload handling (Azure Blob Storage)
- ✅ YouTube OAuth setup (ClientId, ClientSecret, redirect URI)
- ✅ YouTube Data API v3 integration (upload video)
- ✅ PublishController with OAuth callback handler
- ✅ WatermarkService for applying logo overlay in FFmpeg post-production
- ✅ Tier validation (Pro/Studio required for reviews)

**Estimated Effort**: 3-4 weeks (OAuth setup + YouTube API + database models)

---

### **Phase 12: Analytics & Admin Dashboard** ❌ NOT IN BACKEND

**Backend Missing**:

```csharp
// ❌ DOES NOT EXIST:
- AnalyticsModule (currently stub)
- AnalyticsController
- AdminController
- VideoView model
- Notification model
- UsageMeteringService
- VideoViewTrackingService
- NotificationService
```

**What Phase Document Says**:
```
AnalyticsController:
  GET /episodes/{id}/analytics → view count, shares, render count
  GET /teams/{id}/analytics → aggregate stats

AdminController (requires AdminRole):
  GET /admin/stats → DAU/MAU, job queue, error rates, costs
  GET /admin/users → user list with subscription tier
  GET /admin/jobs → recent job list

NotificationController:
  GET /notifications → current user notifications
  PATCH /notifications/{id}/read → mark read
  PATCH /notifications/read-all → mark all read

UsageMeteringService: enforces limits, sends alerts
VideoViewTrackingService: records views from CDN webhook
```

**What Plan 2.0 Says**:
```
Frontend: AnalyticsPage, AdminDashboard, MetricCard, NotificationBell
Hooks: use-analytics.ts, use-admin.ts, use-notifications.ts
Uses recharts for graphs + sparklines
```

**Differentiator**: Plan 2.0 **only covers frontend dashboard UI**. Backend **analytics aggregation, admin metrics, usage metering enforcement, notifications** are not implemented.

**What You Need to Develop**:
- ✅ VideoView table + tracking webhook for CDN logs
- ✅ Notification model + NotificationService
- ✅ AnalyticsController with per-episode + per-team endpoints
- ✅ AdminController with stats + user list + job queue
- ✅ UsageMeteringService (enforce episode quota per tier)
- ✅ Billing alert notifications (80% / 100% usage)
- ✅ SignalR integration for real-time admin metrics
- ✅ Background service for usage counter reset on subscription period renewal

**Estimated Effort**: 2-3 weeks (data aggregation + metering logic)

---

## TOTAL BACKEND DEVELOPMENT NEEDED

### For Phases 9-12: **10-16 weeks of backend development**

| Phase | Component | Effort | Blockers |
|-------|-----------|--------|----------|
| 9 | Render + CDN | 3-4 weeks | Needs Phase 8 animation complete |
| 10 | Timeline CRUD | 4-5 weeks | FFmpeg filter graph complexity |
| 11 | Reviews + YouTube | 3-4 weeks | OAuth setup, YouTube API docs |
| 12 | Analytics + Metering | 2-3 weeks | Minimal blockers |
| **Total** | | **12-16 weeks** | |

### For Phases 6-8: **Parallel backend work needed**

| Phase | Status | Effort | Timeline |
|-------|--------|--------|----------|
| 6 | 40% done (stub services) | 2 weeks | Can start now |
| 7 | 60% done (TTS works) | 1.5 weeks | Can start now |
| 8 | 50% done (cost calc works) | 2.5 weeks | Can start now |
| **Subtotal** | | **5.5 weeks** | |

---

## DIFFERENTIATORS: Plan 2.0 vs Phase Documents

### Plan 2.0 Strengths ✅
1. **Frontend component specifications**: 60+ UI components fully specified with prop interfaces
2. **Design system**: Complete color palette, typography, spacing, responsive breakpoints
3. **Hook patterns**: All data-fetching hooks documented with patterns
4. **Real-time architecture**: SignalR event flows documented
5. **Testing strategy**: 50+ test scenarios documented
6. **TypeScript types**: All domain models (Timeline, Review, etc.) fully typed

### Plan 2.0 Gaps ❌
1. **Backend services**: No business logic for phases 9-12
2. **Database schema**: Only mentioned in phase documents, not in Plan 2.0
3. **External integrations**: Kling.ai, YouTube, ElevenLabs — not in Plan 2.0
4. **Infrastructure**: Azure Blob Storage, CDN configuration — not in Plan 2.0
5. **OAuth flows**: YouTube/Microsoft — not in Plan 2.0
6. **DevOps**: Docker, deployment, CI/CD — not in Plan 2.0

### Phase Documents Strengths ✅
1. **Database schema**: Detailed entity models
2. **API endpoints**: All HTTP routes documented
3. **External integrations**: Mentions YouTube, Kling, etc.
4. **Backend services**: Lists required controllers + services

### Phase Documents Gaps ❌
1. **Frontend component props**: Only high-level descriptions (no TypeScript interfaces)
2. **Real-time architecture**: No SignalR event flow details
3. **Design tokens**: No color palette or typography specs
4. **Testing**: Only mentions "implement tests", no specific test cases
5. **Error handling**: No retry logic or resilience patterns

---

## WHAT YOU ACTUALLY NEED TO DEVELOP

### Immediate (Parallel with Frontend Phases 6-8):

**Backend Services** (Complete these FIRST):
- [ ] StoryboardService (orchestrate AI image generation)
- [ ] VoiceService (TTS preview + voice clone upload)
- [ ] AnimationService (call Kling/MiniMax APIs)

**These unblock frontend development for phases 6-8.**

### Next (After Phase 8, before Phase 9):

**Phase 9 Backend** (3-4 weeks):
- [ ] Render database model + RenderController
- [ ] RenderService (FFmpeg post-production orchestration)
- [ ] CdnService (signed URLs with expiry)
- [ ] SrtGeneratorService (Timeline → subtitles)
- [ ] SignalR RenderProgress + RenderComplete events

### Following (After Phase 9, before Phase 10):

**Phase 10 Backend** (4-5 weeks):
- [ ] Timeline database models (Track, Clip, TextOverlay, MusicTrack)
- [ ] TimelineController (GET/PUT, music CRUD, text CRUD)
- [ ] TimelineService (state management, conflict resolution)
- [ ] TimelineRenderService (timeline → FFmpeg filter graph)
- [ ] Music library management (stock + custom upload)

### Then (After Phase 10, before Phase 11):

**Phase 11 Backend** (3-4 weeks):
- [ ] ReviewLink + ReviewComment models + controller
- [ ] BrandKit model + controller
- [ ] YouTube OAuth setup + callback handler
- [ ] YouTube Data API v3 video upload
- [ ] PublishController for YouTube publishing
- [ ] WatermarkService for logo overlay

### Finally:

**Phase 12 Backend** (2-3 weeks):
- [ ] AnalyticsModule implementation
- [ ] VideoView tracking + CDN webhook
- [ ] Notification model + service
- [ ] UsageMeteringService (quota enforcement)
- [ ] AdminController + stats aggregation

---

## HOW PLAN 2.0 RELATES TO BACKEND

### Plan 2.0 is **Frontend-First Specification**

```
Plan 2.0 Layers:
├─ Design System (Figma tokens)
├─ Component Library (60+ React components)
├─ Data Flow (hooks + React Query)
├─ Real-time (SignalR events)
└─ Testing (E2E + unit test scenarios)

❌ Does NOT include:
- Database schema details
- Service implementations
- External API integrations
- Infrastructure setup
- Deployment pipelines
```

### Backend Development Must Be Parallel

```
Timeline Overlap:
Frontend Phases 6-8:     ■■■■■■■ (Weeks 2-8)
Backend Phases 6-8:      ■■■■■■■ (Weeks 2-8, parallel)
                         ↓ These must sync

Frontend Phase 10:        ■■■■■■■ (Weeks 10-15)
Backend Phase 10:         ■■■■■■■ (Weeks 10-15, parallel)
                          ↓ API contracts first!

Frontend Phase 11:             ■■■■■ (Weeks 13-15)
Backend Phase 11:              ■■■■■ (Weeks 13-15, parallel)
                               ↓ OAuth tested early
```

---

## ANSWER: DO YOU STILL NEED TO DEVELOP PHASES 9-12?

### YES — But split into two tracks:

**Track 1: Frontend Development** ✅
- **Covered by Plan 2.0**: All component specifications, design, testing strategy
- **Status**: Ready to implement with Claude Sonnet
- **Effort**: 12-15 weeks (as planned)
- **Starting Point**: Agent can begin immediately on phases 6-8

**Track 2: Backend Development** ❌ NOT COVERED
- **NOT in Plan 2.0**: Needs separate specification
- **Status**: Requires database schema + service design
- **Effort**: 12-16 weeks (parallel with frontend)
- **Starting Point**: Cannot start until you define DB schema + API contracts

### The Plan 2.0 Gap:

**Plan 2.0 assumes backend APIs exist**, but they don't (phases 9-12 are stubs).

**You need BOTH**:
1. A backend specification (API contracts, DB schema, service dependencies)
2. Frontend implementation (handled by Plan 2.0 + Claude)

---

## RECOMMENDATION

### Do This Now:

1. **Use Plan 2.0 for frontend development** (Phases 6-12)
   - Assign Claude Sonnet for frontend components
   - Estimated 18 weeks

2. **Create backend specification** (Phases 6-12)
   - Database schema for each phase
   - API endpoint contracts
   - Service layer object diagrams
   - External integration points (YouTube, Kling, etc.)
   - Estimated effort: 2-3 weeks planning

3. **Parallel development schedule**:
   - Weeks 2-4: Frontend phases 6-7, Backend stubs → services
   - Weeks 5-8: Frontend phases 8-9, Backend phase 9 core
   - Weeks 9-12: Frontend phases 10, Backend phase 10 core
   - Weeks 13-15: Frontend phases 11-12, Backend phases 11-12

4. **API-First Contract**:
   - Define all endpoints before frontend + backend start
   - Use OpenAPI/Swagger to document contracts
   - Both teams code to the contract

---

## Files You Should Create Next

To extend Plan 2.0 with backend specs:

1. **backend-database-schema.md** (Database models for 9-12)
2. **backend-service-specifications.md** (Service layer for 9-12)
3. **backend-api-contracts.md** (OpenAPI specs for all 9-12 endpoints)
4. **backend-integration-points.md** (YouTube, Kling, Azure, etc.)

Each would be 5-10KB and provide backend developers the same clarity Plan 2.0 provides frontend.

