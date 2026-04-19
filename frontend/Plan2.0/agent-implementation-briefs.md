# AnimStudio V2.0 - Agent Implementation Briefs (Claude Sonnet 4.6)

**Purpose**: Phase-by-phase implementation guides for Claude Sonnet 4.6 coding agent. Each brief includes exact checklist, dependencies, testing requirements, and Claude-specific guidance.

**Coordinating Role**: Lead Developer (architecture review, integration, testing). Developers 1 & 2 execute phases in parallel with Lead coordination.

---

# V1 ENHANCEMENTS (FOUNDATION - 1 WEEK)
## Status: PREREQUISITE - Must complete before any phase can start

**Duration**: 1 week (5 dev-days)  
**Assigned to**: Lead Developer (architecture decisions)  
**Blocks**: All phases (V1-A through V1-E must be in main branch)  

### V1-A: Global Error Handling System

**Purpose**: All errors handled centrally, no white-screen crashes, structured logging to Sentry.

**Implementation Checklist**:

UI Components to Create:
- [ ] `src/app/providers.tsx` - ADD: Error wrapper component
  ```
  export function ErrorBoundary({ children }) {
    return <RootErrorBoundary>{children}</RootErrorBoundary>
  }
  ```
  
- [ ] `src/components/ui/error-boundary.tsx` (NEW)
  - React error boundary wrapper
  - Catches React render errors
  - Shows error UI
  - Logs to Sentry
  - Recovery button

- [ ] `src/components/error-display.tsx` (NEW)
  - Fallback UI when error occurs
  - Shows generic message to users
  - Shows detailed error in dev mode (console only)

Infrastructure Files to Create:
- [ ] `src/lib/logger.ts` (NEW)
  - Functions:
    - `logInfo(message, context?)`
    - `logError(error, context?)`
    - `logWarn(message, context?)`
    - `logDebug(message, context?)`
  - Sends to localStorage (session storage, max 100 entries)
  - Also sends to Sentry if configured
  - Usage: `logger.logError(err, { userId, action: 'render' })`

- [ ] `src/lib/error-context.tsx` (NEW)
  - React context for app-level error state
  - Store: `{ error?: Error, context?: Record }`
  - Methods:
    - `setError(error, context)`
    - `clearError()`
  - Accessible globally via `useErrorContext()` hook

- [ ] `src/lib/sentry.ts` (NEW)
  - Initialize Sentry with DSN from env
  - `Sentry.captureException(error)`
  - `Sentry.captureMessage(msg)`
  - Include user context (userId, teamId)
  - Include breadcrumbs (last 5 actions)

- [ ] `src/constants/error-messages.ts` (NEW)
  - Map error codes to user-friendly messages
  - Example:
    ```
    const ERROR_MESSAGES = {
      RENDER_TIMEOUT: "Render took too long. Please try again.",
      AUTH_FAILED: "Login failed. Please try again.",
      // ... 20+ messages
    }
    ```

API Integration:
- [ ] Modify `src/lib/api-client.ts`:
  - Add timeout to all requests (30 seconds default)
  - Add try-catch wrapper to all calls
  - Return structured error: `{ code, message, details }`
  - Log errors using logger.logError()
  - Don't throw naked errors - wrap them
  
  ```typescript
  export async function apiFetch<T>(endpoint: string, options?): Promise<T> {
    try {
      const controller = new AbortController()
      const timeoutId = setTimeout(() => controller.abort(), 30000)
      
      const response = await fetch(endpoint, {
        ...options,
        signal: controller.signal,
      })
      clearTimeout(timeoutId)
      
      if (!response.ok) {
        logger.logError(new Error(`API Error: ${response.status}`), { endpoint })
        throw new APIError(...)
      }
      return response.json()
    } catch (err) {
      logger.logError(err, { endpoint, action: 'apiFetch' })
      throw wrapError(err, 'API_CALL_FAILED')
    }
  }
  ```

Hooks to Update:
- [ ] All hooks in `src/hooks/*.ts`:
  - Wrap mutations with error handling
  - Use `useErrorContext()` to display errors
  - Example:
    ```
    const { setError } = useErrorContext()
    const mutation = useMutation({
      mutationFn: async (data) => {
        try {
          return await api.createProject(data)
        } catch (err) {
          setError(err, { action: 'createProject' })
          throw err
        }
      }
    })
    ```

Testing:
- [ ] Unit test: `src/lib/__tests__/logger.test.ts`
  - Test log to localStorage
  - Test Sentry integration
  - Min 5 test cases

- [ ] Unit test: `src/lib/__tests__/error-context.test.ts`
  - Test context provider
  - Test setError/clearError
  - Min 4 test cases

- [ ] E2E test: `tests/e2e/error-handling.spec.ts`
  - Simulate API error → should show error UI
  - Simulate render timeout → should show timeout message
  - Simulate network error → should log to Sentry

**Success Criteria**:
- [ ] No unhandled errors in console
- [ ] All errors logged to localStorage
- [ ] Sentry connection works (check Sentry backend)
- [ ] Error UI appears instead of white screen
- [ ] Errors don't break app navigation
- [ ] 80% code coverage on error libs

**Claude Sonnet Notes**:
- Create error boundary first (simplest)
- Then logger + Sentry config
- Last: wire into hooks
- Use existing React Query error patterns as reference
- Make sure logger doesn't throw (defensive coding)

---

### V1-B: Request Cleanup & Abort Manager

**Purpose**: No memory leaks from cancelled requests, proper SignalR cleanup on unmount.

**Files to Create**:

- [ ] `src/lib/abort-manager.ts` (NEW)
  - Class: `AbortManager`
  - Methods:
    - `createController(id: string)` → AbortController
    - `abort(id?: string)` → aborts one or all
    - `clear()` → cleanup
  - Usage:
    ```
    const manager = new AbortManager()
    const ctrl = manager.createController('fetch-1')
    fetch(url, { signal: ctrl.signal })
    // On unmount:
    manager.abort('fetch-1') // or manager.clear()
    ```

