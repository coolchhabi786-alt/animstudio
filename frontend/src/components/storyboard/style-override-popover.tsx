"use client"

import { Palette } from "lucide-react"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import type { StoryboardShot } from "@/lib/mock-data"

const STYLE_PRESETS: { label: string; value: string; color: string }[] = [
  { label: "Realistic",     value: "realistic",    color: "text-gray-700" },
  { label: "Cartoon",       value: "cartoon",      color: "text-yellow-600" },
  { label: "Anime",         value: "anime",        color: "text-pink-600" },
  { label: "Watercolor",    value: "watercolor",   color: "text-blue-500" },
  { label: "Pencil Sketch", value: "pencil",       color: "text-stone-600" },
  { label: "3D Render",     value: "3d-render",    color: "text-violet-600" },
]

interface Props {
  shot: StoryboardShot
  onApply: (style: string) => void
  disabled?: boolean
}

export function StyleOverridePopover({ shot, onApply, disabled }: Props) {
  const handleApply = (style: string) => {
    onApply(style)
    toast.success("Style applied! Regenerating…", { duration: 2500 })
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          size="sm"
          variant="outline"
          disabled={disabled}
          className="gap-1.5 flex-1"
          onClick={(e) => e.stopPropagation()}
        >
          <Palette className="h-3.5 w-3.5" />
          Edit Style
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent className="w-44" align="start">
        <DropdownMenuLabel className="text-xs text-gray-500 uppercase tracking-wide">
          Style Preset
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        {STYLE_PRESETS.map((preset) => (
          <DropdownMenuItem
            key={preset.value}
            onClick={(e) => { e.stopPropagation(); handleApply(preset.value) }}
            className={`text-sm font-medium cursor-pointer ${preset.color} ${
              shot.styleOverride === preset.value ? "bg-accent" : ""
            }`}
          >
            {shot.styleOverride === preset.value && (
              <span className="mr-1">✓</span>
            )}
            {preset.label}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
