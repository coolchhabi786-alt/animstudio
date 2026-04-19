# AnimStudio V2.0 - Figma Design System & Component Specs

**Purpose**: Complete design specification for Figma handoff to design team. Includes all new components, design tokens, and responsive breakpoints.

---

## Design System Foundation

### Color Palette

**Light Theme (Current)**:
```
Primary Colors:
- Blue-50: #f0f9ff
- Blue-100: #e0f2fe
- Blue-200: #bae6fd
- Blue-500: #0ea5e9
- Blue-600: #0284c7 (Primary CTA)
- Blue-700: #0369a1
- Blue-900: #082f49

Gray Neutrals:
- Gray-50: #f9fafb (Hover states)
- Gray-100: #f3f4f6 (Backgrounds)
- Gray-200: #e5e7eb (Borders)
- Gray-400: #9ca3af (Secondary text)
- Gray-600: #4b5563 (Primary text)
- Gray-900: #111827 (Dark backgrounds)

Status Colors:
- Green-600: #16a34a (Success)
- Red-600: #dc2626 (Error/Delete)
- Yellow-600: #ca8a04 (Warning)
- Purple-600: #9333ea (Info/Secondary)
```

**Dark Theme (Timeline Editor)**:
```
- Surface-900: #111827 (Canvas background)
- Surface-800: #1f2937 (Track background)
- Surface-700: #374151 (Hover state)
- Accent-500: #3b82f6 (Clip highlight)
- Accent-Gold: #fbbf24 (Trim handles)

// For future dark mode support (Phase 13)
```

### Typography

**Font Family**: Inter (sans-serif)

**Scale**:
```
Display (48px, 700): Page titles
Heading 1 (32px, 600): Section headers
Heading 2 (24px, 600): Subsection headers
Title (20px, 600): Dialog/card titles
Subtitle (16px, 500): Secondary headers
Body Regular (14px, 400): Main text
Body Small (12px, 400): Secondary text
Label (12px, 500): Form labels, badges
Caption (11px, 400): Timestamps, hints
Mono (12px, 400): Code, tokens

Line Heights:
- Compact: 1.2 (headings)
- Normal: 1.5 (body)
- Relaxed: 1.6 (large text blocks)
```

### Spacing System

**8px grid baseline**:
```
- 0.5x: 4px (tight spacing)
- 1x: 8px
- 1.5x: 12px
- 2x: 16px
- 2.5x: 20px
- 3x: 24px
- 4x: 32px
- 5x: 40px
- 6x: 48px
- 8x: 64px
```

**Usage**:
- Padding: 16-24px for containers
- Gap between elements: 8-16px
- Margin bottom sections: 32-48px
- Component internal spacing: 8-12px

### Shadows

```
sm:    0 1px 2px 0 rgba(0,0,0,0.05)
base:  0 1px 3px 0 rgba(0,0,0,0.1), 0 1px 2px 0 rgba(0,0,0,0.06)
md:    0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06)
lg:    0 10px 15px -3px rgba(0,0,0,0.1), 0 4px 6px -2px rgba(0,0,0,0.05)
xl:    0 20px 25px -5px rgba(0,0,0,0.1), 0 10px 10px -5px rgba(0,0,0,0.04)
```

### Border Radius

```
sm:  2px (primary, inputs)
base: 4px (buttons, cards)
md:  6px (modals)
lg:  8px (large cards)
full: 9999px (pills, avatars)
```

---

## Responsive Breakpoints

```
xs:  320px   (Mobile small)
sm:  640px   (Mobile large)
md:  768px   (Tablet)
lg:  1024px  (Desktop)
xl:  1280px  (Desktop large)
2xl: 1536px  (Desktop XL)

// Usage in components:
- Mobile-first (design mobile, then enhance)
- xs/sm: Stacked layouts, full-width inputs
- md: 2-column layouts start
- lg: 3+ column layouts, side panels
- xl+: Wide layouts with navigation sidebars
```

---

## Component Library (26 New Components)

### PHASE 6 - STORYBOARD COMPONENTS (4)

