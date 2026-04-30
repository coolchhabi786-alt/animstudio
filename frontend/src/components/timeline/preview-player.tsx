"use client";

import { useRef, useEffect, useCallback } from "react";
import { useTimelineStore } from "@/stores/timelineStore";

interface PreviewPlayerProps {
  videoUrl: string;
  durationMs: number;
}

export function PreviewPlayer({ videoUrl }: PreviewPlayerProps) {
  const videoRef      = useRef<HTMLVideoElement>(null);
  const internalSeek  = useRef(false); // prevents feedback loops

  const playheadMs  = useTimelineStore((s) => s.playheadPositionMs);
  const isPlaying   = useTimelineStore((s) => s.isPlaying);
  const setPlayhead = useTimelineStore((s) => s.setPlayheadPosition);
  const play        = useTimelineStore((s) => s.play);
  const pause       = useTimelineStore((s) => s.pause);

  // Timeline playhead → video seek
  useEffect(() => {
    const video = videoRef.current;
    if (!video || internalSeek.current) return;
    const targetSec = playheadMs / 1000;
    if (Math.abs(video.currentTime - targetSec) > 0.15) {
      video.currentTime = targetSec;
    }
  }, [playheadMs]);

  // Store isPlaying → video play / pause
  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;
    if (isPlaying) {
      video.play().catch(() => {});
    } else {
      video.pause();
    }
  }, [isPlaying]);

  // Video timeupdate → store playhead (video is the source of truth while playing)
  const handleTimeUpdate = useCallback(() => {
    const video = videoRef.current;
    if (!video) return;
    internalSeek.current = true;
    setPlayhead(video.currentTime * 1000);
    internalSeek.current = false;
  }, [setPlayhead]);

  const handlePlay  = useCallback(() => play(),  [play]);
  const handlePause = useCallback(() => pause(), [pause]);

  return (
    <div className="relative w-full bg-black rounded-lg overflow-hidden aspect-video shadow-xl">
      <video
        ref={videoRef}
        src={videoUrl}
        className="w-full h-full object-contain"
        onTimeUpdate={handleTimeUpdate}
        onPlay={handlePlay}
        onPause={handlePause}
        controls
        preload="metadata"
      />
    </div>
  );
}
