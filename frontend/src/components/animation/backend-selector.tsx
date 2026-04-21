"use client";

import { Cloud, Cpu } from "lucide-react";
import { cn } from "@/lib/utils";
import { ANIMATION_BACKENDS } from "@/types";
import type { AnimationBackend } from "@/types";

interface Props {
  selectedBackend: AnimationBackend;
  onSelect: (backend: AnimationBackend) => void;
}

const ICONS: Record<AnimationBackend, React.ElementType> = {
  Kling: Cloud,
  Local: Cpu,
};

export function BackendSelector({ selectedBackend, onSelect }: Props) {
  return (
    <div
      role="radiogroup"
      aria-label="Animation backend"
      className="grid grid-cols-1 sm:grid-cols-2 gap-3"
    >
      {ANIMATION_BACKENDS.map((opt) => {
        const selected = selectedBackend === opt.value;
        const Icon = ICONS[opt.value];
        return (
          <button
            key={opt.value}
            role="radio"
            aria-checked={selected}
            onClick={() => onSelect(opt.value)}
            className={cn(
              "text-left rounded-lg border p-4 transition-colors",
              selected
                ? "border-primary bg-primary/5"
                : "border-border hover:border-primary/40",
            )}
          >
            <div className="flex items-center gap-2 mb-1">
              <Icon className="h-4 w-4 text-muted-foreground" />
              <span className="font-medium text-sm">{opt.label}</span>
            </div>
            <p className="text-xs text-muted-foreground">{opt.description}</p>
          </button>
        );
      })}
    </div>
  );
}
