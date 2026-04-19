# Plan 2.0 Frontend Execution: Phase-by-Phase Prompts
## Lead Dev Review | Claude Sonnet 4.6 | Mock Data Testing

**Timeline**: ~18 weeks | **Strategy**: Sequential phases with mock data → switch to real backend later

---

## QUICK START: How to Use This Plan

1. **Copy ENTIRE prompt** (syntax highlighted below)
2. **Paste into Claude Chat**
3. **Attach files** listed at top of each prompt
4. **Claude generates code** → You review & approve
5. **Move to next phase**

---

## PHASE EXECUTION ORDER

### BATCH A: FOUNDATION (Weeks 1-3)
- **Week 1**: Pre-Phase Setup (Mock Data)
- **Weeks 2-3**: Phase 6 + Phase 7 (PARALLEL)

### BATCH B (Weeks 4-5)
- **Week 4**: Phase 8

### BATCH C (Weeks 6-7)  
- **Week 6-7**: Phase 9

### BATCH D (Weeks 8-12) ⭐ LONGEST
- **Weeks 8-12**: Phase 10 (Timeline) - 5 sub-phases

### BATCH E (Weeks 13-14)
- **Weeks 13-14**: Phase 11 (Sharing)

### BATCH F (Weeks 15-18)
- **Weeks 15-18**: Phase 12 (Analytics)

---

## 📋 PROMPT STRUCTURE

Each prompt has this format:

```
🔵 PROMPT [N]: [PHASE NAME]
├─ Files to Attach: [List]
├─ Quick Context: [2-3 lines]
└─ Task: [What to build]

[FULL PROMPT TEXT - Copy entire thing]
```

---

## 🟦 PROMPT 0: PRE-PHASE SETUP (Mock Data)
**Run Week 1 | Effort: 1 day**

### Files to Attach:
- `figma-design-specs.md`
- `v2-implementation-plan.md`
- `v2-summary.md`
- `tsconfig.json` (from your project)

### Task: 
Create 7 mock data files with realistic sample data for Phases 6-12 testing. All tests will use mock data first, then switch to backend APIs later.

```
You are a senior React/TypeScript developer. I am the Lead Developer with 15+ years experience reviewing your work.

OBJECTIVE: Create mock data layer for frontend testing (Phases 6-12).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Testing strategy: Build UI with mock data first, connect to real backend later
- All data in: src/lib/mock-data/
- User uploads sample video/audio to: public/mock-assets/

DELIVERABLES - Create these files:

1️⃣ src/lib/mock-data/mock-storyboard.ts
   - Type: { id, episodeId, shots: StoryboardShot[] }
   - StoryboardShot: { id, sceneNumber, shotIndex, imageUrl, description, styleOverride, regenerationCount }
   - Data: 3 scenes × 4 shots (12 total)
   - URLs: Use Unsplash landscape images (https://images.unsplash.com/...)
   - Output: export const mockStoryboard = { ... }

2️⃣ src/lib/mock-data/mock-voices.ts
   - Type: VoiceAssignment[] with Character data
   - Fields: { id, episodeId, characterId, character: { name, avatarUrl }, voiceName, language }
   - Voices: Alloy, Echo, Fable, Onyx, Nova, Shimmer
   - Data: 5 characters with voice assignments
   - Output: export const mockVoices = [ ... ]

3️⃣ src/lib/mock-data/mock-animation.ts
   - Type: AnimationClip[] 
   - Fields: { id, sceneNumber, shotIndex, clipUrl, durationSeconds, status, costUsd }
   - Status: "queued" | "processing" | "ready" (use "ready" for all)
   - Duration: 5-8 seconds each
   - Cost: $0.056 per clip
   - Data: 12 clips (match 12 storyboard shots)
   - URLs: Use placeholder video URL or data URIs
   - Output: export const mockAnimationClips = [ ... ]

4️⃣ src/lib/mock-data/mock-renders.ts
   - Type: Render[]
   - Fields: { id, episodeId, status, aspectRatio, finalVideoUrl, cdnUrl, durationSeconds }
   - Data: 2 renders (one "complete", one "processing")
   - URLs: Use public video host URLs
   - Output: export const mockRenders = [ ... ]

5️⃣ src/lib/mock-data/mock-timeline.ts ⭐ CRITICAL
   - Type: Timeline with Tracks and Clips
   - Structure:
     * Timeline: { id, episodeId, durationMs: 180000 (3 min), tracks: [] }
     * Tracks: [VideoTrack, AudioTrack, MusicTrack, TextTrack]
     * Each track: { id, trackType, clips: [] }
   - VideoTrack: 12 animation clips (from mock-animation.ts), each 5 sec, positioned sequentially
   - AudioTrack: 1 dialogue audio file, 180 sec total
   - MusicTrack: 1 music file, 180 sec total (fade in/out at edges)
   - TextTrack: 2-3 text overlays (title at 0s, scene labels)
   - All times in milliseconds
   - Output: export const mockTimeline = { ... }

6️⃣ src/lib/mock-data/mock-review-links.ts
   - Type: ReviewLink[] + ReviewComment[]
   - ReviewLink: { id, episodeId, token, expiresAt, isRevoked, password }
   - ReviewComment: { id, reviewLinkId, authorName, text, timestampSeconds, isResolved }
   - Data: 3 review links with 5-8 comments distributed
   - Output: export const mockReviewLinks = { links: [...], comments: [...] }

7️⃣ src/lib/mock-data/mock-analytics.ts
   - Type: DashboardAnalytics + AdminMetrics
   - DashboardAnalytics: { episodeId, viewCount, uniqueViewers, renderCount, shareCount }
   - AdminMetrics: { dau, mau, subscriptionTiers, avgProcessingTime, costPerEpisode, errorRate }
   - Data: 5 episodes analytics + 30-day admin metrics
   - Output: export const mockAnalytics = { dashboards: [...], admin: {...} }

8️⃣ src/lib/mock-data/index.ts
   - Export all 7 files
   - Format: export { mockStoryboard, mockVoices, mockAnimationClips, ... } from './...'

REQUIREMENTS:
✅ All types use TypeScript (no `any`)
✅ All URLs are valid and accessible
✅ All timestamps ISO format or milliseconds (CONSISTENT)
✅ Timeline clips have realistic positioning (no overlaps)
✅ Use UUID v4 for all IDs
✅ Include JSDoc comments on complex types

VALIDATION:
The Lead Dev will check:
- ✅ Imports work (no circular dependencies)
- ✅ All data realistic and sensible
- ✅ Timeline totals match (~180 seconds)
- ✅ No console errors

OUTPUT:
Provide complete working code for all 8 files ready to copy-paste into src/lib/mock-data/
```

---

## 🟢 PROMPT 1: PHASE 6 - Storyboard Studio
**Run Weeks 2-3 (PARALLEL with Phase 7) | Effort: 3 days**

