"use client";

import { useEffect, useRef, useState } from "react";
import { Clapperboard, Play } from "lucide-react";
import { Button } from "@/components/ui/button";
import { AspectRatioPicker } from "@/components/render/aspect-ratio-picker";
import { RenderProgressBar } from "@/components/render/render-progress-bar";
import { DownloadBar } from "@/components/render/download-bar";
import { RenderHistoryTable } from "@/components/render/render-history-table";
import { RenderPreviewDialog } from "@/components/render/render-preview-dialog";
import { mockRenders, MOCK_RENDER_VIDEO_URL } from "@/lib/mock-data";
import type { MockRender, AspectRatio } from "@/lib/mock-data";

const MOCK_EPISODE_ID = "ep-0011-2222-3333-4444-555566667777";

type RenderState = "idle" | "rendering" | "done";

const STAGE_LABELS: [number, string][] = [
  [0,   "Queued…"],
  [20,  "Assembling video frames…"],
  [50,  "Mixing audio…"],
  [80,  "Finalizing…"],
  [100, "Done"],
];

function stageForPercent(pct: number): string {
  let label = STAGE_LABELS[0][1] as string;
  for (const [threshold, text] of STAGE_LABELS) {
    if (pct >= threshold) label = text;
  }
  return label;
}

export default function RenderPage({ params }: { params: { id: string } }) {
  const episodeId = params.id ?? MOCK_EPISODE_ID;

  const [aspectRatio, setAspectRatio] = useState<AspectRatio>("16:9");
  const [renderState, setRenderState] = useState<RenderState>("idle");
  const [progress, setProgress] = useState(0);
  const [stage, setStage] = useState("");
  const [history, setHistory] = useState<MockRender[]>(mockRenders);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewRender, setPreviewRender] = useState<MockRender | null>(null);

  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const currentRenderId = useRef<string>("");

  function startRender() {
    if (renderState === "rendering") return;

    const newId = `render-${Date.now()}-new`;
    currentRenderId.current = newId;

    setRenderState("rendering");
    setProgress(0);
    setStage("Queued…");

    let pct = 0;
    intervalRef.current = setInterval(() => {
      pct += 5;
      setProgress(pct);
      setStage(stageForPercent(pct));

      if (pct >= 100) {
        clearInterval(intervalRef.current!);
        setRenderState("done");

        const completed: MockRender = {
          id: newId,
          episodeId,
          status: "complete",
          aspectRatio,
          outputFormat: "mp4",
          resolution: "1080p",
          finalVideoUrl: MOCK_RENDER_VIDEO_URL,
          cdnUrl: MOCK_RENDER_VIDEO_URL,
          captionsUrl: null,
          durationSeconds: 74,
          fileSizeMb: 2.8,
          progressPercent: 100,
          currentStage: "Done",
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
        };
        setHistory((prev) => [completed, ...prev]);
        setPreviewRender(completed);
      }
    }, 500);
  }

  function openPreview(render: MockRender) {
    setPreviewRender(render);
    setPreviewOpen(true);
  }

  function handleRerender(render: MockRender) {
    setAspectRatio(render.aspectRatio);
    setRenderState("idle");
    setProgress(0);
    setStage("");
  }

  useEffect(() => {
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);

  const isComplete = renderState === "done";
  const completedRender = isComplete ? previewRender : null;

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
            onClick={startRender}
            disabled={renderState === "rendering"}
          >
            {renderState === "rendering"
              ? "Rendering…"
              : renderState === "done"
              ? "Start New Render"
              : "Start Render"}
          </Button>
        </section>

        {/* Right — progress + result */}
        <section className="space-y-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            {isComplete ? "Preview & Download" : "Render Progress"}
          </h2>

          {renderState === "idle" && (
            <p className="text-sm text-muted-foreground py-4">
              Select your output settings and click Start Render to begin.
            </p>
          )}

          {renderState !== "idle" && (
            <RenderProgressBar
              percent={progress}
              currentStage={stage}
              isComplete={isComplete}
            />
          )}

          {isComplete && completedRender && (
            <>
              <DownloadBar
                renderId={completedRender.id}
                videoUrl={MOCK_RENDER_VIDEO_URL}
                srtUrl={null}
              />

              {/* Preview thumbnail / play button */}
              <div
                className="relative rounded-xl overflow-hidden bg-gray-950 border cursor-pointer group"
                style={{ aspectRatio: aspectRatio === "9:16" ? "9/16" : aspectRatio === "1:1" ? "1/1" : "16/9" }}
                onClick={() => completedRender && openPreview(completedRender)}
              >
                <video
                  src={MOCK_RENDER_VIDEO_URL}
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
                  Click to preview
                </span>
              </div>
            </>
          )}
        </section>
      </div>

      {/* History */}
      <section className="space-y-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Render History
        </h2>
        <RenderHistoryTable
          renders={history}
          onRerender={handleRerender}
          onPreview={openPreview}
        />
      </section>

      {/* Preview popup */}
      {previewRender && (
        <RenderPreviewDialog
          open={previewOpen}
          onClose={() => setPreviewOpen(false)}
          videoUrl={previewRender.finalVideoUrl ?? MOCK_RENDER_VIDEO_URL}
          aspectRatio={previewRender.aspectRatio}
          renderId={previewRender.id}
        />
      )}
    </main>
  );
}