- [ ] Update `src/hooks/use-*.ts` hooks:
  - Add `useEffect` cleanup
  - Create abort controller on mount
  - Abort on unmount
  - Example:
    ```
    export function useProjects() {
      const abortRef = useRef(new AbortManager())
      
      useEffect(() => {
        return () => {
          abortRef.current.clear() // cleanup on unmount
        }
      }, [])
      
      const { data } = useQuery({
        queryKey: ['projects'],
        queryFn: () => apiFetch('/projects', {
          signal: abortRef.current.createController('projects').signal
        })
      })
      return { data }
    }
    ```

- [ ] Update `src/hooks/use-signal-r.ts`:
  - Add proper cleanup on unmount
  - Stop listening to events
  - Close connection
  - Example:
    ```
    useEffect(() => {
      return () => {
        if (connection) {
          connection.off('EventName')
          connection.stop()
        }
      }
    }, [connection])
    ```

- [ ] Add React Query cleanup:
  - Modify `src/app/providers.tsx`
  - Set queryClient config:
    ```
    defaultOptions: {
      queries: {
        staleTime: 5 * 60 * 1000,
      }
    }
    ```

Testing:
- [ ] Unit: `src/lib/__tests__/abort-manager.test.ts`
  - Test controller creation
  - Test abort single/all
  - Test clear
  - Min 5 cases

- [ ] Manual: Memory profiler
  - Open DevTools Profiler
  - Navigate to page with hook
  - Unmount component
  - Verify no pending requests
  - Check GraphQL network tab

**Success Criteria**:
- [ ] No pending XHR requests after unmount
- [ ] No SignalR event listeners after unmount
- [ ] No React Query queries after unmount
- [ ] Memory stays stable (profiler test)

---

### V1-C: Unified State Management

**Purpose**: Global UI state, loading states, notifications accessible app-wide.

**Files to Create**:

- [ ] `src/stores/app-store.ts` (NEW)
  - Zustand store for global state
  - State:
    ```
    {
      isLoading: boolean
      notification?: { type, message, duration }
      selectedEpisodeId?: string
      selectedProjectId?: string
      sidebarOpen: boolean
    }
    ```
  - Actions:
    - `showNotification(type, message, duration)`
    - `hideNotification()`
    - `setLoading(flag)`
    - `selectEpisode(id)`
    - `selectProject(id)`
    - `toggleSidebar()`

- [ ] `src/hooks/use-app-store.ts` (NEW)
  - Subscribe to store changes
  - Example:
    ```
    export const useAppStore = () => {
      return useStore(appStore, (state) => state)
    }
    ```

- [ ] Backward-compatible wrapper:
  - Old store accessors still work
  - But delegate to new appStore
  - Ensure no breaking changes to existing code

Testing:
- [ ] Unit: `src/stores/__tests__/app-store.test.ts`
  - Test state updates
  - Test subscribers
  - Min 5 cases

**Success Criteria**:
- [ ] Global loading state accessible from any component
- [ ] Notifications show globally
- [ ] No component imports conflicting stores
- [ ] Existing code still compiles

---

### V1-D: Feature Flags (Security)

**Purpose**: Replace NODE_ENV checks with safe feature flags. Same binary deployable to all environments.

**Files to Create**:

- [ ] `src/lib/feature-flags.ts` (NEW)
  - Source flags from:
    - Environment: `process.env.NEXT_PUBLIC_FEATURE_*`
    - Runtime: localStorage (for testing)
  - Functions:
    - `isFeatureEnabled(name: string): boolean`
    - `getFeatureValue(name: string): any`
  - Flags needed:
    ```
    FEATURE_DEV_AUTH_BYPASS (default: false in prod, true in local)
    FEATURE_STORYBOARD_ENABLED (default: false in v1, true in v2)
    FEATURE_TIMELINE_ENABLED (default: false until phase 10)
    FEATURE_ANALYTICS_ENABLED (default: false until phase 12)
    ```

- [ ] Remove NODE_ENV checks:
  - Search: `process.env.NODE_ENV`
  - Replace with `isFeatureEnabled('DEV_AUTH_BYPASS')`
  - Check in: `src/lib/auth.ts`, API routes

- [ ] `src/middleware.ts` (UPDATE):
  - Add feature flag checks for protected routes
  - Example: Check if admin route, but feature disabled

Testing:
- [ ] Manual:
  - Disable dev bypass in localStorage
  - Verify dev auth button hidden
  - Re-enable, button reappears

**Success Criteria**:
- [ ] No NODE_ENV in production code paths
- [ ] Same build works in all environments
- [ ] Flags can be changed at runtime (localStorage)
- [ ] No console warnings about NODE_ENV

---

### V1-E: Docker & Static Export Fix

**Purpose**: Production Docker container builds, runs, and serves SPA correctly.

**Files to Create/Modify**:

- [ ] `next.config.mjs` (MODIFY):
  - Set: `output: 'export'`
  - But keep API routes (hybrid mode)
  - Set: `basePath: process.env.NEXT_PUBLIC_BASE_PATH || ''`

- [ ] `Dockerfile` (MODIFY):
  - Multi-stage build
  - Stage 1: Node build
    - Install deps
    - Build Next.js (export)
  - Stage 2: Nginx
    - Copy exported files
    - Serve with SPA routing

- [ ] `nginx.conf` (NEW):
  - SPA routing (rewrite all to index.html except api/)
  - Cache headers:
    - JS/CSS: 1 year (immutable)
    - HTML: no-cache
    - Images: 1 week
  - GZIP compression enabled
  - Security headers (CSP, X-Frame-Options)

- [ ] `.dockerignore` (NEW):
  - node_modules, .next, .git, etc

Testing:
- [ ] Build & run Docker:
  ```bash
  docker build -t animstudio-fe .
  docker run -p 3000:3000 animstudio-fe
  ```
- [ ] Verify:
  - App loads on http://localhost:3000
  - SPA routing works (visit /projects, page loads)
  - API calls work (Network tab in DevTools)
  - Static assets cached (headers show immutable)

**Success Criteria**:
- [ ] Docker container builds without errors
- [ ] App loads in browser
- [ ] SPA routing works (no 404s)
- [ ] API calls reach backend
- [ ] Image size < 100MB

---

## V1 Integration Checklist

After all V1-A through V1-E complete:

- [ ] Merge to `development` branch
- [ ] Run full E2E test suite
- [ ] No console errors
- [ ] Lighthouse score: 85+ (performance)
- [ ] Deployment to staging succeeds
- [ ] QA sign-off

