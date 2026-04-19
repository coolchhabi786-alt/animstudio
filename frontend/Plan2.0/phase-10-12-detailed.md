# AnimStudio V2.0 - Phase 10-12 Detailed Implementation

## PHASE 10 - TIMELINE EDITOR (CORE)
## (Week 10-15, 6 weeks, 2 devs - Complex)

**Previous Status**: Not started  
**Complexity**: VERY HIGH - Most complex phase  
**Effort**: 40-50 dev-days  
**Tech Stack**: Konva.js v9 + @dnd-kit/core + Zustand  

---

### Overview: What Users Do

1. Enter timeline editor from episode detail page
2. See 4 horizontal tracks: Video, Audio, Music, Text
3. Drag shots from storyboard onto video track
4. Trim clips (drag handles on left/right)
5. Add transitions (Cut/Fade/Dissolve) between clips
6. Select music from library, add to music track
7. Add text overlays (titles, captions)
8. Set in/out times, preview timeline
9. Save timeline state
10. Queue post-production render

---

### Data Models

#### Timeline Domain Model
```typescript
// Timeline structure (state + persistence)
export interface TimelineState {
  id: string
  episodeId: string
  tracks: TimelineTrack[]
  playheadMs: number
  zoom: number
  duration: number
  isPlaying: boolean
  selectedClipId?: string
  history: TimelineState[] // undo/redo
  historyIndex: number
  createdAt: Date
  updatedAt: Date
}

export interface TimelineTrack {
  id: string
  episodeId: string
  trackType: 'video' | 'audio' | 'music' | 'text'
  clips: TimelineClip[]
  volume: number // 0-100
  muted: boolean
  solo: boolean
  locked: boolean
  sortOrder: number
}

export interface TimelineClip {
  id: string
  trackId: string
  type: 'animation' | 'voiceover' | 'music' | 'text'
  sourceId: string // AnimationClip.id | AudioTrack.id | MusicTrack.id | null
  startMs: number // Absolute position on timeline
  endMs: number
  durationMs: number
  trimStartMs: number // Trim points
  trimEndMs: number
  transitionIn?: TimelineTransition
  transitionOut?: TimelineTransition
  volume?: number
  zIndex: number
  metadata?: Record<string, any>
}

export interface TimelineTransition {
  type: 'cut' | 'fade' | 'dissolve' | 'slideLeft' | 'slideRight'
  durationMs: number
  easing?: 'linear' | 'easeIn' | 'easeOut' | 'easeInOut'
}

export interface TextOverlay {
  id: string
  episodeId: string
  text: string
  fontFamily: string
  fontSize: number
  color: string
  backgroundColor?: string
  opacity: number
  position: {
    x: number // % of video width
    y: number // % of video height
  }
  anchor: 'center' | 'top-left' | 'top-center' | 'bottom-center'
  startMs: number
  endMs: number
  animation?: TextAnimation
  rotation?: number
  scale?: number
}

export interface TextAnimation {
  type: 'fadeIn' | 'slideUp' | 'slideDown' | 'slideLeft' | 'slideRight' | 'zoom' | 'none'
  durationMs: number
  delay?: number
  easing?: string
}

export interface MusicTrack {
  id: string
  teamId: string
  name: string
  url: string
  durationSeconds: number
  isStock: boolean
  uploadedBy?: string
  uploadedAt?: Date
}

// API DTOs
export interface TimelineDto {
  state: TimelineState
  tracks: TimelineTrack[]
  textOverlays: TextOverlay[]
}

export interface TimelineUpdateRequest {
  tracks: TimelineTrack[]
  textOverlays: TextOverlay[]
  playheadMs?: number
}
```

---

### Component Architecture

#### 1. Timeline Editor Page
**File**: `src/app/(dashboard)/studio/[id]/timeline/page.tsx`

```typescript
'use client'

import React, { useRef, useEffect, useCallback, useState } from 'react'
import { useTimeline } from '@/hooks/use-timeline'
import { TimelineCanvas } from '@/components/timeline/timeline-canvas'
import { TimelineRuler } from '@/components/timeline/timeline-ruler'
import { TimelineToolbar } from '@/components/timeline/timeline-toolbar'
import { TrackPanel } from '@/components/timeline/track-panel'
import { MusicPanel } from '@/components/timeline/music-panel'
import { TextOverlayPanel } from '@/components/timeline/text-overlay-panel'
import { TimelineContextProvider } from '@/contexts/timeline-context'
import Konva from 'konva'

interface TimelinePageProps {
  params: { id: string }
}

export default function TimelineEditorPage({ params }: TimelinePageProps) {
  const { id: episodeId } = params
  const [expanded, setExpanded] = useState<'music' | 'text' | null>(null)
  const canvasRef = useRef<Konva.Stage>(null)

  const {
    timeline,
    isLoading,
    error,
    saveTimeline,
    addClip,
    removeClip,
    updateClip,
    addTextOverlay,
    removeTextOverlay,
    updateTextOverlay,
    undo,
    redo,
    play,
    pause,
  } = useTimeline(episodeId)

  if (isLoading) return <div>Loading timeline...</div>

  return (
    <TimelineContextProvider timeline={timeline}>
      <div className="h-screen flex flex-col bg-gray-900 text-white">
        {/* Toolbar */}
        <TimelineToolbar
          playingState={timeline?.isPlaying}
          onPlay={play}
          onPause={pause}
          onUndo={undo}
          onRedo={redo}
          onZoomIn={() => {}}
          onZoomOut={() => {}}
          onSave={saveTimeline}
        />

        {/* Main Editor Area */}
        <div className="flex flex-1 overflow-hidden">
          {/* Left: Track Labels */}
          <TrackPanel
            tracks={timeline?.tracks}
            onTrackToggleMute={(trackId) => {}}
            onTrackToggleSolo={(trackId) => {}}
            onTrackToggleLock={(trackId) => {}}
          />

          {/* Center: Timeline Canvas */}
          <div className="flex-1 flex flex-col overflow-hidden">
            {/* Ruler + Playhead */}
            <TimelineRuler
              durationMs={timeline?.duration || 0}
              playheadMs={timeline?.playheadMs || 0}
              zoom={timeline?.zoom || 1}
              onPlayheadChange={(ms) => {}}
              pixelPerSecond={100}
            />

            {/* Tracks Canvas */}
            <TimelineCanvas
              ref={canvasRef}
              timeline={timeline}
              onClipDrop={(clip, trackId, startMs) => {
                addClip({ ...clip, trackId, startMs })
              }}
              onClipMove={(clipId, trackId, startMs) => {
                updateClip(clipId, { trackId, startMs })
              }}
              onClipTrim={(clipId, trimStartMs, trimEndMs) => {
                updateClip(clipId, { trimStartMs, trimEndMs })
              }}
              onTransitionAdd={(clipId, transition) => {
                updateClip(clipId, { transitionIn: transition })
              }}
              onClipSelect={(clipId) => {}}
            />
          </div>

          {/* Right: Side Panels */}
          <div className="w-64 bg-gray-800 border-l border-gray-700 overflow-y-auto">
            {expanded === 'music' && (
              <MusicPanel
                onTrackSelect={(track) => {
                  // Add to music track
                }}
                onClose={() => setExpanded(null)}
              />
            )}

            {expanded === 'text' && (
              <TextOverlayPanel
                onAdd={(overlay) => addTextOverlay(overlay)}
                onClose={() => setExpanded(null)}
              />
            )}

            {!expanded && (
              <div className="p-4 space-y-2">
                <button
                  onClick={() => setExpanded('music')}
                  className="w-full px-3 py-2 bg-blue-600 hover:bg-blue-700 rounded text-sm"
                >
                  Add Music
                </button>
                <button
                  onClick={() => setExpanded('text')}
                  className="w-full px-3 py-2 bg-purple-600 hover:bg-purple-700 rounded text-sm"
                >
                  Add Text
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </TimelineContextProvider>
  )
}
```

