# AnimStudio V2.0 - Implementation Plan Summary

## Document Overview

I've created a **detailed, implementation-ready** V2.0 plan with the following structure:

### Main Documentation Files
1. **`/memories/session/v2-implementation-plan.md`** - Complete technical specifications
   - V1 Enhancements (Production Hardening)
   - Phase 6-9: Detailed components, APIs, data models
   - Phase 10-12: Ready for expansion

2. **This file** - Quick reference & decision guide

---

## V2.0 Timeline & Resources

### Duration: 18 Weeks (4.5 months)
### Team: 2-3 developers
### Status: **Sprint-ready** (ready to start Week 1)

### Phase Breakdown

| Phase | Duration | Dev Days | Priority | Start | Status |
|-------|----------|----------|----------|-------|--------|
| **V1 Enhancements** | 2 weeks | 10 | CRITICAL | Wk 1 (parallel) | Ready |
| **Phase 6 (Storyboard Complete)** | 2 weeks | 10 | HIGH | Wk 1 | Ready |
| **Phase 7 (Voice Studio)** | 2 weeks | 10 | HIGH | Wk 3 | Ready |
| **Phase 8 (Animation Approval)** | 2 weeks | 10 | HIGH | Wk 5 | Ready |
| **Phase 9 (Delivery/Render)** | 3 weeks | 15 | CRITICAL | Wk 7 | Ready |
| **Phase 10 (Timeline Editor)** | 6 weeks | 30 | CRITICAL | Wk 10 | Draft |
| **Phase 11 (Review/Sharing)** | 3 weeks | 15 | CRITICAL | Wk 13 | Draft |
| **Phase 12 (Analytics)** | 3 weeks | 15 | MEDIUM | Wk 16 | Draft |
| **Integration & QA** | 2 weeks | 10 | HIGH | Wk 19 | - |

---

## V1 Enhancements (Production Hardening)

### V1-A: Global Error Handling
**Effort**: 4 dev-days | **Files**: 7 new/modified

**What it does**:
- Centralized error boundary + context
- Structured error logging with Sentry integration
- Error boundary component for React crashes
- Enhanced API client with error mapping
- Request timeouts (30s) + retry logic

**Key Features**:
✅ Error context manages all app errors
✅ Logger captures logs to localStorage (dev) + Sentry (prod)
✅ Error boundary prevents white-screen crashes
✅ API errors mapped to user-friendly messages
✅ Automatic error retry with exponential backoff

**Deliverables**:
- `src/lib/logger.ts` - Centralized logging
- `src/contexts/error-context.tsx` - Global error state
- `src/hooks/use-error.ts` - Error hook for components
- `src/components/error-boundary.tsx` - React error boundary
- `src/lib/sentry.ts` - Sentry configuration
- Updated `src/lib/api-client.ts` - Error handling + timeouts

**Success Criteria**:
- [ ] All console errors logged to Sentry
- [ ] API errors show friendly user messages
- [ ] React crashes caught by boundary
- [ ] Request timeouts after 30s
- [ ] Retry logic works for transient failures

---

### V1-B: Request Cleanup & Memory Leak Prevention
**Effort**: 3 dev-days | **Files**: 6 modified

**What it does**:
- AbortController for all fetch requests
- SignalR connection cleanup on unmount
- Request deduplication
- Memory leak prevention

**Deliverables**:
- `src/lib/abort-manager.ts` - AbortController management
- Modified all hooks to cancel requests on unmount
- SignalR cleanup in `use-signal-r.ts`

**Success Criteria**:
- [ ] No "memory leak" warnings in DevTools
- [ ] Rapid navigation doesn't pile up requests
- [ ] SignalR connections close on page leave
- [ ] AbortController cancels pending requests

---

### V1-C: State Management Refactor
**Effort**: 2 dev-days | **Files**: 3 modified

**What it does**:
- Consolidate Zustand + React Query + local state
- Create unified `useAppStore()` hook
- Break up monolithic uiStore

**Deliverables**:
- `src/stores/app-store.ts` - New unified store
- Backward-compatible wrappers for old stores

