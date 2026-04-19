/** Mock animation clip data for Phase 8 UI testing. 12 clips matching the 12 storyboard shots. */

export type AnimationStatus = 'queued' | 'processing' | 'ready' | 'failed'
export type AnimationBackend = 'kling' | 'local'

export interface MockAnimationClip {
  id: string
  episodeId: string
  sceneNumber: number
  shotIndex: number
  /** Public MP4 for preview. Using Big Buck Bunny segments as stable placeholder. */
  clipUrl: string
  durationSeconds: number
  status: AnimationStatus
  /** Cost in USD. Kling AI charges $0.056 per clip. */
  costUsd: number
  backend: AnimationBackend
  createdAt: string
  updatedAt: string
}

/** Cost per clip for each backend */
export const ANIMATION_COST_PER_CLIP: Record<AnimationBackend, number> = {
  kling: 0.056,
  local: 0,
}

const BBB_BASE = 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4'

export const mockAnimationClips: MockAnimationClip[] = [
  // Scene 1
  {
    id: 'clip-s1-01-aaaa-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 1,
    shotIndex: 1,
    clipUrl: BBB_BASE,
    durationSeconds: 6,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:00:00.000Z',
    updatedAt: '2026-04-15T08:02:30.000Z',
  },
  {
    id: 'clip-s1-02-aaaa-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 1,
    shotIndex: 2,
    clipUrl: BBB_BASE,
    durationSeconds: 5,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:03:00.000Z',
    updatedAt: '2026-04-15T08:05:10.000Z',
  },
  {
    id: 'clip-s1-03-aaaa-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 1,
    shotIndex: 3,
    clipUrl: BBB_BASE,
    durationSeconds: 7,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:06:00.000Z',
    updatedAt: '2026-04-15T08:08:45.000Z',
  },
  {
    id: 'clip-s1-04-aaaa-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 1,
    shotIndex: 4,
    clipUrl: BBB_BASE,
    durationSeconds: 8,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:09:00.000Z',
    updatedAt: '2026-04-15T08:12:00.000Z',
  },
  // Scene 2
  {
    id: 'clip-s2-01-bbbb-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 2,
    shotIndex: 1,
    clipUrl: BBB_BASE,
    durationSeconds: 6,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:13:00.000Z',
    updatedAt: '2026-04-15T08:15:30.000Z',
  },
  {
    id: 'clip-s2-02-bbbb-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 2,
    shotIndex: 2,
    clipUrl: BBB_BASE,
    durationSeconds: 5,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:16:00.000Z',
    updatedAt: '2026-04-15T08:18:10.000Z',
  },
  {
    id: 'clip-s2-03-bbbb-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 2,
    shotIndex: 3,
    clipUrl: BBB_BASE,
    durationSeconds: 8,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:19:00.000Z',
    updatedAt: '2026-04-15T08:22:00.000Z',
  },
  {
    id: 'clip-s2-04-bbbb-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 2,
    shotIndex: 4,
    clipUrl: BBB_BASE,
    durationSeconds: 6,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:23:00.000Z',
    updatedAt: '2026-04-15T08:25:30.000Z',
  },
  // Scene 3
  {
    id: 'clip-s3-01-cccc-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 3,
    shotIndex: 1,
    clipUrl: BBB_BASE,
    durationSeconds: 7,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:26:00.000Z',
    updatedAt: '2026-04-15T08:29:00.000Z',
  },
  {
    id: 'clip-s3-02-cccc-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 3,
    shotIndex: 2,
    clipUrl: BBB_BASE,
    durationSeconds: 5,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:30:00.000Z',
    updatedAt: '2026-04-15T08:32:10.000Z',
  },
  {
    id: 'clip-s3-03-cccc-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 3,
    shotIndex: 3,
    clipUrl: BBB_BASE,
    durationSeconds: 6,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:33:00.000Z',
    updatedAt: '2026-04-15T08:35:30.000Z',
  },
  {
    id: 'clip-s3-04-cccc-1111-2222-3333-44445555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    sceneNumber: 3,
    shotIndex: 4,
    clipUrl: BBB_BASE,
    durationSeconds: 5,
    status: 'ready',
    costUsd: 0.056,
    backend: 'kling',
    createdAt: '2026-04-15T08:36:00.000Z',
    updatedAt: '2026-04-15T08:38:10.000Z',
  },
]

/** Total cost for all 12 clips at Kling rate */
export const mockAnimationTotalCost = mockAnimationClips.reduce((sum, c) => sum + c.costUsd, 0)

/** Total duration in seconds across all clips */
export const mockAnimationTotalDuration = mockAnimationClips.reduce((sum, c) => sum + c.durationSeconds, 0)
