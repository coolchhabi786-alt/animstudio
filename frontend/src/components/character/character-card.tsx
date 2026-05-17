"use client";

import { useState } from "react";
import Image from "next/image";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { TrainingBadge } from "./training-badge";
import type { CharacterDto } from "@/types";

interface CharacterCardProps {
  character: CharacterDto;
  onDelete?: (id: string) => void;
  onRetry?: (id: string) => void;
  onRegenerateDataset?: (id: string) => void;
  className?: string;
}

export function CharacterCard({
  character,
  onDelete,
  onRetry,
  onRegenerateDataset,
  className,
}: CharacterCardProps) {
  const [pendingRegen, setPendingRegen] = useState(false);

  const isTraining =
    character.trainingStatus === "Training" ||
    character.trainingStatus === "PoseGeneration" ||
    character.trainingStatus === "TrainingQueued";

  const isReady  = character.trainingStatus === "Ready";
  const isFailed = character.trainingStatus === "Failed";
  const isDraft  = character.trainingStatus === "Draft";
  // Dataset generated but LoRA never dispatched (draft with image, or failed after dataset gen)
  const canStartLora = !isTraining && !isReady && !!character.imageUrl;

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

        {/* Training complete banner */}
        {isReady && (
          <div className="flex items-center gap-1.5 rounded-md bg-green-50 border border-green-200 px-2.5 py-1.5">
            <svg
              className="h-3.5 w-3.5 text-green-600 shrink-0"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2.5}
            >
              <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
            </svg>
            <span className="text-xs font-medium text-green-700">Training Complete</span>
          </div>
        )}

        {/* Training progress bar */}
        {isTraining && (
          <div
            role="progressbar"
            aria-valuenow={character.trainingProgressPercent}
            aria-valuemin={0}
            aria-valuemax={100}
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

        {/* Dataset image count */}
        {character.datasetImageCount > 0 && (
          <p className="text-xs text-gray-400">
            <span className="font-medium text-gray-600">{character.datasetImageCount}</span>/15 dataset images
          </p>
        )}

        {/* Credit cost */}
        <p className="mt-auto text-xs text-gray-400">
          <span className="font-medium text-gray-600">{character.creditsCost}</span>{" "}
          credits
        </p>

        {/* Start / Retry LoRA Training button */}
        {canStartLora && onRetry && (
          <Button
            size="sm"
            variant="outline"
            className={
              isFailed
                ? "w-full text-xs border-red-200 text-red-600 hover:bg-red-50"
                : "w-full text-xs border-indigo-200 text-indigo-600 hover:bg-indigo-50"
            }
            onClick={() => onRetry(character.id)}
          >
            {isFailed ? "Retry LoRA Training" : "Start LoRA Training"}
          </Button>
        )}

        {/* Retry full design (Failed with no dataset) */}
        {isFailed && !character.imageUrl && onRetry && (
          <Button
            size="sm"
            variant="outline"
            className="w-full text-xs border-red-200 text-red-600 hover:bg-red-50"
            onClick={() => onRetry(character.id)}
          >
            Retry Training
          </Button>
        )}

        {/* Regenerate Dataset button */}
        {character.imageUrl && !isTraining && onRegenerateDataset && (
          <>
            <Button
              size="sm"
              variant="outline"
              className="w-full text-xs"
              onClick={() => setPendingRegen(true)}
            >
              Regenerate Dataset
            </Button>

            <Dialog open={pendingRegen} onOpenChange={setPendingRegen}>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Regenerate dataset images?</DialogTitle>
                  <DialogDescription>
                    This will re-generate all 15 pose images for &ldquo;{character.name}&rdquo; and
                    restart LoRA training. This costs{" "}
                    <strong>{character.creditsCost} credits</strong>. Existing images and LoRA
                    weights will be discarded.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button variant="ghost" onClick={() => setPendingRegen(false)}>
                    Cancel
                  </Button>
                  <Button
                    className="bg-indigo-600 hover:bg-indigo-700 text-white"
                    onClick={() => {
                      onRegenerateDataset(character.id);
                      setPendingRegen(false);
                    }}
                  >
                    Confirm &amp; Regenerate
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </>
        )}
      </div>
    </div>
  );
}
