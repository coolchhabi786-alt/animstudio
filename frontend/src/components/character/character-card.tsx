"use client";

import Image from "next/image";
import { cn } from "@/lib/utils";
import { TrainingBadge } from "./training-badge";
import type { CharacterDto } from "@/types";

interface CharacterCardProps {
  character: CharacterDto;
  onDelete?: (id: string) => void;
  className?: string;
}

/**
 * CharacterCard — displays a character's thumbnail, name, training status badge,
 * training progress bar, and credit cost. Used in the character gallery grid.
 */
export function CharacterCard({ character, onDelete, className }: CharacterCardProps) {
  const isTraining =
    character.trainingStatus === "Training" ||
    character.trainingStatus === "PoseGeneration" ||
    character.trainingStatus === "TrainingQueued";

  return (
    <div
      className={cn(
        "group relative flex flex-col overflow-hidden rounded-xl border bg-white shadow-sm",
        "transition-shadow hover:shadow-md",
        className
      )}
    >
      {/* Thumbnail */}
      <div className="relative aspect-square w-full bg-gray-100">
        {character.imageUrl ? (
          <Image
            src={character.imageUrl}
            alt={`${character.name} reference image`}
            fill
            className="object-cover"
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 20vw"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center">
            <svg
              className="h-16 w-16 text-gray-300"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1}
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
          </div>
        )}

        {/* Delete button — visible on hover */}
        {onDelete && (
          <button
            aria-label={`Delete ${character.name}`}
            onClick={() => onDelete(character.id)}
            className={cn(
              "absolute right-2 top-2 hidden rounded-full bg-white/80 p-1.5",
              "text-red-500 shadow-sm transition hover:bg-red-50 group-hover:flex"
            )}
          >
            <svg
              className="h-4 w-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
              />
            </svg>
          </button>
        )}
      </div>

      {/* Card body */}
      <div className="flex flex-1 flex-col gap-2 p-3">
        <div className="flex items-start justify-between gap-2">
          <h3 className="line-clamp-2 text-sm font-semibold text-gray-900">
            {character.name}
          </h3>
          <TrainingBadge status={character.trainingStatus} />
        </div>

        {/* Training progress bar */}
        {isTraining && (
          <div role="progressbar" aria-valuenow={character.trainingProgressPercent}
            aria-valuemin={0} aria-valuemax={100}
            aria-label={`Training progress: ${character.trainingProgressPercent}%`}
          >
            <div className="h-1.5 w-full overflow-hidden rounded-full bg-gray-200">
              <div
                className="h-full rounded-full bg-purple-500 transition-all duration-500"
                style={{ width: `${character.trainingProgressPercent}%` }}
              />
            </div>
            <p className="mt-0.5 text-right text-xs text-gray-400">
              {character.trainingProgressPercent}%
            </p>
          </div>
        )}

        {/* Credit cost */}
        <p className="mt-auto text-xs text-gray-400">
          <span className="font-medium text-gray-600">{character.creditsCost}</span>{" "}
          credits
        </p>
      </div>
    </div>
  );
}
