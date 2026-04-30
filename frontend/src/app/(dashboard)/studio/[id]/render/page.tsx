"use client";

import { useState } from "react";
import { Clapperboard, Play } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { AspectRatioPicker } from "@/components/render/aspect-ratio-picker";
import { RenderProgressBar } from "@/components/render/render-progress-bar";
import { DownloadBar } from "@/components/render/download-bar";
import { RenderHistoryTable } from "@/components/render/render-history-table";
import { RenderPreviewDialog } from "@/components/render/render-preview-dialog";
import { useRenderHistory, useStartRender } from "@/hooks/use-render";
import { ASPECT_RATIO_DISPLAY, type RenderDto, type RenderAspectRatio } from "@/types";

export default function RenderPage({ params }: { params: { id: string } }) {
  const episodeId = params.id;

  const [aspectRatio, setAspectRatio] = useState<RenderAspectRatio>("SixteenNine");
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewRender, setPreviewRender] = useState<RenderDto | null>(null);

  const { data: history = [], isLoading: historyLoading } = useRenderHistory(episodeId);
  const startMutation = useStartRender(episodeId);

  // The latest render drives the progress display
  const latest = history[0] ?? null;
  const isRendering = latest?.status === "Pending" || latest?.status === "Rendering";
  const isComplete = latest?.status === "Complete";

  function handleStartRender() {
    startMutation.mutate(
      { aspectRatio },
      {
        onSuccess: () => {
          toast.success("Render started — this may take a few moments.");
        },
        onError: (err) => {
          toast.error(err.message ?? "Failed to start render.");
        },
      },
    );
  }

  function openPreview(render: RenderDto) {
    setPreviewRender(render);
    setPreviewOpen(true);
  }

  function handleRerender(render: RenderDto) {
    setAspectRatio(render.aspectRatio);
  }

  const currentStage = isRendering ? "Rendering…" : isComplete ? "Done" : "";
  const percent = isComplete ? 100 : isRendering ? 50 : 0;

  return (
    <main className="p-6 max-w-5xl mx-auto space-y-10">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 mb-1">
          <Clapperboard className="h-5 w-5 text-violet-500" />
          <h1 className="text-2xl font-bold">Post-Production Render</h1>
        </div>
        <p className="text-sm text-muted-foreground">
          Episode ID: <span className="font-mono">{episodeId}</span>
        </p>
      </div>

      {/* Two-column layout */}
      <div className="grid grid-cols-1 lg:grid-cols-[2fr_3fr] gap-8">
        {/* Left — output settings */}
        <section className="space-y-5">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Output Settings
          </h2>
          <AspectRatioPicker selected={aspectRatio} onSelect={setAspectRatio} />
          <Button
            className="w-full"
            onClick={handleStartRender}
            disabled={startMutation.isPending || isRendering}
          >
            {startMutation.isPending || isRendering
              ? "Rendering…"
              : isComplete
              ? "Start New Render"
              : "Start Render"}
          </Button>
        </section>

        {/* Right — progress + result */}
        <section className="space-y-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            {isComplete ? "Preview & Download" : "Render Progress"}
          </h2>

          {!latest && !startMutation.isPending && (
            <p className="text-sm text-muted-foreground py-4">
              Select your output settings and click Start Render to begin.
            </p>
          )}

          {(isRendering || startMutation.isPending) && (
            <RenderProgressBar
              percent={percent}
              currentStage={currentStage}
              isComplete={false}
            />
          )}

          {isComplete && latest && (
            <>
              <RenderProgressBar percent={100} currentStage="Done" isComplete />

              <DownloadBar
                renderId={latest.id}
                videoUrl={latest.cdnUrl ?? ""}
                srtUrl={latest.srtUrl}
              />

              {latest.cdnUrl && (
                <div
                  className="relative rounded-xl overflow-hidden bg-gray-950 border cursor-pointer group"
                  style={{
                    aspectRatio:
                      latest.aspectRatio === "NineSixteen"
                        ? "9/16"
                        : latest.aspectRatio === "OneOne"
                        ? "1/1"
                        : "16/9",
                  }}
                  onClick={() => openPreview(latest)}
                >
                  <video
                    src={latest.cdnUrl}
                    className="w-full h-full object-cover"
                    preload="metadata"
                    muted
                  />
                  <div className="absolute inset-0 flex items-center justify-center bg-black/50 group-hover:bg-black/40 transition-colors">
                    <div className="rounded-full bg-white/90 p-4 shadow-lg group-hover:scale-110 transition-transform">
                      <Play className="h-7 w-7 text-gray-900 fill-gray-900" />
                    </div>
                  </div>
                  <span className="absolute bottom-3 right-3 text-[11px] text-white bg-black/60 rounded px-2 py-0.5">
                    {ASPECT_RATIO_DISPLAY[latest.aspectRatio]} · Click to preview
                  </span>
                </div>
              )}
            </>
          )}
        </section>
      </div>

      {/* History */}
      <section className="space-y-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Render History
        </h2>
        {historyLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-12 w-full rounded-lg" />
            ))}
          </div>
        ) : (
          <RenderHistoryTable
            renders={history}
            onRerender={handleRerender}
            onPreview={openPreview}
          />
        )}
      </section>

      {/* Preview popup */}
      {previewRender?.cdnUrl && (
        <RenderPreviewDialog
          open={previewOpen}
          onClose={() => setPreviewOpen(false)}
          videoUrl={previewRender.cdnUrl}
          aspectRatio={previewRender.aspectRatio}
          renderId={previewRender.id}
        />
      )}
    </main>
  );
}
