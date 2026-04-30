"use client";

import { useEffect, useRef } from "react";
import { useTimelineStore } from "@/stores/timelineStore";
import { mockTimeline } from "@/lib/mock-data/mock-timeline";
import type { Timeline, TimelineClip } from "@/types/timeline";
import type { MockTimeline } from "@/lib/mock-data/mock-timeline";

/**
 * Adapt MockTimeline → canonical Timeline so the store can accept it.
 * The shapes are identical in practice; this cast makes TypeScript happy
 * without duplicating the mock data.
 */
function adaptMockTimeline(mock: MockTimeline): Timeline {
  return mock as unknown as Timeline;
}

/**
 * useTimelineMock
 *
 * Loads mock timeline data into the Zustand store on mount and drives
 * the playback loop (~33 fps) when isPlaying is true.
 *
 * Returns the full store state + all actions so callers don't need to
 * import useTimelineStore directly.
 */
export function useTimelineMock() {
  const store = useTimelineStore();
  const rafRef = useRef<number | null>(null);
  const lastTickRef = useRef<number>(0);

  // Load mock data once on mount
  useEffect(() => {
    store.loadTimeline(adaptMockTimeline(mockTimeline));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Playback loop using requestAnimationFrame for smooth 60-fps updates
  useEffect(() => {
    if (!store.isPlaying) {
      if (rafRef.current !== null) {
        cancelAnimationFrame(rafRef.current);
        rafRef.current = null;
      }
      return;
    }

    function tick(now: number) {
      const delta = lastTickRef.current === 0 ? 16 : now - lastTickRef.current;
      lastTickRef.current = now;
      store.advancePlayhead(Math.min(delta, 100)); // cap at 100ms to avoid jumps
      rafRef.current = requestAnimationFrame(tick);
    }

    lastTickRef.current = 0;
    rafRef.current = requestAnimationFrame(tick);

    return () => {
      if (rafRef.current !== null) cancelAnimationFrame(rafRef.current);
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [store.isPlaying]);

  return {
    // Data
    timeline:           store.timeline,
    selectedClipId:     store.selectedClipId,
    selectedTrackId:    store.selectedTrackId,
    playheadPositionMs: store.playheadPositionMs,
    isPlaying:          store.isPlaying,
    zoom:               store.zoom,
    historyIndex:       store.historyIndex,
    historyLength:      store.history.length,

    // Computed helpers
    canUndo: store.historyIndex > 0,
    canRedo: store.historyIndex < store.history.length - 1,

    // Actions
    selectClip:          store.selectClip,
    selectTrack:         store.selectTrack,
    setPlayheadPosition: store.setPlayheadPosition,
    play:                store.play,
    pause:               store.pause,
    togglePlayback:      store.togglePlayback,
    setZoom:             store.setZoom,
    zoomIn:              store.zoomIn,
    zoomOut:             store.zoomOut,
    moveClip:            store.moveClip,
    trimClip:            store.trimClip,
    setTransition:       store.setTransition,
    toggleTrackMute:     store.toggleTrackMute,
    toggleTrackLock:     store.toggleTrackLock,
    setTrackVolume:      store.setTrackVolume,
    toggleTrackAutoDuck: store.toggleTrackAutoDuck,
    addMusicClip:        store.addMusicClip,
    addTextOverlay:      store.addTextOverlay,
    updateTextOverlay:   store.updateTextOverlay,
    deleteTextOverlay:   store.deleteTextOverlay,
    undo:                store.undo,
    redo:                store.redo,
  };
}

// Re-export types useful to consumers
export type { Timeline, TimelineClip };
