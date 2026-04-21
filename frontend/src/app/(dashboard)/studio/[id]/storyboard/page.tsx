"use client"

import { useState, useCallback } from "react"
import { toast } from "sonner"
import { Layers } from "lucide-react"
import { mockStoryboard } from "@/lib/mock-data"
import { ShotGrid } from "@/components/storyboard/shot-grid"
import { ShotViewerModal } from "@/components/storyboard/shot-viewer-modal"
import { useStoryboardStore } from "@/stores/storyboardStore"
import type { StoryboardShot } from "@/lib/mock-data"
import type { ShotAction } from "@/components/storyboard/shot-grid"

interface Props {
  params: { id: string }
}

export default function StoryboardStudioPage({ params }: Props) {
  // Local mock state — each shot's fields are mutable for simulated updates
  const [shots, setShots] = useState<StoryboardShot[]>(
    () => mockStoryboard.scenes.flatMap((s) => s.shots),
  )

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

  const scenes = mockStoryboard.scenes
  const currentScene = scenes[currentSceneIndex]
  const currentShots = shots.filter((s) => s.sceneNumber === currentScene.number)

  // Flat list of all shot ids in order for modal prev/next navigation
  const allShotIds = shots.map((s) => s.id)

  const handleCardAction = useCallback(
    (shotId: string, action: ShotAction, payload?: string) => {
      if (action === "regenerate") {
        if (regeneratingShots.has(shotId)) return
        markRegeneration(shotId)
        toast.loading("Regenerating shot…", { id: shotId })
        setTimeout(() => {
          markRegeneration(shotId, true)
          // Bump regeneration count on the shot
          setShots((prev) =>
            prev.map((s) =>
              s.id === shotId
                ? { ...s, regenerationCount: s.regenerationCount + 1 }
                : s,
            ),
          )
          toast.success("Shot regenerated!", { id: shotId })
        }, 2000)
      }

      if (action === "styleEdit" && payload) {
        setShots((prev) =>
          prev.map((s) => (s.id === shotId ? { ...s, styleOverride: payload } : s)),
        )
      }
    },
    [regeneratingShots, markRegeneration],
  )

  const handleShotClick = useCallback(
    (shot: StoryboardShot) => {
      selectShot(shot)
      setIsModalOpen(true)
    },
    [selectShot],
  )

  const handleModalNavigate = useCallback(
    (direction: "prev" | "next") => {
      if (!selectedShot) return
      const idx = allShotIds.indexOf(selectedShot.id)
      const nextIdx =
        direction === "prev"
          ? Math.max(0, idx - 1)
          : Math.min(allShotIds.length - 1, idx + 1)
      const nextShot = shots.find((s) => s.id === allShotIds[nextIdx])
      if (nextShot) selectShot(nextShot)
    },
    [selectedShot, allShotIds, shots, selectShot],
  )

  const handleModalClose = useCallback(() => {
    setIsModalOpen(false)
    selectShot(null)
  }, [selectShot])

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
            Neon City — Episode 1: The Signal
          </p>
        </div>
      </div>

      {/* Shot grid with scene navigation */}
      <ShotGrid
        shots={currentShots}
        regeneratingShots={regeneratingShots}
        currentSceneIndex={currentSceneIndex}
        totalScenes={scenes.length}
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
