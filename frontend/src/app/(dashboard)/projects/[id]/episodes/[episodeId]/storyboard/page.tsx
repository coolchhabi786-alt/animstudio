"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { RefreshCw, Sparkles, Wifi, WifiOff, AlertTriangle, Users } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { SceneGroup } from "@/components/storyboard/scene-group";
import { ShotCardSkeleton } from "@/components/storyboard/shot-card";
import { RegenerateDialog } from "@/components/script/regenerate-dialog";
import { StyleDialog } from "@/components/storyboard/style-dialog";
import {
  useStoryboard,
  useGenerateStoryboard,
  useRegenerateShot,
  useUpdateShotStyle,
  useStoryboardRealtime,
} from "@/hooks/use-storyboard";
import { useScript } from "@/hooks/use-script";
import { useTeam } from "@/hooks/useTeam";
import { useEpisodeCharacters } from "@/hooks/use-characters";
import type { StoryboardShotDto } from "@/types";

interface Props {
  params: { id: string; episodeId: string };
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

export default function StoryboardStudioPage({ params }: Props) {
  const { episodeId } = params;

  const { id: projectId } = params;
  const { team } = useTeam();
  const { data: script } = useScript(episodeId);
  const { data: storyboard, isLoading } = useStoryboard(episodeId);
  const { data: episodeCharacters = [] } = useEpisodeCharacters(episodeId);

  const generate = useGenerateStoryboard(episodeId);
  const regenerateShot = useRegenerateShot(episodeId);
  const updateShotStyle = useUpdateShotStyle(episodeId);

  const { connected } = useStoryboardRealtime({
    episodeId,
    teamId: team?.id,
    hubUrl: `${API_BASE_URL}/hubs/progress`,
  });

  const [isRegenDialogOpen, setIsRegenDialogOpen] = useState(false);
  const [styleDialogShot, setStyleDialogShot] = useState<StoryboardShotDto | null>(
    null,
  );
  const [pendingShotId, setPendingShotId] = useState<string | null>(null);

  // Group shots by scene number for rendering
  const sceneGroups = useMemo(() => {
    if (!storyboard) return [] as { sceneNumber: number; shots: StoryboardShotDto[] }[];
    const map = new Map<number, StoryboardShotDto[]>();
    for (const s of storyboard.shots) {
      (map.get(s.sceneNumber) ?? map.set(s.sceneNumber, []).get(s.sceneNumber)!).push(s);
    }
    return Array.from(map.entries())
      .map(([sceneNumber, shots]) => ({
        sceneNumber,
        shots: shots.slice().sort((a, b) => a.shotIndex - b.shotIndex),
      }))
      .sort((a, b) => a.sceneNumber - b.sceneNumber);
  }, [storyboard]);

  const hasScript = !!script;

  const notReadyChars = episodeCharacters.filter((c) => c.trainingStatus !== "Ready");
  const failedChars   = episodeCharacters.filter((c) => c.trainingStatus === "Failed");
  const trainingChars = episodeCharacters.filter((c) =>
    c.trainingStatus === "Training" || c.trainingStatus === "TrainingQueued" || c.trainingStatus === "PoseGeneration"
  );

  async function handleGenerate(directorNotes: string) {
    try {
      await generate.mutateAsync({ directorNotes: directorNotes || undefined });
      toast.success("Storyboard planning queued! The AI is breaking scenes into shots…");
      setIsRegenDialogOpen(false);
    } catch {
      // toasted by apiFetch
    }
  }

  async function handleRegenerateShot(shot: StoryboardShotDto) {
    setPendingShotId(shot.id);
    try {
      await regenerateShot.mutateAsync({ shotId: shot.id });
      toast.success(`Shot S${shot.sceneNumber}·${shot.shotIndex} re-queued.`);
      if (shot.regenerationCount + 1 > 3) {
        toast.warning("This shot has been regenerated more than 3 times.");
      }
    } catch {
      // toasted
    } finally {
      setPendingShotId(null);
    }
  }

  async function handleConfirmStyle(styleOverride: string | null) {
    if (!styleDialogShot) return;
    const shot = styleDialogShot;
    setPendingShotId(shot.id);
    try {
      await updateShotStyle.mutateAsync({ shotId: shot.id, styleOverride });
      toast.success(`Shot S${shot.sceneNumber}·${shot.shotIndex} style applied.`);
      setStyleDialogShot(null);
    } catch {
      // toasted
    } finally {
      setPendingShotId(null);
    }
  }

  return (
    <main className="p-6 max-w-6xl mx-auto space-y-6">
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">Storyboard Studio</h1>
          <Badge
            variant="outline"
            className={
              connected
                ? "text-green-700 border-green-300 gap-1"
                : "text-gray-500 border-gray-300 gap-1"
            }
          >
            {connected ? <Wifi className="h-3 w-3" /> : <WifiOff className="h-3 w-3" />}
            {connected ? "Live" : "Offline"}
          </Badge>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          {!storyboard && (
            <Button
              onClick={() => handleGenerate("")}
              disabled={!hasScript || generate.isPending || isLoading}
              title={!hasScript ? "A script is required before planning shots" : undefined}
              className="bg-indigo-600 hover:bg-indigo-700 text-white gap-1.5"
            >
              <Sparkles className="h-4 w-4" />
              {generate.isPending ? "Queuing…" : "Generate Storyboard"}
            </Button>
          )}

          {storyboard && (
            <Button
              variant="outline"
              onClick={() => setIsRegenDialogOpen(true)}
              disabled={!hasScript || generate.isPending}
              className="gap-1.5"
            >
              <RefreshCw className="h-4 w-4" />
              Replan
            </Button>
          )}
        </div>
      </div>

      {/* ── Character training status banner ──────────────────────────────────── */}
      {episodeCharacters.length > 0 && notReadyChars.length > 0 && (
        <div className="space-y-2">
          {trainingChars.length > 0 && (
            <div className="flex items-start gap-2 rounded-lg bg-blue-50 border border-blue-200 px-4 py-3">
              <Users className="h-4 w-4 text-blue-600 shrink-0 mt-0.5" />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-blue-900">
                  {trainingChars.length} character{trainingChars.length !== 1 ? "s" : ""} still training
                </p>
                <p className="text-xs text-blue-700 mt-0.5">
                  Storyboard planning is ready to go. Shot images for these characters will be generated
                  automatically once their LoRA training completes.{" "}
                  <Link href={`/projects/${projectId}/characters`} className="underline hover:text-blue-900">
                    Monitor training progress
                  </Link>
                </p>
              </div>
            </div>
          )}
          {failedChars.length > 0 && (
            <div className="flex items-start gap-2 rounded-lg bg-red-50 border border-red-200 px-4 py-3">
              <AlertTriangle className="h-4 w-4 text-red-600 shrink-0 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-red-900">
                  {failedChars.length} character{failedChars.length !== 1 ? "s" : ""} failed training
                </p>
                <p className="text-xs text-red-700 mt-0.5">
                  These characters won&apos;t appear correctly in shot images.{" "}
                  <Link href={`/projects/${projectId}/characters`} className="underline hover:text-red-900">
                    Go to Character Studio to retry training
                  </Link>
                </p>
              </div>
            </div>
          )}
        </div>
      )}

      {/* ── Empty state ──────────────────────────────────────────────────────── */}
      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <ShotCardSkeleton />
          <ShotCardSkeleton />
          <ShotCardSkeleton />
        </div>
      )}

