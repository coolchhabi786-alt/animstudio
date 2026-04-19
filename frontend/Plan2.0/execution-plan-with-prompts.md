# Comprehensive Execution Plan: Plan 2.0 Frontend Development
## Claude Sonnet 4.6 | Lead Dev Review | Mock Data Testing

**Created**: April 19, 2026  
**Lead Developer**: You (15+ years experience)  
**Development Team**: Claude Sonnet 4.6 (AI agents)  
**Timeline**: Phases 6-12, ~18 weeks  
**Strategy**: Mock data first, then switch to real backend  

---

## PART 1: EXECUTION ROADMAP

### Phase Execution Order (Sequential + Parallel)

```
BATCH A (Weeks 1-3): Foundation Setup + Phase 6-7 (PARALLEL)
├─ Pre-Phase Setup: Mock data structure + TypeScript types
├─ Phase 6: Storyboard Studio (Konva.js shot grid, JSON visualization)
└─ Phase 7: Voice Studio (character voice picker, audio preview)

BATCH B (Weeks 4-5): Phase 8 (Animation Approval)
├─ Animation cost estimation UI
├─ Clip player component
└─ Approval dialog workflow

BATCH C (Weeks 6-7): Phase 9 (Render & Delivery)
├─ Aspect ratio picker
├─ Render progress UI (SignalR mock)
└─ CDN URL handling

BATCH D (Weeks 8-12): Phase 10 (Timeline Editor) ⭐ MOST COMPLEX
├─ Sub-Phase 10A: Timeline data model + Zustand store
├─ Sub-Phase 10B: Konva.js canvas + track rendering
├─ Sub-Phase 10C: Trim + drag-drop interactions
├─ Sub-Phase 10D: Music panel + text overlay system
└─ Sub-Phase 10E: Preview rendering + undo/redo

BATCH E (Weeks 13-14): Phase 11 (Review Links & Sharing)
├─ Review link generator
├─ Public review page (no auth)
├─ YouTube publish modal
└─ Brand kit editor

BATCH F (Weeks 15-18): Phase 12 (Analytics & Admin)
├─ Episode analytics dashboard
├─ Admin stats dashboard
├─ Notification system
└─ Usage meter visualization
```

### Dependencies

```
Phase 6 → needs: Storyboard mock data (shots array)
Phase 7 → needs: Voice mock data (characters array)
Phase 8 → needs: Animation clips mock (cost + status)
Phase 9 → needs: Render mock data (progress events)
Phase 10 → DEPENDS ON: Phase 9 (uses rendered clips) ⭐ CRITICAL
Phase 11 → needs: Review link mock data (tokens, comments)
Phase 12 → needs: Analytics + notification mock data
```

---

## PART 2: MOCK DATA STRATEGY

### Mock Data Structure (You provide sample video/audio files)

**Location**: `src/lib/mock-data/`

```typescript
// ✅ Create these files with sample data:

1. mock-storyboard.ts
   - Storyboard with 3 scenes × 4 shots each
   - Each shot: ImageUrl (use Unsplash URLs), description, styleOverride
   - Sample: 12 shots total

2. mock-voices.ts
   - 5 characters with assigned voices
   - Voice names: Alloy, Echo, Fable, Onyx, Nova
   - Sample audio preview URLs (TTS-like)

3. mock-animation.ts
   - 12 animation clips (matching storyboard)
   - Status: queued, processing, ready, failed
   - Duration: 5-8 seconds each
   - Cost: $0.056 per clip (Kling pricing)

4. mock-renders.ts
   - Sample render with progress events
   - Stages: queued → assembling → mixing → done
   - Final CDN URL (signed, 30-day expiry)
   - SRT file content (sample captions)

5. mock-timeline.ts ⭐ CRITICAL FOR PHASE 10
   - 3 video tracks (animation clips from phase 8)
   - 1 audio track (your sample audio file)
   - 1 music track (stock sample)
   - 1 text track (title + scene captions)
   - Timeline length: 3 minutes
   - Each clip has: startMs, endMs, trimStartMs, trimEndMs

6. mock-review-links.ts
   - 3 review links (active, expired, revoked)
   - Comments with timestamps
   - Brand kit settings (logo, colors, watermark)

7. mock-analytics.ts
   - Episode stats (views, shares, render count)
   - Team aggregate stats
   - Job queue history
   - Admin metrics (DAU, MAU, costs)
```

### How to Prepare Mock Data Files

**For your sample video/audio:**

1. Place in `public/mock-assets/`:
   - `sample-video.mp4` (15 seconds, H.264, 1920x1080)
   - `sample-audio.mp3` (30 seconds, mono or stereo)
   - `sample-captions.srt` (SRT subtitle file)

2. Reference in mock data:
   ```typescript
   // mock-animation.ts
   const mockClips = [
     { clipUrl: '/mock-assets/sample-video.mp4', durationSeconds: 5 }
   ]
   ```

3. SignalR mock events (for testing progress):
   ```typescript
   // Mock SignalR events every 2 seconds
   setInterval(() => {
     emit('RenderProgress', { 
       episodeId: '1', 
       percent: Math.random() * 100, 
       stage: 'mixing' 
     })
   }, 2000)
   ```

---

## PART 3: FILE ATTACHMENTS INDEX

### Files to Attach to Claude Chat (by phase)

**PRE-PHASE: Mock Data Templates**
```
📎 Attach:
  ├─ figma-design-specs.md (design tokens)
  ├─ v2-implementation-plan.md (overall architecture)
  ├─ v2-summary.md (quick reference)
  └─ [YOUR] tsconfig.json (TypeScript config)
```

**PHASE 6: Storyboard Studio**
```
📎 Attach:
  ├─ agent-implementation-briefs.md (Phase 6 section)
  ├─ figma-design-specs.md (component specs)
  ├─ phase-10-12-detailed.md (component interfaces)
  ├─ [YOUR] src/types/index.ts (existing types)
  ├─ [YOUR] src/components/ui/*.tsx (button, card, skeleton)
  ├─ [YOUR] src/hooks/use-saga-state.ts (state pattern)
  └─ [MOCK] mock-storyboard.ts
```

**PHASE 7: Voice Studio**
```
📎 Attach:
  ├─ agent-implementation-briefs.md (Phase 7 section)
  ├─ figma-design-specs.md (component specs)
  ├─ [YOUR] src/components/ui/*.tsx (dropdown, badge, input)
  ├─ [YOUR] src/hooks/useCurrentUser.ts (team context)
  ├─ [YOUR] src/stores/authStore.ts (user context)
  └─ [MOCK] mock-voices.ts
```

**PHASE 8: Animation Approval**
```
📎 Attach:
  ├─ agent-implementation-briefs.md (Phase 8 section)
  ├─ figma-design-specs.md (component specs)
  ├─ [YOUR] src/components/ui/*.tsx (dialog, button, progress)
  ├─ [YOUR] src/hooks/use-animation.ts (existing animation hook)
  └─ [MOCK] mock-animation.ts
```

**PHASE 9: Render & Delivery**
```
📎 Attach:
  ├─ agent-implementation-briefs.md (Phase 9 section)
  ├─ figma-design-specs.md (aspect ratio preview images)
  ├─ [YOUR] src/components/ui/*.tsx (tabs, dropdown, button)
  ├─ [YOUR] src/lib/api-client.ts (API client pattern)
  ├─ [YOUR] src/stores/uiStore.ts (global UI state)
  └─ [MOCK] mock-renders.ts
```

**PHASE 10: Timeline Editor (SUB-PHASES)**
```
📎 Attach for ALL Sub-Phases:
  ├─ phase-10-12-detailed.md (ENTIRE Phase 10 specification)
  ├─ figma-design-specs.md (component specs + measurements)
  ├─ [YOUR] package.json (verify @dnd-kit, konva, zustand versions)
  ├─ [YOUR] tailwind.config.ts (theme tokens)
  ├─ [YOUR] src/lib/utils.ts (utility functions)
  └─ [MOCK] mock-timeline.ts

📎 ADDITIONALLY for Sub-Phase 10B (Konva.js):
  ├─ [YOUR] src/components/ui/canvas.tsx (if exists, else we build)
  └─ Konva.js v9 documentation link: https://konva.js.org/api/Konva.Stage.html
```

**PHASE 11: Review Links & Sharing**
```
📎 Attach:
  ├─ agent-implementation-briefs.md (Phase 11 section)
  ├─ figma-design-specs.md (component specs)
  ├─ [YOUR] src/components/ui/*.tsx (drawer, form elements)
  ├─ [YOUR] src/lib/utils.ts (URL generation)
  ├─ [YOUR] middleware.ts (public routes)
  └─ [MOCK] mock-review-links.ts
```

**PHASE 12: Analytics & Admin**
```
📎 Attach:
  ├─ agent-implementation-briefs.md (Phase 12 section)
  ├─ figma-design-specs.md (component specs)
  ├─ [YOUR] src/components/ui/*.tsx (card, tabs, badge)
  ├─ [YOUR] src/lib/utils.ts (formatting helpers)
  ├─ [YOUR] next.config.mjs (environment variables)
  └─ [MOCK] mock-analytics.ts
```

---

## PART 4: PHASE-BY-PHASE PROMPTS

### Format: Copy entire prompt → paste into Claude Chat → attach files listed

---

## 🔵 PROMPT 1: PRE-PHASE SETUP (Mock Data Structure)

