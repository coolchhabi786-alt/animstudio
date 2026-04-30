import type {
  ProjectDto,
  EpisodeDto,
  CharacterDto,
  PagedResult,
  SagaStateDto,
} from "@/types"

// ── Stable mock IDs — kept in sync with the backend dev seeder in Program.cs ──
// Project 1 + Episode 1 match the seeded DB rows so storyboard navigation works
// end-to-end when NEXT_PUBLIC_MOCK_DATA=true.

export const MOCK_TEAM_ID = "00000000-0000-0000-0000-000000000002"
export const MOCK_PROJECT_ID_1 = "22222222-2222-2222-2222-222222222222"
export const MOCK_PROJECT_ID_2 = "77777777-7777-7777-7777-777777777777"
export const MOCK_EPISODE_ID_1 = "33333333-3333-3333-3333-333333333333"
export const MOCK_EPISODE_ID_2 = "44444444-4444-4444-4444-444444444444"
export const MOCK_EPISODE_ID_3 = "55555555-5555-5555-5555-555555555555"

// ── Projects ──────────────────────────────────────────────────────────────────

export const mockProjects: ProjectDto[] = [
  {
    id: MOCK_PROJECT_ID_1,
    teamId: MOCK_TEAM_ID,
    name: "Neon City Chronicles",
    description:
      "A cyberpunk animated series set in 2087 Neo-Tokyo. A detective chases signals through a city that never sleeps.",
    thumbnailUrl:
      "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=225&fit=crop",
    createdAt: "2024-01-15T10:00:00Z",
    updatedAt: "2024-01-22T15:30:00Z",
  },
  {
    id: MOCK_PROJECT_ID_2,
    teamId: MOCK_TEAM_ID,
    name: "The Last Forest",
    description:
      "An environmental fable about the last sentient forest on Earth, fighting for survival against a mechanised civilisation.",
    thumbnailUrl:
      "https://images.unsplash.com/photo-1448375240586-882707db888b?w=400&h=225&fit=crop",
    createdAt: "2024-02-01T09:00:00Z",
    updatedAt: "2024-02-12T11:45:00Z",
  },
]

// ── Episodes ──────────────────────────────────────────────────────────────────

export const mockEpisodesByProject: Record<string, EpisodeDto[]> = {
  [MOCK_PROJECT_ID_1]: [
    {
      id: MOCK_EPISODE_ID_1,
      projectId: MOCK_PROJECT_ID_1,
      name: "Episode 1: The Signal",
      idea: "A rogue AI broadcast is detected from an abandoned skyscraper. Detective Kira Tanaka must navigate corporate espionage and underground rebels to uncover the truth.",
      style: "Cyberpunk",
      status: "StoryboardGen",
      characterIds: ["mock-char-001", "mock-char-002", "mock-char-003"],
      directorNotes:
        "Emphasise neon reflections on rain-slicked streets. High contrast cinematography.",
      createdAt: "2024-01-16T10:00:00Z",
      updatedAt: "2024-01-22T15:30:00Z",
    },
    {
      id: MOCK_EPISODE_ID_2,
      projectId: MOCK_PROJECT_ID_1,
      name: "Episode 2: Underground",
      idea: "The signal leads Kira deep into the city's underground network where rebels are building something corporations want destroyed.",
      style: "Cyberpunk",
      status: "Script",
      createdAt: "2024-01-25T10:00:00Z",
      updatedAt: "2024-01-30T12:00:00Z",
    },
  ],
  [MOCK_PROJECT_ID_2]: [
    {
      id: MOCK_EPISODE_ID_3,
      projectId: MOCK_PROJECT_ID_2,
      name: "Episode 1: Awakening",
      idea: "The ancient forest elder awakens after a century of slumber to find his world changed beyond recognition. He must rally the woodland creatures for one last stand.",
      style: "WatercolorIllustration",
      status: "Idle",
      createdAt: "2024-02-02T09:00:00Z",
      updatedAt: "2024-02-08T14:00:00Z",
    },
  ],
}

// ── Characters ────────────────────────────────────────────────────────────────

export const mockCharactersPage: PagedResult<CharacterDto> = {
  items: [
    {
      id: "mock-char-001",
      teamId: MOCK_TEAM_ID,
      name: "Kira Tanaka",
      description:
        "A sharp-witted detective with augmented eyes that replay recorded memories. Haunted by a case she couldn't solve.",
      styleDna: "cyberpunk female detective, short dark hair, holographic badge, long coat",
      imageUrl:
        "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=200&h=200&fit=crop&crop=face",
      trainingStatus: "Ready",
      trainingProgressPercent: 100,
      creditsCost: 120,
      createdAt: "2024-01-16T10:00:00Z",
      updatedAt: "2024-01-18T12:00:00Z",
    },
    {
      id: "mock-char-002",
      teamId: MOCK_TEAM_ID,
      name: "Detective Ray Okafor",
      description:
        "Kira's veteran partner. Old-school cop who distrusts augmentation but has seen enough to keep an open mind.",
      styleDna: "middle-aged male detective, weathered face, classic trench coat, no cybernetics",
      imageUrl:
        "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200&h=200&fit=crop&crop=face",
      trainingStatus: "Ready",
      trainingProgressPercent: 100,
      creditsCost: 110,
      createdAt: "2024-01-16T11:00:00Z",
      updatedAt: "2024-01-19T09:00:00Z",
    },
    {
      id: "mock-char-003",
      teamId: MOCK_TEAM_ID,
      name: "The Oracle",
      description:
        "A mysterious AI that communicates through hacked billboards and vending machines. Its true form is unknown.",
      styleDna: "glitching holographic entity, shifting geometric patterns, electric blue glow",
      imageUrl:
        "https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=200&h=200&fit=crop",
      trainingStatus: "Training",
      trainingProgressPercent: 67,
      creditsCost: 150,
      createdAt: "2024-01-17T14:00:00Z",
      updatedAt: "2024-01-20T08:00:00Z",
    },
    {
      id: "mock-char-004",
      teamId: MOCK_TEAM_ID,
      name: "Shadow",
      description:
        "Leader of the underground rebel faction. Moves unseen through the city's service tunnels.",
      styleDna: "androgynous rebel, graffiti-covered jacket, face mask, agile build",
      imageUrl:
        "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&h=200&fit=crop&crop=face",
      trainingStatus: "Ready",
      trainingProgressPercent: 100,
      creditsCost: 130,
      createdAt: "2024-01-18T10:00:00Z",
      updatedAt: "2024-01-21T16:00:00Z",
    },
    {
      id: "mock-char-005",
      teamId: MOCK_TEAM_ID,
      name: "Corp Commander",
      description:
        "The antagonist. A ruthless executive who controls the city's surveillance network from a penthouse command centre.",
      styleDna: "corporate villain, immaculate suit, silver neural implants, cold expression",
      imageUrl:
        "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=200&h=200&fit=crop&crop=face",
      trainingStatus: "Draft",
      trainingProgressPercent: 0,
      creditsCost: 0,
      createdAt: "2024-01-20T09:00:00Z",
      updatedAt: "2024-01-20T09:00:00Z",
    },
  ],
  totalCount: 5,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  hasPreviousPage: false,
  hasNextPage: false,
}

// ── Saga / Production State ───────────────────────────────────────────────────

export const mockSagaState: SagaStateDto = {
  id: "mock-saga-001",
  episodeId: MOCK_EPISODE_ID_1,
  currentStage: "StoryboardGen",
  retryCount: 0,
  startedAt: "2024-01-22T14:00:00Z",
  updatedAt: "2024-01-22T15:30:00Z",
  isCompensating: false,
}
