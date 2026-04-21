"use client";

import { useEffect, useRef, useState } from "react";
import { Film } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { BackendSelector } from "@/components/animation/backend-selector";
import { CostBreakdownTable } from "@/components/animation/cost-breakdown-table";
import { ClipPreviewGrid } from "@/components/animation/clip-preview-grid";
import { ApprovalDialog } from "@/components/animation/approval-dialog";
import {
  mockAnimationClips,
  ANIMATION_COST_PER_CLIP,
} from "@/lib/mock-data";
import type { AnimationBackend } from "@/types";
import type { AnimationStatus } from "@/lib/mock-data";

const MOCK_BALANCE = 45.33;
const MOCK_EPISODE_ID = "ep-0011-2222-3333-4444-555566667777";

const SCENES = [
  { sceneNumber: 1, shotCount: 4 },
  { sceneNumber: 2, shotCount: 4 },
  { sceneNumber: 3, shotCount: 4 },
];

type ProcessingState = "idle" | "processing" | "done";

const STATUS_FILTER_MAP: Record<string, AnimationStatus[]> = {
  all: ["ready", "processing", "queued", "failed"],
  ready: ["ready"],
  processing: ["processing"],
  failed: ["failed"],
};

export default function AnimationApprovalPage({
  params,
}: {
  params: { id: string };
}) {
  const episodeId = params.id ?? MOCK_EPISODE_ID;

  const [backend, setBackend] = useState<AnimationBackend>("Kling");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [processingState, setProcessingState] = useState<ProcessingState>("idle");
  const [progress, setProgress] = useState(0);
  const [statusText, setStatusText] = useState("");
  const [activeTab, setActiveTab] = useState("all");

  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const ratePerShot = ANIMATION_COST_PER_CLIP[backend === "Kling" ? "kling" : "local"];
  const totalShots = mockAnimationClips.length;
  const totalCost = totalShots * ratePerShot;

  const filteredClips = mockAnimationClips.filter((c) =>
    STATUS_FILTER_MAP[activeTab]?.includes(c.status),
  );

  const MOCK_ESTIMATE = {
    episodeId,
    backend,
    shotCount: totalShots,
    unitCostUsd: ratePerShot,
    totalCostUsd: totalCost,
    breakdown: mockAnimationClips.map((c) => ({
      sceneNumber: c.sceneNumber,
      shotIndex: c.shotIndex,
      storyboardShotId: undefined,
      unitCostUsd: ratePerShot,
    })),
  };

  function handleApprove() {
    setDialogOpen(false);
    setProcessingState("processing");
    setProgress(0);
    setStatusText("Queuing render jobs…");

    let pct = 0;
    intervalRef.current = setInterval(() => {
      pct += 5;
      setProgress(pct);
      if (pct < 20) setStatusText("Initialising Kling AI pipeline…");
      else if (pct < 50) setStatusText(`Rendering clip ${Math.ceil((pct / 100) * totalShots)} of ${totalShots}…`);
      else if (pct < 80) setStatusText("Encoding output…");
      else setStatusText("Finalising…");

      if (pct >= 100) {
        clearInterval(intervalRef.current!);
        setProcessingState("done");
        setStatusText("All clips rendered successfully");
      }
    }, 1000);
  }

  useEffect(() => {
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);

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

        <CostBreakdownTable
          scenes={SCENES}
          ratePerShot={ratePerShot}
          backend={backend}
        />

        <div className="flex justify-end">
          <Button
            onClick={() => setDialogOpen(true)}
            disabled={processingState === "processing"}
          >
            {processingState === "done"
              ? "Re-approve"
              : "Approve & Process"}
          </Button>
        </div>
      </section>

      {/* Section 2 — Processing Progress */}
      {processingState !== "idle" && (
        <section className="space-y-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Processing Progress
          </h2>
          <div className="rounded-lg border p-5 space-y-3">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">{statusText}</span>
              <span className="font-bold tabular-nums">{progress}%</span>
            </div>
            <Progress value={progress} />
            {processingState === "done" && (
              <p className="text-xs text-emerald-600 font-medium">
                All {totalShots} clips rendered successfully.
              </p>
            )}
          </div>
        </section>
      )}

      {/* Section 3 — Clip Previews */}
      <section className="space-y-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Clip Previews
        </h2>

        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList>
            <TabsTrigger value="all">All ({mockAnimationClips.length})</TabsTrigger>
            <TabsTrigger value="ready">
              Ready ({mockAnimationClips.filter((c) => c.status === "ready").length})
            </TabsTrigger>
            <TabsTrigger value="processing">
              Processing ({mockAnimationClips.filter((c) => c.status === "processing").length})
            </TabsTrigger>
            <TabsTrigger value="failed">
              Failed ({mockAnimationClips.filter((c) => c.status === "failed").length})
            </TabsTrigger>
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
      </section>

      {/* Approval Dialog */}
      <ApprovalDialog
        isOpen={dialogOpen}
        isLoading={false}
        backend={backend}
        estimate={MOCK_ESTIMATE}
        onConfirm={handleApprove}
        onClose={() => setDialogOpen(false)}
        mockBalance={MOCK_BALANCE}
      />
    </main>
  );
}
