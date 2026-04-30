import { create } from "zustand";
import { timelineUtils } from "@/lib/timeline-utils";
import { DRAG_GRID_MS } from "@/lib/timeline/clip-drag-handler";
import type { Timeline, TimelineTrack, TimelineClip, VideoClip, TextOverlay, AudioClip } from "@/types/timeline";

const MAX_HISTORY = 50;

/** Deep-clone via JSON round-trip (safe for plain data objects). */
function deepClone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value));
}

/** Immutably update a clip inside the track array without mutating state. */
function updateClipInTracks(
  tracks: TimelineTrack[],
  clipId: string,
  updater: (clip: TimelineClip) => TimelineClip
): TimelineTrack[] {
  return tracks.map((track) => ({
    ...track,
    clips: track.clips.map((c) => (c.id === clipId ? updater(c) : c)),
  }));
}

/** Snap a ms value to the 100ms grid. */
function snap(ms: number): number {
  return Math.round(ms / DRAG_GRID_MS) * DRAG_GRID_MS;
}

/** True when two clips overlap (strict: touching edges are NOT overlapping). */
function clipsOverlap(a: TimelineClip, b: TimelineClip): boolean {
  return a.startMs < b.startMs + b.durationMs && a.startMs + a.durationMs > b.startMs;
}

/** Find the track and clip by clipId. Returns undefined if not found. */
function findTrackAndClip(
  timeline: Timeline,
  clipId: string
): { track: TimelineTrack; clip: TimelineClip } | undefined {
  for (const track of timeline.tracks) {
    const clip = track.clips.find((c) => c.id === clipId);
    if (clip) return { track, clip };
  }
  return undefined;
}

