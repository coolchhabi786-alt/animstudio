/** Mock storyboard data for Phase 6 UI testing. 3 scenes × 4 shots = 12 total. */

export interface StoryboardShot {
  id: string
  sceneNumber: number
  shotIndex: number
  imageUrl: string
  description: string
  styleOverride?: string
  regenerationCount: number
  updatedAt: string
}

export interface StoryboardScene {
  id: string
  number: number
  shots: StoryboardShot[]
}

export interface MockStoryboard {
  id: string
  episodeId: string
  scenes: StoryboardScene[]
  createdAt: string
  updatedAt: string
}

export const mockStoryboard: MockStoryboard = {
  id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
  episodeId: 'ep-0011-2222-3333-4444-555566667777',
  createdAt: '2026-04-01T08:00:00.000Z',
  updatedAt: '2026-04-19T10:30:00.000Z',
  scenes: [
    {
      id: 'scene-0001-1111-2222-3333-444455556666',
      number: 1,
      shots: [
        {
          id: 'shot-s1-01-aaaa-bbbb-cccc-ddddeeee0001',
          sceneNumber: 1,
          shotIndex: 1,
          imageUrl: 'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&h=600&fit=crop',
          description: 'Wide establishing shot of a futuristic city skyline at dawn. Skyscrapers glow with blue neon accents.',
          styleOverride: undefined,
          regenerationCount: 0,
          updatedAt: '2026-04-01T08:05:00.000Z',
        },
        {
          id: 'shot-s1-02-aaaa-bbbb-cccc-ddddeeee0002',
          sceneNumber: 1,
          shotIndex: 2,
          imageUrl: 'https://images.unsplash.com/photo-1480714378408-67cf0d13bc1b?w=800&h=600&fit=crop',
          description: 'Medium shot of protagonist Maya walking through a crowded market district, looking determined.',
          styleOverride: undefined,
          regenerationCount: 1,
          updatedAt: '2026-04-02T09:10:00.000Z',
        },
        {
          id: 'shot-s1-03-aaaa-bbbb-cccc-ddddeeee0003',
          sceneNumber: 1,
          shotIndex: 3,
          imageUrl: 'https://images.unsplash.com/photo-1519501025264-65ba15a82390?w=800&h=600&fit=crop',
          description: 'Close-up on Maya\'s face as she spots something alarming in the distance. Her eyes widen.',
          styleOverride: 'anime',
          regenerationCount: 2,
          updatedAt: '2026-04-03T11:00:00.000Z',
        },
        {
          id: 'shot-s1-04-aaaa-bbbb-cccc-ddddeeee0004',
          sceneNumber: 1,
          shotIndex: 4,
          imageUrl: 'https://images.unsplash.com/photo-1449824913935-59a10b8d2000?w=800&h=600&fit=crop',
          description: 'Pan shot revealing a massive holographic advertisement malfunctioning above the plaza.',
          styleOverride: undefined,
          regenerationCount: 0,
          updatedAt: '2026-04-01T08:20:00.000Z',
        },
      ],
    },
    {
      id: 'scene-0002-1111-2222-3333-444455556666',
      number: 2,
      shots: [
        {
          id: 'shot-s2-01-aaaa-bbbb-cccc-ddddeeee0005',
          sceneNumber: 2,
          shotIndex: 1,
          imageUrl: 'https://images.unsplash.com/photo-1497366216548-37526070297c?w=800&h=600&fit=crop',
          description: 'Interior of a high-tech research lab. Banks of monitors display complex data streams.',
          styleOverride: undefined,
          regenerationCount: 0,
          updatedAt: '2026-04-01T09:00:00.000Z',
        },
        {
          id: 'shot-s2-02-aaaa-bbbb-cccc-ddddeeee0006',
          sceneNumber: 2,
          shotIndex: 2,
          imageUrl: 'https://images.unsplash.com/photo-1518770660439-4636190af475?w=800&h=600&fit=crop',
          description: 'Dr. Chen (antagonist) studies the anomalous readings, a thin smile crossing his face.',
          styleOverride: 'pixar3d',
          regenerationCount: 1,
          updatedAt: '2026-04-05T14:30:00.000Z',
        },
        {
          id: 'shot-s2-03-aaaa-bbbb-cccc-ddddeeee0007',
          sceneNumber: 2,
          shotIndex: 3,
          imageUrl: 'https://images.unsplash.com/photo-1485827404703-89b55fcc595e?w=800&h=600&fit=crop',
          description: 'Two-shot of Dr. Chen and his assistant reviewing a glowing blue schematic on a holotable.',
          styleOverride: undefined,
          regenerationCount: 0,
          updatedAt: '2026-04-01T09:20:00.000Z',
        },
        {
          id: 'shot-s2-04-aaaa-bbbb-cccc-ddddeeee0008',
          sceneNumber: 2,
          shotIndex: 4,
          imageUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800&h=600&fit=crop',
          description: 'ECU on a spinning device activating — a pulse of energy radiates outward from the core.',
          styleOverride: undefined,
          regenerationCount: 3,
          updatedAt: '2026-04-10T16:45:00.000Z',
        },
      ],
    },
    {
      id: 'scene-0003-1111-2222-3333-444455556666',
      number: 3,
      shots: [
        {
          id: 'shot-s3-01-aaaa-bbbb-cccc-ddddeeee0009',
          sceneNumber: 3,
          shotIndex: 1,
          imageUrl: 'https://images.unsplash.com/photo-1446776811953-b23d57bd21aa?w=800&h=600&fit=crop',
          description: 'Aerial drone shot of the city as lights begin to flicker and fail block by block.',
          styleOverride: undefined,
          regenerationCount: 0,
          updatedAt: '2026-04-01T10:00:00.000Z',
        },
        {
          id: 'shot-s3-02-aaaa-bbbb-cccc-ddddeeee0010',
          sceneNumber: 3,
          shotIndex: 2,
          imageUrl: 'https://images.unsplash.com/photo-1504711434969-e33886168f5c?w=800&h=600&fit=crop',
          description: 'Maya running through a darkened alley, sparks flying from overhead power conduits.',
          styleOverride: undefined,
          regenerationCount: 1,
          updatedAt: '2026-04-07T12:00:00.000Z',
        },
        {
          id: 'shot-s3-03-aaaa-bbbb-cccc-ddddeeee0011',
          sceneNumber: 3,
          shotIndex: 3,
          imageUrl: 'https://images.unsplash.com/photo-1534430480872-3498386e7856?w=800&h=600&fit=crop',
          description: 'Maya slides to a stop before a sealed blast door. She raises her keycard with trembling hands.',
          styleOverride: 'comicbook',
          regenerationCount: 0,
          updatedAt: '2026-04-01T10:20:00.000Z',
        },
        {
          id: 'shot-s3-04-aaaa-bbbb-cccc-ddddeeee0012',
          sceneNumber: 3,
          shotIndex: 4,
          imageUrl: 'https://images.unsplash.com/photo-1531297484001-80022131f5a1?w=800&h=600&fit=crop',
          description: 'The door slides open revealing an empty chamber — the device is gone. Cut to black.',
          styleOverride: undefined,
          regenerationCount: 0,
          updatedAt: '2026-04-01T10:30:00.000Z',
        },
      ],
    },
  ],
}
