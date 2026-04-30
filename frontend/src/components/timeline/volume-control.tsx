"use client";

import { Volume, Volume1, Volume2, VolumeX } from "lucide-react";

interface VolumeControlProps {
  volume: number; // 0–100
  onVolumeChange: (volume: number) => void;
  compact?: boolean;
}

function VolumeIcon({ volume }: { volume: number }) {
  if (volume === 0)  return <VolumeX className="h-3 w-3 shrink-0" />;
  if (volume < 33)   return <Volume  className="h-3 w-3 shrink-0" />;
  if (volume < 66)   return <Volume1 className="h-3 w-3 shrink-0" />;
  return <Volume2 className="h-3 w-3 shrink-0" />;
}

export function VolumeControl({ volume, onVolumeChange, compact = false }: VolumeControlProps) {
  return (
    <div className={`flex items-center gap-1.5 ${compact ? "w-full" : "w-36"}`}>
      <span className="text-slate-400">
        <VolumeIcon volume={volume} />
      </span>
      <input
        type="range"
        min={0}
        max={100}
        step={1}
        value={volume}
        onChange={(e) => onVolumeChange(Number(e.target.value))}
        className="flex-1 h-1 accent-purple-400 cursor-pointer"
        title={`Volume: ${volume}%`}
      />
      <span className="text-[10px] text-slate-400 w-7 text-right tabular-nums">
        {volume}%
      </span>
    </div>
  );
}
