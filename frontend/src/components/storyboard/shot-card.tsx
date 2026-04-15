"use client";

import Image from "next/image";
import { AlertTriangle, RefreshCw, Palette } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import type { StoryboardShotDto } from "@/types";

interface Props {
  shot: StoryboardShotDto;
  onRegenerate: (shot: StoryboardShotDto) => void;
  onChangeStyle: (shot: StoryboardShotDto) => void;
  isPending?: boolean;
}

const REGEN_WARNING_THRESHOLD = 3;

export function ShotCard({ shot, onRegenerate, onChangeStyle, isPending }: Props) {
  const overLimit = shot.regenerationCount > REGEN_WARNING_THRESHOLD;

  return (
    <div className="border rounded-lg overflow-hidden bg-white shadow-sm flex flex-col">
      {/* Image / placeholder */}
      <div className="relative aspect-video bg-gray-100">
        {shot.imageUrl ? (
          <Image
            src={shot.imageUrl}
            alt={`Scene ${shot.sceneNumber} shot ${shot.shotIndex}`}
            fill
            sizes="(max-width: 768px) 100vw, 33vw"
            className="object-cover"
            unoptimized
          />
        ) : (
          <div className="absolute inset-0 flex items-center justify-center text-xs text-gray-400">
            Rendering…
          </div>
        )}

        {/* Scene / shot badge */}
        <Badge className="absolute top-2 left-2 bg-black/70 text-white border-0">
          S{shot.sceneNumber}·{shot.shotIndex}
        </Badge>

        {overLimit && (
          <Badge className="absolute top-2 right-2 bg-amber-500 text-white border-0 gap-1">
            <AlertTriangle className="h-3 w-3" />
            {shot.regenerationCount}×
          </Badge>
        )}
      </div>

      {/* Description + actions */}
      <div className="p-3 flex flex-col gap-2 flex-1">
        <p className="text-xs text-gray-700 line-clamp-3">{shot.description}</p>

        {shot.styleOverride && (
          <p className="text-[11px] text-indigo-600 italic truncate">
            Style: {shot.styleOverride}
          </p>
        )}

        <div className="flex gap-2 mt-auto pt-2">
          <Button
            size="sm"
            variant="outline"
            onClick={() => onChangeStyle(shot)}
            disabled={isPending}
            className="gap-1 flex-1"
          >
            <Palette className="h-3.5 w-3.5" />
            Style
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => onRegenerate(shot)}
            disabled={isPending}
            className="gap-1 flex-1"
          >
            <RefreshCw className="h-3.5 w-3.5" />
            Regenerate
          </Button>
        </div>
      </div>
    </div>
  );
}

export function ShotCardSkeleton() {
  return (
    <div className="border rounded-lg overflow-hidden bg-white shadow-sm">
      <Skeleton className="aspect-video w-full" />
      <div className="p-3 space-y-2">
        <Skeleton className="h-3 w-full" />
        <Skeleton className="h-3 w-2/3" />
        <div className="flex gap-2 pt-1">
          <Skeleton className="h-7 flex-1" />
          <Skeleton className="h-7 flex-1" />
        </div>
      </div>
    </div>
  );
}