**Success Criteria**:
- [ ] All components use new app-store
- [ ] No prop drilling for UI state
- [ ] Loading state accessible globally
- [ ] Notification count available app-wide

---

### V1-D: Security Hardening
**Effort**: 1 dev-day | **Files**: 3 modified

**What it does**:
- Replace NODE_ENV checks with feature flags
- Client-side rate limiting
- Dev auth only via explicit env var

**Deliverables**:
- `src/lib/feature-flags.ts` - Feature flag system
- Modified `src/middleware.ts` - Use feature flags
- Rate limiting in `api-client.ts`

**Success Criteria**:
- [ ] No NODE_ENV in production logic
- [ ] Dev mode only if `ENABLE_DEV_AUTH=true`
- [ ] Rate limiting prevents request floods
- [ ] Can deploy same image to staging/prod

---

### V1-E: Docker & DevOps Fix
**Effort**: 1 dev-day | **Files**: 3 modified

**What it does**:
- Fix static export conflict
- Add Nginx config
- Add health check endpoint

**Deliverables**:
- Modified `Dockerfile` - Proper static export
- New `nginx.conf` - SPA routing + caching
- Modified `next.config.mjs` - Enable static export
- Modified `package.json` - Docker scripts

**Success Criteria**:
- [ ] Docker build completes without errors
- [ ] Container starts and responds to /health
- [ ] SPA routing works (/episodes/123 → index.html)
- [ ] Static assets cached for 1 year
- [ ] Image size < 50MB

---

## Phase 6: Storyboard Studio - Complete UI

### Current Status
✅ Job dispatch works
❌ UI rendering not implemented

### Deliverables (9 components)

#### Main Page Component
- `src/app/(dashboard)/studio/[id]/storyboard/page.tsx` - Full storyboard UI

#### Shot Components
1. **Scene Tab** - Tab buttons for scene selection
2. **Shot Grid** - Responsive grid of shot cards
3. **Shot Card** - Individual shot with image + actions
4. **Full-Screen Viewer** - Lightbox with zoom + navigation
5. **Style Override Dialog** - Style picker for individual shots
6. **Regenerate Dialog** - Confirmation dialog

#### Supporting Features
- Real-time updates via SignalR (ShotUpdated events)
- Shot image rendering from backend
- Per-shot style override with presets
- Scene navigation (Scene 1, 2, 3...)
- Loading skeletons

**Effort**: 10 dev-days | **Components**: 6 new

**Success Criteria**:
- [ ] All shots render with images
- [ ] Full-screen viewer zooms/pans smoothly
- [ ] Style override persists to backend
- [ ] Regenerate calls API, shows progress
- [ ] Real-time updates appear instantly

---

## Phase 7: Voice Studio - UI Implementation

### Current Status
✅ Hooks exist
❌ ZERO UI components

### Deliverables (5 components)

#### Main Page
- `src/app/(dashboard)/studio/[id]/voice/page.tsx` - Voice studio UI

#### Voice Components
1. **Voice Roster Table** - Character list with voice assignments
2. **Voice Picker** - Dropdown for built-in voices
3. **Language Selector** - Language picker with flags
4. **Audio Preview Player** - Play/pause + waveform
5. **Voice Clone Upload** - Drag-and-drop, tier-gated
6. **Batch Update Dialog** - Update multiple characters

**Features**:
- 6 built-in voices (Alloy, Echo, Fable, Onyx, Nova, Shimmer)
- 6 languages (EN, ES, FR, DE, IT, JA)
- TTS preview playback
- Voice cloning for Studio tier (tier-gated)
- Batch voice assignment

**Effort**: 10 dev-days | **Components**: 6 new

**Success Criteria**:
- [ ] Voice roster displays all characters
- [ ] Voice picker dropdown works
- [ ] Language selector shows flags
- [ ] Audio preview plays inline
- [ ] Voice clone upload hidden for non-Studio tiers
- [ ] Batch updates work

---

## Phase 8: Animation Studio - Approval Workflow

### Current Status
✅ Estimate hook works
❌ NO approval UI, no clip preview