**Lead Dev Task**: Review all 5 PRs, ensure cohesive architecture, no conflicts.

---

# PHASE 6 - STORYBOARD STUDIO
## (Week 1-2, 2 weeks, Dev 1 - Complex UX work)

**Duration**: 2 weeks (10 dev-days)  
**Assigned to**: Developer 1 (owns storyboard feature end-to-end)  
**Depends on**: V1 Enhancements complete  
**Blocks**: Phase 10 Timeline (uses storyboard shots as thumbnails)  
**Slack Channel**: #storyboard
**Daily Standup**: 9 AM Pacific

### High-Level Goal

Users can view storyboard scenes and shots, override styles per-shot, trigger regeneration. Real-time updates via SignalR.

### Component Implementation Map

```
Page: /studio/[id]/storyboard
├─ SceneTab (select which scene)
├─ ShotGrid (show all shots in selected scene)
│  └─ ShotCard (individual shot preview)
│     ├─ onClick → ShotViewerModal
│     └─ Menu → StyleOverrideDialog / RegenerateDialog
├─ ShotViewerModal (full-screen shot preview)
│  └─ Actions: Close, Override style, Regenerate
└─ Real-time updates (SignalR) → re-fetch shots

Hook: use-storyboard.ts (MODIFY - exists, needs hooks)
Types: Add to src/types/index.ts
```

### Implementation Checklist

**Week 1 - Thursday**:

UI Components:
- [ ] `src/components/storyboard/scene-tab.tsx`
  - Tabs showing scene numbers
  - Active state highlight
  - onClick handler
  - Props: `scenes[], onSelectScene, activeSceneId`
  - Styling: Use existing button patterns

- [ ] `src/components/storyboard/shot-grid.tsx`
  - Responsive grid (4 cols lg, 3 md, 2 sm)
  - Contains shot-card components
  - Scrollable if 15+ shots
  - Props: `shots[], onSelectShot, selectedShotId`
  - Empty state: "No shots in this scene"

- [ ] `src/components/storyboard/shot-card.tsx`
  - 240x180px card
  - Thumbnail image on load
  - Scene/duration badges on hover
  - 3-dot menu (style override, regenerate, delete)
  - Props: `shot, onSelect, onOverrideStyle, onRegenerate`
  - States: Loading (skeleton), Error (broken image), Normal

**Week 1 - Friday**:

