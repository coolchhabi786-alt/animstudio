"use client";

import { useState } from "react";
import { toast } from "sonner";
import { Pencil, RefreshCw, Sparkles, Check, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { SceneCard, SceneCardSkeleton } from "@/components/script/scene-card";
import { RegenerateDialog } from "@/components/script/regenerate-dialog";
import { ScriptStats } from "@/components/script/script-stats";
import { useEpisodeCharacters } from "@/hooks/use-characters";
import {
  useScript,
  useGenerateScript,
  useSaveScript,
  useRegenerateScript,
} from "@/hooks/use-script";
import type { SceneDto } from "@/types";

interface Props {
  params: { id: string; episodeId: string };
}

export default function ScriptWorkshopPage({ params }: Props) {
  const { id: projectId, episodeId } = params;

  const { data: script, isLoading: scriptLoading } = useScript(episodeId);
  const { data: characters = [], isLoading: charsLoading } =
    useEpisodeCharacters(episodeId);

  const generateScript = useGenerateScript(episodeId);
  const saveScript = useSaveScript(episodeId);
  const regenerateScript = useRegenerateScript(episodeId);

  const [isEditMode, setIsEditMode] = useState(false);
  const [editedScenes, setEditedScenes] = useState<SceneDto[] | null>(null);
  const [isRegenDialogOpen, setIsRegenDialogOpen] = useState(false);

  const isLoading = scriptLoading || charsLoading;

  // Characters must be Ready before scripting
  const hasReadyCharacters = characters.some((c) => c.trainingStatus === "Ready");

  // Display scenes: prefer in-flight edits, then server data, then empty
  const displayScenes = editedScenes ?? script?.screenplay.scenes ?? [];

  function handleSceneDialogueChange(sceneNumber: number, newLines: SceneDto["dialogue"]) {
    const base = editedScenes ?? script?.screenplay.scenes ?? [];
    setEditedScenes(
      base.map((s) => (s.sceneNumber === sceneNumber ? { ...s, dialogue: newLines } : s))
    );
  }

  async function handleGenerate() {
    try {
      await generateScript.mutateAsync({});
      toast.success("Scriptwriting job queued! The AI is writing your screenplay…");
    } catch {
      // apiFetch already shows a toast, nothing extra needed
    }
  }

  async function handleSave() {
    if (!editedScenes || !script) return;
    try {
      await saveScript.mutateAsync({
        title: script.screenplay.title,
        scenes: editedScenes,
      });
      toast.success("Script saved.");
      setEditedScenes(null);
      setIsEditMode(false);
    } catch {
      // error already toasted
    }
  }

  function handleCancelEdit() {
    setEditedScenes(null);
    setIsEditMode(false);
  }

  async function handleRegenerate(directorNotes: string) {
    try {
      await regenerateScript.mutateAsync({ directorNotes: directorNotes || undefined });
      toast.success("Regeneration queued! The AI is rewriting your screenplay…");
      setIsRegenDialogOpen(false);
    } catch {
      // error already toasted
    }
  }

  return (
    <main className="p-6 max-w-4xl mx-auto space-y-6">
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Script Workshop</h1>
          {script?.isManuallyEdited && (
            <Badge variant="outline" className="mt-1 text-xs text-yellow-700 border-yellow-300">
              Manually edited
            </Badge>
          )}
        </div>

        {/* Action buttons */}
        <div className="flex items-center gap-2 flex-wrap">
          {/* Generate (only visible when no script yet) */}
          {!script && (
            <Button
              onClick={handleGenerate}
              disabled={!hasReadyCharacters || generateScript.isPending || isLoading}
              title={!hasReadyCharacters ? "At least one character must be Ready" : undefined}
              className="bg-indigo-600 hover:bg-indigo-700 text-white gap-1.5"
            >
              <Sparkles className="h-4 w-4" />
              {generateScript.isPending ? "Queuing…" : "Generate Script"}
            </Button>
          )}

          {/* Regenerate (visible when script exists) */}
          {script && !isEditMode && (
            <Button
              variant="outline"
              onClick={() => setIsRegenDialogOpen(true)}
              disabled={!hasReadyCharacters || regenerateScript.isPending}
              title={!hasReadyCharacters ? "At least one character must be Ready" : undefined}
              className="gap-1.5"
            >
              <RefreshCw className="h-4 w-4" />
              Regenerate
            </Button>
          )}

          {/* Edit / Save / Cancel */}
          {script && !isEditMode && (
            <Button
              variant="outline"
              onClick={() => setIsEditMode(true)}
              className="gap-1.5"
            >
              <Pencil className="h-4 w-4" />
              Edit
            </Button>
          )}

          {isEditMode && (
            <>
              <Button
                onClick={handleSave}
                disabled={saveScript.isPending}
                className="bg-green-600 hover:bg-green-700 text-white gap-1.5"
              >
                <Check className="h-4 w-4" />
                {saveScript.isPending ? "Saving…" : "Save"}
              </Button>
              <Button variant="ghost" onClick={handleCancelEdit} className="gap-1.5">
                <X className="h-4 w-4" />
                Cancel
              </Button>
            </>
          )}
        </div>
      </div>

      {/* ── Stats bar ─────────────────────────────────────────────────────────── */}
      {!isLoading && displayScenes.length > 0 && (
        <div className="border rounded-lg px-4 py-2.5 bg-gray-50">
          <ScriptStats scenes={displayScenes} />
        </div>
      )}

      {/* ── Screenplay scenes ──────────────────────────────────────────────────── */}
      {isLoading && (
        <div className="space-y-4">
          <SceneCardSkeleton />
          <SceneCardSkeleton />
          <SceneCardSkeleton />
        </div>
      )}

      {!isLoading && !script && (
        <div className="rounded-xl border border-dashed border-gray-300 py-16 flex flex-col items-center gap-4 text-center">
          <div className="h-12 w-12 rounded-full bg-indigo-50 flex items-center justify-center">
            <Sparkles className="h-6 w-6 text-indigo-400" />
          </div>
          <div>
            <p className="font-medium text-gray-700">No script generated yet</p>
            <p className="text-sm text-gray-500 mt-1">
              {hasReadyCharacters
                ? 'Click "Generate Script" to create the screenplay.'
                : "Add and train at least one character before generating a script."}
            </p>
          </div>
        </div>
      )}

      {!isLoading && script && displayScenes.length === 0 && (
        <p className="text-sm text-gray-500">The script has no scenes yet.</p>
      )}

      {!isLoading && displayScenes.length > 0 && (
        <div className="space-y-4">
          {displayScenes.map((scene) => (
            <SceneCard
              key={scene.sceneNumber}
              scene={scene}
              isEditMode={isEditMode}
              characters={characters}
              onDialogueChange={handleSceneDialogueChange}
            />
          ))}
        </div>
      )}

      {/* ── Regenerate dialog ─────────────────────────────────────────────────── */}
      <RegenerateDialog
        isOpen={isRegenDialogOpen}
        isLoading={regenerateScript.isPending}
        onConfirm={handleRegenerate}
        onClose={() => setIsRegenDialogOpen(false)}
      />
    </main>
  );
}
