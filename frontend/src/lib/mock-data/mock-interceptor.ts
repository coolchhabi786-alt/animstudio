/**
 * Maps an API path + method to a mock response.
 * Returns `undefined` when no mock rule matches — real fetch proceeds.
 *
 * Routes connected to real backend (no longer mocked):
 *   - Projects GET/POST/DELETE            (Phase 2)
 *   - Episodes GET/POST/dispatch          (Phase 2)
 *   - Episode characters GET/POST/DELETE  (Phase 4)
 *   - Characters library GET              (Phase 4)
 *   - Saga state GET                      (Phase 2)
 *   - Storyboard GET/POST                 (Phase 6 — IFileStorageService returns real URLs)
 *   - Voice assignments GET/PUT           (Phase 7)
 *   - Animation estimate/clips/approve    (Phase 8)
 *
 * Still mocked:
 *   - POST /api/v1/voices/preview  — backend dev stub returns a non-playable placeholder URL
 */
export function getMockResponse<T>(path: string, options: RequestInit = {}): T | undefined {
  const method = ((options.method as string) ?? "GET").toUpperCase()

  // Voice preview — kept mocked: backend dev stub returns a non-playable placeholder URL.
  if (method === "POST" && path === "/api/v1/voices/preview") {
    return {
      audioUrl: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
      expiresAt: new Date(Date.now() + 3600_000).toISOString(),
    } as unknown as T
  }

  return undefined
}
