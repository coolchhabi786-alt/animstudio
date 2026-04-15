"use client";

import { useState } from "react";
import { ChevronDown, ChevronRight } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import type { SceneDto, DialogueLineDto, CharacterDto } from "@/types";
import { DialogueRow } from "./dialogue-row";

interface SceneCardProps {
  scene: SceneDto;
  isEditMode: boolean;
  characters: CharacterDto[];
  onDialogueChange: (sceneNumber: number, lines: DialogueLineDto[]) => void;
}

const TONE_COLORS: Record<string, string> = {
  happy: "bg-yellow-100 text-yellow-800",
  sad: "bg-blue-100 text-blue-800",
  suspenseful: "bg-purple-100 text-purple-800",
  funny: "bg-orange-100 text-orange-800",
  dramatic: "bg-red-100 text-red-800",
  calm: "bg-green-100 text-green-800",
  romantic: "bg-pink-100 text-pink-800",
  tense: "bg-red-100 text-red-800",
};

function getToneColor(tone: string): string {
  return TONE_COLORS[tone.toLowerCase()] ?? "bg-gray-100 text-gray-800";
}

export function SceneCard({ scene, isEditMode, characters, onDialogueChange }: SceneCardProps) {
  const [isExpanded, setIsExpanded] = useState(true);

  function handleLineChange(index: number, updated: DialogueLineDto) {
    const newLines = scene.dialogue.map((line, i) => (i === index ? updated : line));
    onDialogueChange(scene.sceneNumber, newLines);
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white shadow-sm overflow-hidden">
      {/* Scene header */}
      <button
        type="button"
        onClick={() => setIsExpanded((v) => !v)}
        className="w-full flex items-center justify-between px-4 py-3 bg-gray-50 hover:bg-gray-100 transition-colors text-left"
        aria-expanded={isExpanded}
      >
        <div className="flex items-center gap-3">
          {isExpanded ? (
            <ChevronDown className="h-4 w-4 text-gray-500 shrink-0" aria-hidden="true" />
          ) : (
            <ChevronRight className="h-4 w-4 text-gray-500 shrink-0" aria-hidden="true" />
          )}
          <span className="font-semibold text-sm text-gray-900">
            Scene {scene.sceneNumber}
          </span>
          <span
            className={`hidden sm:inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${getToneColor(scene.emotionalTone)}`}
          >
            {scene.emotionalTone}
          </span>
        </div>
        <Badge variant="outline" className="text-xs shrink-0">
          {scene.dialogue.length} line{scene.dialogue.length !== 1 ? "s" : ""}
        </Badge>
      </button>

      {/* Scene body */}
      {isExpanded && (
        <div className="px-4 py-3 space-y-3">
          {/* Visual description */}
          <p className="text-sm text-gray-600 italic leading-relaxed">
            {scene.visualDescription}
          </p>

          {/* Dialogue table */}
          {scene.dialogue.length === 0 ? (
            <p className="text-xs text-gray-400">No dialogue lines in this scene.</p>
          ) : (
            <div className="overflow-x-auto -mx-4 px-4">
              <table className="w-full min-w-[540px] text-sm border-collapse">
                <thead>
                  <tr className="border-b border-gray-200 text-left">
                    <th className="py-1.5 pr-3 font-medium text-gray-500 w-36">Character</th>
                    <th className="py-1.5 pr-3 font-medium text-gray-500">Dialogue</th>
                    <th className="py-1.5 pr-3 font-medium text-gray-500 w-20 text-right">Start (s)</th>
                    <th className="py-1.5 font-medium text-gray-500 w-20 text-right">End (s)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {scene.dialogue.map((line, i) => (
                    <DialogueRow
                      key={i}
                      line={line}
                      index={i}
                      isEditMode={isEditMode}
                      characters={characters}
                      onChange={(updated) => handleLineChange(i, updated)}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export function SceneCardSkeleton() {
  return (
    <div className="rounded-lg border border-gray-200 bg-white shadow-sm overflow-hidden">
      <div className="px-4 py-3 bg-gray-50 flex items-center gap-3">
        <Skeleton className="h-4 w-4" />
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-5 w-16 rounded-full" />
      </div>
      <div className="px-4 py-3 space-y-2">
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-4/5" />
        <Skeleton className="h-20 w-full mt-2" />
      </div>
    </div>
  );
}