### Files to Attach:
- `agent-implementation-briefs.md` (Phase 6 section only)
- `figma-design-specs.md`
- `phase-10-12-detailed.md` (component specs)
- `src/types/index.ts` (your existing types)
- `src/components/ui/button.tsx`, `card.tsx`, `dialog.tsx`, `skeleton.tsx` (shadcn/ui components)
- `src/hooks/use-saga-state.ts` (state pattern reference)

### Task:
Build Storyboard Studio UI with 5 components + 1 page. Uses mock storyboard data. Features: shot grid, regenerate, full-screen viewer, style override.

```
You are a senior React/TypeScript developer. I am the Lead Developer reviewing your work.

OBJECTIVE: Implement Phase 6 - Storyboard Studio (UI Layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Testing: Mock data from src/lib/mock-data/mock-storyboard.ts
- State: Simple React useState hooks (no Zustand yet)
- Design: See figma-design-specs.md for colors/spacing

BUILD THESE COMPONENTS:

1️⃣ src/components/storyboard/shot-card.tsx
   Props: { shot: StoryboardShot, onRegenerate: () => void, onStyleEdit: () => void }
   - Display: Image thumbnail (200px square) + description + 2 buttons
   - Buttons: "Regenerate" (show regeneration count), "Edit Style"
   - Hover: Subtle shadow + button visibility
   - Loading state: Show skeleton while regenerating
   - Responsive: Mobile=single, Desktop=grid

2️⃣ src/components/storyboard/shot-grid.tsx
   Props: { shots: StoryboardShot[], onCardAction: (shotId, action) => void }
   - Display: Grid of ShotCard (4 per row on desktop)
   - Scene navigator: Two buttons (← Previous | Next →)
   - Responsive: 1 col mobile, 2 col tablet, 4 col desktop
   - Loading: Show 12 skeleton cards
   
3️⃣ src/components/storyboard/shot-viewer-modal.tsx
   Props: { shot: StoryboardShot | null, isOpen: boolean, onClose: () => void, onNavigate: (direction: "prev"|"next") => void }
   - Display: Full-screen lightbox
   - Image: Full size (max-width: 80vw)
   - Navigation: Prev/Next arrows
   - Close: Escape key or X button
   - Info: Show shot description, scene info, regeneration count

4️⃣ src/components/storyboard/style-override-popover.tsx
   Props: { shot: StoryboardShot, onApply: (style: string) => void }
   - Display: Popover with 6 style buttons
   - Presets: "Realistic", "Cartoon", "Anime", "Watercolor", "Pencil Sketch", "3D Render"
   - Visual: Color-coded buttons
   - Feedback: "Style applied! Regenerating..." toast

5️⃣ src/stores/storyboardStore.ts (Simple Zustand)
   State: { currentScene, selectedShot, regeneratingShots }
   Actions: { nextScene(), prevScene(), selectShot(), markRegeneration() }

6️⃣ src/app/(dashboard)/studio/[id]/storyboard/page.tsx
   - Header: "Storyboard Studio" + episode name
   - Content: ShotGrid component
   - Features:
     * Use mock data from src/lib/mock-data/
     * Simulate regenerate (2-second delay)
     * Open modal on shot click
     * Show toast feedback on style apply
   - State: useState hooks (simple, no complex logic)

DEPENDENCIES:
✅ Import shadcn/ui: button, card, dialog, skeleton, popover, toast
✅ Use Tailwind CSS (no inline styles)
✅ Responsive design (Tailwind breakpoints)
✅ TypeScript strict mode

VALIDATION:
The Lead Dev will check:
- ✅ All 6 components render without errors
- ✅ Grid responsive on mobile/tablet/desktop
- ✅ Regenerate simulation works (2-sec delay)
- ✅ Modal opens/closes properly
- ✅ Style buttons trigger toast
- ✅ No console errors

OUTPUT:
Provide complete, production-ready code for all 6 files.
```

---

## 🟢 PROMPT 2: PHASE 7 - Voice Studio  
**Run Weeks 2-3 (PARALLEL with Phase 6) | Effort: 3 days**

### Files to Attach:
- `agent-implementation-briefs.md` (Phase 7 section)
- `figma-design-specs.md`
- `src/types/index.ts`
- `src/components/ui/select.tsx`, `badge.tsx`, `button.tsx`, `input.tsx` (shadcn/ui)

### Task:
Build Voice Studio UI with 5 components + 1 page. Features: voice picker dropdowns, language selector, audio preview player, voice clone upload (tier-locked).

```
You are a senior React/TypeScript developer. I am the Lead Developer reviewing your work.

OBJECTIVE: Implement Phase 7 - Voice Studio (UI Layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Testing: Mock data from src/lib/mock-data/mock-voices.ts
- Audio: HTML5 audio element for playback
- Tier-locking: Show "Upgrade" for voice clone if free tier

BUILD THESE COMPONENTS:

1️⃣ src/components/voice/voice-picker.tsx
   Props: { currentVoice: string, voices: string[], onSelect: (voice: string) => void }
   - UI: shadcn/ui Select component
   - Options: 6 voices with gender labels (M/F)
   - Display format: "Alex (Nova) - Male"

2️⃣ src/components/voice/language-selector.tsx
   Props: { currentLanguage: string, onSelect: (lang: string) => void }
   - UI: shadcn/ui Select component
   - Options: 6 languages with flag emojis (🇺🇸 🇬🇧 🇪🇸 🇫🇷 🇩🇪 🇯🇵)
   - Display format: "🇺🇸 English (US)"

3️⃣ src/components/voice/audio-preview-player.tsx
   Props: { voiceName: string, characterName: string, sampleText: string, onPlay: () => void }
   - Button: "Play Preview"
   - Flow: Click → show loading spinner (2 sec) → play audio
   - Audio element: HTML5 with controls (play, pause, volume, seek)
   - On complete: Hide player

4️⃣ src/components/voice/voice-clone-upload.tsx
   Props: { characterId: string, onUpload: (file: File) => void, isTierLocked: boolean }
   - If locked: Show lock icon + "Upgrade to Studio tier"
   - If unlocked: Drag-and-drop upload area
   - Validation: Accept audio files only
   - Feedback: File name + progress bar (mock 100% in 2 sec)

5️⃣ src/app/(dashboard)/studio/[id]/voice/page.tsx
   - Header: "Voice Studio" + episode name
   - Section 1: Character voice assignments (table-like rows)
     * Each row: Avatar + Name + Voice Picker + Language Selector + Play Button
   - Section 2: Voice cloning (with tier lock)
     * 3 upload components (tier check: show "Upgrade" if free)
   - State: Use mock data
   - No API calls (mock everything)

DEPENDENCIES:
✅ Use shadcn/ui Select, Badge, Button
✅ HTML5 audio element for preview
✅ Tailwind CSS responsive
✅ TypeScript strict

VALIDATION:
The Lead Dev will check:
- ✅ Voice picker updates on selection
- ✅ Language selector works
- ✅ Audio preview plays after 2-second delay
- ✅ Voice clone upload shows progress
- ✅ Tier lock working (shows "Upgrade")
- ✅ No console errors

OUTPUT:
Provide complete code for all 5 components + page.
```

