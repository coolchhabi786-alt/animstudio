"use client"

import { useMemo, useState, useCallback } from "react"
import { toast } from "sonner"
import { Layers } from "lucide-react"
import { ShotGrid } from "@/components/storyboard/shot-grid"
import { ShotViewerModal } from "@/components/storyboard/shot-viewer-modal"
import { useStoryboardStore } from "@/stores/storyboardStore"
import { useStoryboard, useRegenerateShot, useUpdateShotStyle } from "@/hooks/use-storyboard"
import type { StoryboardShotDto } from "@/types"
import type { ShotAction } from "@/components/storyboard/shot-grid"

interface Props {
  params: { id: string }
}

export default function StoryboardStudioPage({ params }: Props) {
  const episodeId = params.id

  const { data: storyboard, isLoading, isError } = useStoryboard(episodeId)
  const regenerateMutation = useRegenerateShot(episodeId)
  const styleMutation = useUpdateShotStyle(episodeId)

  const {
    currentSceneIndex,
    selectedShot,
    regeneratingShots,
    nextScene,
    prevScene,
    selectShot,
    markRegeneration,
  } = useStoryboardStore()

  const [isModalOpen, setIsModalOpen] = useState(false)

  // Group shots by sceneNumber in ascending order to reconstruct scene tabs
  const scenes = useMemo(() => {
    if (!storyboard) return []
    const map = new Map<number, StoryboardShotDto[]>()
    for (const shot of storyboard.shots) {
      const arr = map.get(shot.sceneNumber) ?? []
      arr.push(shot)
      map.set(shot.sceneNumber, arr)
    }
    return Array.from(map.entries())
      .sort(([a], [b]) => a - b)
      .map(([, shots]) => shots)
  }, [storyboard])

  const currentShots = scenes[currentSceneIndex] ?? []
  const allShots = storyboard?.shots ?? []

  const handleCardAction = useCallback(
    (shotId: string, action: ShotAction, payload?: string) => {
      if (action === "regenerate") {
        if (regeneratingShots.has(shotId)) return
        markRegeneration(shotId)
        toast.loading("Regenerating shot…", { id: shotId })
        regenerateMutation.mutate(
          { shotId },
          {
            onSuccess: () => {
              markRegeneration(shotId, true)
              toast.success("Shot queued for regeneration!", { id: shotId })
            },
            onError: () => {
              markRegeneration(shotId, true)
              toast.error("Failed to regenerate shot.", { id: shotId })
            },
          },
        )
      }

      if (action === "styleEdit" && payload) {
        styleMutation.mutate({ shotId, styleOverride: payload })
      }
    },
    [regeneratingShots, markRegeneration, regenerateMutation, styleMutation],
  )

  const handleShotClick = useCallback(
    (shot: StoryboardShotDto) => {
      selectShot(shot)
      setIsModalOpen(true)
    },
    [selectShot],
  )

  const handleModalNavigate = useCallback(
    (direction: "prev" | "next") => {
      if (!selectedShot) return
      const idx = allShots.findIndex((s) => s.id === selectedShot.id)
      const nextIdx =
        direction === "prev"
          ? Math.max(0, idx - 1)
          : Math.min(allShots.length - 1, idx + 1)
      const nextShot = allShots[nextIdx]
      if (nextShot) selectShot(nextShot)
    },
    [selectedShot, allShots, selectShot],
  )

  const handleModalClose = useCallback(() => {
    setIsModalOpen(false)
    selectShot(null)
  }, [selectShot])

  if (isError) {
    return (
      <div className="flex items-center justify-center py-24">
        <p className="text-sm text-red-500">Failed to load storyboard. Please try again.</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center gap-3">
        <div className="p-2 bg-indigo-100 rounded-lg">
          <Layers className="h-5 w-5 text-indigo-600" />
        </div>
        <div>
          <h1 className="text-xl font-bold text-gray-900">Storyboard Studio</h1>
          <p className="text-sm text-gray-500">
            {storyboard?.screenplayTitle ?? (isLoading ? "Loading…" : "No storyboard yet")}
          </p>
        </div>
      </div>

      {/* Shot grid with scene navigation */}
      <ShotGrid
        shots={currentShots}
        regeneratingShots={regeneratingShots}
        isLoading={isLoading}
        currentSceneIndex={currentSceneIndex}
        totalScenes={Math.max(scenes.length, 1)}
        onCardAction={handleCardAction}
        onShotClick={handleShotClick}
        onNextScene={() => nextScene(scenes.length)}
        onPrevScene={prevScene}
      />

      {/* Full-screen shot viewer */}
      <ShotViewerModal
        shot={selectedShot}
        isOpen={isModalOpen}
        onClose={handleModalClose}
        onNavigate={handleModalNavigate}
      />
    </div>
  )
}
