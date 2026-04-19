"use client";

import { useRef, useState } from "react";
import { Play, Pause, Volume2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";

interface Props {
  audioUrl: string | undefined;
  isLoading?: boolean;
}

export function AudioPreviewPlayer({ audioUrl, isLoading }: Props) {
  const audioRef = useRef<HTMLAudioElement>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [progress, setProgress] = useState(0);

  function handlePlayPause() {
    const audio = audioRef.current;
    if (!audio) return;

    if (isPlaying) {
      audio.pause();
    } else {
      audio.play();
    }
    setIsPlaying(!isPlaying);
  }

  function handleTimeUpdate() {
    const audio = audioRef.current;
    if (!audio || !audio.duration) return;
    setProgress((audio.currentTime / audio.duration) * 100);
  }

  function handleEnded() {
    setIsPlaying(false);
    setProgress(0);
  }

  if (!audioUrl) {
    return (
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <Volume2 className="h-4 w-4" />
        <span>{isLoading ? "Generating..." : "No preview"}</span>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-2">
      <audio
        ref={audioRef}
        src={audioUrl}
        onTimeUpdate={handleTimeUpdate}
        onEnded={handleEnded}
        preload="auto"
      />
      <Button
        variant="ghost"
        size="icon"
        className="h-8 w-8"
        onClick={handlePlayPause}
        aria-label={isPlaying ? "Pause preview" : "Play preview"}
      >
        {isPlaying ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
      </Button>
      <Progress value={progress} className="w-24 h-2" />
    </div>
  );
}