---

## 🟡 PROMPT 3: PHASE 8 - Animation Approval  
**Run Week 4 | Effort: 2 days**

### Files to Attach:
- `agent-implementation-briefs.md` (Phase 8 section)
- `figma-design-specs.md`
- `src/types/index.ts`
- `src/components/ui/dialog.tsx`, `button.tsx`, `progress.tsx`, `tabs.tsx`

### Task:
Build Animation Approval UI with 5 components + 1 page. Features: cost breakdown table, backend selector (Kling/Local), video clip previews, approval dialog, progress bar.

```
You are a senior React/TypeScript developer. I am the Lead Developer reviewing your work.

OBJECTIVE: Implement Phase 8 - Animation Approval (UI Layer).

CONTEXT:
- Testing: Mock data from src/lib/mock-data/mock-animation.ts (12 clips)
- Cost: $0.056 per clip (total: 12 × $0.056 = $0.67)
- Backend: Two options - "Kling AI" (default) or "Local"
- All mock (no real API)

BUILD THESE COMPONENTS:

1️⃣ src/components/animation/backend-selector.tsx
   Props: { selectedBackend: string, onSelect: (backend: string) => void }
   - UI: Radio button group (2 options)
   - Option 1: "Kling AI" - "High quality, $0.056/clip"
   - Option 2: "Local" - "Free, lower quality"
   - Visual: Icon + label + description

2️⃣ src/components/animation/cost-breakdown-table.tsx
   Props: { shots: Shot[], rate: number, backend: string }
   - Display: Table
     * Headers: Scene | Shots | Rate ($) | Subtotal ($)
     * Rows: Per scene (Scene 1: 4 shots, Scene 2: 4 shots, Scene 3: 4 shots)
     * Footer: TOTAL COST in bold ($0.67)
   - Format: USD with 2 decimals

3️⃣ src/components/animation/clip-player.tsx
   Props: { clip: AnimationClip, autoPlay?: boolean }
   - Display: HTML5 video player
   - Controls: Play, pause, seek, volume, fullscreen
   - Loading: Skeleton while video loads
   - Error: Show error message if fails
   - Loop: Continue looping

4️⃣ src/components/animation/clip-preview-grid.tsx
   Props: { clips: AnimationClip[], groupByScene?: boolean }
   - Display: Accordion per scene (if groupByScene=true)
   - Each clip: Small 200×200 video player preview
   - Status badge: "Ready", "Processing", "Queued", "Failed" (color-coded)
   - Click clip: Expand to larger player

5️⃣ src/components/animation/approval-dialog.tsx
   Props: { isOpen: boolean, onConfirm: () => void, onCancel: () => void, estimate: AnimationEstimate }
   - Display: Modal dialog
   - Content:
     * Backend name: "Kling AI"
     * Shot count: "12 shots"
     * Total cost: "$0.67"
     * Balance: "$45.33" (mock)
   - Buttons: [Cancel] [Approve & Process]

6️⃣ src/app/(dashboard)/studio/[id]/animation/page.tsx
   - Section 1: Cost Estimator
     * BackendSelector
     * CostBreakdownTable
     * Approve button (opens dialog)
   - Section 2: Processing Progress
     * Progress bar (0-100%)
     * Status text
     * Mock: Increment 5% per second during "processing" (simulation)
   - Section 3: Clip Previews
     * ClipPreviewGrid (grouped by scene)
     * Filter tabs: All, Ready, Processing, Failed
   - All mock data

VALIDATION:
The Lead Dev will check:
- ✅ Cost calculation: 12 × $0.056 = $0.67 ✓
- ✅ Backend selector works (Kling/Local)
- ✅ Tables render correctly
- ✅ Approval dialog shows summary
- ✅ Progress bar animates
- ✅ Video players play
- ✅ No console errors

OUTPUT:
Provide complete code for all 6 files.
```

---

## 🟡 PROMPT 4: PHASE 9 - Render & Delivery
**Run Weeks 6-7 | Effort: 2 days**

### Files to Attach:
- `agent-implementation-briefs.md` (Phase 9 section)
- `figma-design-specs.md`
- `src/types/index.ts`
- `src/components/ui/button.tsx`, `tabs.tsx`, `progress.tsx`

### Task:
Build Render & Delivery UI with 5 components + 1 page. Features: aspect ratio picker (16:9, 9:16, 1:1), render progress with SignalR mock, video player, download buttons, render history.

```
You are a senior React/TypeScript developer. I am the Lead Developer reviewing your work.

OBJECTIVE: Implement Phase 9 - Render & Delivery (UI Layer).

CONTEXT:
- Testing: Mock data from src/lib/mock-data/mock-renders.ts
- Challenge: Simulate SignalR events for progress updates
- Aspect ratios: 16:9 (default), 9:16 (vertical), 1:1 (square)

BUILD THESE COMPONENTS:

1️⃣ src/components/render/aspect-ratio-picker.tsx
   Props: { selected: string, onSelect: (ratio: string) => void }
   - Display: 3 visual cards (not buttons)
     * "16:9" - landscape rectangle visual
     * "9:16" - portrait rectangle visual
     * "1:1" - square visual
   - Each card shows dimensions (e.g., "1920×1080")
   - Selected: Black border, unselected: gray border

2️⃣ src/components/render/render-progress-bar.tsx
   Props: { percent: 0-100, currentStage: string, isComplete: boolean }
   - Display: Linear progress bar
   - Stage labels:
     * 0-20%: "Queued..."
     * 20-50%: "Assembling video frames..."
     * 50-80%: "Mixing audio..."
     * 80-100%: "Finalizing..."
     * 100%: "Complete ✓"
   - Color: Blue (processing), Green (complete)

3️⃣ src/components/render/download-bar.tsx
   Props: { renderId: string, videoUrl: string, srtUrl: string }
   - Display: Horizontal bar with 2 buttons
     * Button 1: "Download MP4" (icon)
     * Button 2: "Download SRT" (icon)
   - Functionality: onClick → mock download file

4️⃣ src/components/render/render-history-table.tsx
   Props: { renders: Render[] }
   - Display: Table
     * Columns: Date Created | Duration | Aspect | Status | Actions
     * Rows: 1 per render (newest first)
     * Status badge: "Complete", "Processing", etc.
   - Actions: "Download" (dropdown), "Re-render"

5️⃣ src/components/render/video-player-with-caption.tsx
   Props: { videoUrl: string, captionUrl?: string }
   - Display: HTML5 video player (full width, responsive)
   - Controls: Play, pause, seek, volume, fullscreen, captions toggle
   - Captions: Display VTT/SRT if provided

6️⃣ src/app/(dashboard)/studio/[id]/render/page.tsx
   - Header: "Post-Production Render" + episode title
   - Left (40%):
     * AspectRatioPicker
     * "Start Render" button
   - Right (60%):
     * RenderProgressBar (show progress during render)
     * DownloadBar (when complete)
     * VideoPlayerWithCaption (when complete)
   - Below:
     * RenderHistoryTable
   - Mock SignalR:
     * On "Start Render" click: Start progress loop
     * Emit RenderProgress every 2 sec, increment percent
     * At 100%: Emit RenderComplete with video URL

VALIDATION:
The Lead Dev will check:
- ✅ Render starts on button click
- ✅ Progress bar animates 0→100% (10 sec simulation)
- ✅ Stage labels update correctly
- ✅ Download buttons appear at 100%
- ✅ Video player plays
- ✅ History table shows renders
- ✅ Re-render button works
- ✅ No console errors

OUTPUT:
Provide complete code for all 6 files.
```