### Deliverables (5 components)

#### Main Page
- `src/app/(dashboard)/studio/[id]/animation/page.tsx` - Animation studio

#### Animation Components
1. **Estimate Card** - Itemized cost breakdown by scene
2. **Backend Selector** - Kling AI vs Local Engine radio
3. **Approval Dialog** - Cost confirmation
4. **Animation Progress** - Real-time progress bar
5. **Clip Player** - Video preview per clip
6. **Retry Button** - Re-queue failed animations

**Features**:
- Cost estimate by scene (Kling $0.056/clip, Local $0)
- Kling AI vs Local Engine selector
- Itemized cost breakdown table
- Final cost display before approval
- Real-time progress via SignalR
- Hover-to-play video preview
- Failed clip indicator + retry button

**Effort**: 10 dev-days | **Components**: 5 new

**Success Criteria**:
- [ ] Estimate card displays correctly
- [ ] Backend selector toggles pricing
- [ ] Approval dialog shows final cost
- [ ] Animation starts after approval
- [ ] Progress bar updates in real-time
- [ ] Clips preview on hover
- [ ] Failed clips show retry button

---

## Phase 9: Post-Production & Video Delivery

### Current Status
❌ NOT STARTED

### Deliverables (6 components)

#### Main Page
- `src/app/(dashboard)/studio/[id]/render/page.tsx` - Render workflow

#### Render Components
1. **Aspect Ratio Picker** - 16:9 / 9:16 / 1:1 selector
2. **Render Progress Card** - Progress stages visualization
3. **Video Player** - Final video playback
4. **Download Bar** - MP4 + SRT download buttons
5. **Render History** - Previous renders list + re-render

**Features**:
- 3 aspect ratio options with visual preview
- Render button (disabled if animation not complete)
- Stage-based progress (Queued → Assembling → Mixing → Done)
- Real-time progress via SignalR
- Signed CDN URLs (30-day expiry)
- SRT captions download
- Render history with timestamps
- Re-render capability

**Effort**: 15 dev-days | **Components**: 5 new

**Success Criteria**:
- [ ] Aspect ratio picker functional
- [ ] Render starts when button clicked
- [ ] Progress updates every 5s via SignalR
- [ ] Final video plays in player
- [ ] Downloads work (video + captions)
- [ ] Render history shows all renders
- [ ] CDN URLs have 30-day expiry

---

## Phase 10-12 (Draft Status)

### Phase 10: Timeline Editor
**Current Status**: Not started | **Complexity**: VERY HIGH | **Effort**: 40-50 dev-days

**Scope**:
- Multi-track canvas (Video/Audio/Music/Text)
- Drag-and-drop shot reordering
- Trimming with handles
- Transitions (Cut/Fade/Dissolve)
- Music library + volume control
- Text overlays + animations
- Playback scrubber + zoom

**Tech Stack**:
- Konva.js (canvas rendering)
- @dnd-kit/core (drag-and-drop)
- Zustand (timeline state)

**Components Needed**: 10-12

**Status**: Ready for detailed specification once Phases 6-9 approved

---

### Phase 11: Review & Sharing
**Current Status**: Not started | **Complexity**: HIGH | **Effort**: 25-30 dev-days

**Scope**:
- Review link generation (expiry, password, token)
- Public review page (no auth)
- Timestamped comment threads
- YouTube publishing workflow
- Brand kit editor (logo, colors, watermark)
- Watermark rendering

**Components Needed**: 8-10

**Status**: Ready for detailed specification once Phases 6-9 approved

---

### Phase 12: Analytics & Admin
**Current Status**: Not started | **Complexity**: MEDIUM | **Effort**: 20-25 dev-days

**Scope**:
- Creator analytics dashboard (views, sparklines)
- Admin dashboard (DAU/MAU, job queue, costs)
- Usage metering enforcement UI
- Notification system (bell + panel)
- Usage quota display per tier

**Components Needed**: 8-10

**Status**: Ready for detailed specification once Phases 6-9 approved

---

## Architecture & Infrastructure

