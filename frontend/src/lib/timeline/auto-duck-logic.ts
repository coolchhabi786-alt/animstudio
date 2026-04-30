import type { AudioClip } from "@/types/timeline";

/**
 * Auto-duck logic: temporarily reduce music volume when a dialogue clip is playing.
 *
 * When any dialogue audio clip is active at `playheadMs`, the music volume is
 * reduced to `duckPercent`% of the track master volume. A 300ms transition ramp
 * is simulated by returning the effective volume for a given playhead position.
 */
export const autoDuckLogic = {
  /**
   * Returns the effective music volume (0–100) for the given playhead position.
   * Applies 300ms fade-in/out around each dialogue clip boundary.
   */
  applyAutoDuck(
    musicVolumePercent: number,
    audioClips: AudioClip[],
    playheadMs: number,
    duckPercent = 40,
    fadeMs = 300
  ): number {
    for (const clip of audioClips) {
      const start = clip.startMs;
      const end   = clip.startMs + clip.durationMs;

      if (playheadMs < start - fadeMs || playheadMs > end + fadeMs) continue;

      // Fade in: start-fadeMs → start
      if (playheadMs >= start - fadeMs && playheadMs < start) {
        const t = (playheadMs - (start - fadeMs)) / fadeMs; // 0→1
        return musicVolumePercent - t * (musicVolumePercent - duckPercent);
      }
      // Duck plateau: start → end
      if (playheadMs >= start && playheadMs <= end) {
        return duckPercent;
      }
      // Fade out: end → end+fadeMs
      if (playheadMs > end && playheadMs <= end + fadeMs) {
        const t = (playheadMs - end) / fadeMs; // 0→1
        return duckPercent + t * (musicVolumePercent - duckPercent);
      }
    }
    return musicVolumePercent;
  },

  /** True when any audio clip overlaps the given time range. */
  hasOverlap(audioClips: AudioClip[], startMs: number, endMs: number): boolean {
    return audioClips.some(
      (c) => c.startMs < endMs && c.startMs + c.durationMs > startMs
    );
  },
};