---

## 🔴 PROMPT 5: PHASE 10A - Timeline Data Model (SUB-PHASE A)
**Run Weeks 8-9 | Effort: 1 day | RUN FIRST**

### Files to Attach:
- `phase-10-12-detailed.md` (Phase 10 full spec)
- `figma-design-specs.md`
- `src/types/index.ts`
- `src/lib/utils.ts` (existing utilities)
- `package.json` (verify versions)

### Task:
Build Timeline data model, Zustand store, utilities, and mock hook. This is the foundation for sub-phases 10B-10E. All data types, conversions (ms↔pixels↔frames), collision detection logic.

```
You are a senior React/TypeScript developer specializing in state management.

OBJECTIVE: Implement Phase 10A - Timeline Data Layer (Model + Store + Utils).

⭐ CRITICAL: Phase 10 splits into 5 sub-phases. 10A is FOUNDATION for 10B-10E.

CONTEXT:
- Timeline: 3 minutes (180,000ms) with 4 track types (video, audio, music, text)
- 50+ clips total managing clips, undo/redo, playback
- Mock data from src/lib/mock-data/mock-timeline.ts

STEP 1: Create Timeline Types
File: src/types/timeline.ts
```typescript
// Enums
export enum TrackType { Video = "video", Audio = "audio", Music = "music", Text = "text" }
export enum TransitionType { Cut = "cut", Fade = "fade", Dissolve = "dissolve" }
export enum TextAnimation { None = "none", FadeIn = "fade-in", SlideUp = "slide-up" }

// Domain Models
export interface TimelineClip {
  id: string
  trackId: string
  sourceId: string
  startMs: number // Position on timeline
  endMs: number
  trimStartMs: number // Trim at source
  trimEndMs: number
  transitionIn: TransitionType
  transitionDuration: number
  sortOrder: number
  mediaUrl?: string
  mediaDuration?: number
  label?: string
}

export interface TimelineTrack {
  id: string
  episodeId: string
  trackType: TrackType
  name: string
  sortOrder: number
  isVisible: boolean
  isLocked: boolean
  clips: TimelineClip[]
  volume?: number // 0-100 for audio/music
}

export interface Timeline {
  id: string
  episodeId: string
  durationMs: number
  fps: number // 24, 30, 60
  tracks: TimelineTrack[]
  createdAt: Date
  updatedAt: Date
}
```

STEP 2: Create Timeline Utilities
File: src/lib/timeline-utils.ts
```typescript
export const timelineUtils = {
  // Conversion
  msToSeconds(ms: number): number
  secondsToMs(seconds: number): number
  msToFrame(ms: number, fps: number): number
  frameToMs(frame: number, fps: number): number
  
  // Positioning (100px = 1 second at 1x zoom)
  pixelsToMs(pixels: number, zoom: number, pixelsPerSecond: number = 100): number
  msToPixels(ms: number, zoom: number, pixelsPerSecond: number = 100): number
  
  // Clip operations
  moveClip(clip: TimelineClip, newStartMs: number): TimelineClip
  trimClip(clip: TimelineClip, trimStart: number, trimEnd: number): TimelineClip
  resizeClip(clip: TimelineClip, newEndMs: number): TimelineClip
  
  // Validation
  isClipOverlapping(clip1: TimelineClip, clip2: TimelineClip, tolerance?: number): boolean
  canPlaceClip(track: TimelineTrack, clip: TimelineClip): boolean
  validateTimeline(timeline: Timeline): string[] // error messages
}
```

STEP 3: Create Zustand Store
File: src/stores/timelineStore.ts
```typescript
interface TimelineState {
  timeline: Timeline | null
  selectedClip: TimelineClip | null
  selectedTrack: TimelineTrack | null
  playheadPositionMs: number
  isPlaying: boolean
  zoom: number // 1-5x
  history: Timeline[]
  historyIndex: number
}

