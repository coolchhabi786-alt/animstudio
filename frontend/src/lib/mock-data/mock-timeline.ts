/**
 * Mock timeline data using real assets from the cartoon automation pipeline.
 *
 * SIMPLIFIED VERSION: Each clip is easily draggable with clear boundaries
 * Video:   8 shots × 5640ms = 45,120ms total video
 * Audio:   3 separate dialogue tracks (no overlap)
 * Music:   1 background track
 * Text:    3 title overlays
 */

export type TrackType      = "video" | "audio" | "music" | "text";
export type TransitionType = "cut" | "fade" | "dissolve" | "slide-left" | "slide-right" | "zoom";

// ── Local types (separate from canonical types for cleaner mock) ──────────────

export interface VideoClip {
  type:         "video";
  id:           string;
  trackId:      string;
  sceneNumber:  number;
  shotIndex:    number;
  clipUrl:      string;
  thumbnailUrl: string;
  startMs:      number;
  durationMs:   number;
  transitionIn: TransitionType;
}

export interface AudioClip {
  type:          "audio";
  id:            string;
  trackId:       string;
  label:         string;
  audioUrl:      string;
  startMs:       number;
  durationMs:    number;
  volumePercent: number;
  fadeInMs:      number;
  fadeOutMs:     number;
}

export interface TextOverlay {
  type:       "text";
  id:         string;
  trackId:    string;
  text:       string;
  startMs:    number;
  durationMs: number;
  fontSize:   number;
  color:      string;
  position:   | "top-left" | "top-center" | "top-right"
              | "center-left" | "center" | "center-right"
              | "bottom-left" | "bottom-center" | "bottom-right";
  animation:  "none" | "fadeIn" | "slideUp" | "slideDown";
}

export type TimelineClip = VideoClip | AudioClip | TextOverlay;

export interface TimelineTrack {
  id:          string;
  trackType:   TrackType;
  label:       string;
  isMuted:     boolean;
  isSolo:      boolean;
  isLocked:    boolean;
  volumePercent?: number;
  autoDuck?:   boolean;
  clips:       TimelineClip[];
}

export interface MockTimeline {
  id:         string;
  episodeId:  string;
  durationMs: number;
  fps:        number;
  tracks:     TimelineTrack[];
  updatedAt:  string;
}

// ── Track IDs ─────────────────────────────────────────────────────────────────

const VIDEO_TRACK_ID    = "track-video-0001-aaaa-bbbb-cccc-dddd";
const AUDIO_TRACK_ID    = "track-audio-0002-aaaa-bbbb-cccc-dddd";
const MUSIC_TRACK_ID    = "track-music-0003-aaaa-bbbb-cccc-dddd";
const TEXT_TRACK_ID     = "track-text--0004-aaaa-bbbb-cccc-dddd";

// ── Asset URL helpers ─────────────────────────────────────────────────────────

function videoUrl(scene: number, shot: number) {
  return `/api/assets/animation/23MarAnimation/scene_0${scene}_shot_0${shot}.mp4`;
}

function audioUrl(scene: number, character: string) {
  return `/api/assets/audio/scene_0${scene}_${character}.mp3`;
}

const THUMBNAIL_MAP: Record<string, string> = {
  "1-1": "scene_01_shot_01_6233dc.png",
  "1-2": "scene_01_shot_02_3b5d67.png",
  "2-1": "scene_02_shot_01_13117c.png",
  "2-2": "scene_02_shot_02_7b60f5.png",
  "2-3": "scene_02_shot_03_1f3279.png",
  "3-1": "scene_03_shot_01_b50a0d.png",
  "3-2": "scene_03_shot_02_480e61.png",
  "3-3": "scene_03_shot_03_96a061.png",
};

function thumbUrl(scene: number, shot: number) {
  const file = THUMBNAIL_MAP[`${scene}-${shot}`];
  return `/api/assets/storyboard/29MarAnimationImages/${file}`;
}

// ── Constants ─────────────────────────────────────────────────────────────────

const SHOT_MS = 5640;  // All shots are exactly 5.64 seconds
const GAP_MS  = 300;   // Gap between shots so clips can be dragged to reorder