#### 2. Timeline Canvas Component (Konva)
**File**: `src/components/timeline/timeline-canvas.tsx`

```typescript
import React, { forwardRef, useEffect, useRef } from 'react'
import { Stage, Layer, Rect, Image, Text, Group } from 'react-konva'
import { DndContext, DragEndEvent, PointerSensor, useSensor, useSensors } from '@dnd-kit/core'
import Konva from 'konva'

interface TimelineCanvasProps {
  timeline: any
  onClipDrop: (clip: any, trackId: string, startMs: number) => void
  onClipMove: (clipId: string, trackId: string, startMs: number) => void
  onClipTrim: (clipId: string, trimStart: number, trimEnd: number) => void
  onTransitionAdd: (clipId: string, transition: any) => void
  onClipSelect: (clipId: string) => void
}

const TimelineCanvas = forwardRef<Konva.Stage, TimelineCanvasProps>(
  (props, ref) => {
    const {
      timeline,
      onClipDrop,
      onClipMove,
      onClipTrim,
      onTransitionAdd,
      onClipSelect,
    } = props

    const TRACK_HEIGHT = 60
    const TRACK_GAP = 10
    const PIXELS_PER_MS = 0.1 // 1px = 10ms

    const sensors = useSensors(
      useSensor(PointerSensor, {
        distance: 8,
      })
    )

    return (
      <DndContext sensors={sensors}>
        <Stage
          ref={ref}
          width={window.innerWidth - 400}
          height={400}
          className="bg-gray-800 border-b border-gray-700"
        >
          <Layer>
            {/* Draw tracks */}
            {timeline?.tracks?.map((track, idx) => {
              const y = idx * (TRACK_HEIGHT + TRACK_GAP)

              return (
                <Group key={track.id}>
                  {/* Track background */}
                  <Rect
                    x={0}
                    y={y}
                    width={window.innerWidth - 400}
                    height={TRACK_HEIGHT}
                    fill={idx % 2 === 0 ? '#1f2937' : '#111827'}
                    stroke="#374151"
                    strokeWidth={1}
                  />

                  {/* Clips in track */}
                  {track.clips?.map((clip) => {
                    const clipX = clip.startMs * PIXELS_PER_MS
                    const clipWidth = clip.durationMs * PIXELS_PER_MS

                    return (
                      <Group
                        key={clip.id}
                        draggable
                        onDragEnd={(e) => {
                          const newStartMs = Math.max(0, e.target.x() / PIXELS_PER_MS)
                          onClipMove(clip.id, track.id, newStartMs)
                        }}
                      >
                        {/* Clip body */}
                        <Rect
                          x={clipX}
                          y={y + 5}
                          width={clipWidth}
                          height={TRACK_HEIGHT - 10}
                          fill="#3b82f6"
                          corner={2}
                          onClick={() => onClipSelect(clip.id)}
                          stroke={clip.id === timeline.selectedClipId ? '#fff' : '#1e40af'}
                          strokeWidth={clip.id === timeline.selectedClipId ? 2 : 1}
                        />

                        {/* Trim Handles */}
                        {/* Left handle */}
                        <Rect
                          x={clipX}
                          y={y + 5}
                          width={4}
                          height={TRACK_HEIGHT - 10}
                          fill="#fbbf24"
                          cursor="col-resize"
                          draggable
                          onDragEnd={(e) => {
                            const delta = e.target.x() - clipX
                            onClipTrim(
                              clip.id,
                              clip.trimStartMs + Math.max(0, delta / PIXELS_PER_MS),
                              clip.trimEndMs
                            )
                          }}
                        />

                        {/* Right handle */}
                        <Rect
                          x={clipX + clipWidth - 4}
                          y={y + 5}
                          width={4}
                          height={TRACK_HEIGHT - 10}
                          fill="#fbbf24"
                          cursor="col-resize"
                          draggable
                          onDragEnd={(e) => {
                            const delta = e.target.x() - (clipX + clipWidth - 4)
                            onClipTrim(
                              clip.id,
                              clip.trimStartMs,
                              clip.trimEndMs + delta / PIXELS_PER_MS
                            )
                          }}
                        />
                      </Group>
                    )
                  })}
                </Group>
              )
            })}

            {/* Playhead */}
            <Group>
              <Rect
                x={(timeline?.playheadMs || 0) * PIXELS_PER_MS}
                y={0}
                width={2}
                height={timeline?.tracks?.length * (TRACK_HEIGHT + TRACK_GAP)}
                fill="#ef4444"
              />
            </Group>
          </Layer>
        </Stage>
      </DndContext>
    )
  }
)

TimelineCanvas.displayName = 'TimelineCanvas'
export { TimelineCanvas }
```

#### 3. Timeline Ruler Component
**File**: `src/components/timeline/timeline-ruler.tsx`

```typescript
import React, { useCallback } from 'react'

interface TimelineRulerProps {
  durationMs: number
  playheadMs: number
  zoom: number
  onPlayheadChange: (ms: number) => void
  pixelPerSecond: number
}

export function TimelineRuler({
  durationMs,
  playheadMs,
  zoom,
  onPlayheadChange,
  pixelPerSecond,
}: TimelineRulerProps) {
  const containerRef = React.useRef<HTMLDivElement>(null)

  const handleClick = (e: React.MouseEvent) => {
    if (!containerRef.current) return
    const rect = containerRef.current.getBoundingClientRect()
    const x = e.clientX - rect.left
    const ms = (x / pixelPerSecond) * 1000
    onPlayheadChange(ms)
  }

  const markers = []
  const intervalMs = 5000 // 5-second markers
  for (let i = 0; i <= durationMs; i += intervalMs) {
    markers.push(i)
  }

  return (
    <div
      ref={containerRef}
      onClick={handleClick}
      className="h-12 bg-gray-800 border-b border-gray-700 flex items-end relative cursor-text"
      style={{ width: `${(durationMs / 1000) * pixelPerSecond}px` }}
    >
      {/* Time markers */}
      {markers.map((ms) => {
        const x = (ms / 1000) * pixelPerSecond
        const seconds = Math.floor(ms / 1000)
        const minutes = Math.floor(seconds / 60)
        const secs = seconds % 60

        return (
          <div
            key={ms}
            className="absolute flex flex-col items-center"
            style={{ left: `${x}px`, transform: 'translateX(-50%)' }}
          >
            <div className="h-2 w-px bg-gray-600" />
            <span className="text-xs text-gray-400 mt-1">
              {minutes}:{secs.toString().padStart(2, '0')}
            </span>
          </div>
        )
      })}

      {/* Playhead */}
      <div
        className="absolute w-0.5 h-full bg-red-500 cursor-pointer"
        style={{
          left: `${(playheadMs / 1000) * pixelPerSecond}px`,
          opacity: 0.7,
        }}
      />
    </div>
  )
}
```

