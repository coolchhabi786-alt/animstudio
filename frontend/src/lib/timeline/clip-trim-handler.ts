import type { TimelineClip } from "@/types/timeline";
import { timelineUtils } from "@/lib/timeline-utils";
import { DRAG_GRID_MS } from "./clip-drag-handler";

export const MIN_CLIP_DURATION_MS = 500;

export const clipTrimHandler = {
  /**
   * Compute the new { startMs, durationMs } after dragging the LEFT trim handle.
   *
   * @param handleDeltaX  Pixels the handle moved rightward (+) or leftward (-).
   * @param zoom          Current zoom level.
   * @param clip          The clip being trimmed (original values).
   */
  calculateTrimStart(
    handleDeltaX: number,
    zoom: number,
    clip: TimelineClip
  ): { startMs: number; durationMs: number } {
    const rawDeltaMs   = timelineUtils.pixelsToMs(handleDeltaX, zoom);
    const snappedDelta = this.snapToGrid(rawDeltaMs);
    const originalEnd  = clip.startMs + clip.durationMs;

    // New start must stay within [0, originalEnd - MIN]
    const rawStart    = clip.startMs + snappedDelta;
    const clampedStart = Math.max(0, Math.min(rawStart, originalEnd - MIN_CLIP_DURATION_MS));
    const newDuration  = originalEnd - clampedStart;

    return { startMs: clampedStart, durationMs: newDuration };
  },

  /**
   * Compute the new durationMs after dragging the RIGHT trim handle.
   *
   * @param handleDeltaX     Pixels the handle moved right (+) or left (-).
   * @param zoom             Current zoom level.
   * @param clip             The clip being trimmed (original values).
   * @param timelineDurationMs  Upper bound for end time.
   */
  calculateTrimEnd(
    handleDeltaX: number,
    zoom: number,
    clip: TimelineClip,
    timelineDurationMs: number
  ): number {
    const rawDeltaMs    = timelineUtils.pixelsToMs(handleDeltaX, zoom);
    const snappedDelta  = this.snapToGrid(rawDeltaMs);
    const rawDuration   = clip.durationMs + snappedDelta;
    const maxDuration   = timelineDurationMs - clip.startMs;
    return Math.max(MIN_CLIP_DURATION_MS, Math.min(rawDuration, maxDuration));
  },

  /**
   * Returns true when the proposed trim values are valid (no negative durations,
   * stays within the timeline).
   */
  validateTrimRange(
    newStartMs: number,
    newDurationMs: number,
    timelineDurationMs: number
  ): boolean {
    if (newStartMs < 0) return false;
    if (newDurationMs < MIN_CLIP_DURATION_MS) return false;
    if (newStartMs + newDurationMs > timelineDurationMs) return false;
    return true;
  },

  /**
   * Enforce minimum clip length after a left-trim operation.
   * Returns the safe (startMs, durationMs) pair.
   */
  enforceMinimumClipLength(
    originalEndMs: number,
    proposedStartMs: number
  ): { startMs: number; durationMs: number } {
    const maxStart = originalEndMs - MIN_CLIP_DURATION_MS;
    const safeStart = Math.min(Math.max(0, proposedStartMs), maxStart);
    return {
      startMs: safeStart,
      durationMs: originalEndMs - safeStart,
    };
  },

  snapToGrid(ms: number, gridMs: number = DRAG_GRID_MS): number {
    return Math.round(ms / gridMs) * gridMs;
  },
} as const;
