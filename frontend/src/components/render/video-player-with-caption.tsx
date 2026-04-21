"use client";

import { useRef, useState } from "react";
import { Subtitles, CaptionsOff } from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props {
  videoUrl: string;
  captionUrl?: string | null;
}

export function VideoPlayerWithCaption({ videoUrl, captionUrl }: Props) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [captionsOn, setCaptionsOn] = useState(true);

  function toggleCaptions() {
    const video = videoRef.current;
    if (!video) return;
    const newState = !captionsOn;
    setCaptionsOn(newState);
    for (let i = 0; i < video.textTracks.length; i++) {
      video.textTracks[i].mode = newState ? "showing" : "hidden";
    }
  }

  return (
    <div className="rounded-xl overflow-hidden border bg-black relative">
      <video
        ref={videoRef}
        src={videoUrl}
        controls
        className="w-full aspect-video"
        controlsList="nodownload"
      >
        {captionUrl && (
          <track
            kind="subtitles"
            src={captionUrl}
            srcLang="en"
            label="English"
            default={captionsOn}
          />
        )}
      </video>

      {captionUrl && (
        <div className="absolute top-2 right-2">
          <Button
            size="icon"
            variant="secondary"
            className="h-8 w-8 bg-black/60 hover:bg-black/80 text-white border-0"
            onClick={toggleCaptions}
            title={captionsOn ? "Hide captions" : "Show captions"}
          >
            {captionsOn ? (
              <Subtitles className="h-4 w-4" />
            ) : (
              <CaptionsOff className="h-4 w-4" />
            )}
          </Button>
        </div>
      )}
    </div>
  );
}