#### 4. Track Panel Component
**File**: `src/components/timeline/track-panel.tsx`

```typescript
import React from 'react'
import { Volume2, Eye, Lock } from 'lucide-react'

interface TrackPanelProps {
  tracks: any[]
  onTrackToggleMute: (trackId: string) => void
  onTrackToggleSolo: (trackId: string) => void
  onTrackToggleLock: (trackId: string) => void
}

export function TrackPanel({
  tracks,
  onTrackToggleMute,
  onTrackToggleSolo,
  onTrackToggleLock,
}: TrackPanelProps) {
  const TRACK_HEIGHT = 60
  const TRACK_GAP = 10

  return (
    <div className="w-48 bg-gray-900 border-r border-gray-700 overflow-y-auto">
      {tracks?.map((track, idx) => {
        const y = idx * (TRACK_HEIGHT + TRACK_GAP)

        return (
          <div
            key={track.id}
            className="h-20 border-b border-gray-700 p-2 flex items-center justify-between bg-gray-800 hover:bg-gray-750"
          >
            <div className="flex-1">
              <p className="text-xs font-bold uppercase">{track.trackType}</p>
              <p className="text-xs text-gray-400">{track.clips?.length || 0} clips</p>
            </div>

            <div className="flex gap-1">
              <button
                onClick={() => onTrackToggleMute(track.id)}
                className={`p-1 rounded ${
                  track.muted ? 'bg-gray-600 text-gray-300' : 'hover:bg-gray-700'
                }`}
                title="Mute"
              >
                <Volume2 className="w-4 h-4" />
              </button>
              <button
                onClick={() => onTrackToggleLock(track.id)}
                className={`p-1 rounded ${
                  track.locked ? 'bg-gray-600 text-gray-300' : 'hover:bg-gray-700'
                }`}
                title="Lock"
              >
                <Lock className="w-4 h-4" />
              </button>
            </div>
          </div>
        )
      })}
    </div>
  )
}
```

#### 5. Music Panel Component
**File**: `src/components/timeline/music-panel.tsx`

```typescript
import React, { useState } from 'react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Music, Upload, Play } from 'lucide-react'

interface MusicPanelProps {
  onTrackSelect: (track: any) => void
  onClose: () => void
}

export function MusicPanel({ onTrackSelect, onClose }: MusicPanelProps) {
  const [searchTerm, setSearchTerm] = useState('')
  const [stockTracks] = useState([
    { id: '1', name: 'Ambient Background', duration: 180 },
    { id: '2', name: 'Upbeat Energy', duration: 120 },
    { id: '3', name: 'Dramatic Tension', duration: 200 },
  ])

  const filtered = stockTracks.filter(t =>
    t.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <div className="p-4 space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="font-bold">Add Music</h3>
        <button
          onClick={onClose}
          className="text-gray-400 hover:text-white text-2xl leading-none"
        >
          ×
        </button>
      </div>

      <Input
        placeholder="Search music..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="bg-gray-700 border-gray-600"
      />

      <div className="space-y-2">
        {filtered.map((track) => (
          <button
            key={track.id}
            onClick={() => {
              onTrackSelect(track)
              onClose()
            }}
            className="w-full p-2 bg-gray-700 hover:bg-gray-600 rounded text-left text-sm flex items-center gap-2"
          >
            <Music className="w-4 h-4" />
            <span className="flex-1">{track.name}</span>
            <span className="text-xs text-gray-400">{track.duration}s</span>
          </button>
        ))}
      </div>

      <div className="border-t border-gray-600 pt-4">
        <p className="text-xs text-gray-400 mb-2">Upload Your Music</p>
        <label className="block">
          <input
            type="file"
            accept="audio/*"
            className="hidden"
            onChange={(e) => {
              if (e.target.files?.[0]) {
                // Handle upload
              }
            }}
          />
          <div className="p-3 border-2 border-dashed border-gray-600 rounded text-center cursor-pointer hover:border-gray-500">
            <Upload className="w-4 h-4 mx-auto mb-1" />
            <p className="text-xs">Upload Audio</p>
          </div>
        </label>
      </div>
    </div>
  )
}
```

#### 6. Text Overlay Panel Component
**File**: `src/components/timeline/text-overlay-panel.tsx`

```typescript
import React, { useState } from 'react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'

interface TextOverlayPanelProps {
  onAdd: (overlay: any) => void
  onClose: () => void
}

export function TextOverlayPanel({ onAdd, onClose }: TextOverlayPanelProps) {
  const [text, setText] = useState('Title')
  const [fontSize, setFontSize] = useState(32)
  const [color, setColor] = useState('#ffffff')
  const [animation, setAnimation] = useState<string>('fadeIn')

  const animations = ['fadeIn', 'slideUp', 'slideDown', 'slideLeft', 'slideRight', 'zoom', 'none']

  const handleAdd = () => {
    onAdd({
      text,
      fontSize,
      color,
      animation,
      startMs: 0,
      endMs: 5000,
    })
    onClose()
  }

  return (
    <div className="p-4 space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="font-bold">Add Text Overlay</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-white text-2xl leading-none">
          ×
        </button>
      </div>

      <div className="space-y-3">
        <div>
          <label className="text-xs font-medium block mb-1">Text</label>
          <Textarea
            value={text}
            onChange={(e) => setText(e.target.value)}
            className="bg-gray-700 border-gray-600 text-sm"
            rows={3}
          />
        </div>

        <div className="grid grid-cols-2 gap-2">
          <div>
            <label className="text-xs font-medium block mb-1">Font Size</label>
            <Input
              type="number"
              value={fontSize}
              onChange={(e) => setFontSize(Number(e.target.value))}
              className="bg-gray-700 border-gray-600"
            />
          </div>

          <div>
            <label className="text-xs font-medium block mb-1">Color</label>
            <input
              type="color"
              value={color}
              onChange={(e) => setColor(e.target.value)}
              className="w-full h-8 rounded cursor-pointer"
            />
          </div>
        </div>

        <div>
          <label className="text-xs font-medium block mb-1">Animation</label>
          <select
            value={animation}
            onChange={(e) => setAnimation(e.target.value)}
            className="w-full p-2 bg-gray-700 border border-gray-600 rounded text-sm"
          >
            {animations.map((anim) => (
              <option key={anim} value={anim}>
                {anim}
              </option>
            ))}
          </select>
        </div>
      </div>

      <Button
        onClick={handleAdd}
        className="w-full bg-blue-600 hover:bg-blue-700"
      >
        Add Overlay
      </Button>
    </div>
  )
}
```

#### 7. Timeline Toolbar
**File**: `src/components/timeline/timeline-toolbar.tsx`

