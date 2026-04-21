"use client";

import { CheckCircle2 } from "lucide-react";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";

interface Props {
  percent: number;
  currentStage: string;
  isComplete: boolean;
}

function stageLabel(percent: number, isComplete: boolean): string {
  if (isComplete || percent >= 100) return "Complete ✓";
  if (percent >= 80) return "Finalizing…";
  if (percent >= 50) return "Mixing audio…";
  if (percent >= 20) return "Assembling video frames…";
  return "Queued…";
}

export function RenderProgressBar({ percent, currentStage, isComplete }: Props) {
  const label = currentStage || stageLabel(percent, isComplete);

  return (
    <div className="rounded-lg border p-5 space-y-3">
      <div className="flex items-center justify-between text-sm">
        <span className={cn("text-muted-foreground", isComplete && "text-emerald-600 font-medium")}>
          {label}
        </span>
        <span className={cn("font-bold tabular-nums", isComplete ? "text-emerald-600" : "text-foreground")}>
          {Math.min(percent, 100)}%
        </span>
      </div>

      <Progress
        value={Math.min(percent, 100)}
        className={cn(isComplete && "[&>*]:bg-emerald-500")}
      />

      {isComplete && (
        <div className="flex items-center gap-1.5 text-xs text-emerald-600 font-medium">
          <CheckCircle2 className="h-3.5 w-3.5" />
          Render finished — your video is ready to download.
        </div>
      )}
    </div>
  );
}
