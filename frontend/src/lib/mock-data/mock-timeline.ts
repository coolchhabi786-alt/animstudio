/**
 * Mock timeline data for Phase 10 UI testing.
 *
 * Total duration: 180,000ms (3 minutes)
 * Tracks: VideoTrack (12 clips), AudioTrack (1 dialogue), MusicTrack (1 bg music), TextTrack (3 overlays)
 *
 * Video clip timing is sequential with no gaps:
 * Clip durations (seconds): 6,5,7,8, 6,5,8,6, 7,5,6,5 = 74s total
 * Remaining ~106s is silent/black padding after video ends (for full 3-min episode with VO).
 */

export type TrackType = 'video' | 'audio' | 'music' | 'text'
export type TransitionType = 'cut' | 'fade' | 'dissolve' | 'slide-left' | 'slide-right' | 'zoom'

export interface VideoClip {
  type: 'video'
  id: string
  trackId: string
  /** Shot reference for labeling */
  sceneNumber: number
  shotIndex: number
  clipUrl: string
  startMs: number
  durationMs: number
  /** Transition INTO this clip from the previous */
  transitionIn: TransitionType
}

export interface AudioClip {
  type: 'audio'
  id: string
  trackId: string
  label: string
  audioUrl: string
  startMs: number
  durationMs: number
  volumePercent: number
  /** Fade in duration in ms */
  fadeInMs: number
  /** Fade out duration in ms */
  fadeOutMs: number
}

export interface TextOverlay {
  type: 'text'
  id: string
  trackId: string
  text: string
  startMs: number
  durationMs: number
  fontSize: number
  color: string
  /** Position on 9-point grid: 'top-left' | 'top-center' | 'top-right' | 'center' | etc. */
  position: 'top-left' | 'top-center' | 'top-right' | 'center-left' | 'center' | 'center-right' | 'bottom-left' | 'bottom-center' | 'bottom-right'
  animation: 'none' | 'fadeIn' | 'slideUp' | 'slideDown'
}

export type TimelineClip = VideoClip | AudioClip | TextOverlay

export interface TimelineTrack {
  id: string
  trackType: TrackType
  label: string
  isMuted: boolean
  isSolo: boolean
  isLocked: boolean
  clips: TimelineClip[]
}

export interface MockTimeline {
  id: string
  episodeId: string
  /** Total episode duration in milliseconds */
  durationMs: number
  /** Frames per second for playback */
  fps: number
  tracks: TimelineTrack[]
  updatedAt: string
}

const VIDEO_TRACK_ID = 'track-video-0001-aaaa-bbbb-cccc-dddd'
const AUDIO_TRACK_ID = 'track-audio-0002-aaaa-bbbb-cccc-dddd'
const MUSIC_TRACK_ID = 'track-music-0003-aaaa-bbbb-cccc-dddd'
const TEXT_TRACK_ID  = 'track-text--0004-aaaa-bbbb-cccc-dddd'

const BBB_BASE = 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4'

/** Sequential start times (ms) for the 12 video clips, no overlaps. */
const clipDurationsMs = [6000, 5000, 7000, 8000, 6000, 5000, 8000, 6000, 7000, 5000, 6000, 5000]
const clipStartsMs: number[] = []
clipDurationsMs.reduce((acc, d, i) => { clipStartsMs[i] = acc; return acc + d }, 0)