// Shot start times with inter-shot gaps
const SHOT_0 = 0;
const SHOT_1 = SHOT_MS + GAP_MS;
const SHOT_2 = (SHOT_MS + GAP_MS) * 2;
const SHOT_3 = (SHOT_MS + GAP_MS) * 3;
const SHOT_4 = (SHOT_MS + GAP_MS) * 4;
const SHOT_5 = (SHOT_MS + GAP_MS) * 5;
const SHOT_6 = (SHOT_MS + GAP_MS) * 6;
const SHOT_7 = (SHOT_MS + GAP_MS) * 7;

const VIDEO_END_MS = (SHOT_MS + GAP_MS) * 8;   // ~47,520ms
const EPISODE_DUR = 60_000;                      // 60s total for credits/padding

// ── Video clips: 8 shots in sequence ───────────────────────────────────────────

const videoClips: VideoClip[] = [
  {
    type: "video", id: "tclip-v-01", trackId: VIDEO_TRACK_ID,
    sceneNumber: 1, shotIndex: 1,
    clipUrl: videoUrl(1, 1), thumbnailUrl: thumbUrl(1, 1),
    startMs: SHOT_0, durationMs: SHOT_MS, transitionIn: "fade",
  },
  {
    type: "video", id: "tclip-v-02", trackId: VIDEO_TRACK_ID,
    sceneNumber: 1, shotIndex: 2,
    clipUrl: videoUrl(1, 2), thumbnailUrl: thumbUrl(1, 2),
    startMs: SHOT_1, durationMs: SHOT_MS, transitionIn: "cut",
  },
  {
    type: "video", id: "tclip-v-03", trackId: VIDEO_TRACK_ID,
    sceneNumber: 2, shotIndex: 1,
    clipUrl: videoUrl(2, 1), thumbnailUrl: thumbUrl(2, 1),
    startMs: SHOT_2, durationMs: SHOT_MS, transitionIn: "dissolve",
  },
  {
    type: "video", id: "tclip-v-04", trackId: VIDEO_TRACK_ID,
    sceneNumber: 2, shotIndex: 2,
    clipUrl: videoUrl(2, 2), thumbnailUrl: thumbUrl(2, 2),
    startMs: SHOT_3, durationMs: SHOT_MS, transitionIn: "cut",
  },
  {
    type: "video", id: "tclip-v-05", trackId: VIDEO_TRACK_ID,
    sceneNumber: 2, shotIndex: 3,
    clipUrl: videoUrl(2, 3), thumbnailUrl: thumbUrl(2, 3),
    startMs: SHOT_4, durationMs: SHOT_MS, transitionIn: "cut",
  },
  {
    type: "video", id: "tclip-v-06", trackId: VIDEO_TRACK_ID,
    sceneNumber: 3, shotIndex: 1,
    clipUrl: videoUrl(3, 1), thumbnailUrl: thumbUrl(3, 1),
    startMs: SHOT_5, durationMs: SHOT_MS, transitionIn: "slide-left",
  },
  {
    type: "video", id: "tclip-v-07", trackId: VIDEO_TRACK_ID,
    sceneNumber: 3, shotIndex: 2,
    clipUrl: videoUrl(3, 2), thumbnailUrl: thumbUrl(3, 2),
    startMs: SHOT_6, durationMs: SHOT_MS, transitionIn: "cut",
  },
  {
    type: "video", id: "tclip-v-08", trackId: VIDEO_TRACK_ID,
    sceneNumber: 3, shotIndex: 3,
    clipUrl: videoUrl(3, 3), thumbnailUrl: thumbUrl(3, 3),
    startMs: SHOT_7, durationMs: SHOT_MS, transitionIn: "fade",
  },
];

// ── Audio file durations (measured via ffprobe — do NOT derive from SHOT_MS) ──
// These are the ACTUAL durations of the MP3 files in the output folder.
// Using video shot lengths as audio durations was the prior bug.
const AUDIO_DURATIONS = {
  scene01MrWhiskers:    1008,  // scene_01_mr._whiskers.mp3
  scene02ProfPaws:      1128,  // scene_02_professor_paws.mp3
  scene03MrWhiskers:    2016,  // scene_03_mr._whiskers.mp3
  scene01DaveTheOwner:  2880,  // scene_01_dave_the_owner.mp3
};