export const useTimelineStore = create<TimelineState>((set, get) => ({
  // Initial state
  timeline: null,
  selectedClip: null,
  selectedTrack: null,
  playheadPositionMs: 0,
  isPlaying: false,
  zoom: 1,
  history: [],
  historyIndex: -1,

  // Actions
  loadTimeline: (timeline: Timeline) => set({
    timeline,
    history: [timeline],
    historyIndex: 0,
    selectedClip: null,
    playheadPositionMs: 0
  }),

  selectClip: (clip: TimelineClip | null) => set({ selectedClip: clip }),
  selectTrack: (track: TimelineTrack | null) => set({ selectedTrack: track }),
  
  setPlayheadPosition: (ms: number) => set({ playheadPositionMs: ms }),
  togglePlayback: () => set(state => ({ isPlaying: !state.isPlaying })),
  play: () => set({ isPlaying: true }),
  pause: () => set({ isPlaying: false }),
  
  setZoom: (zoom: number) => set({ zoom: Math.max(1, Math.min(5, zoom)) }),
  zoomIn: () => set(state => ({ zoom: Math.min(5, state.zoom + 0.5) })),
  zoomOut: () => set(state => ({ zoom: Math.max(1, state.zoom - 0.5) })),

  // Clip mutations (with history)
  moveClip: (clipId: string, trackId: string, newStartMs: number) => {
    set(state => {
      if (!state.timeline) return state
      const updated = { ...state.timeline }
      // Find clip and move it (implement move logic)
      return {
        timeline: updated,
        history: [...state.history.slice(0, state.historyIndex + 1), updated],
        historyIndex: state.historyIndex + 1
      }
    })
  },

  trimClip: (clipId: string, trackId: string, trimStart: number, trimEnd: number) => {
    set(state => {
      if (!state.timeline) return state
      const updated = { ...state.timeline }
      // Implement trim logic
      return {
        timeline: updated,
        history: [...state.history.slice(0, state.historyIndex + 1), updated],
        historyIndex: state.historyIndex + 1
      }
    })
  },

  undo: () => set(state => {
    if (state.historyIndex > 0) {
      return {
        historyIndex: state.historyIndex - 1,
        timeline: state.history[state.historyIndex - 1]
      }
    }
    return state
  }),

  redo: () => set(state => {
    if (state.historyIndex < state.history.length - 1) {
      return {
        historyIndex: state.historyIndex + 1,
        timeline: state.history[state.historyIndex + 1]
      }
    }
    return state
  }),

  // Playback
  advancePlayhead: (deltaMs: number) => {
    set(state => {
      if (!state.timeline) return state
      const newPosition = Math.min(state.playheadPositionMs + deltaMs, state.timeline.durationMs)
      return {
        playheadPositionMs: newPosition,
        isPlaying: newPosition < state.timeline.durationMs
      }
    })
  }
}))
```

STEP 4: Create Mock Hook
File: src/hooks/use-timeline-mock.ts
```typescript
export const useTimelineMock = () => {
  const store = useTimelineStore()
  const [timeline, setTimeline] = useState<Timeline | null>(null)

  // Load mock data on mount
  useEffect(() => {
    const mockTimeline = mockData.timeline
    store.loadTimeline(mockTimeline)
    setTimeline(mockTimeline)
  }, [])

  // Playback loop: advance 30ms every 30ms (~33fps)
  useEffect(() => {
    if (!store.isPlaying) return
    const interval = setInterval(() => {
      store.advancePlayhead(30)
    }, 30)
    return () => clearInterval(interval)
  }, [store.isPlaying])

  return { timeline: store.timeline, ...store }
}
```

DELIVERABLES:
1. src/types/timeline.ts (all types)
2. src/lib/timeline-utils.ts (conversion + validation)
3. src/stores/timelineStore.ts (Zustand store)
4. src/hooks/use-timeline-mock.ts (mock hook)

VALIDATION:
The Lead Dev will check:
- ✅ All types compile (no `any`)
- ✅ Timeline utility functions work correctly
- ✅ Zustand store persists state
- ✅ Undo/redo history maintains 50 states
- ✅ Playhead advances during playback
- ✅ No console errors

OUTPUT:
Provide complete, production-ready code with JSDoc comments.
```

---

## 🔴 PROMPT 6: PHASE 10B - Konva.js Timeline Canvas (SUB-PHASE B)
**Run Week 9 | Effort: 1.5 days | RUN AFTER 10A**

### Files to Attach:
- All files created in Phase 10A
- `phase-10-12-detailed.md` (component specs)
- `figma-design-specs.md`
- `package.json` (verify konva + react-konva versions)

### Task:
Build Konva.js canvas rendering layer with: Timeline ruler, playhead indicator, clip shapes, track lanes. All interactive (hover effects, selection states).

```
You are a senior React/TypeScript developer specializing in Konva.js canvas rendering.

OBJECTIVE: Implement Phase 10B - Timeline Canvas (Rendering Layer).

⭐ DEPENDS ON: Phase 10A (types + store must exist)

CONTEXT:
- Canvas library: Konva.js v9 + react-konva
- Stage dimensions: Responsive (measure container)
- Pixels per second: 100px at 1x zoom
- Track colors: Video=#3B82F6 (blue), Audio=#10B981 (green), Music=#8B5CF6 (purple), Text=#F59E0B (orange)
- Track heights: Video/Music=80px, Audio=60px, Text=50px

BUILD 6 COMPONENTS (Konva-based):

1️⃣ src/components/timeline/timeline-canvas-wrapper.tsx
   - Render: Container div + Konva Stage
   - Responsive: Measure container with ResizeObserver
   - Pass: Width/height to children
   - Scroll: Horizontal scroll if content > container

2️⃣ src/components/timeline/timeline-ruler.tsx (Konva)
   - Display: Time markers at 5-second intervals
   - Format: MM:SS (0:00, 0:05, 0:10, etc.)
   - Height: 40px fixed
   - Background: Light gray
   - Interactive: Click marker to seek playhead

3️⃣ src/components/timeline/playhead-indicator.tsx (Konva)
   - Display: Red vertical line at playheadPositionMs
   - Width: 2px
   - Color: #EF4444
   - Interactive: Drag to seek
   - Z-index: Front layer (always visible)

4️⃣ src/components/timeline/clip-shape.tsx (Konva - Most Complex) ⭐
   Props: { clip: TimelineClip, isSelected: boolean, onSelect, onMove, onTrim }
   - Visual:
     * Rectangle (colored by track type)
     * Label text (white, 12px)
     * Border: 2px darker shade
     * Selection: Glow effect + corner handles
   - Interactions:
     * Click: Select clip
     * Drag: Move horizontal (constrained to track)
     * Drag left edge: Trim start
     * Drag right edge: Trim end
   - Styling:
     * Height: 60px (within 80px track)
     * Rounded corners: 4px
     * Opacity: 1.0 normal, 0.5 if deselected

5️⃣ src/components/timeline/track-lane.tsx (Konva)
   Props: { track: TimelineTrack, clips: TimelineClip[], height: number }
   - Visual: Background rectangle (alternating colors)
   - Children: ClipShape components per clip
   - Padding: 10px top/bottom
   - Label: Track name on left (40px width)

6️⃣ src/components/timeline/timeline-container.tsx (Konva Group)
   - Layers (in order):
     * Layer 1: Track backgrounds
     * Layer 2: Clips (interactive)
     * Layer 3: Playhead (always top)
   - Total height: Sum of all track heights + spacing
   - Total width: Timeline durationMs converted to pixels

STRUCTURE:
```typescript
<TimelineCanvasWrapper>
  <Konva.Stage width={containerWidth} height={containerHeight}>
    <Konva.Layer name="background">
      {/* Track backgrounds */}
    </Konva.Layer>
    <Konva.Layer name="clips">
      {tracks.map(track => (
        <TrackLane key={track.id} track={track} />
      ))}
    </Konva.Layer>
    <Konva.Layer name="playhead">
      <PlayheadIndicator />
    </Konva.Layer>
  </Konva.Stage>
  <TimelineRuler />
</TimelineCanvasWrapper>
```

DELIVERABLES:
1. src/components/timeline/timeline-canvas-wrapper.tsx
2. src/components/timeline/timeline-ruler.tsx
3. src/components/timeline/playhead-indicator.tsx
4. src/components/timeline/clip-shape.tsx ⭐
5. src/components/timeline/track-lane.tsx
6. src/components/timeline/timeline-container.tsx

DEPENDENCIES:
✅ Import: Konva, react-konva
✅ Use timeline store + utils (from 10A)
✅ TypeScript strict mode

VALIDATION:
The Lead Dev will check:
- ✅ Canvas renders all tracks
- ✅ Clips display with correct colors
- ✅ Playhead moves smoothly
- ✅ Ruler shows correct times
- ✅ Can click clip to select
- ✅ Can drag to scroll
- ✅ No console errors
- ✅ Responsive to container size

OUTPUT:
Provide complete, production-ready code with Konva.js documentation links.
```