```
You are a senior React/TypeScript developer. I am the Lead Developer with 15+ years experience reviewing your work.

TASK: Create standardized mock data structure for AnimStudio frontend testing (Plan 2.0).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- State: Zustand + React Query
- Real-time: SignalR (we'll mock events)
- Phase focus: 6-12 (Storyboard → Analytics)
- User role: Creator (we'll handle auth separately)

FILES TO CREATE in `src/lib/mock-data/`:

1. mock-storyboard.ts
   - Storyboard type matching: { id, episodeId, createdAt, shots: StoryboardShot[] }
   - StoryboardShot: { id, shotIndex, sceneNumber, imageUrl, description, styleOverride, regenerationCount, updatedAt }
   - Data: 3 scenes × 4 shots = 12 shots
   - ImageUrl: Use Unsplash API (landscape image URLs like https://images.unsplash.com/photo-XXX?w=800&h=600)
   - Sample descriptions: "Wide shot of protagonist entering office", "Close-up on character reaction", etc.

2. mock-voices.ts
   - VoiceAssignment type: { id, episodeId, characterId, character: Character, voiceName, language, voiceCloneUrl, updatedAt }
   - Character type: { id, name, avatarUrl, description }
   - Built-in voices: [Alloy, Echo, Fable, Onyx, Nova, Shimmer]
   - Data: 5 characters with voice assignments
   - audioPreviewUrl: Generate TTS preview URLs (or use placeholder URLs)

3. mock-animation.ts
   - AnimationClip type: { id, episodeId, sceneNumber, shotIndex, storyboardShotId, clipUrl, durationSeconds, status, createdAt, costUsd }
   - Status enum: "queued" | "processing" | "ready" | "failed"
   - Data: 12 clips (matching 12 storyboard shots)
   - clipUrl: Use video hosting URLs or MP4 data URIs
   - costUsd: $0.056 per clip (Kling pricing)
   - durationSeconds: 5-8 seconds each
   - Total cost: $0.67 (12 × $0.056)

4. mock-renders.ts
   - Render type: { id, episodeId, renderId, status, aspectRatio, finalVideoUrl, cdnUrl, durationSeconds, createdAt, completedAt }
   - RenderProgress event: { episodeId, percent: 0-100, stage: "queued" | "assembling" | "mixing" | "done" }
   - Data: 3 renders (one per episode state)
   - cdnUrl: Signed URL format with 30-day expiry info
   - SRT captions: Sample SRT content (timecodes + text)

5. mock-timeline.ts ⭐ CRITICAL
   - Timeline type: { id, episodeId, tracks: TimelineTrack[] }
   - TimelineTrack type: { id, trackType: "video" | "audio" | "music" | "text", clips: TimelineClip[] }
   - TimelineClip type: { id, sourceId, startMs, endMs, trimStartMs, trimEndMs, transitionIn, transitionDuration, sortOrder }
   - Data structure:
     * Video track: 12 animation clips (from mock-animation.ts)
     * Audio track: 1 dialogue audio file (30 seconds)
     * Music track: 1 stock music file (3 minutes)
     * Text track: 2 text overlays (title card + scene label)
   - Timeline duration: 180,000 ms (3 minutes)
   - All times in milliseconds
   - IMPORTANT: Clips must have realistic durations and positioning

6. mock-review-links.ts
   - ReviewLink type: { id, episodeId, token: string, expiresAt, isRevoked, password, createdAt, createdByUserId }
   - ReviewComment type: { id, reviewLinkId, authorName, text, timestampSeconds, createdAt, isResolved }
   - BrandKit type: { id, teamId, logoUrl, primaryColor, secondaryColor, watermarkPosition, watermarkOpacity }
   - Data: 3 review links with states (active, expired, revoked)
   - Comments: 8 comments distributed across 2 active links

7. mock-analytics.ts
   - DashboardAnalytics type: { episodeId, viewCount, uniqueViewers, renderCount, shareCount }
   - AdminMetrics type: { dau, mau, subscriptionByTier, avgProcessingTime, costPerEpisode, errorRate }
   - JobQueueItem type: { id, stage, status, processingTimeMs, createdAt }
   - Data: Sample analytics for 5 episodes, admin metrics for last 30 days

REQUIREMENTS:
- ✅ Use TypeScript (no `any` types)
- ✅ Export as `const mockData = { storyboard, voices, animation, renders, timeline, reviews, analytics }`
- ✅ Each mock file should be 50-100 lines
- ✅ Include JSDoc comments for complex types
- ✅ All timestamps in ISO format or milliseconds (consistent)
- ✅ All URLs should be valid (use Unsplash, public video hosts, or data URIs)

DELIVERABLES:
1. Create file: src/lib/mock-data/mock-storyboard.ts
2. Create file: src/lib/mock-data/mock-voices.ts
3. Create file: src/lib/mock-data/mock-animation.ts
4. Create file: src/lib/mock-data/mock-renders.ts
5. Create file: src/lib/mock-data/mock-timeline.ts (⭐ MOST CRITICAL)
6. Create file: src/lib/mock-data/mock-review-links.ts
7. Create file: src/lib/mock-data/mock-analytics.ts
8. Create file: src/lib/mock-data/index.ts (export all)

VALIDATION (Lead Dev will check):
- All types match with src/types/index.ts
- All URLs are valid and accessible
- All timestamps are consistent
- Timeline clips have realistic start/end/trim values
- No hard-coded IDs (use v4 UUIDs)

OUTPUT:
Provide complete, ready-to-run code for all 8 files. Include brief README explaining mock data structure.
```

---

## 🟢 PROMPT 2: PHASE 6 - Storyboard Studio (Part A)

```
You are a senior React/TypeScript developer. I am the Lead Developer with 15+ years experience reviewing your work.

TASK: Implement Phase 6 - Storyboard Studio UI (Component Layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Styling: Tailwind CSS + shadcn/ui
- Testing: Mock data only (no backend yet) using src/lib/mock-data/
- Design: See figma-design-specs.md for colors, spacing, typography
- State: Zustand for global UI state

REQUIREMENTS:

1. Create Hook: src/hooks/use-storyboard-mock.ts
   - Export: useStoryboard() hook returning mock data
   - Simulate SignalR: ShotUpdated event every 3 seconds (update random shot)
   - Return: { storyboard, isLoading, updateShot, regenerateShot }
   - NO real API calls (mock only)

2. Modify Types: src/types/index.ts
   - Add StoryboardShot type: { id, shotIndex, sceneNumber, imageUrl, description, styleOverride, regenerationCount, updatedAt }
   - Add Storyboard type: { id, episodeId, shots: StoryboardShot[] }
   - Ensure TypeScript strict mode passes

3. Create Components:

   a) ShotCard.tsx (NEW)
      - Props: { shot: StoryboardShot, onRegenerate: () => void, onStyleEdit: () => void }
      - Display: Thumbnail image + description text + action buttons
      - Responsive: Works on mobile (1 col) → tablet (2 cols) → desktop (4 cols)
      - Hover effect: Subtle shadow + button visibility
      - Button 1: "Regenerate" (show regeneration count)
      - Button 2: "Edit Style" (opens modal)
      - Loading state: Skeleton placeholder while regenerating

   b) ShotGrid.tsx (NEW)
      - Props: { shots: StoryboardShot[], onCardAction: (shotId, action) => void }
      - Render: Grid of ShotCard components
      - Scene navigator: Two buttons (← Previous Scene | Next Scene →)
      - Display: 4 shots per scene (responsive grid)
      - Loading: Show skeleton grid (12 skeletons) while loading
      - CSS Grid: grid-cols-4 on desktop, grid-cols-2 on tablet, grid-cols-1 on mobile

   c) ShotViewerModal.tsx (NEW)
      - Props: { shot: StoryboardShot | null, isOpen: boolean, onClose: () => void }
      - Display: Full-screen lightbox with shot image
      - Navigation: Prev/Next shot arrows (change currentShot in state)
      - Display: Shot description, scene info, regeneration count
      - Zoom: Allow pinch-zoom on mobile
      - Close: Escape key or X button

   d) StyleOverridePopover.tsx (NEW)
      - Props: { shot: StoryboardShot, onApply: (styleOverride: string) => void }
      - UI: Popover (shadcn) with 6 style preset buttons
      - Presets: "Realistic", "Cartoon", "Anime", "Watercolor", "Pencil Sketch", "3D Render"
      - Display: Color-coded buttons with icon + text
      - Feedback: Show "Style applied! Regenerating..." toast

4. Create Page: src/app/(dashboard)/studio/[id]/storyboard/page.tsx
   - Layout: Main container with header (title + breadcrumb)
   - Header: "Storyboard Studio" + episode name
   - Content: ShotGrid component
   - State: Use useStoryboard() mock hook
   - Handlers:
     * onRegenerate: Show loading spinner on card, call mock regenerate (wait 2 seconds)
     * onStyleEdit: Open StyleOverridePopover
   - Error state: Show error message if mock data fails

5. Create Zustand Store: src/stores/storyboardStore.ts (if not exists)
   - State: { currentShot, isLoading, selectedScene, regeneratingShots }
   - Actions: { setCurrentShot, setLoading, nextScene, prevScene, markRegeneration }
   - Persist: No persistence needed for mock

DESIGN TOKENS (from figma-design-specs.md):
- Primary color: #4F46E5 (Indigo)
- Secondary: #8B5CF6 (Purple)
- Background: #F9FAFB (Light gray)
- Border: #E5E7EB (Medium gray)
- Text primary: #111827 (Dark gray)
- Spacing: 8px, 16px, 24px, 32px
- Border radius: 8px
- Font: Inter (family), 14px (body), 16px (button), 20px (heading)

DELIVERABLES:
1. src/hooks/use-storyboard-mock.ts (mock data + SignalR simulation)
2. src/types/index.ts (updated types)
3. src/components/storyboard/shot-card.tsx
4. src/components/storyboard/shot-grid.tsx
5. src/components/storyboard/shot-viewer-modal.tsx
6. src/components/storyboard/style-override-popover.tsx
7. src/app/(dashboard)/studio/[id]/storyboard/page.tsx
8. src/stores/storyboardStore.ts (Zustand)
9. README: Component dependencies and props

VALIDATION (Lead Dev will check):
✅ All components render with mock data
✅ Clicking "Regenerate" shows 2-second delay then new image
✅ Clicking shot opens full-screen modal
✅ Style buttons show toast feedback
✅ Responsive layout works (test on mobile, tablet, desktop)
✅ No console errors
✅ TypeScript strict mode passes

OUTPUT:
Provide complete, production-ready code. Include commented sections for clarity.
```

---

## 🟢 PROMPT 3: PHASE 7 - Voice Studio (Part A)

