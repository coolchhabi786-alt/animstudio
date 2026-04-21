/** Mock render data for Phase 9 UI testing. */

export type RenderStatus = 'queued' | 'assembling' | 'mixing' | 'complete' | 'failed'
export type AspectRatio = '16:9' | '9:16' | '1:1' | '4:3' | '21:9'
export type OutputFormat = 'mp4' | 'webm' | 'prores'
export type Resolution = '1080p' | '2k' | '4k'

export interface MockRender {
  id: string
  episodeId: string
  status: RenderStatus
  aspectRatio: AspectRatio
  outputFormat: OutputFormat
  resolution: Resolution
  /** Served from Next.js public directory. Null if not yet complete. */
  finalVideoUrl: string | null
  cdnUrl: string | null
  /** Subtitle/caption file URL */
  captionsUrl: string | null
  durationSeconds: number
  fileSizeMb: number
  /** Stage-based progress 0–100 */
  progressPercent: number
  currentStage: string
  createdAt: string
  completedAt: string | null
}

export const MOCK_RENDER_VIDEO_URL = '/videos/render/episode.mp4'

export const mockRenders: MockRender[] = [
  {
    id: 'render-0001-aaaa-bbbb-cccc-ddddeeeeffff',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    status: 'complete',
    aspectRatio: '16:9',
    outputFormat: 'mp4',
    resolution: '1080p',
    finalVideoUrl: '/videos/render/episode.mp4',
    cdnUrl: '/videos/render/episode.mp4',
    captionsUrl: null,
    durationSeconds: 74,
    fileSizeMb: 2.8,
    progressPercent: 100,
    currentStage: 'Done',
    createdAt: '2026-04-18T14:00:00.000Z',
    completedAt: '2026-04-18T14:08:34.000Z',
  },
  {
    id: 'render-0002-aaaa-bbbb-cccc-ddddeeeeffff',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    status: 'complete',
    aspectRatio: '16:9',
    outputFormat: 'mp4',
    resolution: '4k',
    finalVideoUrl: '/videos/render/scene-02-final.mp4',
    cdnUrl: '/videos/render/scene-02-final.mp4',
    captionsUrl: null,
    durationSeconds: 30,
    fileSizeMb: 1.3,
    progressPercent: 100,
    currentStage: 'Done',
    createdAt: '2026-04-17T09:30:00.000Z',
    completedAt: '2026-04-17T09:38:45.000Z',
  },
]
