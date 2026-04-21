import { create } from "zustand"
import type { StoryboardShot } from "@/lib/mock-data"

interface StoryboardState {
  currentSceneIndex: number
  selectedShot: StoryboardShot | null
  regeneratingShots: Set<string>
  nextScene: (totalScenes: number) => void
  prevScene: () => void
  selectShot: (shot: StoryboardShot | null) => void
  markRegeneration: (shotId: string, done?: boolean) => void
}

export const useStoryboardStore = create<StoryboardState>((set) => ({
  currentSceneIndex: 0,
  selectedShot: null,
  regeneratingShots: new Set(),

  nextScene: (totalScenes) =>
    set((s) => ({
      currentSceneIndex: Math.min(s.currentSceneIndex + 1, totalScenes - 1),
    })),

  prevScene: () =>
    set((s) => ({
      currentSceneIndex: Math.max(s.currentSceneIndex - 1, 0),
    })),

  selectShot: (shot) => set({ selectedShot: shot }),

  markRegeneration: (shotId, done = false) =>
    set((s) => {
      const next = new Set(s.regeneratingShots)
      if (done) next.delete(shotId)
      else next.add(shotId)
      return { regeneratingShots: next }
    }),
}))