```
You are a senior React/TypeScript developer. I am the Lead Developer with 15+ years experience reviewing your work.

TASK: Implement Phase 7 - Voice Studio UI (Component Layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Styling: Tailwind CSS + shadcn/ui
- Testing: Mock data only (no backend yet) using src/lib/mock-data/
- Audio: HTML5 audio element for playback
- API mock: TTS preview (return placeholder audio URLs)

REQUIREMENTS:

1. Create Hook: src/hooks/use-voice-assignments-mock.ts
   - Export: useVoiceAssignments() hook returning mock data
   - Return: { characters, voiceAssignments, isLoading, updateAssignment, playPreview }
   - playPreview(voiceName): Return promise with audio URL (mock)
   - updateAssignment(characterId, voiceName): Update local state (no API)
   - NO real API calls (mock only)

2. Modify Types: src/types/index.ts
   - Add VoiceAssignment type: { id, episodeId, characterId, character: Character, voiceName, language, voiceCloneUrl, updatedAt }
   - Add Character type (if not exists): { id, name, avatarUrl, description }
   - Voice enum: "Alloy" | "Echo" | "Fable" | "Onyx" | "Nova" | "Shimmer"
   - Language enum: "en-US" | "en-GB" | "es-ES" | "fr-FR" | "de-DE" | "ja-JP"

3. Create Components:

   a) VoicePicker.tsx (NEW)
      - Props: { currentVoice: string, voices: string[], onSelect: (voice: string) => void }
      - UI: shadcn/ui Select component
      - Options: 6 voice options with labels + gender badges (M/F for visual differentiation)
      - Display format: "Alex (Nova) - Male" in dropdown
      - onChange: Trigger onSelect callback

   b) LanguageSelector.tsx (NEW)
      - Props: { currentLanguage: string, onSelect: (lang: string) => void }
      - UI: shadcn/ui Select component
      - Options: 6 language options with flag icons (using text flags or emoji)
      - Display format: "🇺🇸 English (US)" in dropdown
      - onChange: Trigger onSelect callback

   c) AudioPreviewPlayer.tsx (NEW)
      - Props: { voiceName: string, characterName: string, sampleText: string, onPlay: () => void }
      - UI: Button "Play Preview" + HTML5 audio element
      - State: isLoading, isPlaying, error
      - Flow:
        1. Click "Play Preview" → show loading spinner
        2. Call mock playPreview(voiceName) → get audio URL (2 second delay)
        3. Show HTML5 audio player with controls (play/pause, seek, volume)
        4. On playback complete → hide player
      - Error handling: Show error toast if preview fails

   d) VoiceCloneUpload.tsx (NEW)
      - Props: { characterId: string, onUpload: (file: File) => void, isTierLocked: boolean }
      - Display: Conditional render based on isTierLocked
        * If locked (free tier): Show lock icon + "Upgrade to Studio tier"
        * If unlocked: Show upload area
      - Upload area: Drag-and-drop or click to upload .wav/.mp3 file
      - File validation: Accept audio files only (audio/mpeg, audio/wav)
      - Feedback: Show file name + upload progress (mock 100% in 2 seconds)
      - Success: Show "Voice clone uploaded! Processing..." toast

   e) CharacterVoiceRow.tsx (NEW)
      - Props: { character: Character, assignment: VoiceAssignment, onUpdate: (voiceName) => void }
      - Layout: Horizontal row with columns:
        * Column 1: Avatar (40px circular image) + Character name
        * Column 2: Voice picker (VoicePicker component)
        * Column 3: Language selector (LanguageSelector component)
        * Column 4: Play preview button (AudioPreviewPlayer)
      - Responsive: Stack on mobile, row on desktop
      - Padding: 16px per row

4. Create Page: src/app/(dashboard)/studio/[id]/voice/page.tsx
   - Header: "Voice Studio" + episode name
   - Content: Two sections
     * Section 1: Character voice assignments
       - Rendered as table rows (CharacterVoiceRow × 5)
       - No header row (use column labels in first row)
     * Section 2: Voice cloning (Studio tier feature)
       - Subheading: "Clone Custom Voices"
       - 3 VoiceCloneUpload components (for 3 characters)
       - Tier gate: Show "Upgrade" message if free tier
   - State: Use useVoiceAssignments() mock hook
   - Handlers:
     * onVoiceChange: Update assignment locally
     * onLanguageChange: Update assignment locally
     * onPlayPreview: Show AudioPreviewPlayer
     * onVoiceClone: Show upload progress

5. Create Zustand Store: src/stores/voiceStore.ts (if not exists)
   - State: { assignments, previewingCharacterId, isLoading }
   - Actions: { updateAssignment, setPreviewingCharacter, setLoading }
   - Persist: localStorage (remember last selected voices)

DESIGN TOKENS (from figma-design-specs.md):
- Primary: #4F46E5
- Secondary: #8B5CF6
- Success: #10B981 (play button)
- Warning: #F59E0B (locked icon)
- Spacing: 8px, 16px, 24px
- Border radius: 8px

DELIVERABLES:
1. src/hooks/use-voice-assignments-mock.ts
2. src/types/index.ts (updated types)
3. src/components/voice/voice-picker.tsx
4. src/components/voice/language-selector.tsx
5. src/components/voice/audio-preview-player.tsx
6. src/components/voice/voice-clone-upload.tsx
7. src/components/voice/character-voice-row.tsx
8. src/app/(dashboard)/studio/[id]/voice/page.tsx
9. src/stores/voiceStore.ts (Zustand)

VALIDATION (Lead Dev will check):
✅ All components render with mock data
✅ Clicking "Play Preview" shows audio player after 2-second delay
✅ Voice picker updates on selection
✅ Language selector updates on selection
✅ Voice clone upload shows progress bar
✅ Tier lock working (if applicable)
✅ No console errors
✅ TypeScript strict mode passes

OUTPUT:
Provide complete, production-ready code. Include component tree diagram as comment.
```

---

## 🟢 PROMPT 4: PHASE 8 - Animation Approval (Part A)

```
You are a senior React/TypeScript developer. I am the Lead Developer with 15+ years experience reviewing your work.

TASK: Implement Phase 8 - Animation Approval UI (Component Layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Testing: Mock data (12 animation clips from storyboard)
- Cost calculation: $0.056 per clip (Kling pricing)
- State: Zustand for animation state
- Design: See figma-design-specs.md

REQUIREMENTS:

1. Create Hook: src/hooks/use-animation-mock.ts
   - Export: useAnimation() hook
   - Return: { clips, estimate, isLoading, approveAnimation, estimateCost }
   - estimateCost(backend): Calculate cost for all clips
   - approveAnimation(backend): Simulate approval (show 3-second processing)
   - Backend options: "kling" | "local"
   - NO real API calls

2. Update Types: src/types/index.ts
   - Add AnimationClip type: { id, episodeId, sceneNumber, shotIndex, clipUrl, durationSeconds, status: "queued"|"processing"|"ready"|"failed", costUsd, createdAt }
   - Add AnimationJob type: { id, episodeId, backend: "kling"|"local", estimatedCostUsd, actualCostUsd, approvedByUserId, status, createdAt }
   - Add AnimationEstimate type: { totalClips, costPerClip, totalCostUsd, backend }

3. Create Components:

   a) CostBreakdownTable.tsx (NEW)
      - Props: { shots: { sceneNumber, shotIndex }[], rate: number, backend: string }
      - Display: Table with columns
        * Scene | Shots | Rate ($) | Subtotal ($)
        * Rows: Per scene summary
        * Footer row: TOTAL COST (bold, larger font)
      - Format: Currency with 2 decimals ($0.67)
      - Example: Scene 1 has 4 shots × $0.056 = $0.22

   b) BackendSelector.tsx (NEW)
      - Props: { selectedBackend: string, onSelect: (backend: string) => void }
      - UI: Radio button group (2 options)
        * Option 1: "Kling AI" (Default) - Description: "High quality, $0.056/clip"
        * Option 2: "Local" - Description: "Free, lower quality"
      - onChange: Trigger onSelect
      - Visual: Icon + label + description per option

   c) ClipPlayer.tsx (NEW)
      - Props: { clip: AnimationClip, autoPlay?: boolean }
      - Display: HTML5 video player
      - Controls: Play/pause, seek bar, volume, fullscreen
      - Feedback: Loading skeleton before video loads
      - Error: Show error message if video fails
      - Loop: Video loops when finished

   d) ClipPreviewGrid.tsx (NEW)
      - Props: { clips: AnimationClip[], groupByScene?: boolean }
      - Display: Accordion per scene (if groupByScene=true)
        * Scene 1 (4 clips) → [accordion open]
        * Scene 2 (4 clips) → [accordion collapsed]
        * Scene 3 (4 clips) → [accordion collapsed]
      - Each clip: ClipPlayer component in small (200×200) preview
      - Status badge: "Queued", "Processing", "Ready", "Failed" (color-coded)
      - Click clip: Expand to larger player

   e) ApprovalDialog.tsx (NEW)
      - Props: { isOpen: boolean, onConfirm: () => void, onCancel: () => void, estimate: AnimationEstimate }
      - Display: Modal dialog
      - Title: "Approve Animation Generation"
      - Content:
        * Show backend name: "Backend: Kling AI"
        * Show total shot count: "Shots to process: 12"
        * Show total cost: "Estimated cost: $0.67"
        * Show balance: "Credits remaining: $45.33" (mock)
      - Buttons: [Cancel] [Approve & Process]
      - Confirmation: "Processing will take ~5 minutes. Continue?"

4. Create Page: src/app/(dashboard)/studio/[id]/animation/page.tsx
   - Header: "Animation Studio" + episode name
   - Section 1: Cost Estimator
     * Subheading: "Animation Generation"
     * BackendSelector component
     * CostBreakdownTable component
     * Approve button: "Approve & Process" (opens ApprovalDialog)
   - Section 2: Processing Progress
     * Overall progress bar (0-100%)
     * Status: "Queued", "Processing: Shot 3/12", "Complete"
     * Live update: Simulate progress every 1 second (increment %)
   - Section 3: Clip Previews
     * Subheading: "Generated Clips"
     * ClipPreviewGrid component (groupByScene=true)
     * Filter: Tabs to show "All", "Ready", "Processing", "Failed"
   - State: Use useAnimation() mock hook
   - Handlers:
     * onBackendChange: Update estimate
     * onApprove: Open dialog, simulate 3-second processing
     * onClipClick: Show full-size player

5. Create Zustand Store: src/stores/animationStore.ts (if not exists)
   - State: { backend, estimate, isProcessing, progressPercent, clips }
   - Actions: { setBackend, calculateEstimate, startProcessing, updateProgress }

DESIGN TOKENS:
- Success green: #10B981 (Ready status)
- Warning orange: #F59E0B (Processing)
- Muted gray: #6B7280 (Queued)
- Error red: #EF4444 (Failed)
- Currency format: USD with 2 decimals

DELIVERABLES:
1. src/hooks/use-animation-mock.ts
2. src/types/index.ts (updated types)
3. src/components/animation/cost-breakdown-table.tsx
4. src/components/animation/backend-selector.tsx
5. src/components/animation/clip-player.tsx
6. src/components/animation/clip-preview-grid.tsx
7. src/components/animation/approval-dialog.tsx
8. src/app/(dashboard)/studio/[id]/animation/page.tsx
9. src/stores/animationStore.ts

VALIDATION (Lead Dev will check):
✅ Cost calculation correct: 12 clips × $0.056 = $0.67
✅ Backend selector works (Kling/Local)
✅ Approval dialog shows confirmation
✅ Progress bar animates from 0 to 100% during "processing"
✅ Clip players load and play video
✅ Status badges color-coded correctly
✅ No console errors

OUTPUT:
Provide complete, production-ready code. Include cost calculation logic as separate utility function.
```