```typescript
import React from 'react'
import { Play, Pause, RotateCcw, RotateCw, ZoomIn, ZoomOut, Save } from 'lucide-react'

interface TimelineToolbarProps {
  playingState: boolean
  onPlay: () => void
  onPause: () => void
  onUndo: () => void
  onRedo: () => void
  onZoomIn: () => void
  onZoomOut: () => void
  onSave: () => void
}

export function TimelineToolbar({
  playingState,
  onPlay,
  onPause,
  onUndo,
  onRedo,
  onZoomIn,
  onZoomOut,
  onSave,
}: TimelineToolbarProps) {
  return (
    <div className="flex items-center gap-4 px-4 py-3 bg-gray-900 border-b border-gray-700">
      {/* Playback Controls */}
      <div className="flex gap-2 border-r border-gray-700 pr-4">
        <button
          onClick={playingState ? onPause : onPlay}
          className="p-2 hover:bg-gray-800 rounded"
          title={playingState ? 'Pause' : 'Play'}
        >
          {playingState ? (
            <Pause className="w-5 h-5" />
          ) : (
            <Play className="w-5 h-5" />
          )}
        </button>
      </div>

      {/* Undo/Redo */}
      <div className="flex gap-2 border-r border-gray-700 pr-4">
        <button onClick={onUndo} className="p-2 hover:bg-gray-800 rounded" title="Undo">
          <RotateCcw className="w-5 h-5" />
        </button>
        <button onClick={onRedo} className="p-2 hover:bg-gray-800 rounded" title="Redo">
          <RotateCw className="w-5 h-5" />
        </button>
      </div>

      {/* Zoom */}
      <div className="flex gap-2 border-r border-gray-700 pr-4">
        <button onClick={onZoomOut} className="p-2 hover:bg-gray-800 rounded" title="Zoom Out">
          <ZoomOut className="w-5 h-5" />
        </button>
        <button onClick={onZoomIn} className="p-2 hover:bg-gray-800 rounded" title="Zoom In">
          <ZoomIn className="w-5 h-5" />
        </button>
      </div>

      {/* Save */}
      <button
        onClick={onSave}
        className="ml-auto p-2 bg-blue-600 hover:bg-blue-700 rounded flex gap-2 items-center"
      >
        <Save className="w-5 h-5" />
        Save Timeline
      </button>
    </div>
  )
}
```

### 8. Timeline Context & Hook
**File**: `src/contexts/timeline-context.tsx` (NEW)

```typescript
import React from 'react'
import { TimelineState } from '@/types'

interface TimelineContextType {
  timeline: TimelineState | null
  selectedClipId?: string
  setSelectedClipId: (id?: string) => void
}

export const TimelineContext = React.createContext<TimelineContextType | undefined>(undefined)

export function TimelineContextProvider({
  timeline,
  children,
}: {
  timeline: TimelineState | null
  children: React.ReactNode
}) {
  const [selectedClipId, setSelectedClipId] = React.useState<string>()

  return (
    <TimelineContext.Provider value={{ timeline, selectedClipId, setSelectedClipId }}>
      {children}
    </TimelineContext.Provider>
  )
}

export function useTimelineContext() {
  const context = React.useContext(TimelineContext)
  if (!context) {
    throw new Error('useTimelineContext must be used within TimelineContextProvider')
  }
  return context
}
```

**File**: `src/hooks/use-timeline.ts` (NEW)

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api-client'
import { TimelineState, TimelineClip, TextOverlay, TimelineUpdateRequest } from '@/types'
import React from 'react'

