"use client";

import { Download, Subtitles } from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props {
  renderId: string;
  videoUrl: string;
  srtUrl?: string | null;
}

function triggerDownload(url: string, filename: string) {
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.target = "_blank";
  a.rel = "noopener noreferrer";
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
}

export function DownloadBar({ renderId, videoUrl, srtUrl }: Props) {
  const shortId = renderId.slice(0, 8);

  return (
    <div className="flex items-center gap-3 rounded-lg border p-4 bg-emerald-50 border-emerald-200">
      <span className="text-sm font-medium text-emerald-700 flex-1">
        Your render is ready to download.
      </span>
      <Button
        size="sm"
        onClick={() => triggerDownload(videoUrl, `render-${shortId}.mp4`)}
        className="gap-1.5"
      >
        <Download className="h-4 w-4" />
        Download MP4
      </Button>
      {srtUrl && (
        <Button
          size="sm"
          variant="outline"
          onClick={() => triggerDownload(srtUrl, `render-${shortId}.srt`)}
          className="gap-1.5"
        >
          <Subtitles className="h-4 w-4" />
          Download SRT
        </Button>
      )}
    </div>
  );
}
