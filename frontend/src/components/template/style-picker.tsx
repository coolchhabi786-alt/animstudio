"use client";

import { StylePresetDto, Style } from "@/types";
import { Skeleton } from "@/components/ui/skeleton";
import Image from "next/image";
import { cn } from "@/lib/utils";

interface StylePickerProps {
  presets: StylePresetDto[];
  isLoading: boolean;
  selectedStyle: Style | undefined;
  onSelect: (style: Style) => void;
}

export function StylePicker({ presets, isLoading, selectedStyle, onSelect }: StylePickerProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3" aria-label="Loading style presets">
        {Array.from({ length: 8 }).map((_, i) => (
          <Skeleton key={i} className="h-28 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  return (
    <div
      role="listbox"
      aria-label="Visual style"
      className="grid grid-cols-2 sm:grid-cols-4 gap-3"
    >
      {presets.map((preset) => {
        const isSelected = selectedStyle === preset.style;
        return (
          <button
            key={preset.id}
            type="button"
            role="option"
            aria-selected={isSelected}
            aria-label={`Style: ${preset.displayName}. ${preset.description}`}
            onClick={() => onSelect(preset.style as Style)}
            className={cn(
              "group relative flex flex-col rounded-lg border-2 overflow-hidden text-left transition-all duration-150",
              "hover:border-blue-400 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500",
              isSelected
                ? "border-blue-500 shadow-md ring-2 ring-blue-500/30"
                : "border-border bg-card"
            )}
          >
            {/* Sample image swatch */}
            <div className="relative h-20 w-full bg-muted overflow-hidden">
              {preset.sampleImageUrl ? (
                <Image
                  src={preset.sampleImageUrl}
                  alt={preset.displayName}
                  fill
                  className="object-cover transition-transform duration-200 group-hover:scale-105"
                  sizes="(max-width: 768px) 50vw, 25vw"
                />
              ) : (
                <div className="flex h-full items-center justify-center text-muted-foreground text-xs">
                  {preset.displayName}
                </div>
              )}
            </div>

            {/* Label */}
            <div className="px-2 py-1.5">
              <p className="text-xs font-semibold leading-tight">{preset.displayName}</p>
            </div>

            {/* Selected checkmark */}
            {isSelected && (
              <div className="absolute top-1.5 right-1.5 flex h-4 w-4 items-center justify-center rounded-full bg-blue-500 text-white text-[10px] font-bold shadow">
                ✓
              </div>
            )}
          </button>
        );
      })}
    </div>
  );
}