// ── Audio clips: 3 separate dialogue clips ────────────────────────────────────

const audioClips: AudioClip[] = [
  {
    type: "audio", id: "tclip-a-01", trackId: AUDIO_TRACK_ID,
    label: "Scene 1 — Mr. Whiskers",
    audioUrl: audioUrl(1, "mr._whiskers"),
    startMs: 0,
    durationMs: AUDIO_DURATIONS.scene01MrWhiskers,
    volumePercent: 90, fadeInMs: 0, fadeOutMs: 200,
  },
  {
    type: "audio", id: "tclip-a-02", trackId: AUDIO_TRACK_ID,
    label: "Scene 2 — Professor Paws",
    audioUrl: audioUrl(2, "professor_paws"),
    startMs: SHOT_2,
    durationMs: AUDIO_DURATIONS.scene02ProfPaws,
    volumePercent: 90, fadeInMs: 0, fadeOutMs: 200,
  },
  {
    type: "audio", id: "tclip-a-03", trackId: AUDIO_TRACK_ID,
    label: "Scene 3 — Mr. Whiskers",
    audioUrl: audioUrl(3, "mr._whiskers"),
    startMs: SHOT_5,
    durationMs: AUDIO_DURATIONS.scene03MrWhiskers,
    volumePercent: 90, fadeInMs: 0, fadeOutMs: 200,
  },
];

// ── Music: 1 background track ─────────────────────────────────────────────────

const musicClips: AudioClip[] = [
  {
    type: "audio", id: "tclip-m-01", trackId: MUSIC_TRACK_ID,
    label: "Dave The Owner — Ambience",
    audioUrl: audioUrl(1, "dave_the_owner"),
    startMs: 0,
    durationMs: AUDIO_DURATIONS.scene01DaveTheOwner,
    volumePercent: 25, fadeInMs: 500, fadeOutMs: 500,
  },
];

// ── Text overlays ──────────────────────────────────────────────────────────────

const textOverlays: TextOverlay[] = [
  {
    type: "text", id: "tclip-t-01", trackId: TEXT_TRACK_ID,
    text: "The Superpowered Shenanigans\nof Mr. Whiskers",
    startMs: 0, durationMs: 4000,
    fontSize: 42, color: "#ffffff",
    position: "center", animation: "fadeIn",
  },
  {
    type: "text", id: "tclip-t-02", trackId: TEXT_TRACK_ID,
    text: "Scene 2 — The Prank Plan",
    startMs: SHOT_2, durationMs: 2000,
    fontSize: 18, color: "#94a3b8",
    position: "bottom-left", animation: "slideUp",
  },
  {
    type: "text", id: "tclip-t-03", trackId: TEXT_TRACK_ID,
    text: "Scene 3 — Superpowered Shenanigans",
    startMs: SHOT_5, durationMs: 2000,
    fontSize: 18, color: "#94a3b8",
    position: "bottom-left", animation: "slideUp",
  },
];

// ── Export the mock timeline ───────────────────────────────────────────────────

export const mockTimeline: MockTimeline = {
  id:        "timeline-0001-aaaa-bbbb-cccc-ddddeeeeffff",
  episodeId: "mock-ep-001",  // ← MATCHES the sidebar route!
  durationMs: EPISODE_DUR,
  fps: 24,
  updatedAt: new Date().toISOString(),
  tracks: [
    {
      id: VIDEO_TRACK_ID, trackType: "video", label: "Video",
      isMuted: false, isSolo: false, isLocked: false,
      clips: videoClips,
    },
    {
      id: AUDIO_TRACK_ID, trackType: "audio", label: "Dialogue",
      isMuted: false, isSolo: false, isLocked: false,
      clips: audioClips,  // ← TRY TO DRAG THESE
    },
    {
      id: MUSIC_TRACK_ID, trackType: "music", label: "Music / Ambience",
      isMuted: false, isSolo: false, isLocked: false,
      volumePercent: 25, autoDuck: true,
      clips: musicClips,
    },
    {
      id: TEXT_TRACK_ID, trackType: "text", label: "Text Overlays",
      isMuted: false, isSolo: false, isLocked: false,
      clips: textOverlays,
    },
  ],
};