### Testing Strategy (15 dev-days)
- **Unit Tests** (Jest): All utils, hooks, components
- **E2E Tests** (Playwright): Full workflows per phase
- **Accessibility Tests** (axe): WCAG 2.1 AA compliance
- **Visual Regression** (Percy): Component snapshots

### Monitoring & Observability
- **Error Tracking**: Sentry (errors + sessions)
- **Performance**: Web Vitals + Lighthouse
- **Logging**: CloudWatch + local storage
- **Uptime**: Health checks every 30s

### Deployment Strategy
- **Staging**: Pre-production environment for E2E testing
- **Production**: Blue-green deployment with rollback
- **CDN**: Azure CDN for video delivery with 30-day URL signing
- **Backup**: Database backups every 24h

---

## Implementation Workflow

### Week 1 - Start (V1 Enhancements + Phase 6)
1. Set up error handling + logging (V1-A)
2. Implement AbortController cleanup (V1-B)
3. Refactor state management (V1-C)
4. Security + Docker fixes (V1-D, V1-E)
5. Begin Phase 6 implementation
6. **Deliverable**: Error-safe demo, storyboard grid rendering

### Week 3-4 - Parallel (Phase 7)
1. Voice roster table + voice picker
2. Language selector + preview player
3. Voice clone upload (tier-gated)
4. **Deliverable**: Characters can have voices assigned

### Week 5-6 - Sequential (Phase 8)
1. Animation estimate card + backend selector
2. Approval dialog + approval flow
3. Clip player + progress tracking
4. **Deliverable**: Animations can be queued + costs shown

### Week 7-9 - Sequential (Phase 9)
1. Aspect ratio picker + render button
2. Render progress visualization
3. Video player + download buttons
4. Render history
5. **Deliverable**: Complete video delivery workflow

### Week 10+ - Extended
1. Phase 10 (Timeline) - Most complex
2. Phase 11 (Sharing) - OAuth integration
3. Phase 12 (Analytics) - Dashboard UX
4. QA + deployment

---

## File Structure After V2.0 Completion

```
src/
├── app/
│   └── (dashboard)/studio/[id]/
│       ├── storyboard/page.tsx (6 components)
│       ├── voice/page.tsx (5 components)
│       ├── animation/page.tsx (5 components)
│       ├── render/page.tsx (5 components)
│       └── ...existing phases...
├── components/
│   ├── error-boundary.tsx (NEW)
│   ├── storyboard/ (6 NEW)
│   ├── voice/ (5 NEW)
│   ├── animation/ (5 NEW)
│   ├── render/ (5 NEW)
│   └── ...existing...
├── contexts/
│   ├── error-context.tsx (NEW)
│   └── error-provider.tsx (NEW)
├── hooks/
│   ├── use-error.ts (NEW)
│   ├── use-renders.ts (NEW)
│   └── ...existing...
├── lib/
│   ├── logger.ts (NEW)
│   ├── error-utils.ts (NEW)
│   ├── sentry.ts (NEW)
│   ├── feature-flags.ts (NEW)
│   ├── abort-manager.ts (NEW)
│   └── ...existing...
├── stores/
│   ├── app-store.ts (NEW - replaces uiStore/authStore)
│   └── ...existing...
└── ...rest unchanged...

tests/
├── unit/ (NEW - Jest)
│   ├── utils/
│   ├── hooks/
│   └── components/
├── e2e/ (Existing - Playwright)
│   ├── storyboard.spec.ts (UPDATED)
│   ├── voice.spec.ts (UPDATED)
│   ├── animation.spec.ts (NEW)
│   └── render.spec.ts (NEW)
└── a11y/ (NEW - Accessibility)
```

---

## Success Criteria - Phase Completion

### Phase Implemented When:
✅ All planned components built
✅ Hooks connected to API
✅ Real-time updates working (SignalR)
✅ Error handling in place
✅ 80%+ test coverage
✅ Lighthouse score > 90
✅ Zero console errors
✅ UI/UX reviewed + approved

---

## Rollout Strategy