---

## 🟡 PROMPT 5: PHASE 9 - Render & Delivery (Part A)

```
You are a senior React/TypeScript developer. I am the Lead Developer with 15+ years experience reviewing your work.

TASK: Implement Phase 9 - Render & Delivery UI (Component Layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Testing: Mock data with simulated render progress (SignalR events)
- CDN URLs: Mock signed URLs with 30-day expiry
- Aspect ratios: 16:9 (default), 9:16 (vertical), 1:1 (square)
- Main challenge: Real-time progress updates via SignalR

REQUIREMENTS:

1. Create Hook: src/hooks/use-renders-mock.ts
   - Export: useRenders() hook
   - Return: { renders, currentProgress, isRendering, startRender, getRenderUrl }
   - startRender(aspectRatio): Trigger mock render process
   - getRenderUrl(renderId): Return signed CDN URL (mock)
   - SignalR simulation: Emit RenderProgress event every 2 seconds (percent 0→100)
   - SignalR simulation: Emit RenderComplete event when done
   - NO real API calls

2. Update Types: src/types/index.ts
   - Add Render type: { id, episodeId, status: "queued"|"assembling"|"mixing"|"done", aspectRatio: "16:9"|"9:16"|"1:1", finalVideoUrl, finalCdnUrl, durationSeconds, createdAt, completedAt }
   - Add RenderProgress event: { episodeId, percent: 0-100, stage: "queued"|"assembling"|"mixing"|"done" }
   - Add RenderComplete event: { episodeId, cdnUrl, durationSeconds }

3. Create Components:

   a) AspectRatioPicker.tsx (NEW)
      - Props: { selected: string, onSelect: (ratio: string) => void }
      - Display: 3 options as visual cards (not buttons)
        * Card 1: "16:9" (landscape preview showing 16:9 rectangle)
        * Card 2: "9:16" (portrait/vertical preview)
        * Card 3: "1:1" (square preview)
      - Each card: Show aspect ratio dimensions (e.g., "1920×1080")
      - Visual: Black border on selected card, gray border on unselected
      - onClick: Trigger onSelect with ratio string

   b) RenderProgressBar.tsx (NEW)
      - Props: { percent: 0-100, currentStage: string, isComplete: boolean }
      - Display: Linear progress bar (0-100%)
      - Below progress bar: Stage label
        * "Queued..." (0-20%)
        * "Assembling video frames..." (20-50%)
        * "Mixing audio..." (50-80%)
        * "Finalizing..." (80-99%)
        * "Complete ✓" (100%)
      - Animate: Progress bar smoothly transitions
      - Color: Blue while processing, green when complete

   c) DownloadBar.tsx (NEW)
      - Props: { renderId: string, videoUrl: string, srtUrl: string }
      - Display: Horizontal bar with 2 download buttons
        * Button 1: "Download MP4" (video file icon)
        * Button 2: "Download SRT" (captions icon)
      - Functionality: Both buttons download files (mock)
      - Button styling: shadcn/ui button variant="outline"

   d) RenderHistoryTable.tsx (NEW)
      - Props: { renders: Render[] }
      - Display: Table with columns
        * Date Created | Duration | Aspect | Status | Actions
        * Rows: 1 row per render (newest first)
        * Status badge: "Queued", "Processing", "Complete", "Failed"
      - Actions column: 2 buttons
        * "Download" (dropdown: MP4, SRT)
        * "Re-render" (same aspect ratio)
      - Format date: "Apr 19, 2:30 PM"

   e) VideoPlayerWithCaption.tsx (NEW)
      - Props: { videoUrl: string, captionUrl?: string }
      - Display: HTML5 video player (full width, responsive)
      - Controls: Play/pause, seek bar, volume, fullscreen, captions toggle
      - If captionUrl provided: Show VTT/SRT captions on video
      - Loading: Skeleton while video loads
      - Poster: Black background while loading

4. Create Page: src/app/(dashboard)/studio/[id]/render/page.tsx
   - Header: "Post-Production Render" + episode title
   - Layout: Two-column (desktop) or stacked (mobile)
     * Left column (40%):
       - Subheading: "Export Settings"
       - AspectRatioPicker component
       - Render button: "Start Render" (disabled if rendering)
     * Right column (60%):
       - Subheading: "Render Progress"
       - RenderProgressBar component (shows current progress)
       - DownloadBar component (when complete)
       - VideoPlayerWithCaption component (when complete)
   - Below both columns:
     - Subheading: "Render History"
     - RenderHistoryTable component
   - State: Use useRenders() mock hook
   - Handlers:
     * onAspectRatioChange: Update selector
     * onStartRender: Disable button, start mock render (0→100% over 10 seconds)
     * onReRender: Trigger render with saved aspect ratio

5. Mock SignalR Events:
   - Simulate RenderProgress every 2 seconds
   - Example data:
     ```
     { episodeId: "ep-123", percent: 25, stage: "assembling" }
     { episodeId: "ep-123", percent: 50, stage: "mixing" }
     { episodeId: "ep-123", percent: 100, stage: "done" }
     ```
   - On completion: Show RenderComplete event with CDN URL

6. Create Zustand Store: src/stores/renderStore.ts (if not exists)
   - State: { renders, currentRender, isRendering, progressPercent, currentStage }
   - Actions: { setIsRendering, updateProgress, addRender, selectRender }

DESIGN TOKENS:
- Background: #F9FAFB
- Border: #E5E7EB
- Progress color: #3B82F6 (blue)
- Complete color: #10B981 (green)

DELIVERABLES:
1. src/hooks/use-renders-mock.ts
2. src/types/index.ts (updated types)
3. src/components/render/aspect-ratio-picker.tsx
4. src/components/render/render-progress-bar.tsx
5. src/components/render/download-bar.tsx
6. src/components/render/render-history-table.tsx
7. src/components/render/video-player-with-caption.tsx
8. src/app/(dashboard)/studio/[id]/render/page.tsx
9. src/stores/renderStore.ts

VALIDATION (Lead Dev will check):
✅ Render starts with "Start Render" button
✅ Progress bar animates 0→100% over 10 seconds
✅ Stage labels update during rendering
✅ Download buttons appear on completion
✅ Video player plays render output
✅ Render history table shows all renders
✅ Re-render button works
✅ No console errors

OUTPUT:
Provide complete, production-ready code. Include mock SignalR event simulator as separate utility.
```

---

## 🔴 PROMPT 6: PHASE 10 - Timeline Editor (SUB-PHASE 10A: Data Model & Store)

