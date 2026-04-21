"use client";

import { useEffect, useRef, useState } from "react";
import { ChevronDown, ChevronRight, Play, AlertTriangle, Loader2, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import type { MockAnimationClip, AnimationStatus } from "@/lib/mock-data";

interface Props {
  clips: MockAnimationClip[];
  groupByScene?: boolean;
}

const STATUS_STYLES: Record<AnimationStatus, string> = {
  ready: "bg-emerald-100 text-emerald-700 border-emerald-200",
  processing: "bg-indigo-100 text-indigo-700 border-indigo-200",
  queued: "bg-gray-100 text-gray-600 border-gray-200",
  failed: "bg-red-100 text-red-700 border-red-200",
};

const STATUS_LABEL: Record<AnimationStatus, string> = {
  ready: "Ready",
  processing: "Processing",
  queued: "Queued",
  failed: "Failed",
};

// ── Thumbnail ─────────────────────────────────────────────────────────────────

function VideoThumbnail({
  src,
  onCapture,
}: {
  src: string;
  onCapture: (dataUrl: string) => void;
}) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const captured = useRef(false);

  function handleLoadedData() {
    const video = videoRef.current;
    if (!video || captured.current) return;
    video.currentTime = 0.5;
  }

  function handleSeeked() {
    const video = videoRef.current;
    if (!video || captured.current) return;
    captured.current = true;
    try {
      const canvas = document.createElement("canvas");
      canvas.width = video.videoWidth || 320;
      canvas.height = video.videoHeight || 180;
      const ctx = canvas.getContext("2d");
      ctx?.drawImage(video, 0, 0, canvas.width, canvas.height);
      onCapture(canvas.toDataURL("image/jpeg", 0.8));
    } catch {
      // cross-origin or codec issue — ignore, fallback shown
    }
  }

  return (
    <video
      ref={videoRef}
      src={src}
      preload="metadata"
      crossOrigin="anonymous"
      className="hidden"
      onLoadedData={handleLoadedData}
      onSeeked={handleSeeked}
    />
  );
}

// ── Single clip card ──────────────────────────────────────────────────────────

function ClipCard({
  clip,
  onOpen,
}: {
  clip: MockAnimationClip;
  onOpen: () => void;
}) {
  const [thumbnail, setThumbnail] = useState<string | null>(null);

  const isReady = clip.status === "ready";

  return (
    <>
      {/* Hidden video used only for thumbnail extraction */}
      {isReady && (
        <VideoThumbnail src={clip.clipUrl} onCapture={setThumbnail} />
      )}

      <div
        className="relative w-[180px] flex-shrink-0 rounded-xl overflow-hidden border bg-gray-950 cursor-pointer group shadow-sm hover:shadow-md transition-shadow"
        style={{ aspectRatio: "16 / 9" }}
        onClick={isReady ? onOpen : undefined}
        title={isReady ? `Scene ${clip.sceneNumber} · Shot ${clip.shotIndex}` : undefined}
      >
        {/* Thumbnail / state display */}
        {isReady && thumbnail ? (
          <img
            src={thumbnail}
            alt={`Scene ${clip.sceneNumber} Shot ${clip.shotIndex}`}
            className="w-full h-full object-cover"
          />
        ) : isReady && !thumbnail ? (
          <div className="w-full h-full bg-gray-800 animate-pulse" />
        ) : clip.status === "failed" ? (
          <div className="w-full h-full flex flex-col items-center justify-center gap-1 text-red-400">
            <AlertTriangle className="h-6 w-6" />
            <span className="text-[10px]">Failed</span>
          </div>
        ) : (
          <div className="w-full h-full flex flex-col items-center justify-center gap-1 text-gray-500">
            <Loader2 className="h-6 w-6 animate-spin" />
            <span className="text-[10px] capitalize">{clip.status}</span>
          </div>
        )}

        {/* Play overlay on hover (ready only) */}
        {isReady && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity">
            <div className="rounded-full bg-white/90 p-2.5">
              <Play className="h-5 w-5 text-gray-900 fill-gray-900" />
            </div>
          </div>
        )}

        {/* Status badge */}
        <Badge
          variant="outline"
          className={`absolute top-1.5 left-1.5 text-[9px] px-1.5 py-0.5 leading-none border ${STATUS_STYLES[clip.status]}`}
        >
          {STATUS_LABEL[clip.status]}
        </Badge>

        {/* Shot label */}
        <span className="absolute bottom-1.5 right-1.5 text-[9px] text-white bg-black/60 rounded px-1 leading-tight">
          S{clip.sceneNumber}·{clip.shotIndex}
        </span>
      </div>
    </>
  );
}