Modals:
- [ ] `src/components/storyboard/shot-viewer-modal.tsx`
  - Full-screen modal
  - Large shot image (centered)
  - Action buttons (Close, Override Style, Regenerate)
  - Shot info (scene #, duration, created date)
  - Props: `shot, isOpen, onClose, onOverrideStyle, onRegenerate`

- [ ] `src/components/storyboard/style-override-dialog.tsx`
  - Character list (dropdown per character)
  - Props selection
  - Save/Cancel buttons
  - API call to update style
  - Props: `shot, isOpen, onClose`

- [ ] `src/components/storyboard/regenerate-dialog.tsx`
  - Confirmation modal
  - Shows estimated time (30-60 seconds)
  - Progress bar while regenerating
  - Props: `shotId, isOpen, onClose`

**Week 2 - Monday**:

Hooks & State:
- [ ] Modify `src/hooks/use-storyboard.ts`:
  - Add functions:
    - `getShots(sceneId)` → Array<Shot>
    - `overrideStyle(shotId, overrides)` → Promise
    - `regenerateShot(shotId)` → Promise
  - Use React Query for caching
  - Handle SignalR updates (auto-refetch on ShotUpdated event)

- [ ] Add types to `src/types/index.ts`:
  ```
  interface Shot {
    id: string
    episodeId: string
    sceneId: string
    url: string
    duration: number
    characterOverrides?: Map<string, StyleId>
    createdAt: Date
  }
  interface StyleOverride {
    characterId: string
    styleId: string
  }
  ```

**Week 2 - Tuesday**:

Page Integration:
- [ ] `src/app/(dashboard)/studio/[id]/storyboard/page.tsx`
  - Import all components
  - Layout: SceneTab + ShotGrid + Modals
  - State: `[selectedSceneId, setSelectedSceneId]`
  - Event handlers wired to hooks

Real-time Features:
- [ ] SignalR integration:
  - Listen to `ShotUpdated` events
  - On event: refetch shots with React Query
  - Auto-dismiss success toast after

**Week 2 - Wednesday**:

Testing:
- [ ] E2E test: `tests/e2e/storyboard.spec.ts`
  ```
  - Test 1: Load storyboard, see scenes
  - Test 2: Select scene, see shots
  - Test 3: Click shot card, open modal
  - Test 4: Override style, see API call
  - Test 5: Regenerate, see progress → complete
  - Test 6: Real-time update (SignalR)
  ```

- [ ] Unit tests (20 tests minimum):
  - SceneTab: Select scene, highlight active
  - ShotGrid: Render grid, responsive columns
  - ShotCard: Show image, handle menu clicks
  - Modals: Open/close, form submissions

**Week 2 - Thursday/Friday**:

Integration & Refinement:
- [ ] Code review with Lead Dev
  - Check error handling matches V1-A patterns
  - Verify SignalR cleanup (V1-B patterns)
  - Performance: No re-renders (React.memo)

- [ ] Manual testing checklist:
  - [ ] Load storyboard page, no errors
  - [ ] Scenes load, thumbnails render
  - [ ] Can select scene and see different shots
  - [ ] Can click shot, modal opens
  - [ ] Can override style → backend updates
  - [ ] Regeneration progress shows
  - [ ] Real-time update works (multi-browser test)
  - [ ] Responsive at 320px, 768px, 1200px
  - [ ] No console errors/warnings

- [ ] Merge PR to development

### Key Dependencies

**Backend APIs** (must exist):
- `GET /episodes/{id}/storyboard/scenes` → Array<Scene>
- `GET /episodes/{id}/storyboard/scenes/{sceneId}/shots` → Array<Shot>
- `PUT /storyboard/shots/{id}/style` → Update style override
- `POST /storyboard/shots/{id}/regenerate` → Queue regeneration
- SignalR: `ShotUpdated` event

**React Query Setup**:
- `queryKeys.storyboard.scenes(episodeId)`
- `queryKeys.storyboard.shots(episodeId, sceneId)`

### Claude Sonnet 4.6 Guidance

**Strengths to Leverage**:
- Excellent at generating multiple component variants (scenario: different shot states)
- Strong TypeScript generics (for ShotCard<T> renderer pattern)
- Good at test generation (write comprehensive @testing-library tests)

**When You Generate Components**:
1. **Shell first**: Create all component stubs with Props interfaces
2. **Dumb components**: Build ShotCard, ShotViewerModal (pure, no hooks)
3. **Smart components**: Build ShotGrid, SceneTabs (with useState)
4. **Hooks**: Implement use-storyboard last (async complexity)
5. **Tests**: Generate as you go (parallel to implementation)

**Integration Checklist for You**:
1. All components import from correct paths
2. Props interfaces match actual usage
3. Error handling uses V1-A logger pattern
4. SignalR cleanup follows V1-B pattern
5. No console errors on mount/unmount
6. Responsive at all breakpoints
7. 80%+ test coverage (focus on critical paths)

**Known Challenges**:
- Image loading states (might miss skeleton UI) → remind me to add
- SignalR event handling (might not cleanup properly) → follow V1-B pattern strictly
- Modal overlay (might not trap focus) → use existing dialog component from shadcn/ui
- Keyboard navigation (might miss) → ensure Tab order works

### Definition of Done

PR passes when ALL criteria met:
- [ ] All 5 components build without errors
- [ ] Use hooks connect to real backend APIs
- [ ] Real-time updates work (manual test with 2 browsers)
- [ ] No console errors or warnings
- [ ] Responsive at 320/768/1200px breakpoints
- [ ] 80% test coverage minimum
- [ ] Code review approved by Lead Dev
- [ ] Can merge to development branch immediately

---

# PHASE 7 - VOICE STUDIO
## (Week 3-4, 2 weeks, Dev 2 - Straightforward UI work)

**Duration**: 2 weeks (10 dev-days)  
**Assigned to**: Developer 2 (parallel with Phase 6)  
**Depends on**: V1 Enhancements, Phase 6 complete  
**Blocks**: Phase 8  
**Slack Channel**: #voice-studio

### High-Level Goal

Assign voice talents to characters and select language. Real-time audio preview and voice cloning support.

### Component Implementation Map

```
Page: /studio/[id]/voice
├─ VoiceRosterTable
│  └─ Per character: Dropdown VoicePickerDropdown
├─ AudioPreviewPlayer (stand-alone)
├─ LanguageSelector (dropdown)
├─ VoiceCloneUploadModal (on-demand)
└─ Real-time updates (SignalR)

Hook: use-voice-assignments.ts (MODIFY - exists)
```

### Implementation Checklist

**Week 3**:

UI Components:
- [ ] `src/components/voice/voice-roster-table.tsx`
  - Static table: Character | VoiceTalent | Language | Duration | Action
  - Props: `assignments[], characters[], onAssignVoice`
  - Use shadcn table component

- [ ] `src/components/voice/voice-picker-dropdown.tsx`
  - Dropdown showing voice talents
  - Search bar
  - "Create New" button
  - Props: `selectedId?, onSelect`

- [ ] `src/components/voice/language-selector.tsx`
  - Dropdown with language list
  - Grouped by region
  - Props: `selectedLanguage, onSelect`
  - Use existing select component

- [ ] `src/components/voice/audio-preview-player.tsx`
  - Play/pause button
  - Progress bar with scrubber
  - Volume control
  - Shows duration
  - Props: `audioUrl, title?`

**Week 3 - Friday**:

Modal & Hooks:
- [ ] `src/components/voice/voice-clone-upload.tsx`
  - Drag-drop file upload
  - Progress indicator
  - "Create Voice Clone" button
  - Lists uploaded files
  - Props: `isOpen, onClose, onUploadComplete`

- [ ] Modify `src/hooks/use-voice-assignments.ts`:
  - Add: `assignVoice(characterId, voiceId, language)`
  - Add: `getVoiceTalents()` (cached list)
  - Add: `uploadVoiceSample(files)`

**Week 4**:

Page & Real-time:
- [ ] `src/app/(dashboard)/studio/[id]/voice/page.tsx`
  - Assemble VoiceRosterTable
  - Show LanguageSelector at top
  - AudioPreviewPlayer for testing
  - "Upload Voice" button → modal

- [ ] SignalR integration:
  - Listen to `VoiceAssignmentUpdated`
  - Refetch roster on update

Testing:
- [ ] E2E tests (8 scenarios):
  - Load voice page
  - Select voice for character
  - Change language
  - Preview audio
  - Upload voice sample
  - Multi-browser real-time update

- [ ] Unit tests (15 tests):
  - VoicePickerDropdown: Search works
  - VoiceRosterTable: Renders assignments
  - LanguageSelector: Change language
  - AudioPreview: Play/pause/scrub

### Definition of Done

- [ ] 5 components without errors
- [ ] All dropdowns work (click/keyboard)
- [ ] Audio preview plays
- [ ] Voice assignment persists
- [ ] 80% test coverage
- [ ] Merge to development

---

# PHASE 8 - ANIMATION APPROVAL
## (Week 5-6, 2 weeks, Dev 1 - Complex component integration)

**Duration**: 2 weeks (10 dev-days)  
**Assigned to**: Developer 1 (after Storyboard complete)  
**Depends on**: V1 Enhancements, Phase 6, Phase 7  
**Blocks**: Phase 9

### High-Level Goal

Queue render jobs, show progress in real-time, and provide download management UI.

### Component Implementation Map

```
Page: /studio/[id]/render
├─ RenderEstimateCard (before starting)
├─ RenderApprovalDialog (confirmation)
├─ RenderProgressComponent (during render)
├─ RenderPreviewPlayer (after render)
└─ RenderHistoryTable (all previous renders)

Hook: use-renders.ts (NEW)
```

### Implementation Checklist

**Week 5**:

UI Components:
- [ ] `src/components/render/render-estimate-card.tsx`
  - Show: Duration, Cost, Quality, Est. completion
  - Props: `episodeId`
  - Button: "Start Render"
  - Styling: Card with 3-column stats

- [ ] `src/components/render/render-approval-dialog.tsx`
  - Modal with:
    - Thumbnail preview
    - Format dropdown (MP4, WebM, ProRes)
    - Resolution dropdown (1080p, 2K, 4K)
    - Frame rate dropdown
    - Checkboxes (watermark, CDN upload)
  - Props: `isOpen, onApprove, onCancel`

- [ ] `src/components/render/render-progress-component.tsx`
  - Title: "Rendering Scene X of Y"
  - Progress bar (0-100%)
  - Percentage text
  - Time elapsed / remaining
  - Speed display (x1.2 real-time)
  - Props: `renderId`
  - Real-time updates via SignalR

- [ ] `src/components/render/render-preview-player.tsx`
  - Video player with standard controls
  - 16:9 responsive
  - Props: `videoUrl, title?`

- [ ] `src/components/render/render-history-table.tsx`
  - Table: Date | Name | Status | Format | Duration | Action
  - Props: `renders[]`
  - Download button per row

**Week 5 - Friday**:

Hooks:
- [ ] Create `src/hooks/use-renders.ts` (NEW):
  - `useRenders(episodeId)` → Array<Render>
  - `startRender(settings)` → Promise<renderId>
  - `getRenderProgress(renderId)` → Live progress
  - Use React Query + SignalR

**Week 6**:

Page & Real-time:
- [ ] `src/app/(dashboard)/studio/[id]/render/page.tsx`
  - EstimateCard (if no active render)
  - ProgressComponent (if rendering)
  - PreviewPlayer (if completed)
  - HistoryTable (past renders)

- [ ] SignalR integration:
  - Listen to `RenderProgress` events
  - Listen to `RenderComplete` events
  - Auto-refetch progress

Testing:
- [ ] E2E tests (7 scenarios):
  - Load render page
  - See estimate
  - Click render → approval dialog
  - Approve render → queue
  - Watch progress bar
  - Complete render → preview
  - Download render

- [ ] Unit tests (18 tests):
  - Estimate card shows correct values
  - Approval dialog form works
  - Progress bar updates
  - History table displays renders

### Definition of Done

- [ ] 5 components without errors
- [ ] Render job queued successfully
- [ ] Real-time progress working (check SignalR)
- [ ] Preview video plays
- [ ] Download works
- [ ] 80% test coverage
- [ ] Merge to development

---

# PHASE 9 - DELIVERY & EXPORT
## (Week 5-6, 2 weeks, Dev 2 - Parallel with Phase 8)

**Duration**: 2 weeks (10 dev-days)  
**Assigned to**: Developer 2 (parallel execution)  
**Depends on**: V1 Enhancements, Phase 6, Phase 7, Phase 8  
**Blocks**: Phase 10

### High-Level Goal

Select export formats and manage delivery (download, share, social publish).

### Component Implementation Map

```
Page: /studio/[id]/delivery
├─ AspectRatioPicker (dropdown)
├─ OutputFormatCard (grid of format options)
├─ DownloadProgressBar (file download status)
└─ RenderHistoryTable (all renders)

No new hooks needed - reuse use-renders.ts
```

### Implementation Checklist

**Week 5**:

UI Components:
- [ ] `src/components/delivery/aspect-ratio-picker.tsx`
  - Dropdown: 16:9 | 9:16 | 1:1 | 4:3 | 21:9
  - Props: `selectedRatio, onSelect`
  - Show preview icons

- [ ] `src/components/delivery/output-format-grid.tsx`
  - Grid of format cards (2-3 per row)
  - Each card:
    - Format name (MP4, WebM, ProRes)
    - Codec details
    - File size estimate
    - Select button
  - Props: `onSelectFormat`

- [ ] `src/components/delivery/download-progress-bar.tsx`
  - Shows filename + progress
  - Speed, time remaining
  - Pause/Resume buttons
  - Cancel option
  - Props: `filename, progress, speed, timeRemaining`

- [ ] `src/components/delivery/delivery-status-card.tsx`
  - Shows current export settings
  - Aspect ratio
  - Format
  - Quality
  - Props: format config object

**Week 5 - Friday**:

Page:
- [ ] `src/app/(dashboard)/studio/[id]/delivery/page.tsx`
  - AspectRatioPicker (top)
  - OutputFormatGrid (middle)
  - DeliveryStatusCard (preview)
  - DownloadProgressBar components (list)
  - RenderHistoryTable

Testing:
- [ ] E2E tests (6 scenarios):
  - Select aspect ratio
  - Select output format
  - Show delivery details
  - Download file
  - Multi-file download management
  - Cancel download

- [ ] Unit tests (12 tests):
  - Aspect ratio picker
  - Format selection
  - Progress calculations

### Definition of Done

- [ ] 4 components without errors
- [ ] Format selection works
- [ ] Download management functional
- [ ] Progress bar shows realistic values
- [ ] 80% test coverage
- [ ] Merge to development

---

# PHASE 10 - TIMELINE EDITOR
## (Week 10-15, 6 weeks, Dev 1 + Dev 2 - Most complex feature)

**Duration**: 6 weeks (30 dev-days)  
**Assigned to**: Both developers (intensive coordination with Lead)  
**Depends on**: All prior phases (6-9)  
**Slack Channel**: #timeline-editor
**Daily Standup**: 9-9:30 AM Pacific (critical phase)

### High-Level Goal

Full-featured video timeline editor with drag-drop, trimming, transitions, music, text overlays.

### Architecture Overview

**Canvas Engine**: Konva.js v9
- Pros: Smooth rendering, good for 2D animation, mature library
- Cons: Learning curve, large bundle (~180kb)

**Drag & Drop**: @dnd-kit/core
- Pros: Headless, no UI assumptions, good with Konva
- Cons: Requires manual drop zone handling

**State Management**: Zustand (timeline state) + React Context (canvas state)

**Real-time**: SignalR for multi-user collaboration (future)

### Implementation Approach

**Week 10 - Foundation**:

- [ ] Set up Konva.js + @dnd-kit/core
- [ ] Create `TimelineContext` for shared canvas state
- [ ] Create `useTimeline` hook (data fetching + mutations)
- [ ] Create `TimelineCanvas` component (Konva Stage + Layers)

**Week 11 - Tracks & Clips**:

- [ ] Render tracks as Konva Rectangles
- [ ] Render clips as draggable Konva Groups
- [ ] Implement clip dragging (reposition on timeline)
- [ ] Implement trim handles (left/right edges)

**Week 12 - Editor Features**:

- [ ] Ruler with time markers
- [ ] Playhead scrubber (click timeline to seek)
- [ ] Play/pause functionality
- [ ] Zoom controls (affects pixel-per-ms ratio)

**Week 13 - Advanced**:

- [ ] Music panel (add BGM tracks)
- [ ] Text overlay editor (add titles/captions)
- [ ] Transition picker (Cut/Fade/Dissolve)
- [ ] Undo/redo history stack

**Week 14 - Integration**:

- [ ] Real-time updates (SignalR)
- [ ] Save timeline state
- [ ] Auto-save every 30s
- [ ] Error recovery

**Week 15 - Testing & Polish**:

- [ ] E2E comprehensive test suite
- [ ] Performance optimization (no lag with 30+ clips)
- [ ] Responsive design (works on 1200px+ screens)
- [ ] Polish UX (keyboard shortcuts, tooltips)

### Component Checklist

**Core Canvas**:
- [ ] TimelineCanvas.tsx (Konva Stage)
- [ ] TimelineRuler.tsx (Time markers)
- [ ] TrackPanel.tsx (Track controls)
- [ ] Playhead.tsx (Red scrubber line)

**Clips & Editing**:
- [ ] ClipComponent.tsx (Konva clip shape)
- [ ] TrimHandle.tsx (Left/right edge)
- [ ] ClipContextMenu.tsx (Right-click menu)
- [ ] TransitionPicker.tsx (Dropdown selector)

**Extras**:
- [ ] MusicPanel.tsx (Add music tracks)
- [ ] TextOverlayPanel.tsx (Add text)
- [ ] TimelineToolbar.tsx (Play/Undo/Save)
- [ ] HistoryStack.ts (Undo/redo manager)

**Page**:
- [ ] timeline/page.tsx (Layout + orchestration)

### Implementation Details (Claude Attention)

**Canvas Rendering Loop**:
```typescript
// Konva updates on each state change
useEffect(() => {
  if (!stageRef.current) return
  
  // Draw all tracks
  timeline.tracks.forEach((track, idx) => {
    const y = idx * (TRACK_HEIGHT + GAP)
    
    // Draw clips
    track.clips.forEach(clip => {
      const clipX = clip.startMs * PIXELS_PER_MS
      const clipWidth = clip.durationMs * PIXELS_PER_MS
      
      // Create clip shape, add to layer
      const shape = new Konva.Rect({
        x: clipX, y, width: clipWidth, height: TRACK_HEIGHT
        // ... properties
      })
    })
  })
  
  // Draw playhead on top
  // ... 
  
  stageRef.current.batchDraw() // Efficient rendering
}, [timeline, zoom])
```

**Drag & Drop Integration**:
```typescript
// Use @dnd-kit to manage drag operations
// But render with Konva (dnd-kit is headless)
handle ClipNode's onDragEnd:
  - Calculate new position
  - Call updateClip mutation
  - Zustand updates state
  - Canvas re-renders via useEffect above
```

**Common Mistakes to Avoid**:
1. **Don't** re-create all shapes on every render → reuse Konva nodes
2. **Don't** forget to call `batchDraw()` → improves performance
3. **Don't** miss trim handle positioning → must be on clip edges exactly
4. **Don't** forget playhead z-index → must be on top of clips

### Testing Strategy

**Unit Tests**:
- TimelineCanvas mounting/unmounting
- HistoryStack push/pop/undo/redo
- Zoom calculations (PIXELS_PER_MS)
- Clip positioning math

**Integration Tests**:
- Drag clip → verify position updates
- Trim clip → verify duration changes
- Add music → verify track appears
- Play → verify playhead advances

**E2E Tests**:
- Full workflow: Load → Drag clip → Trim → Add music → Save
- Multi-user (2 tabs) → see real-time updates
- Performance (30 clips) → no lag when dragging

### Definition of Done for Phase 10

- [ ] Timeline page loads without errors
- [ ] Tracks render (4 tracks visible)
- [ ] 5+ clips visible and draggable
- [ ] Trimming works (handles adjust in/out)
- [ ] Ruler and playhead functional
- [ ] Play/pause working
- [ ] Zoom controls responsive
- [ ] Music panel adds tracks
- [ ] Text overlays added
- [ ] Transitions applied
- [ ] Save saves to backend
- [ ] Real-time multi-tab sync
- [ ] No lag with 30+ clips
- [ ] Responsive at 1200px+
- [ ] 75% test coverage (complex phase, acceptable lower target)
- [ ] Code review approved

---

# PHASE 11 - SHARING & REVIEW LINKS
## (Week 13-15, 3 weeks, Dev 2 - Social features + OAuth)

**Duration**: 3 weeks (15 dev-days)  
**Assigned to**: Developer 2 (parallel with Phase 10 final weeks)  
**Depends on**: Phase 9 complete, Phase 10 partial (doesn't need full timeline)

### High-Level Goal

Share videos publicly via review links with password/expiry protection, commenting, YouTube publishing.

### Component Implementation Map

```
Page: /studio/[id]/share
├─ ReviewLinkGenerator (create link)
├─ ReviewLinkList (manage active links)
├─ YouTubePublish (publish to YouTube)
└─ BrandKitEditor (configure watermark/colors)

Page: /review/[token] (public, no auth)
├─ VideoPlayer (full-width)
└─ CommentPanel (right sidebar)

Hook: useReviewLinks.ts (NEW)
Hook: usePublishYouTube.ts (NEW)
```

### Key Features

**Review Links**:
- Generate link with optional:
  - Expiry date (7, 14, 30 days or never)
  - Password protection
- Show list of active links
- Copy link to clipboard
- Metrics: view count per link

**Public Review Page** (`/review/[token]`):
- No authentication required
- Show video player
- Show comment thread (timestamps on seek bar)
- Add comment form (name + comment + timestamp)
- Comments persist

**YouTube Publishing**:
- OAuth flow (user clicks "Connect YouTube")
- Publishes render as unlisted/public
- Sets title, description, tags
- Optional: Add watermark/branding

**Brand Kit**:
- Upload logo
- Configure primary/secondary colors
- Watermark position + opacity
- Applied to all shared videos

### Implementation Checklist

**Week 13**:

Review Links:
- [ ] `src/components/share/review-link-generator.tsx`
  - Checkboxes for expiry + password
  - Generate button
  - Shows generated link + copy button

- [ ] `src/components/share/review-link-list.tsx`
  - Table of active links
  - Columns: Created | Expires | Password | Views | Actions

- [ ] `src/app/(dashboard)/studio/[id]/share/page.tsx`
  - Layout: Generator + List + YouTube + Brand Kit

**Week 13 - Friday**:

Public Review Page:
- [ ] `src/app/review/[token]/page.tsx`
  - No auth required (public)
  - Fetch review data by token
  - If password protected, show password modal first

- [ ] `src/components/share/comment-panel.tsx`
  - List of comments with timestamps
  - Add comment form (name optional, use anonymous if not)
  - Sort by timestamp

**Week 14**:

OAuth & Publishing:
- [ ] YouTube OAuth setup:
  - `/api/auth/youtube` endpoint
  - Refresh tokens stored securely
  - User can disconnect account

- [ ] `src/components/share/publish-youtube.tsx`
  - Form: title, description, tags, visibility
  - Connect button (if not logged in)
  - Publish button (queues job)

**Week 14 - Friday**:

Branding:
- [ ] `src/components/share/brand-kit-editor.tsx`
  - Logo upload drag-drop
  - Color pickers (primary, secondary)
  - Watermark position + opacity
  - Save button

- [ ] Hooks:
  - `useReviewLinks(renderId)` (create, list, delete)
  - `useBrandKit(teamId)` (fetch, update, upload logo)
  - `useYouTubeAuth()` (connect, disconnect, publish)

**Week 15**:

Testing:
- [ ] E2E tests (10 scenarios):
  - Generate review link
  - Access public review page (token)
  - Password protection works
  - Add comment (shows in list)
  - Connect to YouTube
  - Publish video to YouTube
  - Update brand kit colors
  - Watermark rendering

- [ ] Unit tests (20 tests):
  - Link generation
  - Comment display + formatting
  - Form validation (title required)
  - Brand kit colors applied

### Definition of Done

- [ ] Review link page works
- [ ] Public review page accessible via token
- [ ] Password protection functional
- [ ] Comments thread working
- [ ] YouTube publish functional
- [ ] Brand kit saves + applies
- [ ] 80% test coverage
- [ ] Merge to development

---

# PHASE 12 - ANALYTICS & ADMIN DASHBOARD
## (Week 16-18, 3 weeks, Dev 1 - Data visualization)

**Duration**: 3 weeks (15 dev-days)  
**Assigned to**: Developer 1 (after Phase 10 complete)  
**Depends on**: Phase 9 complete

### High-Level Goal

Track video metrics (views, shares, engagement). Admin dashboard for system monitoring.

### Component Implementation Map

```
Creator Analytics Page: /studio/[id]/analytics
├─ MetricCard (views, viewers, shares, watch time)
├─ ViewsChart (line graph)
├─ EngagementMetrics (grid of stats)
└─ ReferrersList (top sources)

Admin Dashboard: /admin (admin only)
├─ AdminStatsCards (DAU, MAU, subscriptions)
├─ JobQueueTable (render job queue)
├─ ErrorRateChart
├─ SubscriptionStats
└─ NotificationBell (real-time)

Hooks: useAnalytics.ts, useAdminStats.ts, useNotifications.ts
```

### Features

**Creator Analytics**:
- Views over 24h/7d/30d
- Unique viewers
- Share count
- Average watch time
- Top referrers
- Engagement rate

**Admin Dashboard** (team-admins only):
- DAU/MAU counts
- Subscription breakdown (Free/Pro/Studio)
- Job queue depth + pending jobs
- Error rate trending
- System health status

**Notifications**:
- Job complete (video ready to download)
- Billing alerts (quota approaching 100%)
- Team invites
- System messages
- Real-time via SignalR

**Usage Alerts**:
- Show episode quota usage
- Warn at 80%, block at 100%
- Suggest upgrade path

### Implementation Checklist

**Week 16**:

Metric Components:
- [ ] `src/components/analytics/metric-card.tsx`
  - Display number + trend (% change)
  - Color code (green up, red down)

- [ ] `src/components/analytics/views-chart.tsx`
  - Use recharts library for line graph
  - X-axis: time (hours or days)
  - Y-axis: view count
  - Toggle buttons (24h / 7d / 30d)

- [ ] `src/components/analytics/engagement-metrics.tsx`
  - 2x2 grid of metrics (shares, embeds, watch time, comments)

- [ ] `src/components/analytics/referrers-list.tsx`
  - Table: Referrer source | Count | %
  - Sort by count descending

**Week 16 - Friday**:

Admin Components:
- [ ] `src/components/admin/admin-stats-cards.tsx`
  - 4 cards: DAU | MAU | Active Subs | MRR

- [ ] `src/components/admin/job-queue-table.tsx`
  - Table of pending/active render jobs
  - Columns: ID | Episode | Status | Started | Action

- [ ] `src/components/admin/error-rate-chart.tsx`
  - Line graph of error rate over last 7 days

- [ ] `src/components/admin/subscription-stats.tsx`
  - Pie or horizontal bar showing tier breakdown

**Week 17**:

Notifications:
- [ ] `src/components/notifications/notification-bell.tsx`
  - Icon button with unread count badge
  - Dropdown panel on click

- [ ] `src/components/notifications/notification-panel.tsx`
  - List of notifications
  - Each: icon + title + body + timestamp
  - Mark as read on click

- [ ] `src/components/usage/usage-alert.tsx`
  - Show if quota > 80%
  - Yellow warning at 80%, red critical at 100%
  - Color-coded message

**Week 17 - Friday**:

Pages & Hooks:
- [ ] `src/app/(dashboard)/studio/[id]/analytics/page.tsx`
  - AssembleMetricCards + ViewsChart + EngagementMetrics + ReferrersList

- [ ] `src/app/(dashboard)/admin/page.tsx` (NEW)
  - Only accessible to team admins
  - Mid-page guard check

- [ ] `src/hooks/use-analytics.ts` (NEW):
  - `useVideoAnalytics(episodeId)` → fetch metrics
  - `useTeamAnalytics(teamId)` → fetch team-level metrics

- [ ] `src/hooks/use-admin.ts` (NEW):
  - `useAdminStats()` → fetch system stats (admin only)

- [ ] `src/hooks/use-notifications.ts` (NEW):
  - `useNotifications()` → fetch + SignalR real-time
  - Methods: `markAsRead(id)`

**Week 18**:

Testing:
- [ ] E2E tests (8 scenarios):
  - Load creator analytics
  - View graphs (24h/7d/30d)
  - Load admin dashboard
  - See job queue
  - Receive real-time notification
  - Quota alert displays

- [ ] Unit tests (18 tests):
  - Metric card calculations
  - Chart rendering
  - Admin guards (non-admins can't access)
  - Notifications display

### Definition of Done

- [ ] Analytics page works
- [ ] Admin dashboard restricted (admin only)
- [ ] Real-time notifications working
- [ ] Usage alert displays correctly
- [ ] 80% test coverage
- [ ] Merge to development

---

## Post-Phase Checklist (After All 12 Complete)

**Code Quality**:
- [ ] Full E2E test suite passes (all 50+ tests)
- [ ] No console errors (warnings acceptable)
- [ ] Lighthouse score 85+
- [ ] Type coverage 95%+
- [ ] No security warnings (eslint, OWASP)

**Documentation**:
- [ ] README updated with new features
- [ ] API integration doc for backend team
- [ ] Component storybook (optional, nice-to-have)
- [ ] Deployment guide

**Production Readiness**:
- [ ] QA sign-off (manual testing passed)
- [ ] Load testing (can handle 100 concurrent users)
- [ ] Error monitoring (Sentry) configured
- [ ] Performance budget met
- [ ] Accessibility audit (WAVE)

**Release**:
- [ ] Deploy to staging
- [ ] UAT with stakeholders
- [ ] Create release notes
- [ ] Deploy to production

---

## Cross-Phase Dependencies Summary

| Phase | Depends On | Blocks |
|-------|-----------|--------|
| V1-A/B/C/D/E | None | All phases |
| 6: Storyboard | V1 | 10: Timeline |
| 7: Voice | V1 | 8: Animation |
| 8: Animation | V1, 6, 7 | 9: Delivery |
| 9: Delivery | V1, 6, 7, 8 | 10: Timeline |
| 10: Timeline | V1, 6, 7, 8, 9 | 11: Sharing |
| 11: Sharing | V1, 9, 10 | 12: Analytics |
| 12: Analytics | V1 | - |

---

## Claude Sonnet 4.6 - Universal Guidance

### You're Great At - Leverage These Strengths

**Multi-Component Generation**:
- Asked to create 5 components? Generate shells first, then implement
- Test generation in parallel with component code
- Avoid single-file monoliths

**TypeScript & Type Safety**:
- Use generics for reusable components: `ClipCard<T>`
- Leverage discriminated unions for state
- Type-safe hooks with inference

**Test Generation**:
- Write comprehensive tests with @testing-library
- Cover: render, interactions, edge cases
- Aim for 80%+ coverage

### Challenges - Reminders Needed

**Challenge #1: Over-Engineering**:
- Might add unnecessary abstractions
- **Solution**: Keep it simple initially, refactor if needed
- **Trigger**: If component has 200+ lines, ask about simplification

**Challenge #2: Missing Edge Cases**:
- Might miss error states, empty states, loading states
- **Solution**: Explicitly list required states in brief
- **Trigger**: Always ask for graceful degradation

**Challenge #3: Not Considering Performance**:
- Might not memo components, causing re-renders
- **Solution**: Use React.memo for expensive components
- **Trigger**: For lists of items, always memo Individual item components

**Challenge #4**: Test Coverage Gaps
- Might miss keyboard navigation, accessibility
- **Solution**: Specifically ask for a11y testing
- **Trigger**: Interactive components need keyboard + screen reader tests

### When Generating Code

**Order of Operations**:
1. Define types/interfaces first (TS-first approach)
2. Create component shells with Props interfaces
3. Implement logic in components
4. Add hooks (data fetching)
5. Write tests (mirrors implementation)
6. Add error handling (wraps everything)

**Validation Gates**:
- After each step, ask "Does this compile?"
- Review imports (all correct paths?)
- Check for TypeScript errors (strict mode)

### When You Encounter Issues

**"Import not found"**:
- Check file paths are relative: `@/components/...`
- Verify shadcn/ui components exist

**"Type mismatch"**:
- Likely Props interface doesn't match usage
- Check caller vs component definition

**"No module named..."**:
- Missing dependency (recharts, konva, etc)
- Run `npm install` if needed

**Network Error**:
- Backend API not responding
- Spy on Network tab, verify endpoint exists

### File Structure Conventions

Follow existing project structure:
```
src/
├─ components/     (UI components, dumb + smart)
├─ app/           (Next.js pages)
├─ hooks/         (React hooks, data fetching)
├─ lib/           (Utilities, helpers)
├─ types/         (TypeScript interfaces)
├─ stores/        (Zustand state)
└─ contexts/      (React context)
```

### Working with Lead Developer

**What Lead Will Do**:
- Architecture review (after you submit PR)
- Integration testing (with other phases)
- Performance optimization
- Security audit

**What You Should NOT Do**:
- Don't modify V1 code (that's complete)
- Don't change backend API contracts
- Don't commit directly (always PR)

**When Blocked**:
- Backend API not ready? Use mock data in tests
- Unsure about design? Check Figma design specs
- Performance concerns? Lead will optimize

---

## Final Notes for Claude Sonnet

You're building a professional video editing application. Each phase builds on prior work. Quality matters - users depend on this for their livelihood.

**Mindset**:
- Assume your code will go to production (write accordingly)
- Test edge cases (network failures, timeouts, edge-case inputs)
- Prioritize clarity over cleverness
- Ask clarifying questions if brief is unclear

**Success Looks Like**:
- Features work exactly as specified
- Tests pass without flakiness
- Code review has minimal feedback
- Zero regressions (old features still work)
- Performance is acceptable

Good luck! You've got this. 🚀

