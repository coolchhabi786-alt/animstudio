"use client";

import { useMemo, useState } from "react";
import { toast } from "sonner";
import { Film, Sparkles, Wifi, WifiOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { CostEstimateCard } from "@/components/animation/cost-estimate-card";
import { ClipPlayer, ClipPlayerSkeleton } from "@/components/animation/clip-player";
import { ApprovalDialog } from "@/components/animation/approval-dialog";
import {
  useAnimationClips,
  useAnimationEstimate,
  useAnimationRealtime,
  useApproveAnimation,
} from "@/hooks/use-animation";
import { useTeam } from "@/hooks/useTeam";
import type { AnimationBackend, AnimationClipDto } from "@/types";

interface Props {
  params: { id: string; episodeId: string };
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

export default function AnimationStudioPage({ params }: Props) {
  const { episodeId } = params;
  const { team } = useTeam();

  const [backend, setBackend] = useState<AnimationBackend>("Kling");
  const [isApprovalOpen, setIsApprovalOpen] = useState(false);

  const { data: estimate, isLoading: estimateLoading } = useAnimationEstimate(
    episodeId,
    backend,
  );
  const { data: clips, isLoading: clipsLoading } = useAnimationClips(episodeId);
  const approve = useApproveAnimation(episodeId);

  const { connected } = useAnimationRealtime({
    episodeId,
    teamId: team?.id,
    hubUrl: `${API_BASE_URL}/hubs/progress`,
  });

  const hasClips = !!clips && clips.length > 0;
  const allReady =
    hasClips && clips.every((c) => c.status === "Ready" || c.status === "Failed");

  const sceneGroups = useMemo(() => {
    if (!clips) return [] as { sceneNumber: number; clips: AnimationClipDto[] }[];
    const map = new Map<number, AnimationClipDto[]>();
    for (const c of clips) {
      (map.get(c.sceneNumber) ?? map.set(c.sceneNumber, []).get(c.sceneNumber)!).push(c);
    }
    return Array.from(map.entries())
      .map(([sceneNumber, group]) => ({
        sceneNumber,
        clips: group.slice().sort((a, b) => a.shotIndex - b.shotIndex),
      }))
      .sort((a, b) => a.sceneNumber - b.sceneNumber);
  }, [clips]);

  async function handleConfirmApprove() {
    try {
      await approve.mutateAsync({ backend });
      toast.success("Animation render queued");
      setIsApprovalOpen(false);
    } catch {
      // toast by apiFetch
    }
  }

  return (
    <main className="p-6 max-w-6xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">Animation Studio</h1>
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

        {!hasClips && (
          <Button
            onClick={() => setIsApprovalOpen(true)}
            disabled={
              estimateLoading || !estimate || estimate.shotCount === 0 ||
              approve.isPending
            }
            className="bg-indigo-600 hover:bg-indigo-700 text-white gap-1.5"
          >
            <Sparkles className="h-4 w-4" />
            {approve.isPending ? "Approving…" : "Approve & render"}
          </Button>
        )}
      </div>

      {/* Estimate (pre-approval) */}
      {!hasClips && (
        <CostEstimateCard
          backend={backend}
          onBackendChange={setBackend}
          estimate={estimate}
          isLoading={estimateLoading}
        />
      )}

      {/* Clip grid */}
      {hasClips && (
        <section className="space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold flex items-center gap-2">
              <Film className="h-5 w-5" />
              Clips
              <Badge variant="outline" className="text-xs">
                {clips.filter((c) => c.status === "Ready").length}/{clips.length} ready
              </Badge>
            </h2>
            {allReady && (
              <Badge className="bg-emerald-500 text-white">All rendered</Badge>
            )}
          </div>

          {clipsLoading ? (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <ClipPlayerSkeleton />
              <ClipPlayerSkeleton />
              <ClipPlayerSkeleton />
            </div>
          ) : (
            <div className="space-y-8">
              {sceneGroups.map((group) => (
                <div key={group.sceneNumber} className="space-y-3">
                  <h3 className="text-sm font-semibold text-gray-600">
                    Scene {group.sceneNumber}
                  </h3>
                  <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {group.clips.map((clip) => (
                      <ClipPlayer
                        key={clip.id}
                        clip={clip}
                        episodeId={episodeId}
                      />
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      )}

      <ApprovalDialog
        isOpen={isApprovalOpen}
        isLoading={approve.isPending}
        backend={backend}
        estimate={estimate}
        onConfirm={handleConfirmApprove}
        onClose={() => setIsApprovalOpen(false)}
      />
    </main>
  );
}