#### 1. Scene Tab Component
**Location**: Frame → Scene Tab
**Purpose**: Navigate between scenes in an episode

**States**:
- Default (idle)
- Hover
- Active (selected)
- Disabled (no scenes available)

**Dimensions**:
- Height: 40px
- Min width: 120px
- Padding: 12px 16px

**Content**:
- Scene number (text, bold)
- Line of shot count (badge, gray-400)
- Thumbnail (80x60px, rounded-sm)

**Interactions**:
- Click to select scene
- Hover shows tooltip: "Scene 3 • 5 shots"

---

#### 2. Shot Grid Component
**Location**: Frame → Shot Grid
**Purpose**: Display all shots in a scene as grid

**Layout**:
- Responsive: 4 columns (lg) → 3 (md) → 2 (sm)
- Gap: 12px
- Padding: 16px
- Max height: 500px (scrollable)

**Cell States**:
- Default (unselected)
- Hover (shadow-md)
- Active (border highlight)
- Loading (skeleton)

---

#### 3. Shot Card Component
**Location**: Frame → Shot Card
**Purpose**: Individual shot preview with actions

**Layout**:
- Dimensions: 240x180px (landscape 4:3)
- Card shadow: md
- Rounded: base

**Content**:
- Thumbnail image (full container)
- Overlay on hover:
  - Scene number (top-left, badge)
  - Duration (top-right, caption)
  - 3-dot menu (top-right corner)
  - Zoom icon + Style button (bottom, flex-center)

**States**:
- Default (showing thumbnail)
- Hover (overlay appears with buttons)
- Selected (white border, 2px)
- Loading (skeleton animation)
- Error (broken image icon)

---

#### 4. Style Override Dialog
**Location**: Modal → Style Dialog
**Purpose**: Override character/prop styles for a single shot

**Layout**:
- Width: 480px (md breakpoint)
- Sections: Header, Tabs (Character | Props), Content, Footer
- Header: "Override Style for Shot 3" + close button
- Tabs: Character list vs Prop list

**States**:

**Character Tab**:
- List of characters in shot
- Each item has:
  - Avatar (32x32)
  - Character name
  - Current style (badge, gray)
  - Change button (→ modal within modal)

**Props Tab**:
- Searchable list
- Each item:
  - Icon/thumbnail
  - Prop name
  - Current style
  - Change button

**Footer**:
- Cancel + Apply buttons (right-aligned)

**States**:
- Default (no changes)
- Hover on item (bg-gray-50)
- Item selected (blue highlight)
- Unsaved changes (dialog shows dirty state)

---

#### 5. Regenerate Dialog
**Location**: Modal → Regenerate Confirmation
**Purpose**: Confirm regenerating a shot with new style

**Layout**:
- Width: 400px
- Centered modal with overlay

**Content**:
- Icon (AlertCircle, yellow)
- Title: "Regenerate Shot?"
- Description: "This will update shot X with the new style. This takes ~30-60 seconds."
- Progress bar (empty, shows when confirming)
- Two buttons:
  - Cancel (gray)
  - Regenerate (blue)

**States**:
- Idle (buttons enabled)
- Loading (progress bar animating, buttons disabled)
- Success (card turns green, closes after 2s)
- Error (red alert appears)

---

### PHASE 7 - VOICE COMPONENTS (5)

#### 6. Voice Roster Table
**Location**: Frame → Voice Roster
**Purpose**: Assign voice talents to characters

**Layout**:
- Full width
- Columns: Character | Voice Talent | Language | Duration | Action
- Min height: 300px (scrollable if 10+ characters)

**Row States**:
- Default
- Hover (bg-gray-50)
- Selected (bg-blue-50)
- Loading (skeleton)

**Column Details**:
- Character: Avatar (32x32) + name (bold)
- Voice Talent: Name (gray) or "Unassigned" (italic)
- Language: Badge (e.g., "English")
- Duration: "2m 34s" (caption)
- Action: Dropdown (Assign | Clear | Preview)

**Empty State**:
- Icon + "No voice assignments yet"
- "Click Assign to get started" button

