import type { Timeline, TimelineTrack, TimelineClip, VideoClip } from "@/types/timeline";

const PIXELS_PER_SECOND_BASE = 100; // px/s at zoom=1

export const timelineUtils = {
  // ── Unit conversions ──────────────────────────────────────────────────────

  msToSeconds(ms: number): number {
    return ms / 1000;
  },

  secondsToMs(seconds: number): number {
    return seconds * 1000;
  },

  msToFrame(ms: number, fps: number): number {
    return Math.round((ms / 1000) * fps);
  },

  frameToMs(frame: number, fps: number): number {
    return Math.round((frame / fps) * 1000);
  },

  // ── Pixel ↔ ms (zoom-aware) ───────────────────────────────────────────────

  /** Convert canvas pixels to milliseconds given current zoom level. */
  pixelsToMs(
    pixels: number,
    zoom: number,
    pixelsPerSecond: number = PIXELS_PER_SECOND_BASE
  ): number {
    return (pixels / (pixelsPerSecond * zoom)) * 1000;
  },

  /** Convert milliseconds to canvas pixels given current zoom level. */
  msToPixels(
    ms: number,
    zoom: number,
    pixelsPerSecond: number = PIXELS_PER_SECOND_BASE
  ): number {
    return (ms / 1000) * pixelsPerSecond * zoom;
  },

  // ── Clip operations (return new clip, never mutate) ───────────────────────

  /** Shift a clip to a new absolute timeline position. Duration is unchanged. */
  moveClip(clip: TimelineClip, newStartMs: number): TimelineClip {
    return { ...clip, startMs: Math.max(0, newStartMs) };
  },

  /**
   * Trim a VideoClip by adjusting its start position and duration.
   * trimDeltaStartMs > 0 → trims from the left (clip starts later, shorter).
   * newDurationMs     > 0 → sets final duration directly.
   */
  trimClip(
    clip: VideoClip,
    trimDeltaStartMs: number,
    newDurationMs: number
  ): VideoClip {
    const safeStart = Math.max(0, clip.startMs + trimDeltaStartMs);
    const safeDuration = Math.max(500, newDurationMs); // floor 0.5 s
    return { ...clip, startMs: safeStart, durationMs: safeDuration };
  },

  /** Resize a clip by setting its end position (right-trim handle). */
  resizeClip(clip: TimelineClip, newEndMs: number): TimelineClip {
    const minEnd = clip.startMs + 500; // minimum 0.5 s
    return {
      ...clip,
      durationMs: Math.max(minEnd, newEndMs) - clip.startMs,
    };
  },

  // ── Validation helpers ────────────────────────────────────────────────────

  /** Returns true when two clips overlap (with optional tolerance in ms). */
  isClipOverlapping(
    clip1: TimelineClip,
    clip2: TimelineClip,
    toleranceMs = 0
  ): boolean {
    if (clip1.id === clip2.id) return false;
    const aEnd = clip1.startMs + clip1.durationMs - toleranceMs;
    const bEnd = clip2.startMs + clip2.durationMs - toleranceMs;
    return clip1.startMs < bEnd && aEnd > clip2.startMs;
  },

  /**
   * Returns true when `clip` can be placed on `track` without overlapping
   * any existing clip (the clip itself is excluded from the check).
   */
  canPlaceClip(track: TimelineTrack, clip: TimelineClip): boolean {
    return !track.clips.some(
      (existing) =>
        existing.id !== clip.id &&
        timelineUtils.isClipOverlapping(existing, clip)
    );
  },

  /** Validate a full timeline and return an array of human-readable error messages. */
  validateTimeline(timeline: Timeline): string[] {
    const errors: string[] = [];

    if (timeline.durationMs <= 0) {
      errors.push("Timeline duration must be greater than 0.");
    }

    if (![24, 30, 60].includes(timeline.fps)) {
      errors.push(`Unsupported fps: ${timeline.fps}. Expected 24, 30, or 60.`);
    }

    for (const track of timeline.tracks) {
      const clips = [...track.clips].sort((a, b) => a.startMs - b.startMs);

      for (let i = 0; i < clips.length; i++) {
        const clip = clips[i];

        if (clip.durationMs <= 0) {
          errors.push(`Clip ${clip.id} on track "${track.label}" has zero or negative duration.`);
        }

        if (clip.startMs + clip.durationMs > timeline.durationMs) {
          errors.push(
            `Clip ${clip.id} on track "${track.label}" extends beyond the timeline end.`
          );
        }

        // Check overlap with next clip
        if (i < clips.length - 1) {
          const next = clips[i + 1];
          if (timelineUtils.isClipOverlapping(clip, next)) {
            errors.push(
              `Clips ${clip.id} and ${next.id} overlap on track "${track.label}".`
            );
          }
        }
      }
    }

    return errors;
  },

  // ── Formatting helpers ────────────────────────────────────────────────────

  /** Format milliseconds as MM:SS */
  formatMs(ms: number): string {
    const totalSec = Math.floor(ms / 1000);
    const m = Math.floor(totalSec / 60);
    const s = totalSec % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
  },

  /** Format milliseconds as MM:SS:FF (with frame count) */
  formatMsWithFrames(ms: number, fps: number): string {
    const totalFrames = Math.round((ms / 1000) * fps);
    const f = totalFrames % fps;
    const totalSec = Math.floor(totalFrames / fps);
    const m = Math.floor(totalSec / 60);
    const s = totalSec % 60;
    return `${m}:${s.toString().padStart(2, "0")}:${f.toString().padStart(2, "0")}`;
  },
} as const;
