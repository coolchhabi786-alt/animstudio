"use client";

import { cn } from "@/lib/utils";

const STAGES = [
  { key: "Idle", label: "Idle" },
  { key: "CharacterDesign", label: "Characters" },
  { key: "LoraTraining", label: "LoRA Train" },
  { key: "Script", label: "Script" },
  { key: "Storyboard", label: "Storyboard" },
  { key: "Voice", label: "Voice" },
  { key: "Animation", label: "Animation" },
  { key: "PostProduction", label: "Post" },
  { key: "Done", label: "Done" },
] as const;

interface ProgressStepperProps {
  currentStage?: string;
  isCompensating?: boolean;
}

export function ProgressStepper({ currentStage, isCompensating }: ProgressStepperProps) {
  const currentIndex = STAGES.findIndex((s) => s.key === currentStage);

  return (
    <div className="flex items-center gap-1 w-full overflow-x-auto py-2">
      {STAGES.map((stage, idx) => {
        const isDone = idx < currentIndex;
        const isCurrent = idx === currentIndex;
        const isPending = idx > currentIndex;

        return (
          <div key={stage.key} className="flex items-center gap-1 flex-1 min-w-0">
            <div className="flex flex-col items-center flex-1">
              <div
                className={cn(
                  "w-8 h-8 rounded-full flex items-center justify-center text-xs font-semibold border-2 transition-colors",
                  isDone && "bg-green-500 border-green-500 text-white",
                  isCurrent && isCompensating && "bg-red-500 border-red-500 text-white animate-pulse",
                  isCurrent && !isCompensating && "bg-blue-500 border-blue-500 text-white animate-pulse",
                  isPending && "bg-gray-100 border-gray-300 text-gray-400"
                )}
              >
                {isDone ? "✓" : idx + 1}
              </div>
              <span className="text-xs text-gray-500 mt-1 text-center truncate w-full">{stage.label}</span>
            </div>
            {idx < STAGES.length - 1 && (
              <div
                className={cn(
                  "h-0.5 flex-1 mb-5",
                  isDone ? "bg-green-500" : "bg-gray-200"
                )}
              />
            )}
          </div>
        );
      })}
    </div>
  );
}
