/** Mock voice assignment data for Phase 7 UI testing. 5 characters with voice assignments. */

export type VoiceName = 'alloy' | 'echo' | 'fable' | 'onyx' | 'nova' | 'shimmer'
export type LanguageCode = 'en' | 'es' | 'fr' | 'de' | 'it' | 'ja'

export interface MockCharacter {
  id: string
  name: string
  avatarUrl: string
  role: 'protagonist' | 'antagonist' | 'supporting'
}

export interface MockVoiceAssignment {
  id: string
  episodeId: string
  characterId: string
  character: MockCharacter
  voiceName: VoiceName
  language: LanguageCode
  durationSeconds: number
  updatedAt: string
}

export const mockVoices: MockVoiceAssignment[] = [
  {
    id: 'va-0001-aaaa-bbbb-cccc-ddddeeee1111',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    characterId: 'char-maya-1111-2222-3333-444455556666',
    character: {
      id: 'char-maya-1111-2222-3333-444455556666',
      name: 'Maya Chen',
      avatarUrl: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=64&h=64&fit=crop&crop=face',
      role: 'protagonist',
    },
    voiceName: 'nova',
    language: 'en',
    durationSeconds: 154,
    updatedAt: '2026-04-10T09:00:00.000Z',
  },
  {
    id: 'va-0002-aaaa-bbbb-cccc-ddddeeee2222',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    characterId: 'char-drchen-1111-2222-3333-444455556666',
    character: {
      id: 'char-drchen-1111-2222-3333-444455556666',
      name: 'Dr. Victor Chen',
      avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=64&h=64&fit=crop&crop=face',
      role: 'antagonist',
    },
    voiceName: 'onyx',
    language: 'en',
    durationSeconds: 98,
    updatedAt: '2026-04-10T09:05:00.000Z',
  },
  {
    id: 'va-0003-aaaa-bbbb-cccc-ddddeeee3333',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    characterId: 'char-aria-1111-2222-3333-444455556666',
    character: {
      id: 'char-aria-1111-2222-3333-444455556666',
      name: 'Aria (AI)',
      avatarUrl: 'https://images.unsplash.com/photo-1485827404703-89b55fcc595e?w=64&h=64&fit=crop',
      role: 'supporting',
    },
    voiceName: 'shimmer',
    language: 'en',
    durationSeconds: 73,
    updatedAt: '2026-04-11T14:30:00.000Z',
  },
  {
    id: 'va-0004-aaaa-bbbb-cccc-ddddeeee4444',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    characterId: 'char-kai-1111-2222-3333-444455556666',
    character: {
      id: 'char-kai-1111-2222-3333-444455556666',
      name: 'Kai Okafor',
      avatarUrl: 'https://images.unsplash.com/photo-1531427186611-ecfd6d936c79?w=64&h=64&fit=crop&crop=face',
      role: 'supporting',
    },
    voiceName: 'echo',
    language: 'en',
    durationSeconds: 42,
    updatedAt: '2026-04-12T11:00:00.000Z',
  },
  {
    id: 'va-0005-aaaa-bbbb-cccc-ddddeeee5555',
    episodeId: 'ep-0011-2222-3333-4444-555566667777',
    characterId: 'char-narrator-1111-2222-3333-444455556666',
    character: {
      id: 'char-narrator-1111-2222-3333-444455556666',
      name: 'Narrator',
      avatarUrl: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=64&h=64&fit=crop&crop=face',
      role: 'supporting',
    },
    voiceName: 'fable',
    language: 'en',
    durationSeconds: 27,
    updatedAt: '2026-04-12T11:15:00.000Z',
  },
]