// ── Fullscreen popup player ────────────────────────────────────────────────────

function ClipDialog({
  clip,
  open,
  onClose,
}: {
  clip: MockAnimationClip | null;
  open: boolean;
  onClose: () => void;
}) {
  return (
    <Dialog open={open} onOpenChange={(v) => !v && onClose()}>
      <DialogContent className="max-w-3xl w-full p-0 overflow-hidden bg-gray-950 border-gray-800">
        <DialogHeader className="flex flex-row items-center justify-between px-5 py-3 bg-gray-900 border-b border-gray-800">
          <DialogTitle className="text-sm font-semibold text-white">
            {clip
              ? `Scene ${clip.sceneNumber} · Shot ${clip.shotIndex}`
              : "Preview"}
          </DialogTitle>
          <Button
            size="icon"
            variant="ghost"
            onClick={onClose}
            className="h-7 w-7 text-gray-400 hover:text-white"
          >
            <X className="h-4 w-4" />
          </Button>
        </DialogHeader>

        {clip && (
          <div className="aspect-video w-full bg-black">
            <video
              key={clip.id}
              src={clip.clipUrl}
              controls
              autoPlay
              loop
              className="w-full h-full"
              controlsList="nodownload"
            />
          </div>
        )}

        {clip && (
          <div className="px-5 py-3 bg-gray-900 border-t border-gray-800 flex items-center gap-4 text-xs text-gray-400">
            <span>Duration: {clip.durationSeconds}s</span>
            <span>Backend: {clip.backend}</span>
            <span>Cost: ${clip.costUsd.toFixed(3)}</span>
            <Badge
              variant="outline"
              className={`ml-auto text-[10px] border ${STATUS_STYLES[clip.status]}`}
            >
              {STATUS_LABEL[clip.status]}
            </Badge>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

// ── Scene group (accordion) ────────────────────────────────────────────────────

function SceneGroup({
  sceneNumber,
  clips,
  onOpenClip,
}: {
  sceneNumber: number;
  clips: MockAnimationClip[];
  onOpenClip: (clip: MockAnimationClip) => void;
}) {
  const [open, setOpen] = useState(true);
  const readyCount = clips.filter((c) => c.status === "ready").length;

  return (
    <div className="border rounded-xl overflow-hidden">
      <button
        className="w-full flex items-center gap-2 px-4 py-3 bg-muted/50 hover:bg-muted/80 transition-colors text-left"
        onClick={() => setOpen((v) => !v)}
      >
        {open ? (
          <ChevronDown className="h-4 w-4 text-muted-foreground flex-shrink-0" />
        ) : (
          <ChevronRight className="h-4 w-4 text-muted-foreground flex-shrink-0" />
        )}
        <span className="font-semibold text-sm">Scene {sceneNumber}</span>
        <span className="text-xs text-muted-foreground">
          {clips.length} clip{clips.length !== 1 ? "s" : ""}
        </span>
        <span className="ml-auto text-xs text-emerald-600 font-medium">
          {readyCount} ready
        </span>
      </button>

      {open && (
        <div className="p-4 flex flex-wrap gap-3">
          {clips.map((clip) => (
            <ClipCard key={clip.id} clip={clip} onOpen={() => onOpenClip(clip)} />
          ))}
        </div>
      )}
    </div>
  );
}

// ── Public component ──────────────────────────────────────────────────────────

export function ClipPreviewGrid({ clips, groupByScene = false }: Props) {
  const [activeClip, setActiveClip] = useState<MockAnimationClip | null>(null);

  function openClip(clip: MockAnimationClip) {
    if (clip.status === "ready") setActiveClip(clip);
  }

  function closeClip() {
    setActiveClip(null);
  }

  const scenes = [...new Set(clips.map((c) => c.sceneNumber))].sort((a, b) => a - b);

  return (
    <>
      {groupByScene ? (
        <div className="space-y-3">
          {scenes.map((sceneNumber) => (
            <SceneGroup
              key={sceneNumber}
              sceneNumber={sceneNumber}
              clips={clips.filter((c) => c.sceneNumber === sceneNumber)}
              onOpenClip={openClip}
            />
          ))}
        </div>
      ) : (
        <div className="flex flex-wrap gap-3">
          {clips.map((clip) => (
            <ClipCard key={clip.id} clip={clip} onOpen={() => openClip(clip)} />
          ))}
        </div>
      )}

      <ClipDialog clip={activeClip} open={!!activeClip} onClose={closeClip} />
    </>
  );
}