---

## 🔴 PROMPT 7: PHASE 10C - Trim + Drag-Drop (SUB-PHASE C)
**Run Week 10 | Effort: 1.5 days | RUN AFTER 10B**

### Files to Attach:
- All files from phases 10A + 10B
- `phase-10-12-detailed.md` (interaction specs)

### Task:
Add clip dragging, trimming, collision detection. Clips snap to 100ms grid. Show collision overlay when overlaps detected. Drag-drop utilities with full validation.

```
You are a senior React/TypeScript developer specializing in complex interactions.

OBJECTIVE: Implement Phase 10C - Timeline Interactions (Drag + Trim + Collision).

⭐ DEPENDS ON: Phases 10A + 10B must be complete

CONTEXT:
- Challenge: Drag clips + trim + prevent overlaps + snap to grid
- Grid: 100ms (alignment)
- Constraints: Can't move to different track, can't trim beyond source duration

CREATE UTILITIES:

1️⃣ src/lib/timeline/clip-drag-handler.ts
```typescript
export const clipDragHandler = {
  calculateNewStartMs(dragStartMs: number, pixelDelta: number, zoom: number): number
  constrainClipPosition(clip: TimelineClip, timeline: Timeline, trackId: string): TimelineClip
  detectClipOverlap(clip: TimelineClip, otherClips: TimelineClip[], buffer?: number): boolean
  snapToGrid(ms: number, gridMs: number = 100): number
  getAvailableSpace(track: TimelineTrack, excludeClipId: string): { startMs; endMs }[]
}
```

2️⃣ src/lib/timeline/clip-trim-handler.ts
```typescript
export const clipTrimHandler = {
  calculateTrimStart(dragStartMs: number, pixelDelta: number, zoom: number, clip: TimelineClip): number
  calculateTrimEnd(dragStartMs: number, pixelDelta: number, zoom: number, clip: TimelineClip): number
  validateTrimRange(clip: TimelineClip, newTrimStart: number, newTrimEnd: number): boolean
  enforceMinimumClipLength(clip: TimelineClip, newTrimStart: number, newTrimEnd: number): boolean
}
```

ENHANCE COMPONENTS:

3️⃣ Update src/components/timeline/clip-shape.tsx
   - Add mouseDown handler:
     * Detect drag vs trim (trim zone = 10px on left/right edges)
     * If drag: startDrag()
     * If trim-left: startTrim("start")
     * If trim-right: startTrim("end")
   - startDrag():
     * On dragmove: Calculate newStartMs
     * Snap to grid
     * Check overlap → show overlay if detected
     * Call onMove if valid
   - startTrim():
     * On trimMoveocalstartMs
     * Validate trim range
     * Call onTrim if valid

4️⃣ New src/components/timeline/collision-overlay.tsx
   Props: { clip: TimelineClip, hasCollision: boolean }
   - Display: Semi-transparent red rectangle where collision occurs
   - Opacity: 0.5
   - Appears while dragging if overlap detected
   - Disappears on drag end or collision resolved

UPDATE STORE:

5️⃣ Update src/stores/timelineStore.ts
   - Add actions:
     * moveClip(clipId, trackId, newStartMs) - validates, adds to history
     * trimClip(clipId, trackId, side, newMs) - validates, adds to history
   - Add state: { isDragging, isTrimming, draggedClipId }

DELIVERABLES:
1. src/lib/timeline/clip-drag-handler.ts
2. src/lib/timeline/clip-trim-handler.ts
3. src/components/timeline/clip-shape-enhanced.tsx (updated)
4. src/components/timeline/collision-overlay.tsx
5. Updated: src/stores/timelineStore.ts

VALIDATION:
The Lead Dev will check:
- ✅ Clip drags horizontally only
- ✅ Snap to grid works (100ms)
- ✅ Red overlay on collision detected
- ✅ Trim handles work both edges
- ✅ Trim respects minimum length (500ms)
- ✅ Can't move to wrong track
- ✅ No console errors

OUTPUT:
Provide complete, production-ready code with detailed comments on collision algorithm.
```

---

## 🔴 PROMPT 8: PHASE 10D - Music + Text (SUB-PHASE D)
**Run Week 10 | Effort: 1 day | RUN AFTER 10C**

### Files to Attach:
- All files from phases 10A + 10B + 10C
- `phase-10-12-detailed.md` (music + text specs)

### Task:
Add music track management (volume control, auto-duck) and text overlay system (add, edit, style, animate). Text appears on Konva canvas at correct timing.