const videoClips: VideoClip[] = [
  // Scene 1
  { type: 'video', id: 'tclip-v-01', trackId: VIDEO_TRACK_ID, sceneNumber: 1, shotIndex: 1, clipUrl: BBB_BASE, startMs: clipStartsMs[0],  durationMs: clipDurationsMs[0],  transitionIn: 'fade' },
  { type: 'video', id: 'tclip-v-02', trackId: VIDEO_TRACK_ID, sceneNumber: 1, shotIndex: 2, clipUrl: BBB_BASE, startMs: clipStartsMs[1],  durationMs: clipDurationsMs[1],  transitionIn: 'cut' },
  { type: 'video', id: 'tclip-v-03', trackId: VIDEO_TRACK_ID, sceneNumber: 1, shotIndex: 3, clipUrl: BBB_BASE, startMs: clipStartsMs[2],  durationMs: clipDurationsMs[2],  transitionIn: 'cut' },
  { type: 'video', id: 'tclip-v-04', trackId: VIDEO_TRACK_ID, sceneNumber: 1, shotIndex: 4, clipUrl: BBB_BASE, startMs: clipStartsMs[3],  durationMs: clipDurationsMs[3],  transitionIn: 'cut' },
  // Scene 2
  { type: 'video', id: 'tclip-v-05', trackId: VIDEO_TRACK_ID, sceneNumber: 2, shotIndex: 1, clipUrl: BBB_BASE, startMs: clipStartsMs[4],  durationMs: clipDurationsMs[4],  transitionIn: 'dissolve' },
  { type: 'video', id: 'tclip-v-06', trackId: VIDEO_TRACK_ID, sceneNumber: 2, shotIndex: 2, clipUrl: BBB_BASE, startMs: clipStartsMs[5],  durationMs: clipDurationsMs[5],  transitionIn: 'cut' },
  { type: 'video', id: 'tclip-v-07', trackId: VIDEO_TRACK_ID, sceneNumber: 2, shotIndex: 3, clipUrl: BBB_BASE, startMs: clipStartsMs[6],  durationMs: clipDurationsMs[6],  transitionIn: 'cut' },
  { type: 'video', id: 'tclip-v-08', trackId: VIDEO_TRACK_ID, sceneNumber: 2, shotIndex: 4, clipUrl: BBB_BASE, startMs: clipStartsMs[7],  durationMs: clipDurationsMs[7],  transitionIn: 'cut' },
  // Scene 3
  { type: 'video', id: 'tclip-v-09', trackId: VIDEO_TRACK_ID, sceneNumber: 3, shotIndex: 1, clipUrl: BBB_BASE, startMs: clipStartsMs[8],  durationMs: clipDurationsMs[8],  transitionIn: 'slide-left' },
  { type: 'video', id: 'tclip-v-10', trackId: VIDEO_TRACK_ID, sceneNumber: 3, shotIndex: 2, clipUrl: BBB_BASE, startMs: clipStartsMs[9],  durationMs: clipDurationsMs[9],  transitionIn: 'cut' },
  { type: 'video', id: 'tclip-v-11', trackId: VIDEO_TRACK_ID, sceneNumber: 3, shotIndex: 3, clipUrl: BBB_BASE, startMs: clipStartsMs[10], durationMs: clipDurationsMs[10], transitionIn: 'cut' },
  { type: 'video', id: 'tclip-v-12', trackId: VIDEO_TRACK_ID, sceneNumber: 3, shotIndex: 4, clipUrl: BBB_BASE, startMs: clipStartsMs[11], durationMs: clipDurationsMs[11], transitionIn: 'fade' },
]

const audioClips: AudioClip[] = [
  {
    type: 'audio',
    id: 'tclip-a-01',
    trackId: AUDIO_TRACK_ID,
    label: 'Dialogue - Full Episode',
    audioUrl: 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3',
    startMs: 0,
    durationMs: 180000,
    volumePercent: 90,
    fadeInMs: 0,
    fadeOutMs: 1000,
  },
]

const musicClips: AudioClip[] = [
  {
    type: 'audio',
    id: 'tclip-m-01',
    trackId: MUSIC_TRACK_ID,
    label: 'Background Score - Neon City',
    audioUrl: 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3',
    startMs: 0,
    durationMs: 180000,
    volumePercent: 30,
    fadeInMs: 2000,
    fadeOutMs: 3000,
  },
]

const textOverlays: TextOverlay[] = [
  {
    type: 'text',
    id: 'tclip-t-01',
    trackId: TEXT_TRACK_ID,
    text: 'NEON CITY\nEpisode 1 — "The Signal"',
    startMs: 0,
    durationMs: 4000,
    fontSize: 48,
    color: '#ffffff',
    position: 'center',
    animation: 'fadeIn',
  },
  {
    type: 'text',
    id: 'tclip-t-02',
    trackId: TEXT_TRACK_ID,
    text: 'Scene 2 — Research Lab',
    startMs: 26000,
    durationMs: 2500,
    fontSize: 20,
    color: '#94a3b8',
    position: 'bottom-left',
    animation: 'slideUp',
  },
  {
    type: 'text',
    id: 'tclip-t-03',
    trackId: TEXT_TRACK_ID,
    text: 'Scene 3 — The Pursuit',
    startMs: 58000,
    durationMs: 2500,
    fontSize: 20,
    color: '#94a3b8',
    position: 'bottom-left',
    animation: 'slideUp',
  },
]

export const mockTimeline: MockTimeline = {
  id: 'timeline-0001-aaaa-bbbb-cccc-ddddeeeeffff',
  episodeId: 'ep-0011-2222-3333-4444-555566667777',
  durationMs: 180000,
  fps: 24,
  updatedAt: '2026-04-19T10:00:00.000Z',
  tracks: [
    {
      id: VIDEO_TRACK_ID,
      trackType: 'video',
      label: 'Video',
      isMuted: false,
      isSolo: false,
      isLocked: false,
      clips: videoClips,
    },
    {
      id: AUDIO_TRACK_ID,
      trackType: 'audio',
      label: 'Dialogue',
      isMuted: false,
      isSolo: false,
      isLocked: false,
      clips: audioClips,
    },
    {
      id: MUSIC_TRACK_ID,
      trackType: 'music',
      label: 'Music',
      isMuted: false,
      isSolo: false,
      isLocked: false,
      clips: musicClips,
    },
    {
      id: TEXT_TRACK_ID,
      trackType: 'text',
      label: 'Text Overlays',
      isMuted: false,
      isSolo: false,
      isLocked: false,
      clips: textOverlays,
    },
  ],
}
