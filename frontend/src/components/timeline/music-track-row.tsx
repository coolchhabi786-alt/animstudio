"use client";

import { VolumeControl } from "./volume-control";

interface MusicTrackRowProps {
  trackId: string;
  label: string;
  volumePercent: number;
  autoDuck: boolean;
  onVolumeChange: (trackId: string, volume: number) => void;
  onAutoDuckToggle: (trackId: string) => void;
}

export function MusicTrackRow({
  trackId,
  label,
  volumePercent,
  autoDuck,
  onVolumeChange,
  onAutoDuckToggle,
}: MusicTrackRowProps) {
  return (
    <div className="flex flex-col justify-center gap-1.5 px-2 w-full">
      {/* Track name + auto-duck toggle */}
      <div className="flex items-center justify-between gap-1">
        <span className="text-xs text-slate-300 truncate">{label}</span>
        <button
          onClick={() => onAutoDuckToggle(trackId)}
          title={autoDuck ? "Auto-duck ON — music dips during dialogue" : "Auto-duck OFF"}
          className={`flex items-center gap-0.5 text-[9px] px-1.5 py-0.5 rounded border font-medium shrink-0 transition-colors ${
            autoDuck
              ? "bg-purple-500/30 border-purple-500/60 text-purple-300"
              : "bg-slate-700/50 border-slate-600 text-slate-500"
          }`}
        >
          DUCK
        </button>
      </div>

      {/* Volume slider */}
      <VolumeControl
        volume={volumePercent}
        onVolumeChange={(v) => onVolumeChange(trackId, v)}
        compact
      />
    </div>
  );
}