```
You are a senior React/TypeScript developer.

OBJECTIVE: Implement Phase 10D - Music Panel + Text Overlay System.

⭐ DEPENDS ON: Phases 10A + 10B + 10C complete

CONTEXT:
- Music: Load stock tracks, adjust volume, show auto-duck toggle
- Text: Add titles/captions, style (color, size, position), animate (fade-in, slide-up)

BUILD MUSIC PANEL:

1️⃣ Create src/lib/mock-data/stock-music.ts
   - Export: Array of 10 stock music tracks
   - Each: { id, title, duration, genre, previewUrl, fullUrl }
   - Genres: Ambient, Epic, Uplifting, Suspense, Comedy, etc.

2️⃣ src/components/timeline/music-library.tsx
   - Display: Sidebar with 10 stock tracks
   - Each track: Title, duration, genre, preview button
   - Preview: Play 10-second sample (mock audio URL)
   - Action: "Add to Timeline" button

3️⃣ src/components/timeline/music-track-row.tsx
   - Display: Row for music track
   - Columns: [Track name] [Volume slider] [Auto-duck toggle] [Delete]
   - Volume: 0-100% slider with dB label
   - Auto-duck: Toggle → reduce music volume when dialogue plays

4️⃣ src/components/timeline/volume-control.tsx
   Props: { volume: 0-100, onVolumeChange }
   - UI: Horizontal slider + speaker icon
   - Display: "75%" label
   - Update waveform visualization opacity

5️⃣ src/lib/timeline/auto-duck-logic.ts
```typescript
export const autoDuckLogic = {
  applyAutoDuck(musicClip: TimelineClip, audioClips: TimelineClip[]): number
  // When audio clip starts → return 50% of music volume
  // When audio ends → restore full volume
  // Smooth transition: 300ms fade
}
```

BUILD TEXT OVERLAY:

6️⃣ Update src/types/timeline.ts
```typescript
export interface TextOverlay {
  id: string
  episodeId: string
  text: string
  fontSizePixels: number
  color: string // hex #RRGGBB
  positionX: number // 0-100 percent
  positionY: number // 0-100 percent
  startMs: number
  durationMs: number
  animation: "none" | "fade-in" | "slide-up"
  zIndex: number
}
```

7️⃣ src/components/timeline/text-overlay-panel.tsx
   - Display: Panel below canvas
   - Features:
     * "Add Text" button
     * List of current overlays
     * Each: Start time, duration, preview, edit/delete buttons

8️⃣ src/components/timeline/text-overlay-form.tsx
   - Modal form with fields:
     * Text input (textarea)
     * Font size dropdown (12, 16, 20, 24, 32px)
     * Color picker (RGB + hex)
     * Animation select (None, Fade In, Slide Up)
     * Position grid (9 positions: top-left to bottom-right)
     * Start time (MM:SS format)
     * Duration (seconds)
   - Buttons: [Cancel] [Add to Timeline]

9️⃣ src/components/timeline/text-overlay-preview.tsx
   - Display: Small preview box
   - Shows: Text with selected styling
   - Position: Adjustable by dragging

🔟 src/lib/timeline/text-overlay-utils.ts
```typescript
export const textOverlayUtils = {
  formatTextForDisplay(text: string, fontSize: number, position: string): CSSProperties
  validateTextOverlay(overlay: TextOverlay): string[] // errors
  calculateTextDimensions(text: string, fontSize: number): { width, height }
}
```

UPDATE KONVA CANVAS (from 10B):

1️⃣1️⃣ Update src/components/timeline/timeline-container.tsx
   - Add Layer 4: Text overlays
   - Each TextOverlay renders as Konva.Text
   - Position based on overlay settings
   - Show/hide based on playhead timing

UPDATE ZUSTAND STORE (from 10A):

1️⃣2️⃣ Update src/stores/timelineStore.ts
   - Add actions:
     * addTextOverlay(text: TextOverlay)
     * updateTextOverlay(overlayId, updates)
     * deleteTextOverlay(overlayId)
     * All add to history for undo/redo

DELIVERABLES:
1. src/lib/mock-data/stock-music.ts
2. src/components/timeline/music-library.tsx
3. src/components/timeline/music-track-row.tsx
4. src/components/timeline/volume-control.tsx
5. src/lib/timeline/auto-duck-logic.ts
6. src/components/timeline/text-overlay-panel.tsx
7. src/components/timeline/text-overlay-form.tsx
8. src/components/timeline/text-overlay-preview.tsx
9. src/lib/timeline/text-overlay-utils.ts
10. Updated: src/types/timeline.ts
11. Updated: src/components/timeline/timeline-container.tsx
12. Updated: src/stores/timelineStore.ts

VALIDATION:
The Lead Dev will check:
- ✅ Music tracks in timeline
- ✅ Volume slider works (0-100%)
- ✅ Auto-duck reduces volume during dialogue
- ✅ Can add text overlays
- ✅ Text preview shows formatting
- ✅ Text animations work (fade, slide)
- ✅ Text position adjustable
- ✅ Undo/redo includes text changes
- ✅ No console errors

OUTPUT:
Provide complete, production-ready code with audio mixing comments.
```

---

## 🔴 PROMPT 9: PHASE 10E - Preview Rendering + Undo/Redo (SUB-PHASE E)
**Run Week 11 | Effort: 1 day | RUN AFTER 10D**

### Files to Attach:
- All files from phases 10A-D
- `phase-10-12-detailed.md`

### Task:
Add mock preview video render (5-second simulated render with progress stages), real-time preview player with playhead sync, and enhanced undo/redo with action history + 50-state limit + history panel.

```
You are a senior React/TypeScript developer specializing in history management.

OBJECTIVE: Implement Phase 10E - Preview Rendering + Undo/Redo System.

⭐ DEPENDS ON: Phases 10A-D all complete

CONTEXT:
- Preview rendering: Mock 5-second render with progress stages
- Undo/Redo: Full history with 50-operation limit + keyboard shortcuts
- Sync: Timeline playhead ↔ preview video playback (two-way sync)

BUILD PREVIEW RENDERING:

1️⃣ src/lib/timeline/preview-render-service.ts (Mock)
```typescript
export const previewRenderService = {
  startRender(timeline: Timeline): Promise<{
    videoUrl: string
    durationSeconds: number
    progress: Observable<number> // Emits 0, 25, 50, 75, 100
  }>
  // Mock: Emit progress at 1-sec intervals (0→25→50→75→100 over 5 secs)
  // Return video URL after 5sec delay
}
```

2️⃣ src/components/timeline/timeline-preview-render.tsx
   - Button: "Generate Preview"
   - Progress display:
     * "Compositing video frames..." (0-40%)
     * "Mixing audio tracks..." (40-70%)
     * "Exporting video..." (70-100%)
   - On complete: Show video is ready

3️⃣ src/components/timeline/preview-player.tsx
   Props: { videoUrl: string }
   - Display: HTML5 video player
   - Sync: Playhead ↔ video (two-way binding)
     * If video plays → timeline playhead advances
     * If playhead dragged → video seeks to position
   - Controls: Play, pause, seek, volume, fullscreen

BUILD UNDO/REDO:

4️⃣ src/lib/timeline/history-manager.ts
```typescript
export class HistoryManager {
  private stack: Timeline[] = []
  private currentIndex: number = -1
  private maxHistorySize: number = 50

  push(state: Timeline): void {
    // Add state, discard future states
    this.stack = this.stack.slice(0, this.currentIndex + 1)
    this.stack.push(state)
    if (this.stack.length > this.maxHistorySize) this.stack.shift()
    this.currentIndex = this.stack.length - 1
  }