export function useTimeline(episodeId: string) {
  const queryClient = useQueryClient()

  // Fetch timeline
  const { data: timelineData, isLoading } = useQuery({
    queryKey: ['timeline', episodeId],
    queryFn: () => apiFetch<TimelineState>(`/episodes/${episodeId}/timeline`),
  })

  // Add clip mutation
  const addClipMutation = useMutation({
    mutationFn: async (clip: Partial<TimelineClip>) => {
      return apiFetch(`/episodes/${episodeId}/timeline/clips`, {
        method: 'POST',
        body: JSON.stringify(clip),
        headers: { 'Content-Type': 'application/json' },
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeline', episodeId] })
    },
  })

  // Update clip mutation
  const updateClipMutation = useMutation({
    mutationFn: async ({ clipId, updates }: { clipId: string; updates: Partial<TimelineClip> }) => {
      return apiFetch(`/episodes/${episodeId}/timeline/clips/${clipId}`, {
        method: 'PUT',
        body: JSON.stringify(updates),
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeline', episodeId] })
    },
  })

  // Remove clip mutation
  const removeClipMutation = useMutation({
    mutationFn: (clipId: string) =>
      apiFetch(`/episodes/${episodeId}/timeline/clips/${clipId}`, {
        method: 'DELETE',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeline', episodeId] })
    },
  })

  // Add text overlay mutation
  const addTextOverlayMutation = useMutation({
    mutationFn: async (overlay: Partial<TextOverlay>) => {
      return apiFetch(`/episodes/${episodeId}/timeline/text-overlays`, {
        method: 'POST',
        body: JSON.stringify(overlay),
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeline', episodeId] })
    },
  })

  // Remove text overlay mutation
  const removeTextOverlayMutation = useMutation({
    mutationFn: (overlayId: string) =>
      apiFetch(`/episodes/${episodeId}/timeline/text-overlays/${overlayId}`, {
        method: 'DELETE',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['timeline', episodeId] })
    },
  })

  // Save timeline mutation
  const saveTimelineMutation = useMutation({
    mutationFn: async (updates: TimelineUpdateRequest) => {
      return apiFetch(`/episodes/${episodeId}/timeline`, {
        method: 'PUT',
        body: JSON.stringify(updates),
      })
    },
  })

  return {
    timeline: timelineData,
    isLoading,
    error: null,
    addClip: (clip: Partial<TimelineClip>) => addClipMutation.mutateAsync(clip),
    removeClip: (clipId: string) => removeClipMutation.mutateAsync(clipId),
    updateClip: (clipId: string, updates: Partial<TimelineClip>) =>
      updateClipMutation.mutateAsync({ clipId, updates }),
    addTextOverlay: (overlay: Partial<TextOverlay>) => addTextOverlayMutation.mutateAsync(overlay),
    removeTextOverlay: (id: string) => removeTextOverlayMutation.mutateAsync(id),
    updateTextOverlay: (id: string, updates: Partial<TextOverlay>) => {},
    saveTimeline: (updates: TimelineUpdateRequest) => saveTimelineMutation.mutateAsync(updates),
    undo: () => {},
    redo: () => {},
    play: () => {},
    pause: () => {},
  }
}
```

### Phase 10 Implementation Checklist

UI Components:
- [ ] Timeline canvas renders with Konva.js
- [ ] 4 tracks (Video, Audio, Music, Text) visible
- [ ] Clips draggable and repositionable
- [ ] Trimming handles work (left/right)
- [ ] Timeline ruler with time markers
- [ ] Playhead scrubber functional
- [ ] Zoom in/out working

Features:
- [ ] Drag clips from storyboard to timeline
- [ ] Multiple clips per track
- [ ] Trim clips (adjust in/out times)
- [ ] Add transitions (dropdown: Cut/Fade/Dissolve)
- [ ] Music library picker + add to track
- [ ] Text overlay creation with font/color/animation
- [ ] Mute/solo/lock track controls
- [ ] Play/pause playback

State Management:
- [ ] Undo/redo history stack (at least 20 levels)
- [ ] Auto-save to backend every 30s
- [ ] Manual save button works
- [ ] Playhead position persists
- [ ] Clip selections persist in session

Performance:
- [ ] No lag when dragging 10+ clips
- [ ] Smooth scrolling timeline
- [ ] Zoom doesn't cause jank
- [ ] Memory doesn't leak (DevTools check)

---

# PHASE 11 - REVIEW & SHARING
## (Week 13-15, 3 weeks, 2 devs)

**Previous Status**: Not started  
**Complexity**: HIGH - OAuth + sharing features  
**Effort**: 25-30 dev-days  

---

### Overview: What Users Do

1. Render a video
2. Click "Share" button
3. Generate review link (optional: set expiry, password)
4. Share link publicly or with team
5. Others view video, add timestamped comments
6. Creator receives notifications on comments
7. Creator publishes to YouTube (Studio tier)
8. Configure brand kit (logo, watermark, colors)

---

### Data Models

```typescript
export interface ReviewLinkDto {
  id: string
  episodeId: string
  renderId: string
  token: string
  expiresAt?: Date
  isRevoked: boolean
  passwordHash?: string
  createdByUserId: string
  createdAt: Date
  commentCount: number
}

export interface ReviewCommentDto {
  id: string
  reviewLinkId: string
  authorName: string
  text: string
  timestampSeconds: number
  createdAt: Date
  isResolved: boolean
}

export interface BrandKitDto {
  id: string
  teamId: string
  logoUrl: string
  primaryColor: string
  secondaryColor: string
  watermarkPosition: 'bottom-right' | 'bottom-left' | 'top-right'
  watermarkOpacity: number
}

export interface SocialPublishDto {
  id: string
  episodeId: string
  renderId: string
  platform: 'youtube'
  externalVideoId: string
  status: 'pending' | 'published' | 'failed'
  publishedAt?: Date
  error?: string
}
```

### Component Architecture

#### 1. Share Page
**File**: `src/app/(dashboard)/studio/[id]/share/page.tsx`

```typescript
'use client'

import React, { useState } from 'react'
import { useRenders } from '@/hooks/use-renders'
import { ReviewLinkGenerator } from '@/components/share/review-link-generator'
import { ReviewLinkList } from '@/components/share/review-link-list'
import { YouTubePublish } from '@/components/share/publish-youtube'
import { BrandKitEditor } from '@/components/share/brand-kit-editor'
import { useSubscription } from '@/hooks/useSubscription'

interface SharePageProps {
  params: { id: string }
}

export default function SharePage({ params }: SharePageProps) {
  const { id: episodeId } = params
  const { renders } = useRenders(episodeId)
  const { subscription } = useSubscription()

  const latestRender = renders?.[0]
  const isStudioTier = subscription?.tier === 'studio'

  return (
    <div className="space-y-8">
      <h1 className="text-2xl font-bold">Share & Review</h1>

      {/* Review Links Section */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold">Review Links</h2>
        
        {latestRender ? (
          <>
            <ReviewLinkGenerator renderId={latestRender.id} />
            <ReviewLinkList renderId={latestRender.id} />
          </>
        ) : (
          <p className="text-gray-600">Render a video first to create review links.</p>
        )}
      </div>

      {/* Divider */}
      <hr className="border-gray-200" />

      {/* YouTube Publishing Section */}
      {isStudioTier && latestRender && (
        <>
          <div className="space-y-4">
            <h2 className="text-xl font-bold">YouTube Publishing</h2>
            <YouTubePublish renderId={latestRender.id} />
          </div>

          <hr className="border-gray-200" />
        </>
      )}

      {/* Brand Kit Section */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold">Brand Kit</h2>
        <BrandKitEditor />
      </div>
    </div>
  )
}
```

#### 2. Review Link Generator
**File**: `src/components/share/review-link-generator.tsx`

```typescript
import React, { useState } from 'react'
import { Copy, Check } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface ReviewLinkGeneratorProps {
  renderId: string
}

export function ReviewLinkGenerator({ renderId }: ReviewLinkGeneratorProps) {
  const [useExpiry, setUseExpiry] = useState(false)
  const [usePassword, setUsePassword] = useState(false)
  const [expiryDays, setExpiryDays] = useState(7)
  const [password, setPassword] = useState('')
  const [generatedLink, setGeneratedLink] = useState<string | null>(null)
  const [copied, setCopied] = useState(false)

  const handleGenerate = async () => {
    const response = await fetch(`/api/renders/${renderId}/review-links`, {
      method: 'POST',
      body: JSON.stringify({
        expiryDays: useExpiry ? expiryDays : null,
        password: usePassword ? password : null,
      }),
      headers: { 'Content-Type': 'application/json' },
    })

    if (response.ok) {
      const data = await response.json()
      setGeneratedLink(`${window.location.origin}/review/${data.token}`)
    }
  }

  const handleCopy = async () => {
    if (generatedLink) {
      await navigator.clipboard.writeText(generatedLink)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Generate Review Link</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Options */}
        <div className="space-y-3">
          {/* Expiry */}
          <div className="flex items-center gap-2">
            <Checkbox
              id="use-expiry"
              checked={useExpiry}
              onCheckedChange={setUseExpiry}
            />
            <label htmlFor="use-expiry" className="text-sm font-medium">
              Link expires in
            </label>
            {useExpiry && (
              <Input
                type="number"
                value={expiryDays}
                onChange={(e) => setExpiryDays(Number(e.target.value))}
                className="w-16"
              />
            )}
            {useExpiry && <span className="text-sm text-gray-600">days</span>}
          </div>

          {/* Password */}
          <div className="flex items-center gap-2">
            <Checkbox
              id="use-password"
              checked={usePassword}
              onCheckedChange={setUsePassword}
            />
            <label htmlFor="use-password" className="text-sm font-medium">
              Require password
            </label>
            {usePassword && (
              <Input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter password"
                className="flex-1"
              />
            )}
          </div>
        </div>

        {/* Generate Button */}
        <Button
          onClick={handleGenerate}
          className="w-full bg-blue-600 hover:bg-blue-700"
        >
          Generate Link
        </Button>

        {/* Generated Link */}
        {generatedLink && (
          <div className="p-3 bg-gray-100 rounded flex items-center gap-2">
            <Input
              type="text"
              value={generatedLink}
              readOnly
              className="flex-1"
            />
            <button
              onClick={handleCopy}
              className="p-2 hover:bg-gray-200 rounded"
            >
              {copied ? (
                <Check className="w-5 h-5 text-green-600" />
              ) : (
                <Copy className="w-5 h-5" />
              )}
            </button>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
```

#### 3. Public Review Page
**File**: `src/app/review/[token]/page.tsx` (NEW)

```typescript
'use client'

import React, { useEffect, useState } from 'react'
import { VideoPlayer } from '@/components/render/video-player'
import { CommentPanel } from '@/components/share/comment-panel'
import { Input } from '@/components/ui/input'
import { AlertCircle } from 'lucide-react'

interface ReviewPageProps {
  params: { token: string }
}

export default function ReviewPage({ params }: ReviewPageProps) {
  const [reviewData, setReviewData] = useState<any>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [password, setPassword] = useState('')
  const [isPasswordProtected, setIsPasswordProtected] = useState(false)

  useEffect(() => {
    const fetchReview = async () => {
      try {
        const response = await fetch(`/api/review/${params.token}`)

        if (response.status === 401) {
          setIsPasswordProtected(true)
          setIsLoading(false)
          return
        }

        if (!response.ok) {
          setError('Review link not found or expired')
          setIsLoading(false)
          return
        }

        const data = await response.json()
        setReviewData(data)
        setIsLoading(false)
      } catch (err) {
        setError('Failed to load review')
        setIsLoading(false)
      }
    }

    fetchReview()
  }, [params.token])

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const response = await fetch(`/api/review/${params.token}`, {
        method: 'POST',
        body: JSON.stringify({ password }),
        headers: { 'Content-Type': 'application/json' },
      })

      if (response.ok) {
        const data = await response.json()
        setReviewData(data)
        setIsPasswordProtected(false)
      } else {
        setError('Incorrect password')
      }
    } catch (err) {
      setError('Password verification failed')
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-white">Loading review...</div>
      </div>
    )
  }

  if (isPasswordProtected) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <form
          onSubmit={handlePasswordSubmit}
          className="bg-gray-800 p-8 rounded-lg w-96"
        >
          <h1 className="text-white text-2xl font-bold mb-4">Password Protected</h1>
          <Input
            type="password"
            placeholder="Enter password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="mb-4"
          />
          <button
            type="submit"
            className="w-full px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Unlock
          </button>
        </form>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="bg-red-50 p-4 rounded flex items-center gap-3">
          <AlertCircle className="w-6 h-6 text-red-600" />
          <div>
            <p className="font-bold text-red-900">{error}</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-900">
      <div className="flex">
        {/* Video Player */}
        <div className="flex-1">
          <VideoPlayer cdnUrl={reviewData?.render?.cdnUrl} />
        </div>

        {/* Comment Panel */}
        <div className="w-96 bg-gray-800 border-l border-gray-700 overflow-y-auto">
          <CommentPanel
            reviewLinkId={reviewData?.id}
            comments={reviewData?.comments || []}
          />
        </div>
      </div>
    </div>
  )
}
```

#### 4. Comment Panel
**File**: `src/components/share/comment-panel.tsx`

```typescript
import React, { useState } from 'react'
import { formatDistanceToNow } from 'date-fns'
import { Send } from 'lucide-react'
import { Textarea } from '@/components/ui/textarea'
import { ReviewCommentDto } from '@/types'

interface CommentPanelProps {
  reviewLinkId: string
  comments: ReviewCommentDto[]
}

export function CommentPanel({ reviewLinkId, comments }: CommentPanelProps) {
  const [newComment, setNewComment] = useState('')
  const [currentTime, setCurrentTime] = useState(0)

  const handleAddComment = async () => {
    if (!newComment.trim()) return

    await fetch(`/api/review/${reviewLinkId}/comments`, {
      method: 'POST',
      body: JSON.stringify({
        text: newComment,
        timestampSeconds: Math.floor(currentTime),
        authorName: 'Anonymous',
      }),
      headers: { 'Content-Type': 'application/json' },
    })

    setNewComment('')
  }

  return (
    <div className="flex flex-col h-full">
      {/* Comments List */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {comments.map((comment) => (
          <div key={comment.id} className="bg-gray-700 p-3 rounded text-sm">
            <div className="flex justify-between items-start mb-1">
              <p className="font-bold text-white">{comment.authorName}</p>
              <span className="text-xs text-gray-400">
                {comment.timestampSeconds}s
              </span>
            </div>
            <p className="text-gray-200">{comment.text}</p>
            <p className="text-xs text-gray-500 mt-1">
              {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
            </p>
          </div>
        ))}
      </div>

      {/* Add Comment Form */}
      <div className="border-t border-gray-600 p-4 space-y-2">
        <Textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Add a comment..."
          className="bg-gray-700 border-gray-600"
          rows={3}
        />
        <button
          onClick={handleAddComment}
          className="w-full px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded flex items-center justify-center gap-2"
        >
          <Send className="w-4 h-4" />
          Comment
        </button>
      </div>
    </div>
  )
}
```

#### 5. YouTube Publish Component
**File**: `src/components/share/publish-youtube.tsx`

```typescript
import React, { useState } from 'react'
import { Youtube } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { AlertCircle, Check } from 'lucide-react'

interface YouTubePublishProps {
  renderId: string
}

export function YouTubePublish({ renderId }: YouTubePublishProps) {
  const [isConnected, setIsConnected] = useState(false)
  const [isPublishing, setIsPublishing] = useState(false)
  const [title, setTitle] = useState('My Animation')
  const [description, setDescription] = useState('')
  const [visibility, setVisibility] = useState<'private' | 'unlisted' | 'public'>('unlisted')

  const handleConnect = async () => {
    // Start OAuth flow
    const redirectUrl = await fetch('/api/youtube/auth-url').then(r => r.json())
    window.location.href = redirectUrl.url
  }

  const handlePublish = async () => {
    setIsPublishing(true)
    try {
      await fetch(`/api/renders/${renderId}/publish/youtube`, {
        method: 'POST',
        body: JSON.stringify({
          title,
          description,
          privacyStatus: visibility,
        }),
        headers: { 'Content-Type': 'application/json' },
      })
      // Show success message
    } catch (error) {
      console.error('Publish failed:', error)
    } finally {
      setIsPublishing(false)
    }
  }

  if (!isConnected) {
    return (
      <div className="p-4 bg-gray-50 border border-gray-200 rounded flex items-center gap-3">
        <Youtube className="w-6 h-6 text-red-600" />
        <div className="flex-1">
          <p className="font-medium">Connect YouTube Account</p>
          <p className="text-sm text-gray-600">Sign in to publish directly to YouTube</p>
        </div>
        <Button onClick={handleConnect} className="bg-red-600 hover:bg-red-700">
          Connect
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-4 p-4 bg-gray-50 border border-gray-200 rounded">
      <div>
        <label className="block text-sm font-medium mb-1">Title</label>
        <Input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Video title"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Description</label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Video description"
          className="w-full px-3 py-2 border border-gray-300 rounded"
          rows={4}
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Visibility</label>
        <select
          value={visibility}
          onChange={(e) => setVisibility(e.target.value as any)}
          className="w-full px-3 py-2 border border-gray-300 rounded"
        >
          <option value="private">Private</option>
          <option value="unlisted">Unlisted</option>
          <option value="public">Public</option>
        </select>
      </div>

      <Button
        onClick={handlePublish}
        disabled={isPublishing}
        className="w-full bg-red-600 hover:bg-red-700"
      >
        {isPublishing ? 'Publishing...' : 'Publish to YouTube'}
      </Button>
    </div>
  )
}
```

#### 6. Brand Kit Editor
**File**: `src/components/share/brand-kit-editor.tsx`

```typescript
import React, { useState, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useBrandKit } from '@/hooks/use-brand-kit'
import { Upload } from 'lucide-react'

export function BrandKitEditor() {
  const { brandKit, updateBrandKit, uploadLogo } = useBrandKit()
  const [formData, setFormData] = useState(brandKit || {})
  const [logoFile, setLogoFile] = useState<File | null>(null)

  useEffect(() => {
    if (brandKit) setFormData(brandKit)
  }, [brandKit])

  const handleSave = async () => {
    if (logoFile) {
      await uploadLogo(logoFile)
    }
    await updateBrandKit(formData)
  }

  return (
    <div className="space-y-4 p-4 bg-gray-50 border border-gray-200 rounded">
      {/* Logo Upload */}
      <div>
        <label className="block text-sm font-medium mb-2">Logo</label>
        <label className="flex items-center justify-center border-2 border-dashed border-gray-300 rounded p-6 cursor-pointer hover:border-gray-400">
          <div className="text-center">
            <Upload className="w-6 h-6 text-gray-400 mx-auto mb-2" />
            <p className="text-sm text-gray-600">Upload Logo</p>
          </div>
          <input
            type="file"
            accept="image/*"
            className="hidden"
            onChange={(e) => setLogoFile(e.target.files?.[0] || null)}
          />
        </label>
      </div>

      {/* Colors */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium mb-1">Primary Color</label>
          <input
            type="color"
            value={formData.primaryColor || '#000000'}
            onChange={(e) => setFormData({ ...formData, primaryColor: e.target.value })}
            className="w-full h-10 rounded cursor-pointer"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">Secondary Color</label>
          <input
            type="color"
            value={formData.secondaryColor || '#ffffff'}
            onChange={(e) => setFormData({ ...formData, secondaryColor: e.target.value })}
            className="w-full h-10 rounded cursor-pointer"
          />
        </div>
      </div>

      {/* Watermark */}
      <div>
        <label className="block text-sm font-medium mb-2">Watermark Position</label>
        <select
          value={formData.watermarkPosition || 'bottom-right'}
          onChange={(e) => setFormData({ ...formData, watermarkPosition: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded"
        >
          <option value="bottom-right">Bottom Right</option>
          <option value="bottom-left">Bottom Left</option>
          <option value="top-right">Top Right</option>
        </select>
      </div>

      <Button onClick={handleSave} className="w-full bg-blue-600 hover:bg-blue-700">
        Save Brand Kit
      </Button>
    </div>
  )
}
```

### Phase 11 Implementation Checklist
- [ ] Review link generator dialog
- [ ] Link expiry date picker working
- [ ] Password protection optional
- [ ] Public review page loads (no auth)
- [ ] Video player in review page
- [ ] Comment timestamps on progress bar
- [ ] Add comment form working
- [ ] Comments persist to backend
- [ ] YouTube connection flow (OAuth)
- [ ] YouTube publish dialog
- [ ] Brand kit editor (logo, colors)
- [ ] Watermark preview
- [ ] All pages responsive

---

# PHASE 12 - ANALYTICS & ADMIN
## (Week 16-18, 3 weeks, 1 dev)

**Previous Status**: Not started  
**Complexity**: MEDIUM - Dashboard UX  
**Effort**: 20-25 dev-days  

---

### Overview

1. **Creator Analytics**: Per-episode metrics (views, shares, renders)
2. **Admin Dashboard**: DAU/MAU, job queue, costs, errors
3. **Notifications**: Real-time job completion, billing alerts, team invites
4. **Usage Metering**: Display quota per tier, enforce limits

---

### Data Models

```typescript
export interface VideoAnalyticsDto {
  episodeId: string
  renderId?: string
  viewCount: number
  uniqueViewers: number
  shareCount: number
  embedCount: number
  averageWatchTime: number
  topReferrers: Array<{ source: string; count: number }>
  views24h: Array<{ timestamp: Date; count: number }>
}

export interface TeamAnalyticsDto {
  teamId: string
  totalEpisodes: number
  totalViews: number
  totalRenders: number
  averageRenderTime: number
  costPerMonth: number
  quotaUsed: number
  quotaTotal: number
}

export interface AdminStatsDto {
  dau: number // Daily active users
  mau: number // Monthly active users
  totalSubscriptions: number
  activeSubscriptions: number
  subscriptionsByTier: Record<string, number>
  jobQueueDepth: number
  averageJobDuration: number
  errorRate: number
  failedJobsLast24h: number
}

export interface NotificationDto {
  id: string
  userId: string
  type: 'job_complete' | 'team_invite' | 'billing_alert' | 'system_message'
  title: string
  body: string
  isRead: boolean
  relatedEntityId?: string
  relatedEntityType?: string
  createdAt: Date
}

// Type for billing alerts
export interface UsageAlertDto {
  teamId: string
  tier: string
  episodeQuota: number
  episodeUsed: number
  percentageUsed: number
  alertLevel: 'warning' | 'critical' // 80%+ or 100%
}
```

### 1. Analytics Dashboard
**File**: `src/app/(dashboard)/studio/[id]/analytics/page.tsx`

```typescript
'use client'

import React from 'react'
import { useVideoAnalytics } from '@/hooks/use-analytics'
import { MetricCard } from '@/components/analytics/metric-card'
import { ViewsChart } from '@/components/analytics/views-chart'
import { ReferrersList } from '@/components/analytics/referrers-list'
import { EngagementMetrics } from '@/components/analytics/engagement-metrics'

interface AnalyticsPageProps {
  params: { id: string }
}

export default function AnalyticsPage({ params }: AnalyticsPageProps) {
  const { id: episodeId } = params
  const { analytics, isLoading } = useVideoAnalytics(episodeId)

  if (isLoading) return <div>Loading analytics...</div>

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Video Analytics</h1>

      {/* Top Metrics Row */}
      <div className="grid grid-cols-4 gap-4">
        <MetricCard
          label="Total Views"
          value={analytics?.viewCount || 0}
          trend={12}
        />
        <MetricCard
          label="Unique Viewers"
          value={analytics?.uniqueViewers || 0}
          trend={8}
        />
        <MetricCard
          label="Shares"
          value={analytics?.shareCount || 0}
          trend={-3}
        />
        <MetricCard
          label="Avg Watch Time"
          value={`${Math.round(analytics?.averageWatchTime || 0)}s`}
          trend={5}
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-2 gap-4">
        <ViewsChart views={analytics?.views24h || []} />
        <EngagementMetrics analytics={analytics} />
      </div>

      {/* Referrers */}
      {analytics?.topReferrers && (
        <ReferrersList referrers={analytics.topReferrers} />
      )}
    </div>
  )
}
```

### 2. Admin Dashboard
**File**: `src/app/(dashboard)/admin/page.tsx` (NEW)

```typescript
'use client'

import React from 'react'
import { useAdminStats } from '@/hooks/use-admin'
import { AdminStatsCards } from '@/components/admin/admin-stats-cards'
import { JobQueueTable } from '@/components/admin/job-queue-table'
import { ErrorRateChart } from '@/components/admin/error-rate-chart'
import { SubscriptionStats } from '@/components/admin/subscription-stats'

export default function AdminPage() {
  const { stats, isLoading } = useAdminStats()

  if (isLoading) return <div>Loading...</div>

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Admin Dashboard</h1>

      {/* Top Stats */}
      <AdminStatsCards stats={stats} />

      {/* Job Queue */}
      <div>
        <h2 className="text-xl font-bold mb-4">Job Queue</h2>
        <JobQueueTable />
      </div>

      {/* Errors & Performance */}
      <div className="grid grid-cols-2 gap-4">
        <ErrorRateChart />
        <SubscriptionStats stats={stats} />
      </div>
    </div>
  )
}
```

### 3. Notification Bell Component
**File**: `src/components/notifications/notification-bell.tsx`

```typescript
import React, { useState, useEffect } from 'react'
import { Bell } from 'lucide-react'
import { NotificationPanel } from './notification-panel'
import { useNotifications } from '@/hooks/use-notifications'

export function NotificationBell() {
  const { notifications, unreadCount, markAsRead } = useNotifications()
  const [isOpen, setIsOpen] = useState(false)

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 hover:bg-gray-100 rounded-full"
      >
        <Bell className="w-5 h-5" />
        {unreadCount > 0 && (
          <span className="absolute top-0 right-0 w-5 h-5 bg-red-600 text-white text-xs rounded-full flex items-center justify-center">
            {Math.min(unreadCount, 9)}
          </span>
        )}
      </button>

      {isOpen && (
        <NotificationPanel
          notifications={notifications}
          onMarkAsRead={markAsRead}
          onClose={() => setIsOpen(false)}
        />
      )}
    </div>
  )
}
```

### 4. Usage Alert Component
**File**: `src/components/usage/usage-alert.tsx`

```typescript
import React from 'react'
import { AlertCircle } from 'lucide-react'
import { Progress } from '@/components/ui/progress'
import { useSubscription } from '@/hooks/useSubscription'

export function UsageAlert() {
  const { subscription, usage } = useSubscription()

  if (!usage || usage.percentageUsed < 80) return null

  const isCritical = usage.percentageUsed >= 100

  return (
    <div className={`p-4 rounded-lg flex items-start gap-3 ${
      isCritical
        ? 'bg-red-50 border border-red-200'
        : 'bg-yellow-50 border border-yellow-200'
    }`}>
      <AlertCircle className={`w-5 h-5 flex-shrink-0 ${
        isCritical ? 'text-red-600' : 'text-yellow-600'
      }`} />

      <div className="flex-1">
        <p className={`font-bold ${
          isCritical ? 'text-red-900' : 'text-yellow-900'
        }`}>
          {isCritical ? 'Quota Exceeded' : 'Approaching Quota'}
        </p>

        <p className={`text-sm ${
          isCritical ? 'text-red-700' : 'text-yellow-700'
        }`}>
          {usage.episodeUsed} / {usage.episodeQuota} episodes used this month
        </p>

        <Progress value={usage.percentageUsed} className="mt-2" />

        <p className="text-xs text-gray-600 mt-2">
          Upgrade your plan to increase your episode quota
        </p>
      </div>
    </div>
  )
}
```

### 5. Notification Hook
**File**: `src/hooks/use-notifications.ts` (NEW)

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api-client'
import { NotificationDto } from '@/types'
import React from 'react'
import { useSignalR } from './use-signal-r'

export function useNotifications() {
  const queryClient = useQueryClient()
  const { connection } = useSignalR(`${process.env.NEXT_PUBLIC_API_BASE_URL}/notification-hub`)

  // Fetch notifications
  const { data: notifications = [] } = useQuery({
    queryKey: ['notifications'],
    queryFn: () => apiFetch<NotificationDto[]>('/notifications?limit=50'),
    refetchInterval: 30000, // Poll every 30s
  })

  const unreadCount = notifications.filter(n => !n.isRead).length

  // Real-time notification via SignalR
  React.useEffect(() => {
    if (!connection) return

    const handleNotification = (notification: NotificationDto) => {
      queryClient.setQueryData(['notifications'], (old: NotificationDto[] | undefined) => {
        return [notification, ...(old || [])]
      })
    }

    connection.on('NewNotification', handleNotification)

    return () => {
      connection?.off('NewNotification')
    }
  }, [connection, queryClient])

  // Mark as read
  const markAsReadMutation = useMutation({
    mutationFn: (notificationId: string) =>
      apiFetch(`/notifications/${notificationId}/read`, { method: 'PATCH' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
    },
  })

  return {
    notifications,
    unreadCount,
    markAsRead: (id: string) => markAsReadMutation.mutateAsync(id),
  }
}
```

### 6. Analytics Hook
**File**: `src/hooks/use-analytics.ts` (NEW)

```typescript
import { useQuery } from '@tanstack/react-query'
import { apiFetch } from '@/lib/api-client'
import { VideoAnalyticsDto } from '@/types'

export function useVideoAnalytics(episodeId: string) {
  const { data, isLoading, error } = useQuery({
    queryKey: ['analytics', episodeId],
    queryFn: () => apiFetch<VideoAnalyticsDto>(`/episodes/${episodeId}/analytics`),
    staleTime: 60000, // 1 minute
  })

  return {
    analytics: data,
    isLoading,
    error,
  }
}

export function useAdminStats() {
  const { data, isLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: () => apiFetch('/admin/stats'),
    refetchInterval: 60000, // 1 minute
  })

  return {
    stats: data,
    isLoading,
  }
}

export function useTeamAnalytics(teamId: string) {
  const { data, isLoading } = useQuery({
    queryKey: ['team-analytics', teamId],
    queryFn: () => apiFetch(`/teams/${teamId}/analytics`),
    staleTime: 300000, // 5 minutes
  })

  return {
    analytics: data,
    isLoading,
  }
}
```

### Phase 12 Implementation Checklist
- [ ] Creator analytics page loads
- [ ] Views chart displays data
- [ ] Referrer list shows top sources
- [ ] Admin dashboard accessible only to admins
- [ ] DAU/MAU displays correctly
- [ ] Job queue table shows recent jobs
- [ ] Subscription tier breakdown shows
- [ ] Notification bell in header
- [ ] Notifications panel slides out
- [ ] Real-time notifications via SignalR
- [ ] Usage alert displays when 80%+
- [ ] Usage quota enforced (block rendering if > 100%)
- [ ] All analytics pages responsive

---

## Type Updates Needed

Add to `src/types/index.ts`:

```typescript
// Timeline types (Phase 10)
export interface TimelineState { /* ... */ }
export interface TimelineTrack { /* ... */ }
export interface TimelineClip { /* ... */ }
export interface TextOverlay { /* ... */ }
export interface MusicTrack { /* ... */ }

// Review types (Phase 11)
export interface ReviewLinkDto { /* ... */ }
export interface ReviewCommentDto { /* ... */ }
export interface BrandKitDto { /* ... */ }
export interface SocialPublishDto { /* ... */ }

// Analytics types (Phase 12)
export interface VideoAnalyticsDto { /* ... */ }
export interface TeamAnalyticsDto { /* ... */ }
export interface AdminStatsDto { /* ... */ }
export interface NotificationDto { /* ... */ }
```

