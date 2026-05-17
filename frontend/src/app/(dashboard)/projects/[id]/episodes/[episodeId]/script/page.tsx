"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { Pencil, RefreshCw, Sparkles, Check, X, ArrowRight, AlertTriangle, Users, CheckCircle2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { SceneCard, SceneCardSkeleton } from "@/components/script/scene-card";
import { RegenerateDialog } from "@/components/script/regenerate-dialog";
import { ScriptStats } from "@/components/script/script-stats";
import { ExtractedCharactersReview } from "@/components/character/extracted-characters-review";
import { CharacterSetupPanel } from "@/components/character/character-setup-panel";
import { useEpisodeCharacters, useCharacters } from "@/hooks/use-characters";
import { useEpisode, useUpdateEpisode } from "@/hooks/use-episodes";
import type { CharacterSelection } from "@/types";
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
  const { episodeId } = params;

  const { data: episode } = useEpisode(episodeId);
  const updateEpisode = useUpdateEpisode(episodeId);
  const { data: script, isLoading: scriptLoading } = useScript(episodeId);
  const { data: characters = [], isLoading: charsLoading } =
    useEpisodeCharacters(episodeId);
  const { data: teamCharactersPage } = useCharacters(1, 100);

  const generateScript = useGenerateScript(episodeId);
  const saveScript = useSaveScript(episodeId);
  const regenerateScript = useRegenerateScript(episodeId);

  const [isEditMode, setIsEditMode] = useState(false);
  const [editedScenes, setEditedScenes] = useState<SceneDto[] | null>(null);
  const [isRegenDialogOpen, setIsRegenDialogOpen] = useState(false);
  const [charactersApproved, setCharactersApproved] = useState(false);
  const [characterSelection, setCharacterSelection] = useState<CharacterSelection | null>(null);
  const [setupConfirmed, setSetupConfirmed] = useState(false);
  const [showWaitMessage, setShowWaitMessage] = useState(false);

  useEffect(() => {
    if (!generateScript.isPending) {
      setShowWaitMessage(false);
      return;
    }
    const timer = setTimeout(() => setShowWaitMessage(true), 20_000);
    return () => clearTimeout(timer);
  }, [generateScript.isPending]);

  // Idea editor state — used when the episode has no idea yet
  const [ideaDraft, setIdeaDraft] = useState("");
  const [isSavingIdea, setIsSavingIdea] = useState(false);

  const isLoading = scriptLoading || charsLoading;
  const hasIdea = !!(episode?.idea?.trim());

  const readyTeamCharacters = (teamCharactersPage?.items ?? []).filter(
    (c) => c.trainingStatus === "Ready"
  );
  const showSetupPanel = hasIdea && !script && readyTeamCharacters.length > 0 && !setupConfirmed;

  const draftCharacters = characters.filter((c) => c.trainingStatus === "Draft");
  const showCharacterReview = !!script && draftCharacters.length > 0 && !charactersApproved;

  const displayScenes = editedScenes ?? script?.screenplay.scenes ?? [];

  async function handleSaveIdea() {
    const trimmed = ideaDraft.trim();
    if (!trimmed) return;
    setIsSavingIdea(true);
    try {
      await updateEpisode.mutateAsync({ idea: trimmed });
      toast.success("Story idea saved.");
      setIdeaDraft("");
    } catch {
      // apiFetch already shows a toast
    } finally {
      setIsSavingIdea(false);
    }
  }

  function handleSceneDialogueChange(sceneNumber: number, newLines: SceneDto["dialogue"]) {
    const base = editedScenes ?? script?.screenplay.scenes ?? [];
    setEditedScenes(
      base.map((s) => (s.sceneNumber === sceneNumber ? { ...s, dialogue: newLines } : s))
    );
  }

  async function handleGenerate() {
    try {
      await generateScript.mutateAsync({
        existingCharacterIds: characterSelection?.existingCharacterIds ?? [],
        allowNewCharacters: characterSelection?.allowNewCharacters ?? true,
        newCharacterCount: characterSelection?.newCharacterCount,
        newCharacterNames: characterSelection?.newCharacterNames,
      });
      toast.success("Scriptwriting job queued! The AI is writing your screenplay…");
    } catch {
      // apiFetch already shows a toast
    }
  }

  async function handleSetupConfirm(selection: CharacterSelection) {
    setCharacterSelection(selection);
    setSetupConfirmed(true);
    try {
      await generateScript.mutateAsync({
        existingCharacterIds: selection.existingCharacterIds,
        allowNewCharacters: selection.allowNewCharacters,
        newCharacterCount: selection.newCharacterCount,
        newCharacterNames: selection.newCharacterNames,
      });
      toast.success("Scriptwriting job queued! The AI is writing your screenplay…");
    } catch {
      // apiFetch already shows a toast
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

  // ── Step 0: Episode has no idea yet — collect it before anything else ────────
  if (!isLoading && episode !== undefined && !hasIdea) {
    return (
      <main className="p-6 max-w-2xl mx-auto space-y-6">
        <div className="rounded-xl border border-indigo-200 bg-indigo-50/60 p-6 space-y-4">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center shrink-0">
              <Sparkles className="h-5 w-5 text-indigo-600" />
            </div>
            <div>
              <h2 className="text-base font-semibold text-indigo-900">Set Your Story Idea</h2>
              <p className="text-sm text-indigo-600">
                Describe the story in a few sentences. The AI will use this to write the full screenplay.
              </p>
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="idea-input" className="text-sm font-medium text-gray-700">
              Episode Idea / Brief
            </Label>
            <Textarea
              id="idea-input"
              rows={5}
              maxLength={5000}
              placeholder="e.g. A young inventor discovers a map hidden in her grandmother's attic that leads to a lost city beneath the ocean…"
              value={ideaDraft}
              onChange={(e) => setIdeaDraft(e.target.value)}
              className="bg-white resize-none"
              autoFocus
            />
            <p className="text-xs text-gray-400 text-right">{ideaDraft.length}/5000</p>
          </div>

          <div className="flex justify-end">
            <Button
              onClick={handleSaveIdea}
              disabled={!ideaDraft.trim() || isSavingIdea}
              className="bg-indigo-600 hover:bg-indigo-700 text-white gap-2"
            >
              <Check className="h-4 w-4" />
              {isSavingIdea ? "Saving…" : "Save Idea & Continue"}
            </Button>
          </div>
        </div>
      </main>
    );
  }

  return (
    <main className="p-6 max-w-4xl mx-auto space-y-6">
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Script Workshop</h2>
          {script?.isManuallyEdited && (
            <Badge variant="outline" className="mt-1 text-xs text-yellow-700 border-yellow-300">
              Manually edited
            </Badge>
          )}
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          {!script && !showSetupPanel && (
            <div className="flex flex-col items-end gap-1">
              <Button
                onClick={handleGenerate}
                disabled={generateScript.isPending || isLoading}
                className="bg-indigo-600 hover:bg-indigo-700 text-white gap-1.5"
              >
                <Sparkles className="h-4 w-4" />
                {generateScript.isPending ? "Generating…" : "Generate Script"}
              </Button>
              {showWaitMessage && (
                <p className="text-xs text-muted-foreground text-right max-w-xs">
                  Script generation is in progress — this can take a few minutes.
                  You can leave this page and come back to check.
                </p>
              )}
            </div>
          )}

          {script && !isEditMode && (
            <Button
              variant="outline"
              onClick={() => setIsRegenDialogOpen(true)}
              disabled={regenerateScript.isPending}
              className="gap-1.5"
            >
              <RefreshCw className="h-4 w-4" />
              Regenerate
            </Button>
          )}

          {script && !isEditMode && (
            <Button variant="outline" onClick={() => setIsEditMode(true)} className="gap-1.5">
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

      {/* ── Episode idea card (read-only, shown when script not yet generated) ── */}
      {!isLoading && !script && episode?.idea && (
        <div className="rounded-lg border border-gray-200 bg-white p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-gray-400 mb-1">
            Episode Idea
          </p>
          <p className="text-sm text-gray-700">{episode.idea}</p>
        </div>
      )}

      {/* ── Stats bar ─────────────────────────────────────────────────────────── */}
      {!isLoading && displayScenes.length > 0 && (
        <div className="border rounded-lg px-4 py-2.5 bg-gray-50">
          <ScriptStats scenes={displayScenes} />
        </div>
      )}

      {/* ── Loading skeletons ──────────────────────────────────────────────────── */}
      {isLoading && (
        <div className="space-y-4">
          <SceneCardSkeleton />
          <SceneCardSkeleton />
          <SceneCardSkeleton />
        </div>
      )}

      {/* ── Character setup panel (returning users with Ready characters) ─────── */}
      {showSetupPanel && <CharacterSetupPanel onConfirm={handleSetupConfirm} />}

      {/* ── Empty state (idea set but no script yet) ──────────────────────────── */}
      {!isLoading && !script && !showSetupPanel && (
        <div className="rounded-xl border border-dashed border-gray-300 py-16 flex flex-col items-center gap-4 text-center">
          <div className="h-14 w-14 rounded-full bg-indigo-50 flex items-center justify-center">
            <Sparkles className="h-7 w-7 text-indigo-400" />
          </div>
          <div className="space-y-1">
            <p className="font-semibold text-gray-800 text-lg">No script yet</p>
            <p className="text-sm text-gray-500 max-w-sm">
              Click &ldquo;Generate Script&rdquo; and the AI will write a complete screenplay —
              scenes, dialogue, and character list — based on your episode idea.
            </p>
          </div>
          <Button
            onClick={handleGenerate}
            disabled={generateScript.isPending}
            className="bg-indigo-600 hover:bg-indigo-700 text-white gap-2 mt-2"
            size="lg"
          >
            <Sparkles className="h-5 w-5" />
            {generateScript.isPending ? "Generating…" : "Generate Script"}
          </Button>
          {showWaitMessage && (
            <p className="text-sm text-muted-foreground mt-2 max-w-sm">
              Script generation is in progress — this can take a few minutes.
              You can leave this page and come back to check.
            </p>
          )}
        </div>
      )}

      {/* ── Extracted characters review ─────────────────────────────────────────── */}
      {showCharacterReview && (
        <ExtractedCharactersReview
          projectId={params.id}
          episodeId={episodeId}
          characters={draftCharacters}
          onApproved={() => setCharactersApproved(true)}
        />
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

      {/* ── Next Step: Proceed to Storyboard ──────────────────────────────────── */}
      {!isLoading && !!script && (
        <NextStepPanel
          projectId={params.id}
          episodeId={episodeId}
          characters={characters}
        />
      )}
    </main>
  );
}

// ── Next Step Panel ────────────────────────────────────────────────────────────

interface NextStepPanelProps {
  projectId: string;
  episodeId: string;
  characters: import("@/types").CharacterDto[];
}

function NextStepPanel({ projectId, episodeId, characters }: NextStepPanelProps) {
  const readyCount   = characters.filter((c) => c.trainingStatus === "Ready").length;
  const failedCount  = characters.filter((c) => c.trainingStatus === "Failed").length;
  const trainingCount = characters.filter((c) =>
    c.trainingStatus === "Training" || c.trainingStatus === "TrainingQueued" || c.trainingStatus === "PoseGeneration"
  ).length;
  const draftCount   = characters.filter((c) => c.trainingStatus === "Draft").length;
  const total        = characters.length;

  const allReady  = total > 0 && readyCount === total;
  const anyFailed = failedCount > 0;
  const anyTraining = trainingCount > 0;

  return (
    <div className="rounded-xl border bg-white p-5 space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h3 className="text-sm font-semibold text-gray-900">Ready for Storyboard?</h3>
          <p className="text-xs text-gray-500 mt-0.5">
            Storyboard planning works now — shot images render as characters finish training.
          </p>
        </div>
        <Link
          href={`/projects/${projectId}/episodes/${episodeId}/storyboard`}
          className="shrink-0"
        >
          <Button className="bg-indigo-600 hover:bg-indigo-700 text-white gap-2 w-full sm:w-auto">
            Continue to Storyboard
            <ArrowRight className="h-4 w-4" />
          </Button>
        </Link>
      </div>

      {/* Character status summary */}
      {total === 0 ? (
        <div className="flex items-start gap-2 rounded-lg bg-yellow-50 border border-yellow-200 px-3.5 py-3">
          <AlertTriangle className="h-4 w-4 text-yellow-600 shrink-0 mt-0.5" />
          <div>
            <p className="text-xs font-medium text-yellow-800">No characters defined for this episode</p>
            <p className="text-xs text-yellow-700 mt-0.5">
              You can still generate a storyboard, but shots won&apos;t include character-specific visuals.{" "}
              <Link href={`/projects/${projectId}/characters`} className="underline hover:text-yellow-900">
                Go to Character Studio
              </Link>{" "}
              to add characters.
            </p>
          </div>
        </div>
      ) : allReady ? (
        <div className="flex items-start gap-2 rounded-lg bg-green-50 border border-green-200 px-3.5 py-3">
          <CheckCircle2 className="h-4 w-4 text-green-600 shrink-0 mt-0.5" />
          <p className="text-xs font-medium text-green-800">
            All {total} character{total !== 1 ? "s" : ""} trained and ready — storyboard image generation will use character LoRA weights.
          </p>
        </div>
      ) : (
        <div className="space-y-2">
          {anyTraining && (
            <div className="flex items-start gap-2 rounded-lg bg-blue-50 border border-blue-200 px-3.5 py-3">
              <Users className="h-4 w-4 text-blue-600 shrink-0 mt-0.5" />
              <div className="flex-1 min-w-0">
                <p className="text-xs font-medium text-blue-900">
                  {trainingCount} of {total} character{total !== 1 ? "s" : ""} still training
                  {readyCount > 0 && ` (${readyCount} ready)`}
                </p>
                <p className="text-xs text-blue-700 mt-0.5">
                  You can proceed to storyboard planning now — shot images will be generated
                  automatically once each character&apos;s training completes.{" "}
                  <Link href={`/projects/${projectId}/characters`} className="underline hover:text-blue-900">
                    Monitor training
                  </Link>
                </p>
              </div>
            </div>
          )}
          {draftCount > 0 && (
            <div className="flex items-start gap-2 rounded-lg bg-yellow-50 border border-yellow-200 px-3.5 py-3">
              <AlertTriangle className="h-4 w-4 text-yellow-600 shrink-0 mt-0.5" />
              <div>
                <p className="text-xs font-medium text-yellow-800">
                  {draftCount} character{draftCount !== 1 ? "s" : ""} not yet approved for training
                </p>
                <p className="text-xs text-yellow-700 mt-0.5">
                  Use &ldquo;Approve &amp; Start Training&rdquo; above, or visit{" "}
                  <Link href={`/projects/${projectId}/characters`} className="underline hover:text-yellow-900">
                    Character Studio
                  </Link>{" "}
                  to manage them.
                </p>
              </div>
            </div>
          )}
          {anyFailed && (
            <div className="flex items-start gap-2 rounded-lg bg-red-50 border border-red-200 px-3.5 py-3">
              <AlertTriangle className="h-4 w-4 text-red-600 shrink-0 mt-0.5" />
              <div>
                <p className="text-xs font-medium text-red-800">
                  {failedCount} character{failedCount !== 1 ? "s" : ""} failed training
                </p>
                <p className="text-xs text-red-700 mt-0.5">
                  Visit{" "}
                  <Link href={`/projects/${projectId}/characters`} className="underline hover:text-red-900">
                    Character Studio
                  </Link>{" "}
                  to retry training. Failed characters won&apos;t appear in storyboard shots.
                </p>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