      {!isLoading && !storyboard && (
        <div className="rounded-xl border border-dashed border-gray-300 py-16 flex flex-col items-center gap-4 text-center">
          <div className="h-12 w-12 rounded-full bg-indigo-50 flex items-center justify-center">
            <Sparkles className="h-6 w-6 text-indigo-400" />
          </div>
          <div>
            <p className="font-medium text-gray-700">No storyboard yet</p>
            <p className="text-sm text-gray-500 mt-1">
              {hasScript
                ? 'Click "Generate Storyboard" to plan the shots.'
                : "Generate a script first — the storyboard breaks the screenplay into shots."}
            </p>
          </div>
        </div>
      )}

      {!isLoading && storyboard && sceneGroups.length === 0 && (
        <p className="text-sm text-gray-500">
          The storyboard has no shots yet. The planning job may still be running.
        </p>
      )}

      {!isLoading && storyboard && sceneGroups.length > 0 && (
        <div className="space-y-8">
          {sceneGroups.map((group) => (
            <SceneGroup
              key={group.sceneNumber}
              sceneNumber={group.sceneNumber}
              shots={group.shots}
              onRegenerate={handleRegenerateShot}
              onChangeStyle={(shot) => setStyleDialogShot(shot)}
              pendingShotId={pendingShotId}
            />
          ))}
        </div>
      )}

      {/* ── Dialogs ─────────────────────────────────────────────────────────── */}
      <RegenerateDialog
        isOpen={isRegenDialogOpen}
        isLoading={generate.isPending}
        onConfirm={handleGenerate}
        onClose={() => setIsRegenDialogOpen(false)}
      />

      <StyleDialog
        shot={styleDialogShot}
        isOpen={!!styleDialogShot}
        isLoading={updateShotStyle.isPending}
        onConfirm={handleConfirmStyle}
        onClose={() => setStyleDialogShot(null)}
      />
    </main>
  );
}
