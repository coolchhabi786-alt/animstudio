import type { JobDto, VoiceAssignmentDto } from "@/types"
import {
  mockProjects,
  mockEpisodesByProject,
  mockCharactersPage,
  mockStoryboardDto,
  mockSagaState,
} from "./mock-projects"
import { mockVoices } from "./mock-voices"

/**
 * Maps an API path + method to a mock response.
 * Returns `undefined` when no mock rule matches (real fetch proceeds).
 */
export function getMockResponse<T>(path: string, options: RequestInit = {}): T | undefined {
  const method = ((options.method as string) ?? "GET").toUpperCase()

  // ── Projects ─────────────────────────────────────────────────────────────

  if (method === "GET" && path === "/api/v1/projects") {
    return {
      items: mockProjects,
      totalCount: mockProjects.length,
      page: 1,
      pageSize: 20,
    } as unknown as T
  }

  const projectExactMatch = path.match(/^\/api\/v1\/projects\/([^/]+)$/)
  if (method === "GET" && projectExactMatch) {
    const found = mockProjects.find((p) => p.id === projectExactMatch[1]) ?? mockProjects[0]
    return found as unknown as T
  }

  if (method === "POST" && path === "/api/v1/projects") {
    const body = safeParseBody(options.body)
    return {
      id: `mock-proj-${Date.now()}`,
      teamId: "mock-team-001",
      name: body?.name ?? "New Project",
      description: body?.description ?? "",
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    } as unknown as T
  }

  if (method === "DELETE" && projectExactMatch) {
    return null as unknown as T
  }

  // ── Episodes ──────────────────────────────────────────────────────────────

  const projectEpisodesMatch = path.match(/^\/api\/v1\/projects\/([^/]+)\/episodes$/)
  if (method === "GET" && projectEpisodesMatch) {
    const eps =
      mockEpisodesByProject[projectEpisodesMatch[1]] ??
      Object.values(mockEpisodesByProject).flat()
    return eps as unknown as T
  }

  if (method === "POST" && projectEpisodesMatch) {
    const body = safeParseBody(options.body)
    return {
      id: `mock-ep-${Date.now()}`,
      projectId: projectEpisodesMatch[1],
      name: body?.name ?? "New Episode",
      idea: body?.idea ?? "",
      style: body?.style ?? "",
      status: "Idle",
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    } as unknown as T
  }

  const episodeExactMatch = path.match(/^\/api\/v1\/episodes\/([^/]+)$/)
  if (method === "GET" && episodeExactMatch) {
    const allEps = Object.values(mockEpisodesByProject).flat()
    const found = allEps.find((e) => e.id === episodeExactMatch[1]) ?? allEps[0]
    return found as unknown as T
  }

  if (method === "POST" && path.match(/^\/api\/v1\/episodes\/[^/]+\/dispatch$/)) {
    return null as unknown as T
  }

  // ── Characters ────────────────────────────────────────────────────────────

  if (method === "GET" && path.startsWith("/api/v1/characters")) {
    return mockCharactersPage as unknown as T
  }

  const episodeCharactersMatch = path.match(/^\/api\/v1\/episodes\/([^/]+)\/characters$/)
  if (method === "GET" && episodeCharactersMatch) {
    return mockCharactersPage.items.slice(0, 3) as unknown as T
  }

  if (method === "POST" && episodeCharactersMatch) {
    return null as unknown as T
  }

  if (path.match(/^\/api\/v1\/episodes\/[^/]+\/characters\/[^/]+$/) && method === "DELETE") {
    return null as unknown as T
  }

  // ── Storyboard ────────────────────────────────────────────────────────────

  const storyboardMatch = path.match(/^\/api\/v1\/episodes\/([^/]+)\/storyboard$/)
  if (method === "GET" && storyboardMatch) {
    return { ...mockStoryboardDto, episodeId: storyboardMatch[1] } as unknown as T
  }

  if (method === "POST" && storyboardMatch) {
    const mockJob: JobDto = {
      id: `mock-job-${Date.now()}`,
      episodeId: storyboardMatch[1],
      type: "StoryboardPlan",
      status: "Queued",
      queuedAt: new Date().toISOString(),
      attemptNumber: 1,
    }
    return mockJob as unknown as T
  }

  // ── Voice assignments ─────────────────────────────────────────────────────

  const voicesMatch = path.match(/^\/api\/v1\/episodes\/([^/]+)\/voices$/)
  if (method === "GET" && voicesMatch) {
    const assignments: VoiceAssignmentDto[] = mockVoices.map((v) => ({
      id: v.id,
      episodeId: voicesMatch[1],
      characterId: v.characterId,
      characterName: v.character.name,
      voiceName: v.voiceName,
      language: v.language,
      updatedAt: v.updatedAt,
    }))
    return assignments as unknown as T
  }

  if (method === "PUT" && voicesMatch) {
    return [] as unknown as T
  }

  if (method === "POST" && path === "/api/v1/voices/preview") {
    return {
      audioUrl: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
      expiresAt: new Date(Date.now() + 3600_000).toISOString(),
    } as unknown as T
  }

  // ── Saga / production state ───────────────────────────────────────────────

  const sagaMatch = path.match(/^\/api\/v1\/episodes\/([^/]+)\/saga$/)
  if (method === "GET" && sagaMatch) {
    return { ...mockSagaState, episodeId: sagaMatch[1] } as unknown as T
  }

  return undefined
}

function safeParseBody(body: BodyInit | null | undefined): Record<string, string> | null {
  try {
    return body ? JSON.parse(body as string) : null
  } catch {
    return null
  }
}
