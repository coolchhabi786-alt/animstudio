"use client";

import { useRef, useState } from "react";
import { Play, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props {
  voiceName: string;
  characterName: string;
  sampleText: string;
  onPlay: () => void;
}

// Royalty-free short audio sample used as a stand-in for TTS preview
const DEMO_AUDIO_URL =
  "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3";

export function AudioPreviewPlayer({ voiceName, characterName, sampleText, onPlay }: Props) {
  const audioRef = useRef<HTMLAudioElement>(null);
  const [phase, setPhase] = useState<"idle" | "loading" | "playing">("idle");

  function handlePlayPreview() {
    if (phase !== "idle") return;
    onPlay();
    setPhase("loading");
    setTimeout(() => {
      setPhase("playing");
      audioRef.current?.play();
    }, 2000);
  }

  function handleEnded() {
    setPhase("idle");
  }

  return (
    <div className="flex items-center gap-2">
      {phase === "idle" && (
        <Button
          variant="outline"
          size="sm"
          className="gap-1.5"
          onClick={handlePlayPreview}
          aria-label={`Play voice preview for ${characterName}`}
        >
          <Play className="h-3.5 w-3.5" />
          Play Preview
        </Button>
      )}

      {phase === "loading" && (
        <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span>Generating…</span>
        </div>
      )}

      {phase === "playing" && (
        <audio
          ref={audioRef}
          src={DEMO_AUDIO_URL}
          controls
          onEnded={handleEnded}
          className="h-8 w-52"
          aria-label={`${voiceName} voice preview for ${characterName}`}
        />
      )}

      {/* Pre-mount audio element so it's ready when phase === playing */}
      {phase !== "playing" && (
        <audio ref={audioRef} src={DEMO_AUDIO_URL} preload="auto" className="hidden" />
      )}
    </div>
  );
}