```
You are a senior React/TypeScript developer specializing in state management. I am the Lead Developer with 15+ years experience reviewing your work.

TASK: Implement Phase 10 - Timeline Editor DATA LAYER (Model + Zustand store + utilities).

⭐ CRITICAL: Phases 10A-E are split across 5 prompts. READ THIS BEFORE STARTING OTHER PHASES 10 PROMPTS.

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- State management: Zustand (no Redux, we're keeping it simple)
- Testing: Mock timeline data from src/lib/mock-data/mock-timeline.ts
- Canvas: Konva.js v9 (we build this in 10B)
- Challenge: Handle 50+ timeline clips, real-time sync, undo/redo
- Compatibility: Must work with later sub-phases

REQUIREMENTS:

1. Create Comprehensive Timeline Types: src/types/timeline.ts (NEW)

   ```typescript
   // Enums
   export enum TrackType {
     Video = "video",
     Audio = "audio",
     Music = "music",
     Text = "text"
   }

   export enum TransitionType {
     Cut = "cut",
     Fade = "fade",
     Dissolve = "dissolve"
   }

   export enum TextAnimation {
     None = "none",
     FadeIn = "fade-in",
     SlideUp = "slide-up"
   }

   // Domain Models
   export interface TimelineClip {
     id: string
     trackId: string
     sourceId: string // References animation clip, audio file, music track, or text
     startMs: number // Position on timeline
     endMs: number // Position on timeline
     trimStartMs: number // Trim at source
     trimEndMs: number // Trim at source
     transitionIn: TransitionType
     transitionDuration: number // milliseconds
     sortOrder: number
     mediaUrl?: string // For video/audio/music preview
     mediaDuration?: number // Source duration
     label?: string // For text clips
   }

   export interface TimelineTrack {
     id: string
     episodeId: string
     trackType: TrackType
     name: string
     sampleRate?: number // For audio tracks
     channels?: number // For audio tracks
     sortOrder: number
     isVisible: boolean
     isLocked: boolean
     clips: TimelineClip[]
     volume?: number // 0-100 for audio/music
   }

   export interface Timeline {
     id: string
     episodeId: string
     durationMs: number // Total timeline length
     fps: number // Frames per second (24, 30, etc.)
     isSpaceVisible: { video: boolean; audio: boolean; music: boolean; text: boolean }
     tracks: TimelineTrack[]
     createdAt: Date
     updatedAt: Date
   }

   export interface TimelineState {
     timeline: Timeline | null
     selectedClip: TimelineClip | null
     selectedTrack: TimelineTrack | null
     isDragging: boolean
     isTrimming: boolean
     trimMode: "start" | "end" | null
     playheadPositionMs: number
     isPlaying: boolean
     zoom: number // 1-5 (1x, 2x, 3x, etc.)
     history: Timeline[] // For undo/redo
     historyIndex: number
   }
   ```

2. Create Timeline Utilities: src/lib/timeline-utils.ts (NEW)

   ```typescript
   export const timelineUtils = {
     // Clip positioning
     moveClip(clip: TimelineClip, newStartMs: number): TimelineClip
     trimClip(clip: TimelineClip, newTrimStart: number, newTrimEnd: number): TimelineClip
     resizeClip(clip: TimelineClip, newEndMs: number): TimelineClip
     
     // Track management
     addTrack(timeline: Timeline, trackType: TrackType): Timeline
     deleteTrack(timeline: Timeline, trackId: string): Timeline
     reorderTracks(timeline: Timeline, fromIndex: number, toIndex: number): Timeline
     
     // Clip CRUD
     addClip(track: TimelineTrack, clip: TimelineClip): TimelineTrack
     deleteClip(track: TimelineTrack, clipId: string): TimelineTrack
     updateClip(track: TimelineTrack, clipId: string, updates: Partial<TimelineClip>): TimelineTrack
     reorderClips(track: TimelineTrack, fromIndex: number, toIndex: number): TimelineTrack
     
     // Conversion & calculation
     msToSeconds(ms: number): number
     secondsToMs(seconds: number): number
     msToFrame(ms: number, fps: number): number
     frameToMs(frame: number, fps: number): number
     pixelsToMs(pixels: number, zoom: number, pixelsPerSecond: number): number
     msToPixels(ms: number, zoom: number, pixelsPerSecond: number): number
     
     // Validation
     isClipOverlapping(clip1: TimelineClip, clip2: TimelineClip, tolerance?: number): boolean
     validateTimeline(timeline: Timeline): ValidationError[]
     canPlaceClip(track: TimelineTrack, clip: TimelineClip): boolean
   }
   ```

3. Create Timeline Zustand Store: src/stores/timelineStore.ts (NEW)

   ```typescript
   export const useTimelineStore = create<TimelineState>((set, get) => ({
     // ─── STATE ───────────────────────────────────────────────────
     timeline: null,
     selectedClip: null,
     selectedTrack: null,
     isDragging: false,
     isTrimming: false,
     trimMode: null,
     playheadPositionMs: 0,
     isPlaying: false,
     zoom: 1,
     history: [],
     historyIndex: -1,

     // ─── ACTIONS ──────────────────────────────────────────────────
     
     // Load timeline
     loadTimeline: (timeline: Timeline) => set({
       timeline,
       history: [timeline],
       historyIndex: 0,
       selectedClip: null,
       playheadPositionMs: 0
     }),

     // Clip selection
     selectClip: (clip: TimelineClip | null) => set({ selectedClip: clip }),
     
     // Track selection
     selectTrack: (track: TimelineTrack | null) => set({ selectedTrack: track }),

     // Playhead control
     setPlayheadPosition: (ms: number) => set({ playheadPositionMs: ms }),
     togglePlayback: () => set(state => ({ isPlaying: !state.isPlaying })),
     play: () => set({ isPlaying: true }),
     pause: () => set({ isPlaying: false }),

     // Zoom control
     setZoom: (zoom: number) => set({ zoom: Math.max(1, Math.min(5, zoom)) }),
     zoomIn: () => set(state => ({ zoom: Math.min(5, state.zoom + 0.5) })),
     zoomOut: () => set(state => ({ zoom: Math.max(1, state.zoom - 0.5) })),

     // Clip operations (with history)
     moveClip: (clipId: string, trackId: string, newStartMs: number) => {
       set(state => {
         const updated = { ...state.timeline }
         // Find and move clip
         // Add to history
         return { timeline: updated, historyIndex: state.historyIndex + 1 }
       })
     },

     trimClip: (clipId: string, trackId: string, trimStart: number, trimEnd: number) => {
       set(state => {
         const updated = { ...state.timeline }
         // Find and trim clip
         // Add to history
         return { timeline: updated }
       })
     },

     // Undo/Redo
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

     // PlayHead movement (every 30ms during playback)
     advancePlayhead: (deltaMs: number) => {
       set(state => {
         if (!state.timeline) return state
         const newPosition = Math.min(
           state.playheadPositionMs + deltaMs,
           state.timeline.durationMs
         )
         return {
           playheadPositionMs: newPosition,
           isPlaying: newPosition < state.timeline.durationMs
         }
       })
     }
   }))
   ```

4. Create Mock Timeline Provider: src/hooks/use-timeline-mock.ts (NEW)

   ```typescript
   export const useTimelineMock = () => {
     const store = useTimelineStore()
     const [timeline, setTimeline] = useState<Timeline | null>(null)

     // Load mock data on mount
     useEffect(() => {
       const mockTimeline = mockData.timeline // from src/lib/mock-data/
       store.loadTimeline(mockTimeline)
       setTimeline(mockTimeline)
     }, [])

     // Playback loop: advance playhead every 30ms (if playing)
     useEffect(() => {
       if (!store.isPlaying) return
       
       const interval = setInterval(() => {
         store.advancePlayhead(30) // 30ms per frame (~33fps)
       }, 30)
       
       return () => clearInterval(interval)
     }, [store.isPlaying])

     return {
       timeline: store.timeline,
       ...store
     }
   }
   ```

5. Create Validation & Error Types: src/types/timeline-errors.ts (NEW)

   ```typescript
   export interface ValidationError {
     type: "overlap" | "out-of-bounds" | "invalid-duration" | "missing-source"
     clipId?: string
     trackId?: string
     message: string
     severity: "error" | "warning"
   }

   export class TimelineError extends Error {
     constructor(message: string, public code: string) {
       super(message)
     }
   }
   ```

DELIVERABLES:
1. src/types/timeline.ts (comprehensive timeline types)
2. src/types/timeline-errors.ts (error types)
3. src/lib/timeline-utils.ts (utility functions)
4. src/stores/timelineStore.ts (Zustand store)
5. src/hooks/use-timeline-mock.ts (mock data + playback)
6. README: Timeline architecture overview

VALIDATION (Lead Dev will check):
✅ All types compile with no `any`
✅ Store methods work without errors
✅ Timeline utilities produce correct values
✅ Undo/redo history persists
✅ Playhead advances during playback
✅ Zoom calculations correct (1-5x)
✅ No circular dependencies

OUTPUT:
Provide complete, production-ready TypeScript code. Include extensive JSDoc comments explaining complex logic.
```

---

## 🔴 PROMPT 7: PHASE 10 - Timeline Editor (SUB-PHASE 10B: Konva.js Canvas Rendering)

⚠️ **RUN THIS AFTER PROMPT 6 (10A)**

