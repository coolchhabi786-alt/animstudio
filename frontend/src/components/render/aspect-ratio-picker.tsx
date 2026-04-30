"use client";

import { cn } from "@/lib/utils";
import type { RenderAspectRatio } from "@/types";

const RATIOS: {
  value: RenderAspectRatio;
  label: string;
  display: string;
  dims: string;
  w: number;
  h: number;
}[] = [
  { value: "SixteenNine", label: "Landscape", display: "16:9", dims: "1920×1080", w: 64, h: 36 },
  { value: "NineSixteen", label: "Portrait",  display: "9:16", dims: "1080×1920", w: 36, h: 64 },
  { value: "OneOne",      label: "Square",    display: "1:1",  dims: "1080×1080", w: 50, h: 50 },
];

interface Props {
  selected: RenderAspectRatio;
  onSelect: (ratio: RenderAspectRatio) => void;
}

export function AspectRatioPicker({ selected, onSelect }: Props) {
  return (
    <div className="flex gap-3 flex-wrap">
      {RATIOS.map(({ value, label, display, dims, w, h }) => {
        const active = selected === value;
        return (
          <button
            key={value}
            onClick={() => onSelect(value)}
            className={cn(
              "flex flex-col items-center gap-2 rounded-xl border-2 p-4 transition-colors hover:border-primary/60",
              active ? "border-primary bg-primary/5" : "border-border"
            )}
          >
            <div
              className={cn(
                "rounded",
                active ? "bg-primary/20 border border-primary/40" : "bg-muted border border-border"
              )}
              style={{ width: w, height: h }}
            />
            <div className="text-center">
              <p className={cn("text-sm font-semibold", active ? "text-primary" : "text-foreground")}>
                {display}
              </p>
              <p className="text-[11px] text-muted-foreground">{label}</p>
              <p className="text-[10px] text-muted-foreground font-mono">{dims}</p>
            </div>
          </button>
        );
      })}
    </div>
  );
}
