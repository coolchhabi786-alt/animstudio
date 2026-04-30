/**
 * Canonical timeline types for Phase 10.
 * Aligned with src/lib/mock-data/mock-timeline.ts so the Zustand store
 * can load mock data without adaptation.
 */

// ── Enums ────────────────────────────────────────────────────────────────────

export enum TrackType {
  Video = "video",
  Audio = "audio",
  Music = "music",
  Text  = "text",
}

export enum TransitionType {
  Cut       = "cut",
  Fade      = "fade",
  Dissolve  = "dissolve",
  SlideLeft = "slide-left",
  SlideRight= "slide-right",
  Zoom      = "zoom",
}

export enum TextAnimation {
  None      = "none",
  FadeIn    = "fadeIn",
  SlideUp   = "slideUp",
  SlideDown = "slideDown",
}

export type TextPosition =
  | "top-left" | "top-center" | "top-right"
  | "center-left" | "center" | "center-right"
  | "bottom-left" | "bottom-center" | "bottom-right";

// ── Clip variants (discriminated union) ──────────────────────────────────────

export interface VideoClip {
  type: "video";
  id: string;
  trackId: string;
  sceneNumber: number;
  shotIndex: number;
  clipUrl: string;
  thumbnailUrl?: string; // storyboard image shown in timeline strip
  startMs: number;
  durationMs: number;
  transitionIn: TransitionType;
}

export interface AudioClip {
  type: "audio";
  id: string;
  trackId: string;
  label: string;
  audioUrl: string;
  startMs: number;
  durationMs: number;
  volumePercent: number;
  fadeInMs: number;
  fadeOutMs: number;
}

export interface TextClip {
  type: "text";
  id: string;
  trackId: string;
  text: string;
  startMs: number;
  durationMs: number;
  fontSize: number;
  color: string;
  position: TextPosition;
  animation: TextAnimation;
}

/** Union of all clip variants. Every clip has `id`, `trackId`, `startMs`, `durationMs`. */
export type TimelineClip = VideoClip | AudioClip | TextClip;

// ── Track & Timeline ─────────────────────────────────────────────────────────

export interface TextOverlay {
  id: string;
  episodeId: string;
  text: string;
  fontSizePixels: number;
  color: string;
  positionX: number; // 0–100 percent of video width
  positionY: number; // 0–100 percent of video height
  startMs: number;
  durationMs: number;
  animation: TextAnimation;
  zIndex: number;
}

export interface TimelineTrack {
  id: string;
  trackType: TrackType;
  label: string;
  isMuted: boolean;
  isSolo: boolean;
  isLocked: boolean;
  clips: TimelineClip[];
  volumePercent?: number; // master volume for audio/music tracks (0–100)
  autoDuck?: boolean;     // music tracks only — reduce volume during dialogue
}

export interface Timeline {
  id: string;
  episodeId: string;
  durationMs: number;
  fps: number;
  tracks: TimelineTrack[];
  textOverlays: TextOverlay[];
  updatedAt: string;
}
