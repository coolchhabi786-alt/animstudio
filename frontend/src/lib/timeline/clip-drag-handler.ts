import type { TimelineClip } from "@/types/timeline";
import type { TimelineTrack } from "@/types/timeline";
import { timelineUtils } from "@/lib/timeline-utils";

export const DRAG_GRID_MS = 100; // snap resolution

export const clipDragHandler = {
  /** Snap ms value to the nearest grid step. */
  snapToGrid(ms: number, gridMs: number = DRAG_GRID_MS): number {
    return Math.round(ms / gridMs) * gridMs;
  },

  /**
   * Convert an absolute-pixel x position (post-drag) into a snapped ms value,
   * constrained to [0, timelineDurationMs - clip.durationMs].
   */
  calculateNewStartMs(
    absX: number,
    zoom: number,
    clipDurationMs: number,
    timelineDurationMs: number
  ): number {
    const raw = timelineUtils.pixelsToMs(absX, zoom);
    const snapped = this.snapToGrid(Math.max(0, raw));
    const max = Math.max(0, timelineDurationMs - clipDurationMs);
    return Math.min(snapped, max);
  },

  /**
   * Returns true when `clip` overlaps any clip in `otherClips`.
   * An optional `bufferMs` shrinks each clip by that amount on both ends
   * (use a small value like 1 to avoid false positives on exact-adjacent clips).
   */
  detectClipOverlap(
    clip: TimelineClip,
    otherClips: TimelineClip[],
    bufferMs = 1
  ): boolean {
    const clipStart = clip.startMs + bufferMs;
    const clipEnd   = clip.startMs + clip.durationMs - bufferMs;
    return otherClips.some((other) => {
      if (other.id === clip.id) return false;
      const otherEnd = other.startMs + other.durationMs;
      return clipStart < otherEnd && clipEnd > other.startMs;
    });
  },

  /**
   * Constrain clip position so it stays within the timeline and has a snapped
   * start. Does NOT check overlap — use detectClipOverlap separately.
   */
  constrainClipPosition(
    clip: TimelineClip,
    timelineDurationMs: number
  ): TimelineClip {
    const snapped = this.snapToGrid(Math.max(0, clip.startMs));
    const maxStart = Math.max(0, timelineDurationMs - clip.durationMs);
    return { ...clip, startMs: Math.min(snapped, maxStart) };
  },

  /**
   * Return a list of free time windows in a track (gaps between existing clips),
   * excluding the clip identified by `excludeClipId`.
   */
  getAvailableSpaces(
    track: TimelineTrack,
    excludeClipId: string
  ): Array<{ startMs: number; endMs: number }> {
    const sorted = track.clips
      .filter((c) => c.id !== excludeClipId)
      .sort((a, b) => a.startMs - b.startMs);

    const spaces: Array<{ startMs: number; endMs: number }> = [];
    let cursor = 0;

    for (const c of sorted) {
      if (c.startMs > cursor) {
        spaces.push({ startMs: cursor, endMs: c.startMs });
      }
      cursor = Math.max(cursor, c.startMs + c.durationMs);
    }

    spaces.push({ startMs: cursor, endMs: Infinity });
    return spaces;
  },
} as const;