---

#### 7. Voice Talent Picker
**Location**: Dropdown → Voice Picker
**Purpose**: Select voice talent from library or create

**Layout**:
- Width: 300px (dropdown)
- Max height: 400px (scrollable)
- Header: Search bar + "Create New" button (top)

**List Items**:
- Thumbnail (24x24, avatar)
- Name (bold)
- Language (gray)
- Character count (caption, "5 uses")
- Click to select

**States**:
- Default (list shown)
- Hover on item (bg-gray-50)
- Selected (blue indicator)
- Searching (filtered list)
- Empty search (no results message)

---

#### 8. Language Selector
**Location**: Dropdown → Language Select
**Purpose**: Choose language for voice synthesis

**Layout**:
- Width: 200px
- Trigger: Current language label + chevron

**Dropdown Options**:
- English (US) - default check
- English (UK)
- Spanish (Spain)
- Spanish (Latin America)
- French
- German
- Japanese
- Mandarin
- + 5 more (grouped by region)

**States**:
- Closed (shows selected language)
- Open (list expanded)
- Hover on option (bg-gray-50)
- Selected (check icon + blue text)

---

#### 9. Audio Preview Player
**Location**: Component → Audio Player
**Purpose**: Play/preview audio clips

**Layout**:
- Height: 48px
- Width: varies (min 200px)
- Horizontal layout

**Elements**:
- Play/pause button (left, 32x32)
- Progress bar (center, flex-1)
  - Shows current time / total time
  - Draggable scrubber
- Volume control (right, 24x24)
- Duration display (right, caption)

**States**:
- Stopped (play icon, progress at 0)
- Playing (pause icon, animated)
- Hovered (progress bar highlight)
- Scrubbing (playback paused, cursor shows)
- Muted (volume icon crossed out)

---

#### 10. Voice Clone Upload
**Location**: Modal → Upload Voice
**Purpose**: Upload voice samples for cloning

**Layout**:
- Width: 480px
- Sections: Dropzone, List of uploads, Footer

**Dropzone**:
- Dashed border (2px, gray-300)
- Icon: Upload arrow
- Text: "Drag audio files or click to upload"
- Support: MP3, WAV (max 10MB per file)
- Multiple files allowed

**Upload List**:
- Each file shows:
  - Filename (left)
  - Duration (center, gray)
  - Status (right: uploading/done/error)
  - Progress bar (if uploading)

**Footer**:
- Cancel + Create Voice Clone buttons (right)

---

### PHASE 8 - ANIMATION COMPONENTS (5)

#### 11. Render Estimate Card
**Location**: Component → Estimate Card
**Purpose**: Show render time/cost estimate before queuing

**Layout**:
- Width: 360px
- Padding: 20px
- Rounded: base

**Content**:
- Title: "Render Estimate"
- Rows:
  - Duration: "3 minutes 24 seconds"
  - Cost: "$0.12 USD" (with tooltip about tier pricing)
  - Quality: "4K @ 60fps"
  - Est. completion: "Fri, 2:30 PM"

**States**:
- Default (ready to render)
- Hover (shadow-md)
- Disabled (grayed out, reason shown)

**Button**: "Start Render" (bottom, full-width blue)

---

#### 12. Render Approval Dialog
**Location**: Modal → Approval Flow
**Purpose**: Confirm animation render before queuing

**Layout**:
- Width: 500px
- Sections: Preview Thumbnail, Settings, Review, Footer

**Preview Thumbnail**:
- 400x300px
- Show first frame of animation
- Play button on hover

**Settings Block**:
- Output format (Dropdown: MP4 / WebM / ProRes)
- Resolution (Dropdown: 1080p / 2K / 4K)
- Frame rate (Dropdown: 24fps / 30fps / 60fps)
- Codec (H.264, VP9, etc)

**Review Block**:
- Estimated duration + cost (read-only)
- Checkbox: "Include watermark" (checked by default)
- Checkbox: "Auto-upload to CDN" (unchecked)

**Footer Buttons**:
- Cancel (gray)
- Back (gray, if from previous step)
- Render (blue, action button)

