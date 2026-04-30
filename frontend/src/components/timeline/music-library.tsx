"use client";

import { useState, useRef } from "react";
import { Play, Square, Plus, Music } from "lucide-react";
import { STOCK_MUSIC_TRACKS, type StockMusicTrack } from "@/lib/mock-data/stock-music";

interface MusicLibraryProps {
  onAddToTimeline: (track: StockMusicTrack, startMs: number) => void;
}

function formatDuration(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}:${s.toString().padStart(2, "0")}`;
}

const GENRE_COLORS: Record<string, string> = {
  Ambient:     "bg-cyan-500/20 text-cyan-300 border-cyan-500/40",
  Epic:        "bg-red-500/20 text-red-300 border-red-500/40",
  Uplifting:   "bg-yellow-500/20 text-yellow-300 border-yellow-500/40",
  Suspense:    "bg-orange-500/20 text-orange-300 border-orange-500/40",
  Comedy:      "bg-pink-500/20 text-pink-300 border-pink-500/40",
  Electronic:  "bg-violet-500/20 text-violet-300 border-violet-500/40",
  World:       "bg-teal-500/20 text-teal-300 border-teal-500/40",
};

export function MusicLibrary({ onAddToTimeline }: MusicLibraryProps) {
  const [previewingId, setPreviewingId] = useState<string | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  function handlePreview(track: StockMusicTrack) {
    if (previewingId === track.id) {
      // Stop
      audioRef.current?.pause();
      setPreviewingId(null);
      return;
    }
    if (audioRef.current) {
      audioRef.current.pause();
    }
    const audio = new Audio(track.previewUrl);
    audio.volume = 0.5;
    audio.play().catch(() => {});
    // Stop after 10 seconds
    setTimeout(() => {
      audio.pause();
      setPreviewingId(null);
    }, 10_000);
    audio.onended = () => setPreviewingId(null);
    audioRef.current = audio;
    setPreviewingId(track.id);
  }

  return (
    <div className="flex flex-col h-full bg-[#0f1623] border-l border-slate-800 w-64 shrink-0">
      {/* Header */}
      <div className="flex items-center gap-2 px-3 py-2.5 border-b border-slate-800">
        <Music className="h-4 w-4 text-purple-400" />
        <span className="text-sm font-medium text-slate-200">Music Library</span>
        <span className="ml-auto text-[10px] text-slate-500">{STOCK_MUSIC_TRACKS.length} tracks</span>
      </div>

      {/* Track list */}
      <div className="flex-1 overflow-y-auto divide-y divide-slate-800/60">
        {STOCK_MUSIC_TRACKS.map((track) => {
          const isPreviewing = previewingId === track.id;
          const genreClass = GENRE_COLORS[track.genre] ?? "bg-slate-700/40 text-slate-400 border-slate-600";
          return (
            <div
              key={track.id}
              className="flex flex-col gap-1.5 px-3 py-2 hover:bg-slate-800/40 transition-colors"
            >
              {/* Title + genre badge */}
              <div className="flex items-start justify-between gap-2">
                <span className="text-xs text-slate-200 font-medium leading-tight">
                  {track.title}
                </span>
                <span
                  className={`text-[9px] px-1 py-0.5 rounded border font-semibold shrink-0 ${genreClass}`}
                >
                  {track.genre}
                </span>
              </div>

              {/* Mood + duration */}
              <div className="flex items-center justify-between text-[10px] text-slate-500">
                <span>{track.mood}</span>
                <span className="tabular-nums">{formatDuration(track.durationSeconds)}</span>
              </div>

              {/* Actions */}
              <div className="flex items-center gap-1.5 mt-0.5">
                <button
                  onClick={() => handlePreview(track)}
                  className={`flex items-center gap-1 text-[10px] px-2 py-1 rounded transition-colors ${
                    isPreviewing
                      ? "bg-purple-600 text-white"
                      : "bg-slate-700 text-slate-300 hover:bg-slate-600"
                  }`}
                >
                  {isPreviewing ? (
                    <><Square className="h-2.5 w-2.5" /> Stop</>
                  ) : (
                    <><Play className="h-2.5 w-2.5" /> Preview</>
                  )}
                </button>

                <button
                  onClick={() => onAddToTimeline(track, 0)}
                  className="flex items-center gap-1 text-[10px] px-2 py-1 rounded bg-purple-600/80 text-white hover:bg-purple-600 transition-colors ml-auto"
                >
                  <Plus className="h-2.5 w-2.5" /> Add
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
