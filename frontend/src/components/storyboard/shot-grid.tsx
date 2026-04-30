"use client"

import Image from "next/image"
import { RefreshCw, ChevronLeft, ChevronRight, Loader2, AlertTriangle } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import { StyleOverridePopover } from "@/components/storyboard/style-override-popover"
import type { StoryboardShotDto } from "@/types"

// ─── Shot Card ────────────────────────────────────────────────────────────────

interface ShotCardProps {
  shot: StoryboardShotDto
  isRegenerating: boolean
  onRegenerate: () => void
  onStyleEdit: (style: string) => void
  onClick: () => void
}

function ShotCard({ shot, isRegenerating, onRegenerate, onStyleEdit, onClick }: ShotCardProps) {
  return (
    <div
      className="group relative border rounded-lg overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow cursor-pointer flex flex-col"
      onClick={onClick}
    >
      {/* Thumbnail */}
      <div className="relative aspect-video bg-gray-100">
        {isRegenerating ? (
          <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 bg-gray-50">
            <Loader2 className="h-6 w-6 animate-spin text-blue-500" />
            <span className="text-xs text-gray-500">Regenerating…</span>
          </div>
        ) : shot.imageUrl ? (
          <Image
            src={shot.imageUrl}
            alt={`Scene ${shot.sceneNumber} shot ${shot.shotIndex}`}
            fill
            sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 25vw"
            className="object-cover transition-transform group-hover:scale-105"
            unoptimized
          />
        ) : (
          <div className="absolute inset-0 flex flex-col items-center justify-center gap-1 bg-gray-100">
            <Loader2 className="h-5 w-5 text-gray-400" />
            <span className="text-xs text-gray-400">Generating…</span>
          </div>
        )}

        {/* Top-left badge */}
        <Badge className="absolute top-2 left-2 bg-black/70 text-white border-0 text-[10px]">
          S{shot.sceneNumber}·{shot.shotIndex}
        </Badge>

        {/* Regen count warning */}
        {shot.regenerationCount > 2 && (
          <Badge className="absolute top-2 right-2 bg-amber-500 text-white border-0 gap-1 text-[10px]">
            <AlertTriangle className="h-2.5 w-2.5" />
            {shot.regenerationCount}×
          </Badge>
        )}

        {/* Style overlay badge */}
        {shot.styleOverride && (
          <Badge className="absolute bottom-2 left-2 bg-indigo-500/90 text-white border-0 text-[10px]">
            {shot.styleOverride}
          </Badge>
        )}
      </div>

      {/* Info + actions */}
      <div className="p-3 flex flex-col gap-2 flex-1">
        <p className="text-xs text-gray-700 line-clamp-3 leading-relaxed">
          {shot.description}
        </p>

        {/* Buttons — stop propagation so they don't trigger the card click */}
        <div
          className="flex gap-2 mt-auto pt-1"
          onClick={(e) => e.stopPropagation()}
        >
          <StyleOverridePopover
            shot={shot}
            onApply={onStyleEdit}
            disabled={isRegenerating}
          />
          <Button
            size="sm"
            variant="outline"
            onClick={onRegenerate}
            disabled={isRegenerating}
            className="gap-1 flex-1"
          >
            <RefreshCw className="h-3.5 w-3.5" />
            Regen {shot.regenerationCount > 0 && `(${shot.regenerationCount})`}
          </Button>
        </div>
      </div>
    </div>
  )
}

function ShotCardSkeleton() {
  return (
    <div className="border rounded-lg overflow-hidden bg-white shadow-sm">
      <Skeleton className="aspect-video w-full" />
      <div className="p-3 space-y-2">
        <Skeleton className="h-3 w-full" />
        <Skeleton className="h-3 w-3/4" />
        <div className="flex gap-2 pt-1">
          <Skeleton className="h-8 flex-1" />
          <Skeleton className="h-8 flex-1" />
        </div>
      </div>
    </div>
  )
}

// ─── Shot Grid ────────────────────────────────────────────────────────────────

export type ShotAction = "regenerate" | "styleEdit"

interface Props {
  shots: StoryboardShotDto[]
  regeneratingShots: Set<string>
  isLoading?: boolean
  currentSceneIndex: number
  totalScenes: number
  onCardAction: (shotId: string, action: ShotAction, payload?: string) => void
  onShotClick: (shot: StoryboardShotDto) => void
  onNextScene: () => void
  onPrevScene: () => void
}

export function ShotGrid({
  shots,
  regeneratingShots,
  isLoading,
  currentSceneIndex,
  totalScenes,
  onCardAction,
  onShotClick,
  onNextScene,
  onPrevScene,
}: Props) {
  return (
    <div className="space-y-4">
      {/* Scene navigator */}
      <div className="flex items-center justify-between">
        <Button
          variant="outline"
          size="sm"
          onClick={onPrevScene}
          disabled={currentSceneIndex === 0}
          className="gap-1"
        >
          <ChevronLeft className="h-4 w-4" />
          Previous
        </Button>

        <span className="text-sm font-medium text-gray-600">
          Scene {currentSceneIndex + 1} of {totalScenes}
        </span>

        <Button
          variant="outline"
          size="sm"
          onClick={onNextScene}
          disabled={currentSceneIndex === totalScenes - 1}
          className="gap-1"
        >
          Next
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>

      {/* Grid */}
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        {isLoading
          ? Array.from({ length: 12 }).map((_, i) => <ShotCardSkeleton key={i} />)
          : shots.map((shot) => (
              <ShotCard
                key={shot.id}
                shot={shot}
                isRegenerating={regeneratingShots.has(shot.id)}
                onRegenerate={() => onCardAction(shot.id, "regenerate")}
                onStyleEdit={(style) => onCardAction(shot.id, "styleEdit", style)}
                onClick={() => onShotClick(shot)}
              />
            ))}
      </div>

      {!isLoading && shots.length === 0 && (
        <p className="text-center py-12 text-sm text-gray-500">
          No shots in this scene.
        </p>
      )}
    </div>
  )
}