```
You are a senior React/TypeScript developer specializing in Konva.js canvas rendering.

TASK: Implement Phase 10B - Timeline Canvas (Konva.js rendering layer).

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Canvas: Konva.js v9 (npm install konva react-konva)
- Stage dimensions: Dynamic (responsive to container)
- Pixels per second: 100px/sec (adjusts with zoom)
- Track height: 80px (video/music), 60px (audio), 50px (text)
- Clip colors: Video=blue (#3B82F6), Audio=green (#10B981), Music=purple (#8B5CF6), Text=orange (#F59E0B)

DESIGN:
```
┌─ Timeline Canvas ──────────────────────────────────────────────────────────┐
│ ┌─ Ruler (Top) ───────────────────────────────────────────────────────────┐ │
│ │ 0:00    0:05    0:10    0:15    0:20    0:25    0:30                   │ │
│ └──────────────────────────────────────────────────────────────────────────┘ │
│ │ │ ↑ Playhead (red line at current time)                                  │ 
│ ├─ Video Track ──────────────────────────────────────────┐                 │
│ │ [Clip 1]────[Clip 2]────[Clip 3]    [Clip 4]          │ 80px height    │
│ ├─────────────────────────────────────────────────────────┘                 │
│ ├─ Audio Track ──────────────────────────────────────────┐                 │
│ │ [Audio]──────────────────────────────────────────────  │ 60px height    │
│ ├─────────────────────────────────────────────────────────┘                 │
│ ├─ Music Track ──────────────────────────────────────────┐                 │
│ │ [Music Track]───────────────────────────────────────── │ 80px height    │
│ ├─────────────────────────────────────────────────────────┘                 │
│ └────────────────────────────────────────────────────────────────────────────┘
```

REQUIREMENTS:

1. Create TimelineCanvasWrapper.tsx (NEW)
   - Props: { width: number, height: number }
   - State: Container ref for responsive dimensions
   - Render: Konva Stage + container
   - Responsibilities:
     * Measure container width/height (ResizeObserver)
     * Pass dimensions to child components
     * Handle mouse/keyboard events
     * Manage scroll position

2. Create TimelineRuler.tsx (Konva component)
   - Display: Time markers at 5-second intervals
   - Format: MM:SS (0:00, 0:05, 0:10, etc.)
   - Interactive: Click on marker to jump playhead
   - Height: 40px fixed
   - Background: Light gray (#F3F4F6)

3. Create PlayheadIndicator.tsx (Konva component)
   - Display: Red vertical line at playheadPositionMs
   - Width: 2px
   - Color: #EF4444 (red)
   - Interactive: Drag to seek playhead
   - Z-index: Always on top (front layer)

4. Create ClipShape.tsx (Konva component) ⭐ MOST COMPLEX
   - Props: { clip: TimelineClip, isSelected: boolean, onSelect: () => void, onMove: (newStartMs) => void, onTrim: (side, newMs) => void }
   - Visual:
     * Rectangle shape (colored by track type)
     * Border: Dark shade of color (2px)
     * Text label: Clip name/description (white text, 12px)
     * Selection: Light glow effect + corner handles if selected
   - Trim handles:
     * Left edge: Hover shows resize cursor, drag to trim start
     * Right edge: Hover shows resize cursor, drag to trim end
   - Interactions:
     * Click: Select clip (call onSelect)
     * Drag: Move clip horizontally (constrained to track)
     * Drag left edge: Trim clip start
     * Drag right edge: Trim clip end
   - Styling:
     * Height: 60px (within 80px track, 10px margin top+bottom)
     * Rounded corners: 4px
     * Opacity: Full (1.0) if visible, 0.5 if adjacent clip selected

5. Create TrackLane.tsx (Konva component)
   - Props: { track: TimelineTrack, clips: TimelineClip[], height: number }
   - Visual: Background rectangle (alternating light gray every 2 tracks)
   - Children: Array of ClipShape components
   - Events: Drop zone for reordering clips
   - Label: Track name on left side (40px width)

6. Create TimelineContainer.tsx (Konva Group)
   - Render layers (in order):
     * Layer 1: Track backgrounds
     * Layer 2: Clips (with interaction handlers)
     * Layer 3: Playhead indicator
   - Responsiveness: Group width matches container width
   - Scroll: Horizontal scroll if content exceeds container

7. Create TimelineInteractionManager.tsx (Hook)
   ```typescript
   export const useTimelineInteraction = () => {
     const store = useTimelineStore()
     
     const handleClipSelect = (clipId: string) => { /* */ }
     const handleClipMove = (clipId: string, trackId: string, newStartMs: number) => { /* */ }
     const handleClipTrim = (clipId: string, trackId: string, side: "start"|"end", newMs: number) => { /* */ }
     const handlePlayheadDrag = (newPositionMs: number) => { /* */ }
     const handleClipDelete = (clipId: string, trackId: string) => { /* */ }
     
     return { handleClipSelect, handleClipMove, handleClipTrim, handlePlayheadDrag, handleClipDelete }
   }
   ```

8. Create TimelineKeybinds.tsx (Hook)
   ```typescript
   export const useTimelineKeybinds = () => {
     const store = useTimelineStore()
     
     useEffect(() => {
       const handleKeyDown = (e: KeyboardEvent) => {
         if (e.key === "Delete" && store.selectedClip) store.deleteClip(...)
         if (e.ctrlKey && e.key === "z") store.undo()
         if (e.ctrlKey && e.shiftKey && e.key === "z") store.redo()
         if (e.key === " ") {
           e.preventDefault()
           store.togglePlayback() // Spacebar to play/pause
         }
       }
       window.addEventListener("keydown", handleKeyDown)
       return () => window.removeEventListener("keydown", handleKeyDown)
     }, [store])
   }
   ```

9. Component Composition: src/app/(dashboard)/studio/[id]/timeline/CANVAS-LAYER.tsx
   ```typescript
   <TimelineCanvasWrapper>
     <Konva.Stage>
       <Konva.Layer name="background">
         {/* Track backgrounds */}
       </Konva.Layer>
       <Konva.Layer name="clips">
         {tracks.map(track => (
           <TrackLane key={track.id} track={track} height={TRACK_HEIGHTS[track.trackType]} />
         ))}
       </Konva.Layer>
       <Konva.Layer name="playhead">
         <PlayheadIndicator />
       </Konva.Layer>
     </Konva.Stage>
     <TimelineRuler />
   </TimelineCanvasWrapper>
   ```

PERFORMANCE CONSIDERATIONS:
- Use Konva layer optimization (only re-render changed layers)
- Memoize ClipShape components (React.memo)
- Virtualize tracks if > 10 tracks (render only visible)
- Debounce drag events (50ms)

DELIVERABLES:
1. src/components/timeline/timeline-canvas-wrapper.tsx
2. src/components/timeline/timeline-ruler.tsx
3. src/components/timeline/playhead-indicator.tsx
4. src/components/timeline/clip-shape.tsx (⭐ most important)
5. src/components/timeline/track-lane.tsx
6. src/components/timeline/timeline-container.tsx
7. src/hooks/use-timeline-interaction.ts
8. src/hooks/use-timeline-keybinds.ts
9. README: Konva component architecture

VALIDATION (Lead Dev will check):
✅ Canvas renders without errors
✅ Clips display with correct colors
✅ Playhead moves smoothly
✅ Ruler shows correct time markers
✅ Clips can be dragged and trimmed
✅ Playhead can be dragged to seek
✅ Keyboard shortcuts work (Delete, Undo, Redo, Spacebar)
✅ No console errors

OUTPUT:
Provide complete, production-ready code. Include Konva documentation links for complex operations.
```

---

## 🔴 PROMPT 8: PHASE 10 - Timeline Editor (SUB-PHASE 10C: Trim & Drag-Drop Interactions)

⚠️ **RUN THIS AFTER PROMPT 7 (10B)**

```
You are a senior React/TypeScript developer specializing in complex drag-drop interactions.

TASK: Implement Phase 10C - Timeline Trim + Drag-Drop + Collision Detection.

CONTEXT:
- Build on: TimelineStore (10A) + Konva canvas (10B)
- Challenge: Clip dragging + trimming + collision prevention
- Library: @dnd-kit/core (already in package.json)
- Constraints:
  * Clips can't move to other tracks (locked to track)
  * Clips can't trim beyond source duration or overlap
  * Clips snap to 100ms grid (for alignment)

REQUIREMENTS:

1. Create DragContextProvider.tsx (New)
   ```typescript
   interface DragState {
     isDragging: boolean
     draggedClipId: string | null
     draggedTrackId: string | null
     dragStartX: number
     dragStartMs: number
     currentX: number
   }

   export const useDragContext = createContext<DragState & actions>()
   ```

2. Create ClipDragHandler.ts (Utility)
   ```typescript
   export const clipDragHandler = {
     // Calculate new start time based on mouse movement
     calculateNewStartMs(dragStartMs: number, pixelDelta: number, zoom: number): number
     
     // Constrain clip within track bounds
     constrainClipPosition(clip: TimelineClip, timeline: Timeline, trackId: string): TimelineClip
     
     // Check for overlaps with other clips
     detectClipOverlap(clip: TimelineClip, otherClips: TimelineClip[], buffer?: number): boolean
     
     // Snap clip to nearest 100ms grid
     snapToGrid(ms: number, gridMs: number = 100): number
     
     // Calculate free space for moving clip
     getAvailableSpace(track: TimelineTrack, excludeClipId: string): { startMs: number; endMs: number }[]
   }
   ```

3. Create ClipTrimHandler.ts (Utility)
   ```typescript
   export const clipTrimHandler = {
     // Calculate trim when dragging left edge
     calculateTrimStart(dragStartMs: number, pixelDelta: number, zoom: number, clip: TimelineClip): number
     
     // Calculate trim when dragging right edge
     calculateTrimEnd(dragStartMs: number, pixelDelta: number, zoom: number, clip: TimelineClip): number
     
     // Validate trim doesn't exceed source duration
     validateTrimRange(clip: TimelineClip, newTrimStart: number, newTrimEnd: number): boolean
     
     // Minimum clip length (500ms so clip remains visible)
     enforceMinimumClipLength(clip: TimelineClip, newTrimStart: number, newTrimEnd: number): boolean
   }
   ```

4. Enhance ClipShape.tsx with Drag-Drop ⭐
   ```typescript
   interface ClipShapeProps {
     clip: TimelineClip
     isSelected: boolean
     onSelect: () => void
     onMove: (newStartMs: number) => void
     onTrim: (side: "start" | "end", newMs: number) => void
   }

   // Add these handlers:
   const handleMouseDown = (e: Konva.KonvaEventObject<MouseEvent>) => {
     const rect = e.target.getStage()?.getPointerPosition()
     if (!rect) return

     // Determine if trimming or dragging
     const trimZoneWidth = 10 // pixels
     const clipBounds = e.target.getAbsolutePosition()
     const isLeftTrim = Math.abs(rect.x - clipBounds.x) < trimZoneWidth
     const isRightTrim = Math.abs(rect.x - (clipBounds.x + e.target.width())) < trimZoneWidth

     if (isLeftTrim) startTrim("start")
     else if (isRightTrim) startTrim("end")
     else startDrag()
   }

   const startDrag = () => {
     // Drag logic with collision detection
     const dragStartMs = clip.startMs
     const originalX = e.target.x()

     e.target.on("dragmove", () => {
       const deltaX = e.target.x() - originalX
       const newStartMs = clipDragHandler.calculateNewStartMs(dragStartMs, deltaX, zoom)
       const constrained = clipDragHandler.constrainClipPosition(...)
       const snapped = clipDragHandler.snapToGrid(constrained.startMs)
       
       // Check overlap
       if (!clipDragHandler.detectClipOverlap(newClip, otherClips)) {
         onMove(snapped)
       } else {
         // Revert position (red flash feedback)
         e.target.x(originalX)
       }
     })
   }

   const startTrim = (side: "start" | "end") => {
     // Trim logic
     const dragStartMs = side === "start" ? clip.trimStartMs : clip.trimEndMs
     const originalX = e.target.x()

     e.target.on("dragmove", () => {
       const deltaX = e.target.x() - originalX
       const newMs = side === "start"
         ? clipTrimHandler.calculateTrimStart(dragStartMs, deltaX, zoom, clip)
         : clipTrimHandler.calculateTrimEnd(dragStartMs, deltaX, zoom, clip)
       
       if (clipTrimHandler.validateTrimRange(clip, newMs, other)) {
         onTrim(side, newMs)
       }
     })
   }
   ```

5. Create Collision Detection Overlay.tsx ⭐
   - Visual feedback when clip would overlap
   - Show semi-transparent red rectangle where collision would occur
   - Disappear when collision is resolved or drag is released

6. Update Timeline Store (from 10A):
   ```typescript
   // Add these actions to store:
   moveClip: (clipId: string, trackId: string, newStartMs: number) => {
     // Validate no overlaps
     // Update clip startMs + endMs (preserve duration)
     // Add to history
   }

   trimClip: (clipId: string, trackId: string, side: "start"|"end", newMs: number) => {
     // Validate trim range
     // Update trimStartMs or trimEndMs
     // Add to history
   }
   ```

DELIVERABLES:
1. src/lib/timeline/clip-drag-handler.ts
2. src/lib/timeline/clip-trim-handler.ts
3. src/components/timeline/clip-shape-enhanced.tsx (updated)
4. src/components/timeline/collision-overlay.tsx
5. src/hooks/use-drag-context.ts
6. README: Drag-drop + trim interaction flow

VALIDATION (Lead Dev will check):
✅ Clip can be dragged horizontally only
✅ Dragging snaps to 100ms grid
✅ Red overlay appears when overlap detected
✅ Trim handles work on both edges
✅ Trim respects minimum length (500ms)
✅ Trim respects source duration
✅ Can't drag clip to wrong track
✅ Undo/redo works with drag operations

OUTPUT:
Provide complete, production-ready code. Include detailed comments on collision detection algorithm.
```