/** Generate a simple unique ID for new items. */
function uid(): string {
  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 7)}`;
}

// ── State shape ───────────────────────────────────────────────────────────────

interface TimelineStoreState {
  timeline: Timeline | null;
  selectedClipId: string | null;
  selectedTrackId: string | null;
  playheadPositionMs: number;
  isPlaying: boolean;
  zoom: number;
  /** Capped at MAX_HISTORY snapshots */
  history: Timeline[];
  historyIndex: number;

  // ── Transient drag / trim state (not persisted in history) ────────────────
  isDragging: boolean;
  isTrimming: boolean;
  draggedClipId: string | null;

  // ── Actions ────────────────────────────────────────────────────────────────

  loadTimeline: (timeline: Timeline) => void;

  selectClip:  (clipId: string | null) => void;
  selectTrack: (trackId: string | null) => void;

  setPlayheadPosition: (ms: number) => void;
  togglePlayback: () => void;
  play:  () => void;
  pause: () => void;

  setZoom:  (zoom: number) => void;
  zoomIn:   () => void;
  zoomOut:  () => void;

  /**
   * Move a clip to a new absolute start position.
   * Snaps to 100ms grid, constrains within timeline bounds,
   * and rejects the move if it would overlap another clip.
   */
  moveClip: (clipId: string, newStartMs: number) => void;

  /**
   * Trim a VideoClip's start/duration.
   * Enforces minimum duration (500ms) and rejects invalid ranges.
   */
  trimClip: (clipId: string, trimDeltaStartMs: number, newDurationMs: number) => void;

  /** Change the transition on a VideoClip. */
  setTransition: (clipId: string, transition: VideoClip["transitionIn"]) => void;

  toggleTrackMute: (trackId: string) => void;
  toggleTrackLock: (trackId: string) => void;

  /** Set the master volume (0–100) for an audio or music track. */
  setTrackVolume: (trackId: string, volumePercent: number) => void;

  /** Toggle the auto-duck flag on a music track. */
  toggleTrackAutoDuck: (trackId: string) => void;

  /** Add an AudioClip to a track. */
  addMusicClip: (clip: Omit<AudioClip, "id">, trackId: string) => void;

  // ── Text overlay actions ──────────────────────────────────────────────────
  addTextOverlay:    (overlay: Omit<TextOverlay, "id">) => void;
  updateTextOverlay: (id: string, updates: Partial<TextOverlay>) => void;
  deleteTextOverlay: (id: string) => void;

  undo: () => void;
  redo: () => void;

  advancePlayhead: (deltaMs: number) => void;

  /** Mark a clip as being dragged (for external consumers). */
  setDragging: (clipId: string | null) => void;
  /** Mark a clip as being trimmed (for external consumers). */
  setTrimming: (clipId: string | null) => void;
}

// ── Store ─────────────────────────────────────────────────────────────────────

export const useTimelineStore = create<TimelineStoreState>((set, get) => {
  function commitTimeline(updated: Timeline) {
    const { history, historyIndex } = get();
    const truncated = history.slice(0, historyIndex + 1);
    const next = [...truncated, deepClone(updated)].slice(-MAX_HISTORY);
    set({ timeline: updated, history: next, historyIndex: next.length - 1 });
  }

  return {
    // ── Initial state ────────────────────────────────────────────────────────
    timeline: null,
    selectedClipId: null,
    selectedTrackId: null,
    playheadPositionMs: 0,
    isPlaying: false,
    zoom: 1,
    history: [],
    historyIndex: -1,
    isDragging: false,
    isTrimming: false,
    draggedClipId: null,

    // ── Load ─────────────────────────────────────────────────────────────────
    loadTimeline: (timeline) => {
      // Ensure textOverlays is always present even on older mock shapes
      const snapshot = deepClone({ ...timeline, textOverlays: timeline.textOverlays ?? [] });
      set({
        timeline: snapshot,
        history: [snapshot],
        historyIndex: 0,
        selectedClipId: null,
        selectedTrackId: null,
        playheadPositionMs: 0,
        isPlaying: false,
      });
    },

    // ── Selection ────────────────────────────────────────────────────────────
    selectClip:  (clipId)  => set({ selectedClipId: clipId }),
    selectTrack: (trackId) => set({ selectedTrackId: trackId }),

    // ── Playhead / playback ───────────────────────────────────────────────────
    setPlayheadPosition: (ms) =>
      set((state) => ({
        playheadPositionMs: Math.max(0, Math.min(ms, state.timeline?.durationMs ?? 0)),
      })),

    togglePlayback: () => set((s) => ({ isPlaying: !s.isPlaying })),
    play:  () => set({ isPlaying: true }),
    pause: () => set({ isPlaying: false }),

    advancePlayhead: (deltaMs) =>
      set((state) => {
        if (!state.timeline) return state;
        const newPos = Math.min(
          state.playheadPositionMs + deltaMs,
          state.timeline.durationMs
        );
        return {
          playheadPositionMs: newPos,
          isPlaying: newPos < state.timeline.durationMs,
        };
      }),

    // ── Zoom ─────────────────────────────────────────────────────────────────
    setZoom:  (zoom) => set({ zoom: Math.max(1, Math.min(5, zoom)) }),
    zoomIn:   ()     => set((s) => ({ zoom: Math.min(5, +(s.zoom + 0.5).toFixed(1)) })),
    zoomOut:  ()     => set((s) => ({ zoom: Math.max(1, +(s.zoom - 0.5).toFixed(1)) })),

    // ── Drag / trim state ────────────────────────────────────────────────────
    setDragging: (clipId) =>
      set({ isDragging: clipId !== null, draggedClipId: clipId }),
    setTrimming: (clipId) =>
      set({ isTrimming: clipId !== null, draggedClipId: clipId }),

    // ── Clip mutations ────────────────────────────────────────────────────────

    moveClip: (clipId, newStartMs) => {
      const { timeline } = get();
      if (!timeline) return;

      const found = findTrackAndClip(timeline, clipId);
      if (!found) return;
      const { track, clip } = found;

      // Snap + constrain within timeline
      const snapped   = snap(Math.max(0, newStartMs));
      const maxStart  = Math.max(0, timeline.durationMs - clip.durationMs);
      const finalMs   = Math.min(snapped, maxStart);

      // Reject if the new position overlaps any sibling clip
      const provisional = { ...clip, startMs: finalMs };
      const overlaps    = track.clips.some(
        (other) => other.id !== clipId && clipsOverlap(provisional, other)
      );
      if (overlaps) return;

      const updated: Timeline = {
        ...timeline,
        tracks: updateClipInTracks(timeline.tracks, clipId, (c) =>
          timelineUtils.moveClip(c, finalMs)
        ),
      };
      commitTimeline(updated);
    },

    trimClip: (clipId, trimDeltaStartMs, newDurationMs) => {
      const { timeline } = get();
      if (!timeline) return;

      const found = findTrackAndClip(timeline, clipId);
      if (!found) return;
      const { clip } = found;

      // Snap duration
      const snappedDuration = snap(Math.max(500, newDurationMs));
      const rawNewStart     = clip.startMs + trimDeltaStartMs;
      const snappedStart    = snap(Math.max(0, rawNewStart));
      const originalEnd     = clip.startMs + clip.durationMs;

      // Enforce that end doesn't move past timeline
      if (snappedStart + snappedDuration > timeline.durationMs) return;
      // Enforce minimum duration
      if (snappedDuration < 500) return;
      // If left-trimming, ensure we don't move start beyond original end
      if (snappedStart >= originalEnd) return;

      const updated: Timeline = {
        ...timeline,
        tracks: updateClipInTracks(timeline.tracks, clipId, (c) => {
          if (c.type !== "video") return c;
          return timelineUtils.trimClip(c, trimDeltaStartMs, snappedDuration);
        }),
      };
      commitTimeline(updated);
    },

    setTransition: (clipId, transition) => {
      const { timeline } = get();
      if (!timeline) return;
      const updated: Timeline = {
        ...timeline,
        tracks: updateClipInTracks(timeline.tracks, clipId, (c) => {
          if (c.type !== "video") return c;
          return { ...c, transitionIn: transition };
        }),
      };
      commitTimeline(updated);
    },

    toggleTrackMute: (trackId) => {
      const { timeline } = get();
      if (!timeline) return;
      const updated: Timeline = {
        ...timeline,
        tracks: timeline.tracks.map((t) =>
          t.id === trackId ? { ...t, isMuted: !t.isMuted } : t
        ),
      };
      commitTimeline(updated);
    },

    toggleTrackLock: (trackId) => {
      const { timeline } = get();
      if (!timeline) return;
      const updated: Timeline = {
        ...timeline,
        tracks: timeline.tracks.map((t) =>
          t.id === trackId ? { ...t, isLocked: !t.isLocked } : t
        ),
      };
      commitTimeline(updated);
    },

    // ── Volume / auto-duck ────────────────────────────────────────────────────

    setTrackVolume: (trackId, volumePercent) => {
      const { timeline } = get();
      if (!timeline) return;
      const clamped = Math.max(0, Math.min(100, volumePercent));
      const updated: Timeline = {
        ...timeline,
        tracks: timeline.tracks.map((t) =>
          t.id === trackId ? { ...t, volumePercent: clamped } : t
        ),
      };
      commitTimeline(updated);
    },

    toggleTrackAutoDuck: (trackId) => {
      const { timeline } = get();
      if (!timeline) return;
      const updated: Timeline = {
        ...timeline,
        tracks: timeline.tracks.map((t) =>
          t.id === trackId ? { ...t, autoDuck: !t.autoDuck } : t
        ),
      };
      commitTimeline(updated);
    },

    addMusicClip: (clipData, trackId) => {
      const { timeline } = get();
      if (!timeline) return;
      const newClip: AudioClip = { ...clipData, id: uid() };
      const updated: Timeline = {
        ...timeline,
        tracks: timeline.tracks.map((t) =>
          t.id === trackId ? { ...t, clips: [...t.clips, newClip] } : t
        ),
      };
      commitTimeline(updated);
    },

    // ── Text overlay mutations ────────────────────────────────────────────────

    addTextOverlay: (overlayData) => {
      const { timeline } = get();
      if (!timeline) return;
      const overlay: TextOverlay = { ...overlayData, id: uid() };
      const updated: Timeline = {
        ...timeline,
        textOverlays: [...(timeline.textOverlays ?? []), overlay],
      };
      commitTimeline(updated);
    },

    updateTextOverlay: (id, updates) => {
      const { timeline } = get();
      if (!timeline) return;
      const updated: Timeline = {
        ...timeline,
        textOverlays: (timeline.textOverlays ?? []).map((o) =>
          o.id === id ? { ...o, ...updates } : o
        ),
      };
      commitTimeline(updated);
    },

    deleteTextOverlay: (id) => {
      const { timeline } = get();
      if (!timeline) return;
      const updated: Timeline = {
        ...timeline,
        textOverlays: (timeline.textOverlays ?? []).filter((o) => o.id !== id),
      };
      commitTimeline(updated);
    },

    // ── Undo / Redo ───────────────────────────────────────────────────────────
    undo: () =>
      set((state) => {
        if (state.historyIndex <= 0) return state;
        const newIndex = state.historyIndex - 1;
        return {
          historyIndex: newIndex,
          timeline: deepClone(state.history[newIndex]),
          selectedClipId: null,
        };
      }),

    redo: () =>
      set((state) => {
        if (state.historyIndex >= state.history.length - 1) return state;
        const newIndex = state.historyIndex + 1;
        return {
          historyIndex: newIndex,
          timeline: deepClone(state.history[newIndex]),
          selectedClipId: null,
        };
      }),
  };
});

// ── Selector helpers ──────────────────────────────────────────────────────────

export const selectTimeline       = (s: TimelineStoreState) => s.timeline;
export const selectPlayhead       = (s: TimelineStoreState) => s.playheadPositionMs;
export const selectIsPlaying      = (s: TimelineStoreState) => s.isPlaying;
export const selectZoom           = (s: TimelineStoreState) => s.zoom;
export const selectSelectedClipId = (s: TimelineStoreState) => s.selectedClipId;
export const selectCanUndo        = (s: TimelineStoreState) => s.historyIndex > 0;
export const selectCanRedo        = (s: TimelineStoreState) => s.historyIndex < s.history.length - 1;
export const selectIsDragging     = (s: TimelineStoreState) => s.isDragging;
export const selectIsTrimming     = (s: TimelineStoreState) => s.isTrimming;
export const selectTextOverlays   = (s: TimelineStoreState) => s.timeline?.textOverlays ?? [];