  undo(): Timeline | null { /* */ }
  redo(): Timeline | null { /* */ }
  canUndo(): boolean
  canRedo(): boolean
  getHistorySize(): { current; max }
}
```

5️⃣ src/components/timeline/undo-redo-toolbar.tsx
   - Display: Top toolbar with 2 buttons
     * "↶ Undo" (disabled if can't undo)
     * "↷ Redo" (disabled if can't redo)
   - Tooltip: Show last action "Undo: Moved clip 3"
   - Keyboard: Ctrl+Z (undo), Ctrl+Shift+Z (redo)

6️⃣ Update src/hooks/use-timeline-keybinds.ts
```typescript
export const useTimelineKeybinds = () => {
  const store = useTimelineStore()
  
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.ctrlKey && e.key === "z" && !e.shiftKey) store.undo()
      if (e.ctrlKey && e.shiftKey && e.key === "z") store.redo()
      if (e.key === "Delete" && store.selectedClip) store.deleteClip(...)
      if (e.key === " ") {
        e.preventDefault()
        store.togglePlayback()
      }
    }
    window.addEventListener("keydown", handleKeyDown)
    return () => window.removeEventListener("keydown", handleKeyDown)
  }, [store])
}
```

7️⃣ src/components/timeline/history-panel.tsx (Dev tool - optional)
   - Display: Side panel showing history stack (for debugging)
   - Show: "Drag clip #3", "Trim clip #5", etc.
   - Current state: Bold highlight
   - Click history item: Jump to that state
   - Only show if isDev=true

UPDATE STORE:

8️⃣ Update src/stores/timelineStore.ts
   - Replace history array with HistoryManager instance
   - All mutating actions add state to history:
     * moveClip, trimClip, addTextOverlay, updateVolumeaddTextOverlay, etc.
   - updateAll undo/redo methods to use HistoryManager

UPDATE PAGE LAYOUT:

9️⃣ Update src/app/(dashboard)/studio/[id]/timeline/page.tsx
```
┌─ Toolbar ──────────────────────────────────────┐
│ [↶ Undo] [↷ Redo] [Generate Preview]           │
└────────────────────────────────────────────────┘
┌─ Canvas Area ──────────────────────────────────┐
│ (Konva timeline: ruler, playhead, clips)       │
│ [Music Panel] [Text Overlay Panel]             │
└────────────────────────────────────────────────┘
┌─ Preview (Collapsed by default) ───────────────┐
│ [Show/Hide Preview] [Video player when shown]  │
└────────────────────────────────────────────────┘
```

DELIVERABLES:
1. src/lib/timeline/preview-render-service.ts
2. src/lib/timeline/history-manager.ts
3. src/components/timeline/timeline-preview-render.tsx
4. src/components/timeline/preview-player.tsx
5. src/components/timeline/undo-redo-toolbar.tsx
6. src/components/timeline/history-panel.tsx (optional dev tool)
7. Updated: src/hooks/use-timeline-keybinds.ts
8. Updated: src/stores/timelineStore.ts
9. Updated: src/app/(dashboard)/studio/[id]/timeline/page.tsx

VALIDATION:
The Lead Dev will check:
- ✅ "Generate Preview" button works
- ✅ Progress stages display correctly
- ✅ Preview player syncs with timeline playhead
- ✅ Undo button reverts last action
- ✅ Redo button restores undone action
- ✅ Can undo 50 actions deep
- ✅ Future states discarded after new edit
- ✅ Keyboard shortcuts work (Ctrl+Z, Ctrl+Shift+Z)
- ✅ Undo/redo buttons disable appropriately
- ✅ No console errors

OUTPUT:
Provide complete, production-ready code with history state diagram in comments.
```

---

## 🟡 PROMPT 10: PHASE 11 - Review Links & Sharing
**Run Weeks 13-14 | Effort: 4 days | RUN AFTER PHASE 10**

### Files to Attach:
- All Plan 2.0 documentation files
- `figma-design-specs.md`
- `src/types/index.ts`
- `src/components/ui/*` (button, dialog, dropdown, etc.)

### Task:
Build sharing page with review link generator, brand kit editor. Build public review page (no auth, token-based access). Add YouTube publish modal.

[DETAILED PROMPT - Similar structure to above]

---

## 🟡 PROMPT 11: PHASE 12 - Analytics & Admin
**Run Weeks 15-18 | Effort: 3 days | RUN AFTER PHASE 11**

### Files to Attach:
- All Plan 2.0 documentation
- `figma-design-specs.md`
- `src/types/index.ts`
- recharts components (ensure installed)

### Task:
Build analytics dashboard (episodes), admin dashboard (team metrics), notification system, usage meter with tier-based limits.

[DETAILED PROMPT - Similar structure above]

---

## 📊 EXECUTION CHECKLIST

Use this to track progress:

```
WEEK 1: Pre-Phase Setup
☐ Copy Prompt 0 (Mock Data)
☐ Paste into Claude Chat
☐ Attach 3 files listed
☐ Claude generates all 8 mock files
☐ Save to src/lib/mock-data/
☐ Verify: No import errors, all types compile

WEEK 2-3: Phase 6 + Phase 7 (PARALLEL)
☐ Copy Prompt 1 (Phase 6) → Paste into NEW Claude chat
☐ Attach files listed (design specs, types, UI components)
☐ Simultaneously: Copy Prompt 2 (Phase 7) → Paste into SEPARATE Claude chat
☐ Phase 6: 6 files generated, review in detail
☐ Phase 7: 5 files generated, review in detail
☐ Both working before moving forward

WEEK 4: Phase 8
☐ Copy Prompt 3 → Paste into Claude
☐ 6 files generated
☐ Verify cost calculation: 12 × $0.056 = $0.67

WEEK 5-7: Phase 9
☐ Copy Prompt 4 → Paste into Claude
☐ 6 files generated
☐ Verify render progress animates
☐ Test SignalR mock events

WEEK 8-12: Phase 10 (LONGEST - 5 prompts)
☐ Prompt 5 (10A): Data model + types + store
☐ Verify: No circular deps, all types compile
☐ Prompt 6 (10B): Konva.js canvas rendering
☐ Verify: Canvas renders clips with colors
☐ Prompt 7 (10C): Drag + trim + collision
☐ Verify: Clips snap to grid, red overlay on overlap
☐ Prompt 8 (10D): Music + text overlays
☐ Verify: Music volume works, text renders
☐ Prompt 9 (10E): Preview + undo/redo
☐ Verify: Preview renders in 5 sec, undo/redo works

WEEK 13-14: Phase 11
☐ Copy Prompt 10 → Paste into Claude
☐ Review link generator
☐ Public review page (no auth)
☐ YouTube publish modal
☐ Brand kit editor

WEEK 15-18: Phase 12
☐ Copy Prompt 11 → Paste into Claude
☐ Analytics dashboard
☐ Admin dashboard
☐ Notifications
☐ Usage meter

FINAL: System Integration
☐ All phases compile (no errors)
☐ Mock data working end-to-end
☐ Ready to connect to real backend APIs
```

---

## 🎯 NEXT STEPS

1. **Copy PROMPT 0** (Pre-Phase) from above
2. **Paste into Claude Chat** (fresh conversation)
3. **Attach files**:
   - figma-design-specs.md
   - v2-implementation-plan.md
   - v2-summary.md
   - tsconfig.json (from your project)
4. **Claude generates** 8 mock data files
5. **Save all files** to `src/lib/mock-data/`
6. **Verify no errors**: `npm run type-check`
7. **Move to Prompt 1** (Phase 6)

---

## 📞 QUESTIONS DURING EXECUTION?

If Claude generates code that doesn't compile or has issues:

1. **Copy error message** + file path
2. **Paste back into same Claude chat** (not new conversation)
3. **Say**: "Error in [file.tsx]: [error message]. Fix this."
4. **Claude fixes** in same conversation (context maintained)

Keep conversations separate per phase (Phase 6 in one chat, Phase 7 in another, etc.) for clarity.

---

**YOU'RE READY.** Start with Prompt 0 (Pre-Phase) now! 🚀
