"use client"

import { useEffect } from "react"
import Image from "next/image"
import { X, ChevronLeft, ChevronRight, RefreshCw } from "lucide-react"
import { Button } from "@/components/ui/button"
import type { StoryboardShotDto } from "@/types"

interface Props {
  shot: StoryboardShotDto | null
  isOpen: boolean
  onClose: () => void
  onNavigate: (direction: "prev" | "next") => void
}

export function ShotViewerModal({ shot, isOpen, onClose, onNavigate }: Props) {
  useEffect(() => {
    if (!isOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose()
      if (e.key === "ArrowLeft") onNavigate("prev")
      if (e.key === "ArrowRight") onNavigate("next")
    }
    window.addEventListener("keydown", onKey)
    return () => window.removeEventListener("keydown", onKey)
  }, [isOpen, onClose, onNavigate])

  if (!isOpen || !shot) return null

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/85 backdrop-blur-sm"
      onClick={onClose}
    >
      {/* Close button */}
      <button
        onClick={onClose}
        className="absolute top-4 right-4 p-2 rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors"
        aria-label="Close"
      >
        <X className="h-5 w-5" />
      </button>

      {/* Prev arrow */}
      <button
        onClick={(e) => { e.stopPropagation(); onNavigate("prev") }}
        className="absolute left-4 p-3 rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors"
        aria-label="Previous shot"
      >
        <ChevronLeft className="h-6 w-6" />
      </button>

      {/* Content */}
      <div
        className="relative flex flex-col items-center gap-4 max-w-[80vw] max-h-[90vh]"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Image */}
        <div className="relative w-[80vw] max-w-3xl aspect-video rounded-lg overflow-hidden shadow-2xl bg-gray-900">
          {shot.imageUrl ? (
            <Image
              src={shot.imageUrl}
              alt={`Scene ${shot.sceneNumber} Shot ${shot.shotIndex}`}
              fill
              sizes="80vw"
              className="object-cover"
              unoptimized
              priority
            />
          ) : (
            <div className="absolute inset-0 flex items-center justify-center text-white/40 text-sm">
              Image not yet generated
            </div>
          )}
        </div>

        {/* Info panel */}
        <div className="w-full max-w-3xl bg-white/10 backdrop-blur-md rounded-lg p-4 text-white">
          <div className="flex items-start justify-between gap-3">
            <div className="flex-1">
              <p className="text-xs font-semibold text-white/60 mb-1">
                Scene {shot.sceneNumber} · Shot {shot.shotIndex}
                {shot.styleOverride && (
                  <span className="ml-2 px-1.5 py-0.5 bg-indigo-500/40 rounded text-indigo-200">
                    {shot.styleOverride}
                  </span>
                )}
              </p>
              <p className="text-sm leading-relaxed">{shot.description}</p>
            </div>

            {shot.regenerationCount > 0 && (
              <div className="flex items-center gap-1 text-xs text-white/50 shrink-0">
                <RefreshCw className="h-3 w-3" />
                {shot.regenerationCount}×
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Next arrow */}
      <button
        onClick={(e) => { e.stopPropagation(); onNavigate("next") }}
        className="absolute right-4 p-3 rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors"
        aria-label="Next shot"
      >
        <ChevronRight className="h-6 w-6" />
      </button>
    </div>
  )
}