### Stage 1: V1 Enhancements + Phase 6-9
- **Target**: End of Week 9
- **Status**: Pilot-ready
- **Users**: Internal QA team
- **Goal**: All core workflows functional

### Stage 2: Phase 10-11 Completion
- **Target**: End of Week 15
- **Status**: Feature-complete
- **Users**: Beta customers
- **Goal**: All planned features ready

### Stage 3: Phase 12 + Production Hardening
- **Target**: End of Week 18
- **Status**: Production-ready
- **Users**: General availability
- **Goal**: 99.9% SLA, full monitoring

---

## Decision Points - FINALIZED ✅

### 1. Timeline Editor Approach ✅ DECIDED
**Chosen**: Konva.js v9 with @dnd-kit/core
**Reasoning**: 
- Balance of features + learning curve
- Excellent canvas performance for 100+ clips
- Active community + good documentation
- React integration seamless
**Implementation**: Canvas-based multi-track with drag-and-drop
**Alternative Fallback**: DOM-based if performance issues (less likely)

### 2. Video Preview Strategy ✅ DECIDED
**Chosen**: Thumbnail-based (storyboard frames) + playhead seeking for MVP
**Reasoning**:
- Real-time rendering too expensive (GPU intensive)
- Storyboard thumbnails sufficient for visual feedback
- Faster iteration + shipping
**Phase 2 Enhancement**: Add low-res canvas preview in future iteration
**Backend**: Use existing storyboard shots as thumbnails

### 3. Monitoring Stack ✅ DECIDED
**Chosen**: Sentry (errors) + AWS CloudWatch (logs + metrics)
**Reasoning**:
- Sentry excellent for frontend error tracking + session replay
- CloudWatch native to AWS ecosystem
- Cost-effective for SaaS scale
- Less ops burden than ELK
**Implementation**:
- Sentry DSN via `NEXT_PUBLIC_SENTRY_DSN` env var
- CloudWatch via AWS SDK (server-side logs)
- Browser localStorage for local debugging

### 4. Test Coverage Target ✅ DECIDED
**Chosen**: 70% minimum (critical paths + happy flow)
**Reasoning**:
- Coverage > 70% catches regressions
- Focus on critical paths (auth, video workflow)
- 100% is diminishing returns
**Breakdown**:
- Unit tests: 40% (utils, services, hooks)
- Integration tests: 20% (API flows)
- E2E tests: 10% (critical user journeys)

### 5. Parallel vs Sequential Phases ✅ DECIDED
**Chosen**: Parallel with coordination
**Resource Allocation**:
- **Dev 1** (Senior): Phases 6 + 8 (Storyboard + Animation) - complex UX
- **Dev 2** (Mid): Phase 7 + Phase 9 (Voice + Delivery) - straightforward components
- **Lead** (Full-time): V1 Enhancements + coordination + Phase 10 (Timeline) planning

**Execution Timeline**:
```
Week 1-2:   V1 Enhancements (all devs coordinate)
Week 1-2:   Phase 6 starts (Dev 1)
Week 3-4:   Phase 7 starts (Dev 2)
Week 5-6:   Phase 8 starts (Dev 1)
Week 5-6:   Phase 9 starts (Dev 2) - parallel with 8
Week 7-9:   Phase 9 continues, Phase 6-8 complete
Week 10+:   Phase 10 (Timeline) - both devs
Week 13+:   Phase 11 (Sharing) - both devs
Week 16+:   Phase 12 (Analytics) - 1 dev
```

**Note on Claude Sonnet 4.6**: 
- Can handle complex component logic
- Excellent at generating test code
- Should work autonomously on phases with clear specs
- Will need human review for final integration

---

## Ready to Execute?

✅ Complete technical specifications for Phases 6-9
✅ Data models and API contracts defined
✅ Component architecture documented
✅ Implementation order sequenced
✅ Risk mitigation strategies in place
✅ Success criteria defined

**Next Steps**:
1. Review this summary
2. Answer decision points (5 questions above)
3. Confirm team allocation (2-3 devs)
4. Set start date
5. Configure environments (Sentry DSN, etc.)
6. Begin Week 1 sprints

