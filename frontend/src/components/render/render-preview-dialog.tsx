"use client";

import { X, Download } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import type { AspectRatio } from "@/lib/mock-data";

interface Props {
  open: boolean;
  onClose: () => void;
  videoUrl: string;
  aspectRatio: AspectRatio;
  renderId: string;
}

const RATIO_META: Record<AspectRatio, { maxW: string; paddingTop: string; label: string }> = {
  "16:9": { maxW: "max-w-4xl", paddingTop: "56.25%", label: "Landscape 16:9" },
  "9:16": { maxW: "max-w-xs",  paddingTop: "177.78%", label: "Portrait 9:16" },
  "1:1":  { maxW: "max-w-xl",  paddingTop: "100%",    label: "Square 1:1" },
  "4:3":  { maxW: "max-w-2xl", paddingTop: "75%",     label: "Standard 4:3" },
  "21:9": { maxW: "max-w-5xl", paddingTop: "42.86%",  label: "Ultrawide 21:9" },
};

function triggerDownload(url: string, filename: string) {
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
}

export function RenderPreviewDialog({ open, onClose, videoUrl, aspectRatio, renderId }: Props) {
  const meta = RATIO_META[aspectRatio] ?? RATIO_META["16:9"];
  const shortId = renderId.slice(0, 8);

  return (
    <Dialog open={open} onOpenChange={(v) => !v && onClose()}>
      <DialogContent
        className={`${meta.maxW} w-full p-0 overflow-hidden bg-gray-950 border-gray-800`}
      >
        <DialogHeader className="flex flex-row items-center justify-between px-5 py-3 bg-gray-900 border-b border-gray-800">
          <div className="flex items-center gap-3">
            <DialogTitle className="text-sm font-semibold text-white">
              Render Preview
            </DialogTitle>
            <Badge variant="outline" className="text-[10px] border-gray-600 text-gray-300">
              {meta.label}
            </Badge>
          </div>
          <Button
            size="icon"
            variant="ghost"
            onClick={onClose}
            className="h-7 w-7 text-gray-400 hover:text-white"
          >
            <X className="h-4 w-4" />
          </Button>
        </DialogHeader>

        {/* Aspect-ratio-preserving video container */}
        <div className="relative w-full bg-black" style={{ paddingTop: meta.paddingTop }}>
          <video
            key={videoUrl}
            src={videoUrl}
            controls
            autoPlay
            className="absolute inset-0 w-full h-full"
            controlsList="nodownload"
          />
        </div>

        <div className="px-5 py-3 bg-gray-900 border-t border-gray-800 flex items-center gap-3">
          <span className="text-xs text-gray-400 flex-1">
            Render ID: <span className="font-mono">{shortId}…</span>
          </span>
          <Button
            size="sm"
            className="gap-1.5"
            onClick={() => triggerDownload(videoUrl, `render-${shortId}.mp4`)}
          >
            <Download className="h-3.5 w-3.5" />
            Download MP4
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