---

#### 13. Render Progress Component
**Location**: Component → Progress Card
**Purpose**: Show real-time render progress

**Layout**:
- Width: varies
- Height: 120px
- Padding: 16px
- Rounded: base

**Content**:
- Title: "Rendering Scene 2 of 8"
- Phase text: "Encoding video..." (caption, gray)
- Progress bar (width: 100%, height: 8px)
- Percentage text: "34%" (right-aligned, bold)
- Details (bottom):
  - Time elapsed: "2m 15s"
  - Est. remaining: "4m 30s"
  - Speed: "1.2x real-time"

**States**:
- Idle (progress 0%)
- Rendering (animated progress bar)
- Paused (progress freezes, "Paused" label)
- Complete (progress 100%, green color)
- Error (progress stops, red background)

**Interactions**:
- Pause/Resume buttons (if idle)
- Cancel button (if rendering)

---

#### 14. Render Preview Player
**Location**: Component → Video Preview
**Purpose**: Preview rendered clips before download

**Layout**:
- Responsive: full-width up to 800px
- Aspect ratio: 16:9
- Border: gray-200
- Rounded: md

**Elements**:
- Video player (full container)
- Play/pause button (center overlay)
- Progress bar (bottom)
- Controls (bottom right):
  - Play/pause
  - Volume
  - Fullscreen
  - Timestamp

**States**:
- Default (paused, first frame visible)
- Playing (play button hidden, progress bar visible)
- Fullscreen (expanded to viewport)
- Hover (controls visible/highlighted)

---

### PHASE 9 - DELIVERY COMPONENTS (5)

#### 15. Aspect Ratio Picker
**Location**: Dropdown → Aspect Ratio
**Purpose**: Select output video aspect ratio

**Layout**:
- Width: 200px dropdown

**Options** (with preview icons):
- 16:9 (Widescreen) - default
- 9:16 (Portrait/Mobile)
- 1:1 (Square/Social)
- 4:3 (Standard)
- 21:9 (Ultrawide)

**Each Option**:
- Small preview rectangle
- Label text
- Common use (caption, gray)

**States**:
- Default (current selection highlighted)
- Hover on option (bg-gray-50)
- Selected (blue highlight + check)

---

#### 16. Output Format Card
**Location**: Component → Format Card
**Purpose**: Show selected export format with details

**Layout**:
- Width: 280px
- Padding: 16px
- Rounded: base
- Border: gray-200

**Content**:
- Format name (bold): "MP4 - H.264"
- Details (2 rows):
  - Codec: VP9
  - Container: MPEG-4
- Specs (3 rows):
  - Resolution: 1920x1080
  - Bitrate: 8 Mbps
  - File size: ~45 MB
- Checkbox: "Optimized for web" (unchecked)

**States**:
- Default (unselected)
- Hover (shadow-base)
- Active (border-blue highlight)
- Disabled (gray out with reason)

---

#### 17. Download Progress Bar
**Location**: Component → Download Progress
**Purpose**: Show file download status

**Layout**:
- Width: varies (min 280px)
- Height: 64px
- Rounded: base
- Padding: 12px

**Content**:
- Left: Download icon + filename (bold)
- Center: Progress bar (100% width)
- Right:
  - Speed (caption): "2.3 MB/s"
  - Time remaining (caption): "2m 15s remaining"
  - Percentage (bold): "67%"
- Bottom: Full path (caption, gray)

**States**:
- Downloading (progress animating)
- Paused (progress freezes, Pause button → Resume)
- Complete (green checkmark, 100%)
- Error (red background, retry button)
- Pending (progress bar empty, icon spinning)

---

#### 18. Render History Table
**Location**: Frame → History
**Purpose**: List of recent renders with status

**Layout**:
- Full width, responsive
- Columns: Date | Name | Status | Format | Duration | Action

**Row Details**:
- Date: "Mar 15, 2024 2:30 PM"
- Name: "Final Scene - V2" (bold, blue if downloadable)
- Status: Badge (Complete/Failed/Rendering/Queued)
- Format: "1080p MP4"
- Duration: "3m 24s"
- Action: Menu (Download | Delete | Details)

