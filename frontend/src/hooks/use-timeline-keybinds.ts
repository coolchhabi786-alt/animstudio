"use client";

import { useEffect } from "react";
import { useTimelineStore } from "@/stores/timelineStore";

/**
 * Registers global keyboard shortcuts for the timeline editor.
 * Must be called once inside the timeline page component.
 */
export function useTimelineKeybinds() {
  const undo           = useTimelineStore((s) => s.undo);
  const redo           = useTimelineStore((s) => s.redo);
  const togglePlayback = useTimelineStore((s) => s.togglePlayback);
  const selectClip     = useTimelineStore((s) => s.selectClip);
  const selectedClipId = useTimelineStore((s) => s.selectedClipId);

  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      // Never fire inside form fields
      const tag = (e.target as HTMLElement)?.tagName?.toUpperCase();
      if (tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT") return;

      if (e.ctrlKey && !e.shiftKey && e.key === "z") {
        e.preventDefault();
        undo();
      } else if ((e.ctrlKey && e.shiftKey && e.key === "z") || (e.ctrlKey && e.key === "y")) {
        e.preventDefault();
        redo();
      } else if (e.key === " ") {
        e.preventDefault();
        togglePlayback();
      } else if (e.key === "Escape" && selectedClipId) {
        selectClip(null);
      }
    }

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [undo, redo, togglePlayback, selectClip, selectedClipId]);
}
