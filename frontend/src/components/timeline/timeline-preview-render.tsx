"use client";

import { useState } from "react";
import {
  Video, Loader2, CheckCircle2, XCircle, ChevronDown, ChevronUp, AlertCircle,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { PreviewPlayer } from "./preview-player";
import type { Timeline } from "@/types/timeline";
import type { RenderAspectRatio } from "@/types";
import { ASPECT_RATIO_DISPLAY } from "@/types";
import { useRenderHistory, useStartRender, useRender } from "@/hooks/use-render";

interface TimelinePreviewRenderProps {
  timeline: Timeline;
  episodeId?: string;
}

export function TimelinePreviewRender({ timeline, episodeId }: TimelinePreviewRenderProps) {
  const [expanded,       setExpanded]       = useState(false);
  const [activeRenderId, setActiveRenderId] = useState<string | null>(null);
  const [aspectRatio,    setAspectRatio]    = useState<RenderAspectRatio>("SixteenNine");

  // Fetch render history for this episode
  const { data: history } = useRenderHistory(episodeId);

  // Poll the render we just triggered until it settles
  const { data: polledRender } = useRender(
    activeRenderId ?? undefined,
  );

  // Start a new render
  const { mutate: startRender, isPending: isStarting } = useStartRender(episodeId ?? "");

  // The actively-tracked render (polled if just triggered, else from history)
  const activeRender = polledRender
    ?? (activeRenderId ? history?.find((r) => r.id === activeRenderId) : null);

  // Most recent complete render from history (for showing an existing preview)
  const latestComplete = history?.find((r) => r.status === "Complete");

  // Video URL: prefer the render we just started; fallback to latest complete
  const previewRender = activeRender?.status === "Complete" ? activeRender : latestComplete;
  const videoUrl      = previewRender?.finalVideoUrl ?? previewRender?.cdnUrl ?? null;

  const isRendering =
    activeRender?.status === "Pending" || activeRender?.status === "Rendering";

  function handleGenerate() {
    if (!episodeId) return;
    startRender(
      { aspectRatio },
      {
        onSuccess: (r) => {
          setActiveRenderId(r.id);
          setExpanded(true);
        },
      },
    );
  }

  // Determine button label/state
  const buttonBusy = isStarting || isRendering;

  return (
    <div className="shrink-0 flex flex-col bg-[#0d1421] border-t border-slate-800">
      {/* ── Header bar ─────────────────────────────────────────────────── */}
      <div className="flex items-center gap-3 px-4 py-2">

        {/* Aspect ratio selector */}
        {episodeId && (
          <select
            value={aspectRatio}
            onChange={(e) => setAspectRatio(e.target.value as RenderAspectRatio)}
            disabled={buttonBusy}
            className="h-7 rounded bg-slate-800 border border-slate-700 text-xs text-slate-300 px-1.5 focus:outline-none"
          >
            {(Object.keys(ASPECT_RATIO_DISPLAY) as RenderAspectRatio[]).map((ar) => (
              <option key={ar} value={ar}>{ASPECT_RATIO_DISPLAY[ar]}</option>
            ))}
          </select>
        )}

        {/* Generate button */}
        <Button
          size="sm"
          variant="ghost"
          className={`h-8 gap-1.5 text-xs shrink-0 ${
            buttonBusy
              ? "text-blue-400 bg-blue-500/10"
              : activeRender?.status === "Complete"
              ? "text-emerald-400"
              : activeRender?.status === "Failed"
              ? "text-red-400"
              : "text-slate-400 hover:text-slate-200"
          }`}
          onClick={handleGenerate}
          disabled={buttonBusy || !episodeId}
          title={!episodeId ? "No episode — timeline is using demo data" : undefined}
        >
          {buttonBusy ? (
            <Loader2 className="h-3.5 w-3.5 animate-spin" />
          ) : activeRender?.status === "Complete" ? (
            <CheckCircle2 className="h-3.5 w-3.5" />
          ) : activeRender?.status === "Failed" ? (
            <XCircle className="h-3.5 w-3.5" />
          ) : (
            <Video className="h-3.5 w-3.5" />
          )}
          {buttonBusy
            ? isStarting ? "Starting…" : "Rendering…"
            : activeRender?.status === "Complete"
            ? "Re-render"
            : activeRender?.status === "Failed"
            ? "Retry"
            : "Generate Preview"}
        </Button>

        {/* No episode warning */}
        {!episodeId && (
          <span className="flex items-center gap-1 text-[10px] text-amber-500/70">
            <AlertCircle className="h-3 w-3" />
            Demo mode — rendering unavailable
          </span>
        )}

        {/* Render status / progress text */}
        {episodeId && activeRender && (
          <span className={`text-[10px] ${
            activeRender.status === "Complete" ? "text-emerald-400"
            : activeRender.status === "Failed"  ? "text-red-400"
            : "text-slate-400"
          }`}>
            {activeRender.status === "Pending"   ? "Queued…"
              : activeRender.status === "Rendering" ? "Processing…"
              : activeRender.status === "Complete"  ? `Done · ${activeRender.durationSeconds.toFixed(1)}s`
              : activeRender.errorMessage ?? "Failed"}
          </span>
        )}

        {/* Inline progress bar */}
        {isRendering && (
          <div className="flex-1 h-1.5 bg-slate-700 rounded-full overflow-hidden">
            <div className="h-full bg-blue-500 rounded-full animate-pulse w-1/3" />
          </div>
        )}

        {/* Show/hide toggle when there's something to show */}
        {videoUrl && (
          <button
            className="ml-auto flex items-center gap-1 text-[10px] text-slate-500 hover:text-slate-300 transition-colors"
            onClick={() => setExpanded((v) => !v)}
          >
            {expanded ? "Hide Preview" : "Show Preview"}
            {expanded ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
          </button>
        )}

        {/* Existing complete render note */}
        {!activeRenderId && latestComplete && !videoUrl && (
          <span className="ml-auto text-[10px] text-slate-500">
            Last render: {latestComplete.aspectRatio} · {latestComplete.status}
          </span>
        )}
      </div>

      {/* ── Video player ───────────────────────────────────────────────── */}
      {expanded && videoUrl && (
        <div className="px-4 pb-4 max-h-96">
          <PreviewPlayer videoUrl={videoUrl} durationMs={timeline.durationMs} />
        </div>
      )}
    </div>
  );
}