**States**:
- Default
- Hover (bg-gray-50)
- Selected row (bg-blue-50)
- Rendering (pulsing animation on row)
- Failed (red-tinted row)

**Empty State**:
- Icon + "No renders yet"
- "Start rendering to see history" button

---

### PHASE 10 - TIMELINE COMPONENTS (7)

#### 19. Timeline Canvas
**Location**: Full-screen component
**Purpose**: Konva.js canvas for timeline editing

**Layout**:
- Flex container (width: full, height: 60vh)
- Dark background (gray-800)
- Border: gray-700 bottom

**Content**:
- Tracks (horizontal rows):
  - Track label (left panel)
  - Clips (draggable rectangles)
  - Trim handles (gold bars on edges)
- Ruler (top, time markers)
- Playhead (red vertical line)
- Selection highlight (blue border on active clip)

**States**:
- Idle (no drag)
- Dragging clip (cursor: grabbing)
- Dragging trim (cursor: col-resize)
- Hovering clip (highlight, shadow)
- Clip selected (white border)

---

#### 20. Timeline Ruler
**Location**: Component → Ruler
**Purpose**: Time markers and playhead scrubber

**Layout**:
- Height: 48px
- Width: 100% (scrollable with timeline)
- Background: gray-800
- Border: bottom gray-700

**Elements**:
- Markers (vertical lines, 5s intervals)
- Time labels (MM:SS format)
- Playhead (2px red line, clickable, draggable)

**States**:
- Default (markers showing)
- Hover (cursor: text, shows tooltip on hover)
- Dragging playhead (red highlight)
- Zoomed (markers update to 1s, 10s, etc based on zoom level)

---

#### 21. Track Control Panel
**Location**: Left sidebar, timeline editor
**Purpose**: Mute/solo/lock controls per track

**Layout**:
- Width: 200px
- Rows (one per track, 60px height)
- Scrollable with canvas

**Row Content**:
- Left: Track label (bold) + clip count (gray)
  - "Video • 5 clips"
  - "Music • 1 clip"
- Right: Icon buttons (mute, solo, lock)
  - 32x32 icon buttons
  - Toggle states (active = highlighted)