---

## 🔴 PROMPT 9: PHASE 10 - Timeline Editor (SUB-PHASE 10D: Music Panel + Text Overlay)

⚠️ **RUN THIS AFTER PROMPT 8 (10C)**

```
You are a senior React/TypeScript developer specializing in audio track management.

TASK: Implement Phase 10D - Music Panel + Text Overlay System.

CONTEXT:
- Build on: Timeline data model (10A) + Canvas (10B) + Interactions (10C)
- Two subsystems: (1) Music track management, (2) Text overlay editing
- Challenge: Real-time audio preview sync
- State: Zustand timeline store

REQUIREMENTS - MUSIC PANEL:

1. Create MusicLibrary.tsx Component
   - Display: Sidebar showing stock music tracks (10 curated)
   - Each track: Title, duration (MM:SS), genre tag, preview button
   - Preview: Click "Play" → play 10-second sample (HTML5 audio)
   - Action: "Add to Timeline" button
   - Responsive: Full width on mobile, 300px sidebar on desktop

2. Create Stock Music Data: lib/mock-data/stock-music.ts
   - 10 sample tracks
   - Model: { id, title, duration, genre, previewUrl, fullUrl }
   - Genres: "Ambient", "Epic", "Uplifting", "Suspense", "Comedy", etc.

3. Create MusicTrackRow.tsx Component
   - Display: Row in timeline for music track
   - Columns: [TrackName] [Volume Slider] [Auto-Duck Toggle] [Delete]
   - Volume: 0-100% slider (visualize as decibel scale -∞ to 0dB)
   - Auto-Duck: Toggle to automatically reduce music volume when dialogue plays
   - Delete: Remove music from timeline

4. Create VolumeControl.tsx Component
   - Props: { volume: 0-100, onVolumeChange: (vol) => void }
   - Visual: Horizontal slider with icon (speaker symbol)
   - Display: "75%" label next to slider
   - Real-time: Updates waveform visualization opacity

5. Create AutoDuckLogic.ts (Utility)
   - When audio clip starts: Lower music volume by 50%
   - When audio clip ends: Return music to original volume
   - Smooth transition: 300ms fade in/out
   - Implementation: Zustand store + useEffect hook

REQUIREMENTS - TEXT OVERLAY PANEL:

6. Create TextOverlayPanel.tsx Component (NEW)
   - Display: Panel below timeline canvas
   - Features:
     * Add text button: "Add Title" → opens dialog
     * List of current text overlays (if any)
     * Each overlay: start time, duration, text preview, edit/delete buttons

7. Create TextOverlayForm.tsx Component
   - Modal form with fields:
     * Text input: "Enter text..." (multiline textarea)
     * Font size: Dropdown (12px, 16px, 20px, 24px, 32px)
     * Color picker: RGB + hex color input
     * Animation: Select dropdown ("None", "Fade In", "Slide Up")
     * Position: Grid selector (9 positions: top-left to bottom-right)
     * Start time: Input (MM:SS format, converted to ms)
     * Duration: Input (SS format, seconds)
   - Buttons: [Cancel] [Add to Timeline]

8. Create TextOverlayPreview.tsx Component (NEW)
   - Display: Small preview box showing text with selected styling
   - Context: Shows how text would appear on video
   - Position: Adjustable by dragging on preview

9. Create TextEdit utilities: lib/timeline/text-overlay-utils.ts
   - formatTextForDisplay(text: string, fontSize: number, position: string): CSSProperties
   - validateTextOverlay(overlay: TextOverlay): ValidationError[]
   - calculateTextDimensions(text: string, fontSize: number): { width, height }

INTEGRATION:

10. Update TimelineStore (10A) with text overlay actions:
    - addTextOverlay(text: TextOverlay)
    - updateTextOverlay(overlayId: string, updates: Partial<TextOverlay>)
    - deleteTextOverlay(overlayId: string)
    - All actions add to history for undo/redo

11. Add text rendering to Konva Canvas (10B):
    - Konva.Text component per text overlay
    - Position based on overlay settings
    - Update on playhead position (show/hide text based on timing)

12. Update Timeline types (10A):
    ```typescript
    export interface TextOverlay {
      id: string
      episodeId: string
      text: string
      fontSizePixels: number
      color: string // hex #RRGGBB
      positionX: number // 0-100 (percent)
      positionY: number // 0-100 (percent)
      startMs: number
      durationMs: number
      animation: "none" | "fade-in" | "slide-up"
      zIndex: number
    }
    ```

DELIVERABLES:
1. src/components/timeline/music-panel.tsx
2. src/components/timeline/music-library.tsx
3. src/components/timeline/music-track-row.tsx
4. src/components/timeline/volume-control.tsx
5. src/components/timeline/text-overlay-panel.tsx
6. src/components/timeline/text-overlay-form.tsx
7. src/components/timeline/text-overlay-preview.tsx
8. src/lib/timeline/text-overlay-utils.ts
9. src/lib/timeline/auto-duck-logic.ts
10. src/lib/mock-data/stock-music.ts

VALIDATION (Lead Dev will check):
✅ Music tracks display in timeline
✅ Volume slider works (0-100%)
✅ Auto-duck reduces music volume when dialogue plays
✅ Can add text overlays to timeline
✅ Text preview shows formatting
✅ Text animations work
✅ Text position adjustable
✅ Undo/redo includes text changes

OUTPUT:
Provide complete, production-ready code. Include audio mixing logic as comments.
```

---

## 🔴 PROMPT 10: PHASE 10 - Timeline Editor (SUB-PHASE 10E: Preview Rendering + Undo/Redo)

⚠️ **RUN THIS AFTER PROMPT 9 (10D)**

