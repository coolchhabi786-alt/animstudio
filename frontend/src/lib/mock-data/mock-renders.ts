/** Mock render data for Phase 9 UI testing. 2 renders: one complete, one processing. */

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
  /** Signed CDN URL valid for 30 days. Null if not yet complete. */
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

export const mockRenders: MockRender[] = [
  {
    id: 'render-0001-aaaa-bbbb-cccc-ddddeeeeffff',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    status: 'complete',
    aspectRatio: '16:9',
    outputFormat: 'mp4',
    resolution: '1080p',
    finalVideoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4',
    cdnUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4',
    captionsUrl: null,
    durationSeconds: 74,
    fileSizeMb: 45.2,
    progressPercent: 100,
    currentStage: 'Done',
    createdAt: '2026-04-18T14:00:00.000Z',
    completedAt: '2026-04-18T14:08:34.000Z',
  },
  {
    id: 'render-0002-aaaa-bbbb-cccc-ddddeeeeffff',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    status: 'mixing',
    aspectRatio: '16:9',
    outputFormat: 'mp4',
    resolution: '4k',
    finalVideoUrl: null,
    cdnUrl: null,
    captionsUrl: null,
    durationSeconds: 74,
    fileSizeMb: 0,
    progressPercent: 67,
    currentStage: 'Mixing audio tracks...',
    createdAt: '2026-04-19T09:30:00.000Z',
    completedAt: null,
  },
]
