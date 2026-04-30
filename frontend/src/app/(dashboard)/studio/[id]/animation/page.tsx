"use client";

import { useState } from "react";
import { Film, RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { BackendSelector } from "@/components/animation/backend-selector";
import { CostBreakdownTable } from "@/components/animation/cost-breakdown-table";
import { ClipPreviewGrid } from "@/components/animation/clip-preview-grid";
import { ApprovalDialog } from "@/components/animation/approval-dialog";
import {
  useAnimationEstimate,
  useAnimationClips,
  useApproveAnimation,
  useAnimationRealtime,
} from "@/hooks/use-animation";
import type { AnimationBackend, AnimationClipDto, ClipStatus } from "@/types";

const HUB_URL = process.env.NEXT_PUBLIC_SIGNALR_HUB_URL ?? "http://localhost:5001/hubs/progress";

const STATUS_FILTER: Record<string, ClipStatus[]> = {
  all:       ["Ready", "Rendering", "Pending", "Failed"],
  ready:     ["Ready"],
  processing:["Rendering", "Pending"],
  failed:    ["Failed"],
};

export default function AnimationApprovalPage({
  params,
}: {
  params: { id: string };
}) {
  const episodeId = params.id;

  const [backend, setBackend] = useState<AnimationBackend>("Local");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [activeTab, setActiveTab] = useState("all");

  const { data: estimate, isLoading: estimateLoading } = useAnimationEstimate(episodeId, backend);
  const { data: clips = [], isLoading: clipsLoading, refetch: refetchClips } = useAnimationClips(episodeId);
  const approveMutation = useApproveAnimation(episodeId);

  // Wire SignalR — real-time clip updates
  // teamId is not in the episode response yet; read from a stored user context if available.
  // For now pass undefined — the hook silently skips group join when teamId is absent.
  useAnimationRealtime({
    episodeId,
    teamId: undefined,
    hubUrl: HUB_URL,
  });

  const scenes = estimate
    ? [...new Set(estimate.breakdown.map((b) => b.sceneNumber))].sort().map((n) => ({
        sceneNumber: n,
        shotCount: estimate.breakdown.filter((b) => b.sceneNumber === n).length,
      }))
    : [];

  const filteredClips = clips.filter((c: AnimationClipDto) =>
    STATUS_FILTER[activeTab]?.includes(c.status),
  );

  const readyCount = clips.filter((c) => c.status === "Ready").length;
  const failedCount = clips.filter((c) => c.status === "Failed").length;
  const processingCount = clips.filter((c) => c.status === "Rendering" || c.status === "Pending").length;

  function handleApprove() {
    setDialogOpen(false);
    approveMutation.mutate(
      { backend },
      {
        onSuccess: () => {
          toast.success("Animation job approved — clips are being processed.");
        },
        onError: (err) => {
          toast.error(err.message ?? "Failed to approve animation.");
        },
      },
    );
  }

  const isApproving = approveMutation.isPending;
  const hasApproved = approveMutation.isSuccess || clips.length > 0;

  return (
    <main className="p-6 max-w-5xl mx-auto space-y-10">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 mb-1">
          <Film className="h-5 w-5 text-violet-500" />
          <h1 className="text-2xl font-bold">Animation Approval</h1>
        </div>
        <p className="text-sm text-muted-foreground">
          Episode ID: <span className="font-mono">{episodeId}</span>
        </p>
      </div>

      {/* Section 1 — Cost Estimator */}
      <section className="space-y-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Cost Estimator
        </h2>

        <BackendSelector selectedBackend={backend} onSelect={setBackend} />

        {estimateLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-8 w-full" />
            <Skeleton className="h-8 w-full" />
            <Skeleton className="h-8 w-3/4" />
          </div>
        ) : (
          <CostBreakdownTable
            scenes={scenes}
            ratePerShot={estimate?.unitCostUsd ?? 0}
            backend={backend}
          />
        )}

        <div className="flex items-center justify-between">
          {estimate && (
            <p className="text-sm text-muted-foreground">
              {estimate.shotCount} shot{estimate.shotCount !== 1 ? "s" : ""} ·{" "}
              <span className="font-medium text-foreground">
                ${estimate.totalCostUsd.toFixed(3)} total
              </span>
            </p>
          )}
          <div className="ml-auto flex gap-2">
            {hasApproved && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => void refetchClips()}
                className="gap-1.5"
              >
                <RefreshCw className="h-3.5 w-3.5" />
                Refresh
              </Button>
            )}
            <Button
              onClick={() => setDialogOpen(true)}
              disabled={isApproving || estimateLoading || !estimate}
            >
              {isApproving ? "Approving…" : hasApproved ? "Re-approve" : "Approve & Process"}
            </Button>
          </div>
        </div>
      </section>

      {/* Section 2 — Clip Previews */}
      {(hasApproved || clipsLoading) && (
        <section className="space-y-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Clip Previews
          </h2>

          {clipsLoading ? (
            <div className="grid grid-cols-4 gap-3">
              {Array.from({ length: 8 }).map((_, i) => (
                <Skeleton key={i} className="aspect-video rounded-xl" />
              ))}
            </div>
          ) : (
            <Tabs value={activeTab} onValueChange={setActiveTab}>
              <TabsList>
                <TabsTrigger value="all">All ({clips.length})</TabsTrigger>
                <TabsTrigger value="ready">Ready ({readyCount})</TabsTrigger>
                <TabsTrigger value="processing">Processing ({processingCount})</TabsTrigger>
                <TabsTrigger value="failed">Failed ({failedCount})</TabsTrigger>
              </TabsList>

              {["all", "ready", "processing", "failed"].map((tab) => (
                <TabsContent key={tab} value={tab} className="pt-4">
                  {filteredClips.length === 0 ? (
                    <p className="text-sm text-muted-foreground py-6 text-center">
                      No clips in this category.
                    </p>
                  ) : (
                    <ClipPreviewGrid clips={filteredClips} groupByScene={tab === "all"} />
                  )}
                </TabsContent>
              ))}
            </Tabs>
          )}
        </section>
      )}

      {/* Approval Dialog */}
      <ApprovalDialog
        isOpen={dialogOpen}
        isLoading={isApproving}
        backend={backend}
        estimate={estimate}
        onConfirm={handleApprove}
        onClose={() => setDialogOpen(false)}
      />
    </main>
  );
}
