"use client";

import type { TextAnimation } from "@/types/timeline";
import { textOverlayUtils, type GridPosition } from "@/lib/timeline/text-overlay-utils";

interface TextOverlayPreviewProps {
  text: string;
  fontSizePixels: number;
  color: string;
  positionX: number;
  positionY: number;
  animation: TextAnimation;
}

const ANIMATION_LABELS: Record<string, string> = {
  none:      "No animation",
  fadeIn:    "Fade in",
  slideUp:   "Slide up",
  slideDown: "Slide down",
};

export function TextOverlayPreview({
  text,
  fontSizePixels,
  color,
  positionX,
  positionY,
  animation,
}: TextOverlayPreviewProps) {
  const gridPos: GridPosition = textOverlayUtils.percentToGrid(positionX, positionY);
  const style = textOverlayUtils.formatTextForDisplay({ fontSizePixels, color, positionX, positionY });

  return (
    <div className="flex flex-col gap-1.5">
      <span className="text-[10px] text-slate-500 uppercase tracking-wider">Preview</span>

      {/* 16:9 preview box */}
      <div
        className="relative w-full bg-slate-900 rounded border border-slate-700 overflow-hidden"
        style={{ paddingBottom: "56.25%" /* 16:9 */ }}
      >
        {/* Video frame placeholder */}
        <div className="absolute inset-0 flex items-center justify-center">
          <div className="w-full h-full bg-gradient-to-br from-slate-800 to-slate-900 opacity-50" />
          <span className="absolute text-slate-700 text-xs">Video Frame</span>
        </div>

        {/* Text overlay */}
        {text.trim() && (
          <p
            style={{
              ...style,
              fontSize: Math.max(8, fontSizePixels * 0.25), // scale down for preview box
            }}
          >
            {text}
          </p>
        )}
      </div>

      {/* Position + animation info */}
      <div className="flex items-center justify-between text-[10px] text-slate-500">
        <span>Position: <b className="text-slate-300">{gridPos.replace(/-/g, " ")}</b></span>
        <span>Animation: <b className="text-slate-300">{ANIMATION_LABELS[animation] ?? animation}</b></span>
      </div>
    </div>
  );
}
