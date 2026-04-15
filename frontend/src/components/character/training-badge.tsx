"use client";

import { cn } from "@/lib/utils";
import type { TrainingStatus } from "@/types";

interface TrainingBadgeProps {
  status: TrainingStatus;
  className?: string;
}

const statusConfig: Record<
  TrainingStatus,
  { label: string; classes: string; pulse: boolean }
> = {
  Draft: {
    label: "Draft",
    classes: "bg-gray-100 text-gray-600 border-gray-200",
    pulse: false,
  },
  PoseGeneration: {
    label: "Generating Poses",
    classes: "bg-blue-100 text-blue-700 border-blue-200",
    pulse: true,
  },
  TrainingQueued: {
    label: "Queued",
    classes: "bg-yellow-100 text-yellow-700 border-yellow-200",
    pulse: true,
  },
  Training: {
    label: "Training",
    classes: "bg-purple-100 text-purple-700 border-purple-200",
    pulse: true,
  },
  Ready: {
    label: "Ready",
    classes: "bg-green-100 text-green-700 border-green-200",
    pulse: false,
  },
  Failed: {
    label: "Failed",
    classes: "bg-red-100 text-red-700 border-red-200",
    pulse: false,
  },
};

/**
 * AnimatedTrainingBadge — shows a pulsing indicator during active training stages
 * and a static badge for terminal states (Draft, Ready, Failed).
 */
export function TrainingBadge({ status, className }: TrainingBadgeProps) {
  const config = statusConfig[status];

  return (
    <span
      aria-label={`Training status: ${config.label}`}
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium",
        config.classes,
        className
      )}
    >
      {config.pulse ? (
        <span className="relative flex h-2 w-2">
          <span
            className={cn(
              "absolute inline-flex h-full w-full animate-ping rounded-full opacity-75",
              status === "Training" || status === "PoseGeneration"
                ? "bg-purple-500"
                : "bg-yellow-500"
            )}
          />
          <span
            className={cn(
              "relative inline-flex h-2 w-2 rounded-full",
              status === "Training" || status === "PoseGeneration"
                ? "bg-purple-600"
                : "bg-yellow-600"
            )}
          />
        </span>
      ) : status === "Ready" ? (
        <svg
          className="h-3 w-3 text-green-600"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={3}
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M5 13l4 4L19 7"
          />
        </svg>
      ) : null}
      {config.label}
    </span>
  );
}