**States**:
- Default (icons normal)
- Hover (bg-gray-700)
- Muted (icon red, track dimmed in canvas)
- Solo (icon yellow, other tracks dimmed)
- Locked (icon gray, clips don't drag)
- All three active (layered icons or notification)

---

#### 22. Music Panel Selector
**Location**: Right sidebar, collapsible
**Purpose**: Add music tracks from library

**Layout**:
- Width: 280px
- Max height: 400px (scrollable list)
- Padding: 16px

**Content**:
- Search bar (top)
- Category tabs: Library | Uploads | Favorites
- Track list:
  - Icon (music note)
  - Name (bold)
  - Duration (gray)
  - Play button (hover)
  - Add button (+ icon)

**States**:
- Closed (collapse arrow visible in header)
- Open (list showing)
- Hover on track (bg-gray-700)
- Track playing (has player mini-controls)
- Uploading (progress indicator on custom track)

---

#### 23. Text Overlay Editor
**Location**: Right sidebar, collapsible
**Purpose**: Add/edit text overlays on timeline

**Layout**:
- Width: 280px
- Sections: Text input, Font controls, Position, Animation

**Content**:
- Text area (3 lines)
- Font size slider (8px→80px)
- Color picker (clickable square)
- Animation dropdown (fadeIn, slideUp, etc)
- Position selector (9-point grid, center is default)
- Duration inputs (start/end time)

**States**:
- Default (empty, all fields disabled)
- Editing (fields enabled)
- Preview (shows text preview in black box)
- Font size range (8-80px, slider)

---

#### 24. Transition Picker
**Location**: Dropdown on clip edge
**Purpose**: Select transition type between clips

**Layout**:
- Width: 180px
- Options list

**Transitions**:
- Cut (instant, default)
- Fade (0.5s)
- Dissolve (0.5s)
- Slide Left (0.3s)
- Slide Right (0.3s)
- Zoom (0.3s)

**Each Option**:
- Icon/visual preview
- Name
- Duration (small text)

**States**:
- Default (showing current)
- Hover (highlight)
- Selected (check icon)
- Disabled (grayed if not applicable)

---

#### 25. Timeline Toolbar
**Location**: Top of editor
**Purpose**: Timeline controls and actions

**Layout**:
- Height: 48px
- Flex layout, horizontal
- Dark background (gray-900)
- Border: bottom gray-700

**Button Groups**:
- Playback (Play | Pause)
- History (Undo | Redo)
- Zoom (Zoom In | Zoom Out)
- Save (Save Timeline, blue)

**All buttons**:
- Icon only (32x32)
- Hover state (bg-gray-800)
- Active state (bg-gray-700, white icon)
- Disabled state (grayed out)

**States**:
- Idle (all buttons enabled except Redo if no history)
- Playing (Play button hidden, Pause visible)
- Unsaved (Save button highlighted/animated)

---

### PHASE 11 - SHARING COMPONENTS (5)

#### 26. Review Link Generator
**Location**: Card → Share Dialog
**Purpose**: Create shareable review links

**Layout**:
- Width: 400px
- Card with padding 20px
- Sections: Options, Generate, Output

**Options**:
- Checkbox: "Link expires In" + days input (7 days default)
- Checkbox: "Require password" + password input
- Both optional

**Generate Button**:
- Full width, blue
- Shows "Generate Link"

**Output**:
- Text input (readonly) + Copy button
- Success message on copy: "Copied!" (green check)

**States**:
- Idle (Generate button enabled)
- Generating (button disabled, spinner)
- Generated (link shown, Copy button highlighted)
- Expired link option (checkbox shows expiry date after generation)

---

#### 27. Review Link List
**Location**: Frame → Active Links
**Purpose**: Show all active review links

**Layout**:
- Table format
- Columns: Created | Expires | Password | Views | Action

**Rows**:
- Created: Date/time (bold)
- Expires: "In 5 days" or "Never" (if no expiry)
- Password: Dot indicator (• = has password)
- Views: "12 views"
- Action: Menu (Copy | Share | Delete)

**States**:
- Default
- Hover (bg-gray-50)
- Expired link (grayed out, "Expired" badge)
- No links (empty state: "No review links created yet")

---

#### 28. Public Review Page
**Location**: Full page → /review/[token]
**Purpose**: Public-facing video review interface

**Layout**:
- Flex split:
  - Left: Video player (flex: 1)
  - Right: Comment panel (width: 320px)
- Dark theme (gray-900 background)

**Video Player**:
- 16:9 responsive
- Standard controls (play, volume, fullscreen, progress)

**Comment Panel**:
- Scrollable comment list
- Add comment form (textarea + Send button)
- Timestamps clickable (jump to playhead)

**States**:
- Loading (skeleton in both areas)
- Error (404 if link invalid/expired)
- Password protected (modal overlay with password input)
- Comments loading (skeleton comments)

---

#### 29. YouTube Publish Dialog
**Location**: Modal → Publish Form
**Purpose**: Publish render directly to YouTube

**Layout**:
- Width: 480px
- Sections: Connection status, Form fields, Footer

**Status**:
- Connected: "✓ YouTube connected"
- Not connected: Button "Connect YouTube Account"

**Form Fields**:
- Title (full width input)
- Description (textarea, 3 lines)
- Visibility (dropdown: Private | Unlisted | Public)
- Tags (input, comma-separated)

**Preview**:
- Thumbnail preview (200x112px)
- Format: "1080p MP4"

**Footer**:
- Cancel + Publish buttons
- Publish button disabled until title filled

**States**:
- Idle (form empty, Publish disabled)
- Filled (Publish button enabled)
- Publishing (button shows spinner, fields disabled)
- Success (green checkmark, close after 2s)
- Error (red message, retry)

---

#### 30. Brand Kit Editor
**Location**: Panel → Brand Kit
**Purpose**: Configure brand colors, logo, watermark

**Layout**:
- Padding: 20px
- Sections: Logo, Colors, Watermark, Save button

**Logo Section**:
- Dashed border upload area
- Shows current logo if exists
- Remove button appears on hover

**Color Section**:
- Two color pickers side-by-side:
  - Primary color (square, clickable)
  - Secondary color (square, clickable)
- Hex input fields below each

**Watermark Section**:
- Position dropdown (Bottom Right, Bottom Left, Top Right)
- Opacity slider (0-100%)
- Preview image showing watermark placement

**Footer**:
- Save button (blue, full width)
- Unsaved changes indicator (orange border or badge)

---

### PHASE 12 - ANALYTICS COMPONENTS (5)

#### 31. Analytics Metric Card
**Location**: Frame → Metrics Row
**Purpose**: Display single metric with trend

**Layout**:
- Width: ~200px
- Padding: 16px
- Rounded: base
- Border: gray-200
- Background: gray-50

**Content**:
- Label (gray, small)
- Value (bold, large)
- Trend (badge, showing % change):
  - Green ↑ for positive
  - Red ↓ for negative
  - Gray for neutral

**Example**:
```
Total Views
2,847
↑ 12% from last week
```

**States**:
- Default
- Hover (shadow-md)
- Loading (shimmer animation)
- Error (grayed out with error icon)

---

#### 32. Views Analytics Chart
**Location**: Component → Views Graph
**Purpose**: Show view count over time (24h or 7d)

**Layout**:
- Width: 100% (max 600px)
- Height: 300px
- Rounded: base
- Border: gray-200

**Chart**:
- Area chart (blue fill)
- X-axis: time (hours or days)
- Y-axis: view count
- Tooltip on hover shows exact values
- Toggle buttons: 24h | 7d | 30d (above chart)

**States**:
- Loading (skeleton animation)
- Showing data (area filled)
- No data (empty state: "No views yet")

---

#### 33. Engagement Metrics Box
**Location**: Component → Engagement Card
**Purpose**: Show shares, embeds, watch time

**Layout**:
- 2x2 grid of mini-cards
- Responsive to 1 col on mobile

**Cards**:
- Shares: Number + icon
- Embeds: Number + icon
- Avg watch time: Duration + icon
- Comment count: Number + icon

**Styling**:
- Each card: 160px x 100px
- Gray-50 background
- Gray-200 border
- Rounded: base

**States**:
- Default
- Hover (shadow, slight scale)

---

#### 34. Job Queue Table
**Location**: Admin page
**Purpose**: Show pending/completed render jobs

**Layout**:
- Full width table
- Columns: ID | Episode | Status | Started | Duration | Action

**Rows**:
- ID: Unique ID (monospace)
- Episode: Episode name
- Status: Badge (Queued/Running/Completed/Failed)
- Started: Timestamp
- Duration: Elapsed / Est. total
- Action: Menu (Pause | Cancel | Details)

**Status Colors**:
- Queued: Gray
- Running: Blue (animated)
- Completed: Green
- Failed: Red

**States**:
- Default
- Hover (bg-gray-50)
- Running row (highlights, animates)
- Empty (no jobs)

---

#### 35. Subscription Stats Card
**Location**: Admin dashboard
**Purpose**: Show subscriber breakdown by tier

**Layout**:
- Card, width: ~300px
- Padding: 20px
- Rounded: base

**Content**:
- Title: "Subscriptions by Tier"
- List:
  - Free: 234 users
  - Pro: 156 users
  - Studio: 45 users
- Each row shows count (bold) + percentage (gray)
- Colored dot indicator per tier

**Chart**:
- Below list: Horizontal stacked bar showing ratios
- Colors match tier colors

**States**:
- Default
- Hover on row (highlight)

---

## Responsive Design Implementation

### Mobile (xs/sm: 320-640px)
- **Phase 10 Timeline**: 
  - Tracks stack vertically (not side-by-side)
  - Canvas height reduced (300px)
  - Controls move to bottom (vertical toolbar)
  - Left panel collapses to drawer
  - Right panel (music/text) moves to modal overlay

- **Phase 11 Review Page**:
  - Video takes full width
  - Comment panel becomes modal popup
  - List view for comments (one per line)

- **Phase 12 Analytics**:
  - Cards stack single column
  - Chart responsive (full width)
  - Table becomes card list (one cell per line)

### Tablet (md: 768px)
- **Phase 10**:
  - Tracks show with labels (left panel visible)
  - Canvas at 500px height
  - Right panel shows music/text in tabs

- **Phase 11**:
  - Video left (60% width)
  - Comments right sidebar (40% width)

- **Phase 12**:
  - 2-column card grid (Metric cards)
  - Table shows all columns

### Desktop (lg+: 1024px+)
- Full layouts as designed above
- Multi-column grids
- Side panels visible
- All controls accessible

---

## Component States Matrix

| Component | Default | Hover | Active | Loading | Error | Disabled |
|-----------|---------|-------|--------|---------|-------|----------|
| Button | blue-600 | blue-700 | blue-800 | spinner | red bg | gray-400 |
| Input | gray-200 border | gray-300 border | blue-500 border | - | red-500 border | gray-100 bg |
| Card | gray-50 bg | shadow-md | blue-50 bg | skeleton | red-50 bg | gray-300 opacity |
| Checkbox | unchecked | - | checked | - | red border | opacity-50 |
| Avatar | image | shadow-md | white border | skeleton | gray icon | opacity-50 |
| Badge | gray-100 | - | white bg | - | red-100 | opacity-50 |

---

## Animation Specifications

### Transitions (all use cubic-bezier(0.4, 0, 0.2, 1))
```
- Hover effects: 150ms
- Panel collapse/expand: 300ms
- Modal fade in: 150ms
- Progress bar animate: 2s (infinite while rendering)
- Skeleton shimmer: 1.5s (infinite)
- Loading spinner: 1s (infinite, linear)
```

### Icon Animations
- Spinner: Rotate 360° in 1s (linear)
- Pulsing notification: Scale 0.95→1 in 1.5s (ease-in-out)
- Skeleton shimmer: Left to right in 1.5s

---

## Accessibility Specifications

### Colors
- Contrast ratios: 4.5:1 minimum (AA standard)
- Blue-600 on white: ✓ 5.5:1
- Gray-600 on gray-50: ✓ 7.2:1
- Status indicators: Always include text, not just color

### Keyboard Navigation
- Tab order: left→right, top→bottom
- Buttons/inputs focusable (visible focus ring)
- Dropdowns keyboard accessible (arrow keys)
- Modal: Trap focus inside, ESC to close

### Screen Readers
- Form labels: associated with inputs
- Buttons: descriptive text (not just icons)
- Icons: aria-label or hidden (aria-hidden="true")
- Live regions: Status updates use aria-live="polite"

### Motion
- Provide "Reduce motion" preference option
- Disable animations if prefers-reduced-motion: reduce
- Essential animations only (progress, playback)

---

## Export Specifications for Developers

### File Organization
```
Figma Page Structure:
├─ Design System
│  ├─ Colors (swatches)
│  ├─ Typography (styles)
│  └─ Spacing (documentation)
├─ Components (organized by phase)
│  ├─ Phase 6 - Storyboard
│  ├─ Phase 7 - Voice
│  ├─ Phase 8 - Animation
│  ├─ Phase 9 - Delivery
│  ├─ Phase 10 - Timeline
│  ├─ Phase 11 - Sharing
│  └─ Phase 12 - Analytics
└─ Screens (full page mockups)
```

### Component Notes (in Figma)
- Each component has a description with:
  - Purpose statement
  - Usage guidelines
  - Props (required vs optional)
  - States documented
  - Links to relevant hooks/APIs

### Export Assets
- Color palette: JSON / Tailwind config
- Typography: Font files + CSS variables
- Icons: SVG zip (32px, 24px, 16px sizes)
- Shadows: Tailwind config (sm/base/md/lg/xl)