```
You are a senior React/TypeScript developer specializing in history management.

TASK: Implement Phase 10E - Timeline Preview Rendering + Undo/Redo System.

CONTEXT:
- Build on: All sub-phases 10A-D
- Challenge: Render timeline to video preview (mock)
- Undo/Redo: Full history with 50+ operation states
- Performance: Memoize preview calculations

REQUIREMENTS - PREVIEW RENDERING:

1. Create TimelinePreviewRender.tsx Component
   - Display: "Generate Preview" button
   - Progress: Show rendering progress (0-100%)
   - Stages:
     * "Compositing video frames..." (0-40%)
     * "Mixing audio tracks..." (40-70%)
     * "Exporting video..." (70-100%)
   - Output: Playable video player (HTML5)
   - Mock rendering: 5-second delay (simulates backend)

2. Create PreviewRenderService.ts (Mock API)
   ```typescript
   export const previewRenderService = {
     startRender(timeline: Timeline): Promise<{
       videoUrl: string
       durationSeconds: number
       progress: Observable<number>
     }>,
     
     // Mock: Progress emits 0, 25, 50, 75, 100 at 1-second intervals
     // After 5 seconds, returns video URL
   }
   ```

3. Create PreviewPlayer.tsx Component
   - Display: HTML5 video player with controls
   - Sync: Playhead advances with video playback (two-way)
   - If video plays → timeline playhead advances
   - If timeline playhead dragged → video seeks to position
   - Controls: Play/pause, seek bar, volume, fullscreen
   - Duration display: MM:SS format

4. Update Timeline Page to include Preview:
   - Add "Generate Preview" section above canvas
   - Show progress during render
   - Display player after completion

REQUIREMENTS - UNDO/REDO:

5. Create HistoryManager.ts (Advanced utility)
   ```typescript
   export class HistoryManager {
     private stack: Timeline[] = []
     private currentIndex: number = -1
     private maxHistorySize: number = 50

     push(state: Timeline): void {
       // Add to history, discard future states if undone
       this.stack = this.stack.slice(0, this.currentIndex + 1)
       this.stack.push(state)
       if (this.stack.length > this.maxHistorySize) {
         this.stack.shift()
       }
       this.currentIndex = this.stack.length - 1
     }

     undo(): Timeline | null {
       if (this.currentIndex > 0) {
         this.currentIndex--
         return this.stack[this.currentIndex]
       }
       return null
     }

     redo(): Timeline | null {
       if (this.currentIndex < this.stack.length - 1) {
         this.currentIndex++
         return this.stack[this.currentIndex]
       }
       return null
     }

     canUndo(): boolean
     canRedo(): boolean
     getHistorySize(): { current: number; max: number }
   }
   ```

6. Update TimelineStore (10A) with HistoryManager:
   ```typescript
   export const useTimelineStore = create((set, get) => ({
     // Replace old history array with HistoryManager
     historyManager: new HistoryManager(),

     // Update all mutating actions:
     moveClip: (clipId, trackId, newStartMs) => {
       set(state => {
         const updated = mutateTimeline(state.timeline, ...)
         state.historyManager.push(updated)
         return { timeline: updated }
       })
     },

     undo: () => {
       set(state => {
         const previous = state.historyManager.undo()
         return { timeline: previous || state.timeline }
       })
     },

     redo: () => {
       set(state => {
         const next = state.historyManager.redo()
         return { timeline: next || state.timeline }
       })
     }
   }))
   ```

7. Create HistoryPanel.tsx Component (Dev tool)
   - Display: Side panel showing history stack (for debugging)
   - Show: "Drag to position #5", "Trim clip #2", etc.
   - Highlight: Current position in history (bold)
   - Click history item: Jump to that state
   - Only show if isDev=true

8. Create UndoRedoToolbar.tsx Component
   - Display: Top toolbar with undo/redo buttons
   - Icons: Undo arrow ↶, Redo arrow ↷
   - Disabled state: Gray out if canUndo/canRedo are false
   - Keyboard: Ctrl+Z (undo), Ctrl+Shift+Z (redo)
   - Feedback: Show last action as tooltip "Undo: Moved clip 3"

9. Implement Keyboard Shortcuts Hook (enhance 10B):
   ```typescript
   export const useTimelineKeybinds = () => {
     const store = useTimelineStore()

     useEffect(() => {
       const handleKeyDown = (e: KeyboardEvent) => {
         if (e.ctrlKey && e.key === "z" && !e.shiftKey) {
           store.undo()
         }
         if (e.ctrlKey && e.shiftKey && e.key === "z") {
           store.redo()
         }
         if (e.key === "Delete" && store.selectedClip) {
           store.deleteClip(store.selectedClip.id)
         }
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

10. Create ActionHistory interface for logging:
    ```typescript
    export interface TimelineAction {
      type: "MOVE_CLIP" | "TRIM_CLIP" | "ADD_TEXT" | "CHANGE_VOLUME"
      payload: any
      timestamp: Date
      description: string // "Moved clip 3 from 0:00 to 0:05"
    }
    ```

11. Update Timeline Page Layout:
    ```
    ┌─ Toolbar ────────────────────────────────────┐
    │ [↶ Undo] [↷ Redo] [Generate Preview]         │
    └──────────────────────────────────────────────┘
    ┌─ Canvas Area ────────────────────────────────┐
    │ (Konva timeline with ruler, playhead, clips) │
    │ [Music Panel] [Text Overlay Panel]           │
    └──────────────────────────────────────────────┘
    ```

DELIVERABLES:
1. src/lib/timeline/history-manager.ts
2. src/components/timeline/timeline-preview-render.tsx
3. src/components/timeline/preview-player.tsx
4. src/components/timeline/undo-redo-toolbar.tsx
5. src/components/timeline/history-panel.tsx (dev tool, optional)
6. src/lib/timeline/preview-render-service.ts (mock)
7. src/hooks/use-timeline-keybinds-enhanced.ts (updated)
8. README: Undo/redo architecture + history depth explanation

VALIDATION (Lead Dev will check):
✅ Undo button works (reverts last action)
✅ Redo button works (restores undone action)
✅ Can undo 50 actions deep
✅ Future states discarded after new edit
✅ Generate Preview button shows progress
✅ Preview player syncs with timeline playhead
✅ Keyboard shortcuts work (Ctrl+Z, Ctrl+Shift+Z)
✅ Undo/redo buttons disabled appropriately

OUTPUT:
Provide complete, production-ready code. Include history state management diagram as comments.
```

---

## 🟡 PROMPT 11: PHASE 11 - Review Links & Sharing (Part A)

⚠️ **RUN AFTER PHASE 10 COMPLETE**

```
You are a senior React/TypeScript developer. I am the Lead Developer reviewing your work.

TASK: Implement Phase 11 - Review Links & Sharing UI.

CONTEXT:
- Framework: Next.js 14 + React 18 + TypeScript
- Public pages: /review/[token] routes (NO authentication required)
- Challenge: Token-based access + password protection
- State: Zustand for review state

REQUIREMENTS:

1. Create Hook: src/hooks/use-review-links-mock.ts
   - Export: useReviewLinks() hook
   - Return: { links, createLink, revokeLink, generateUrl }
   - generateUrl(token): Return shareable URL
   - NO real API calls

2. Create Types: Update src/types/index.ts
   - ReviewLink: { id, episodeId, token, expiresAt, isRevoked, password, createdByUserId, createdAt }
   - ReviewComment: { id, reviewLinkId, authorName, text, timestampSeconds, createdAt, isResolved }

3. Create Components:

   a) ReviewLinkGenerator.tsx (NEW)
      - UI: Form in card
      - Fields:
        * Expiry date picker (7 days, 30 days, 90 days, custom)
        * Optional password input
        * Generate button
      - Output: Shows generated URL + copy button + QR code
      - Feedback: "Copied!" toast on copy

   b) ReviewLinkCard.tsx (NEW)
      - Display: Card showing one review link
      - Content:
        * Link URL (truncated or with copy button)
        * Created date
        * Comment count
        * Status badge (Active, Expired, Revoked)
        * Revoke button
      - Actions: Copy URL, Generate QR, Revoke

   c) ActiveLinksTable.tsx (NEW)
      - Display: Table of all active review links
      - Columns: URL | Created Date | Comments | Status | Actions
      - Rows: One per active link
      - Actions: Copy, QR code, Revoke

4. Create Public Review Page: src/app/review/[token]/page.tsx
   - No auth required
   - Show: Password input (if link has password)
   - After password validation:
     * Full video player
     * Comment panel (right side)
   - Design: Full-screen video + comments sidebar

5. Create CommentPanel.tsx Component (Public)
   - Display: Sortable list of comments
   - Each comment: Author name, timestamp, text, resolve button
   - Add comment form: Name input + text + submit
   - Timestamp: Show as video marker on progress bar

6. Create Share Page: src/app/(dashboard)/studio/[id]/share/page.tsx
   - Section 1: Review link generator
   - Section 2: Active links list
   - Section 3: YouTube publish section (if Studio tier)
   - Section 4: Brand kit editor

...CONTINUE WITH MORE COMPONENTS...

OUTPUT:
Provide complete, production-ready code.
```

---

## 🟡 PROMPT 12: PHASE 12 - Analytics & Admin (Part A)

⚠️ **RUN AFTER PHASE 11 COMPLETE**

```
You are a senior React/TypeScript developer. I am the Lead Developer reviewing your work.

TASK: Implement Phase 12 - Analytics & Admin Dashboard.

CONTEXT:
- Two dashboards: Creator (personal) + Admin (team-wide)
- Charts: recharts library (already in package.json)
- Metrics: DAU, MAU, costs, error rates
- Testing: Mock data only

REQUIREMENTS:

...CONTINUE WITH PHASE 12...

OUTPUT:
Provide complete, production-ready code.
```

---

## PART 5: EXECUTION TIMELINE WITH CHECKPOINTS

| Phase | Duration | Status | Lead Dev Checkpoint | Notes |
|-------|----------|--------|----------------------|-------|
| **PRE** | 2 days | ✅ START HERE | Review mock data structure | Run Prompt 1 |
| **6** | 3 days | 📋 WAITING | Validate all components render | Run Prompts 2 |
| **7** | 3 days | 📋 WAITING | Validate voice selector works | Run Prompt 3 |
| **8** | 2 days | 📋 WAITING | Validate cost calculation | Run Prompt 4 |
| **9** | 2 days | 📋 WAITING | Validate render progress | Run Prompt 5 |
| **10A-E** | 6 days | 📋 WAITING (LONGEST PHASE) | Complex - break into 5 sub-phases | Run Prompts 6-10 |
| **11** | 4 days | 📋 WAITING | Public review page works | Run Prompt 11 |
| **12** | 3 days | 📋 WAITING | Admin dashboard renders | Run Prompt 12 |
| **TOTAL** | 25 days | | | ~3.5 weeks with sequential execution |

---

## PART 6: PHASE 10 SPECIAL HANDLING (Most Complex)

### Why Phase 10 Takes 6 Days (vs 2-3 for others):

**Sub-Phase Breakdown:**
- **10A** (1 day): Data model + Zustand store + mock data
  - Output: TypeScript types + hook
  - Complexity: Designing timeline data structure correctly
- **10B** (1.5 days): Konva.js rendering + playhead + ruler
  - Output: Canvas rendering layer
  - Complexity: Konva.js API + performance optimization
- **10C** (1.5 days): Trim + drag-drop + collision detection
  - Output: Interaction handlers
  - Complexity: Snap-to-grid + overlap prevention
- **10D** (1 day): Music panel + text overlay system
  - Output: Audio + text UI + auto-duck logic
  - Complexity: Audio mixing logic
- **10E** (1 day): Preview rendering + undo/redo
  - Output: History manager + preview player
  - Complexity: State history with 50+ snapshots

### Why You Can't Run Sub-Phases Out of Order:

```
10A (Types) → 10B (Canvas) → 10C (Interactions) → 10D (Music/Text) → 10E (Redo)
   ↓           ↓              ↓                      ↓                 ↓
   |─ 10B depends on 10A types
   |─ 10C depends on 10B Konva components
   |─ 10D depends on 10A-C being working (store + canvas + interactions)
   |─ 10E depends on all previous (update store with better history)
```

---

## PART 7: STRATEGY FOR MOCK VIDEO/AUDIO

### How to Prepare Your Sample Files:

```bash
# Create mock assets folder
mkdir -p public/mock-assets

# Add your sample files:
# 1. sample-video.mp4 (15-30 seconds, H.264, 1920x1080)
# 2. sample-audio.mp3 (30 seconds, mono or stereo)
# 3. sample-captions.srt (SRT subtitle file)
```

### In mock-timeline.ts:

```typescript
const mockTimeline: Timeline = {
  id: 'mock-timeline-1',
  episodeId: 'ep-123',
  durationMs: 30000, // 30 seconds (duration of sample-audio.mp3)
  fps: 24,
  tracks: [
    {
      id: 'video-track-1',
      trackType: 'video',
      clips: [
        {
          id: 'clip-1',
          startMs: 0,
          endMs: 5000,
          mediaUrl: '/mock-assets/sample-video.mp4',
          // ... other properties
        },
        // ... more clips, each 5 seconds long
      ]
    },
    {
      id: 'audio-track-1',
      trackType: 'audio',
      clips: [
        {
          id: 'audio-1',
          startMs: 0,
          endMs: 30000, // Full 30-second audio
          mediaUrl: '/mock-assets/sample-audio.mp3',
        }
      ]
    }
  ]
}
```

---

## END OF EXECUTION PLAN

**Next step**: Copy PROMPT 1 (PRE-PHASE) and paste into Claude Chat. Attach the 4 files listed under "PRE-PHASE: Mock Data Templates". Let Claude Sonnet generate the mock data.

Once mock data is complete, proceed to PROMPT 2 (Phase 6) with Prompts 2-3 running in parallel.
