"use client";

import { useEffect, useRef, useState } from "react";
import { Loader2, PlayCircle, RotateCw, AlertTriangle } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { fetchSignedClipUrl } from "@/hooks/use-animation";
import type { AnimationClipDto } from "@/types";

interface Props {
  clip: AnimationClipDto;
  episodeId: string;
}

export function ClipPlayer({ clip, episodeId }: Props) {
  const [signedUrl, setSignedUrl] = useState<string | null>(null);
  const [isFetching, setIsFetching] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loop, setLoop] = useState(false);
  const videoRef = useRef<HTMLVideoElement | null>(null);

  async function loadSignedUrl() {
    if (!clip.clipUrl || clip.status !== "Ready") return;
    setIsFetching(true);
    setError(null);
    try {
      const res = await fetchSignedClipUrl(episodeId, clip.id);
      setSignedUrl(res.url);
    } catch {
      setError("Failed to fetch signed URL");
    } finally {
      setIsFetching(false);
    }
  }

  useEffect(() => {
    if (clip.status === "Ready" && !signedUrl) void loadSignedUrl();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [clip.status, clip.clipUrl]);

  useEffect(() => {
    if (videoRef.current) videoRef.current.loop = loop;
  }, [loop]);

  const statusColor =
    clip.status === "Ready"
      ? "bg-emerald-100 text-emerald-700 border-emerald-200"
      : clip.status === "Failed"
      ? "bg-red-100 text-red-700 border-red-200"
      : clip.status === "Rendering"
      ? "bg-indigo-100 text-indigo-700 border-indigo-200"
      : "bg-gray-100 text-gray-600 border-gray-200";

  return (
    <div className="border rounded-lg overflow-hidden bg-white shadow-sm flex flex-col">
      <div className="relative aspect-video bg-gray-900 flex items-center justify-center">
        {clip.status === "Ready" && signedUrl ? (
          <video
            ref={videoRef}
            src={signedUrl}
            controls
            loop={loop}
            className="h-full w-full object-contain bg-black"
          />
        ) : clip.status === "Failed" ? (
          <div className="flex flex-col items-center gap-2 text-red-400 text-xs">
            <AlertTriangle className="h-5 w-5" />
            Render failed
          </div>
        ) : clip.status === "Rendering" || isFetching ? (
          <div className="flex flex-col items-center gap-2 text-indigo-200 text-xs">
            <Loader2 className="h-5 w-5 animate-spin" />
            {clip.status === "Rendering" ? "Rendering…" : "Signing URL…"}
          </div>
        ) : (
          <div className="flex flex-col items-center gap-2 text-gray-400 text-xs">
            <PlayCircle className="h-5 w-5" />
            Queued
          </div>
        )}

        <Badge className="absolute top-2 left-2 bg-black/70 text-white border-0">
          S{clip.sceneNumber}·{clip.shotIndex}
        </Badge>

        <Badge
          variant="outline"
          className={`absolute top-2 right-2 ${statusColor}`}
        >
          {clip.status}
        </Badge>
      </div>

      <div className="p-3 flex items-center justify-between gap-2">
        <div className="text-xs text-muted-foreground">
          {clip.durationSeconds
            ? `${clip.durationSeconds.toFixed(1)}s`
            : "—"}
        </div>
        <div className="flex gap-2">
          {clip.status === "Ready" && (
            <Button
              size="sm"
              variant={loop ? "default" : "outline"}
              onClick={() => setLoop((v) => !v)}
              className="gap-1"
            >
              <RotateCw className="h-3.5 w-3.5" />
              Loop
            </Button>
          )}
          {error && (
            <Button size="sm" variant="outline" onClick={loadSignedUrl}>
              Retry
            </Button>
          )}
        </div>
      </div>

      {error && (
        <p className="px-3 pb-2 text-[11px] text-red-600">{error}</p>
      )}
    </div>
  );
}

export function ClipPlayerSkeleton() {
  return (
    <div className="border rounded-lg overflow-hidden bg-white shadow-sm">
      <Skeleton className="aspect-video w-full" />
      <div className="p-3 flex items-center justify-between">
        <Skeleton className="h-3 w-10" />
        <Skeleton className="h-7 w-16" />
      </div>
    </div>
  );
}
