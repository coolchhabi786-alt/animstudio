"use client";

import { useState } from "react";
import { useTimelineMock } from "@/hooks/use-timeline-mock";
import { useTimelineStore } from "@/stores/timelineStore";
import { useTimelineKeybinds } from "@/hooks/use-timeline-keybinds";
import { TimelineCanvasWrapper } from "@/components/timeline/timeline-canvas-wrapper";
import { MusicLibrary } from "@/components/timeline/music-library";
import { MusicTrackRow } from "@/components/timeline/music-track-row";
import { TextOverlayPanel } from "@/components/timeline/text-overlay-panel";
import { TimelinePreviewRender } from "@/components/timeline/timeline-preview-render";
import { UndoRedoToolbar } from "@/components/timeline/undo-redo-toolbar";
import { HistoryPanel } from "@/components/timeline/history-panel";
import { timelineUtils } from "@/lib/timeline-utils";
import type { StockMusicTrack } from "@/lib/mock-data/stock-music";
import type { TextOverlay } from "@/types/timeline";
import {
  SkipBack,
  SkipForward,
  Play,
  Pause,
  ZoomIn,
  ZoomOut,
  Volume2,
  VolumeX,
  Lock,
  Unlock,
  Music,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { TRACK_HEIGHTS } from "@/components/timeline/timeline-constants";

// ── Track label colors ────────────────────────────────────────────────────────

const TRACK_TYPE_COLORS: Record<string, string> = {
  video: "bg-blue-500/20 border-blue-500/40 text-blue-300",
  audio: "bg-emerald-500/20 border-emerald-500/40 text-emerald-300",
  music: "bg-purple-500/20 border-purple-500/40 text-purple-300",
  text:  "bg-amber-500/20 border-amber-500/40 text-amber-300",
};

// ── Page ─────────────────────────────────────────────────────────────────────

export default function TimelinePage({ params }: { params: { id: string } }) {
  const episodeId = params.id;
  const {
    timeline,
    playheadPositionMs,
    isPlaying,
    zoom,
    togglePlayback,
    setPlayheadPosition,
    zoomIn,
    zoomOut,
    toggleTrackMute,
    toggleTrackLock,
    selectedClipId,
    selectClip,
  } = useTimelineMock();

  useTimelineKeybinds();

  const setTrackVolume     = useTimelineStore((s) => s.setTrackVolume);
  const toggleTrackAutoDuck = useTimelineStore((s) => s.toggleTrackAutoDuck);
  const addMusicClip       = useTimelineStore((s) => s.addMusicClip);
  const addTextOverlay     = useTimelineStore((s) => s.addTextOverlay);
  const updateTextOverlay  = useTimelineStore((s) => s.updateTextOverlay);
  const deleteTextOverlay  = useTimelineStore((s) => s.deleteTextOverlay);

  const [showMusicLib, setShowMusicLib] = useState(false);

  const TRACK_LABEL_W = 160;

  // Find the first music track ID for "Add to Timeline" action
  const musicTrackId = timeline?.tracks.find((t) => t.trackType === "music")?.id;

  function handleAddMusicToTimeline(stock: StockMusicTrack, startMs: number) {
    if (!musicTrackId || !timeline) return;
    addMusicClip(
      {
        type:          "audio",
        trackId:       musicTrackId,
        label:         stock.title,
        audioUrl:      stock.fullUrl,
        startMs,
        durationMs:    stock.durationSeconds * 1000,
        volumePercent: 50,
        fadeInMs:      2000,
        fadeOutMs:     2000,
      },
      musicTrackId
    );
  }

  return (
    <div className="flex flex-col h-full bg-[#0a0f1a] text-white overflow-auto">

      {/* ── Toolbar ──────────────────────────────────────── */}
      <div className="flex items-center gap-2 px-3 py-2 bg-[#111827] border-b border-slate-800 shrink-0">
        {/* Undo / Redo */}
        <UndoRedoToolbar />

        <div className="w-px h-5 bg-slate-700 mx-1" />

        {/* Playback */}
        <Button size="icon" variant="ghost" className="h-8 w-8 text-slate-300" onClick={() => setPlayheadPosition(0)}>
          <SkipBack className="h-4 w-4" />
        </Button>
        <Button
          size="icon"
          variant="ghost"
          className="h-8 w-8 text-white bg-blue-600 hover:bg-blue-700"
          onClick={togglePlayback}
        >
          {isPlaying ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
        </Button>
        <Button
          size="icon"
          variant="ghost"
          className="h-8 w-8 text-slate-300"
          onClick={() => setPlayheadPosition(timeline?.durationMs ?? 0)}
        >
          <SkipForward className="h-4 w-4" />
        </Button>

        <div className="w-px h-5 bg-slate-700 mx-1" />

        {/* Timecode */}
        {timeline && (
          <span className="font-mono text-xs text-slate-300 w-28 text-center">
            {timelineUtils.formatMsWithFrames(playheadPositionMs, timeline.fps)}
            {" / "}
            {timelineUtils.formatMs(timeline.durationMs)}
          </span>
        )}

        <div className="flex-1" />

        {/* Music library toggle */}
        <Button
          size="sm"
          variant="ghost"
          className={`h-8 gap-1.5 text-xs ${showMusicLib ? "text-purple-400 bg-purple-500/10" : "text-slate-400"}`}
          onClick={() => setShowMusicLib((v) => !v)}
        >
          <Music className="h-3.5 w-3.5" />
          Music Library
        </Button>

        <div className="w-px h-5 bg-slate-700 mx-1" />

        {/* Zoom */}
        <Button size="icon" variant="ghost" className="h-8 w-8 text-slate-300" onClick={zoomOut} disabled={zoom <= 1}>
          <ZoomOut className="h-4 w-4" />
        </Button>
        <span className="text-xs text-slate-400 w-10 text-center">{zoom.toFixed(1)}×</span>
        <Button size="icon" variant="ghost" className="h-8 w-8 text-slate-300" onClick={zoomIn} disabled={zoom >= 5}>
          <ZoomIn className="h-4 w-4" />
        </Button>
      </div>

      {/* ── Editor area ──────────────────────────────────── */}
      <div className="flex flex-1 overflow-hidden">

        {/* ── Left: track label panel ───────────────────── */}
        <div
          className="shrink-0 flex flex-col bg-[#0f1623] border-r border-slate-800"
          style={{ width: TRACK_LABEL_W }}
        >
          {/* Ruler spacer */}
          <div className="h-10 border-b border-slate-800 flex items-center px-3">
            <span className="text-[10px] text-slate-500 uppercase tracking-wider">Tracks</span>
          </div>

          {/* Track rows */}
          {timeline?.tracks.map((track) => {
            const h = TRACK_HEIGHTS[track.trackType] ?? 60;
            const colorClass = TRACK_TYPE_COLORS[track.trackType] ?? "";
            const isMusic = track.trackType === "music";

            return (
              <div
                key={track.id}
                className="flex items-center justify-between px-2 border-b border-slate-800 shrink-0"
                style={{ height: h }}
              >
                {isMusic ? (
                  /* Music track: volume slider + auto-duck */
                  <MusicTrackRow
                    trackId={track.id}
                    label={track.label}
                    volumePercent={track.volumePercent ?? 50}
                    autoDuck={track.autoDuck ?? false}
                    onVolumeChange={setTrackVolume}
                    onAutoDuckToggle={toggleTrackAutoDuck}
                  />
                ) : (
                  /* All other tracks: label + mute/lock */
                  <>
                    <div className="flex items-center gap-2 min-w-0">
                      <span className={`text-[10px] px-1.5 py-0.5 rounded border font-medium shrink-0 ${colorClass}`}>
                        {track.trackType.toUpperCase().slice(0, 3)}
                      </span>
                      <span className="text-xs text-slate-300 truncate">{track.label}</span>
                    </div>
                    <div className="flex items-center gap-1 shrink-0">
                      <button
                        className={`p-1 rounded hover:bg-slate-700 transition-colors ${track.isMuted ? "text-red-400" : "text-slate-500"}`}
                        onClick={() => toggleTrackMute(track.id)}
                        title={track.isMuted ? "Unmute" : "Mute"}
                      >
                        {track.isMuted ? <VolumeX className="h-3 w-3" /> : <Volume2 className="h-3 w-3" />}
                      </button>
                      <button
                        className={`p-1 rounded hover:bg-slate-700 transition-colors ${track.isLocked ? "text-amber-400" : "text-slate-500"}`}
                        onClick={() => toggleTrackLock(track.id)}
                        title={track.isLocked ? "Unlock" : "Lock"}
                      >
                        {track.isLocked ? <Lock className="h-3 w-3" /> : <Unlock className="h-3 w-3" />}
                      </button>
                    </div>
                  </>
                )}
              </div>
            );
          })}
        </div>

        {/* ── Centre: canvas + text overlay panel ──────── */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Konva canvas */}
          <div className="flex-1 overflow-hidden">
            <TimelineCanvasWrapper />
          </div>

          {/* Text overlay panel */}
          {timeline && (
            <TextOverlayPanel
              timeline={timeline}
              onAdd={(data) => addTextOverlay(data as Omit<TextOverlay, "id">)}
              onUpdate={updateTextOverlay}
              onDelete={deleteTextOverlay}
            />
          )}
        </div>

        {/* ── Right: Music library (conditional) ───────── */}
        {showMusicLib && (
          <MusicLibrary onAddToTimeline={handleAddMusicToTimeline} />
        )}
      </div>

      {/* ── Preview render + player ──────────────────────── */}
      {timeline && (
        <TimelinePreviewRender
          timeline={timeline}
          episodeId={episodeId}
        />
      )}

      {/* ── Dev history panel ────────────────────────────── */}
      <HistoryPanel isDev={process.env.NODE_ENV === "development"} />

      {/* ── Bottom: clip inspector ───────────────────────── */}
      {selectedClipId && timeline && (() => {
        const clip = timeline.tracks.flatMap((t) => t.clips).find((c) => c.id === selectedClipId);
        if (!clip) return null;
        return (
          <div className="shrink-0 flex items-center gap-6 px-4 py-2 bg-[#111827] border-t border-slate-800 text-xs text-slate-400">
            <span className="text-slate-200 font-medium">
              {clip.type === "video" ? `Scene ${clip.sceneNumber} · Shot ${clip.shotIndex}` :
               clip.type === "audio" ? clip.label :
               clip.text.slice(0, 40)}
            </span>
            <span>Start: <b className="text-slate-200">{timelineUtils.formatMsWithFrames(clip.startMs, timeline.fps)}</b></span>
            <span>Duration: <b className="text-slate-200">{timelineUtils.formatMs(clip.durationMs)}</b></span>
            {clip.type === "video" && (
              <span>Transition: <b className="text-slate-200">{clip.transitionIn}</b></span>
            )}
            <button
              className="ml-auto text-slate-500 hover:text-slate-300"
              onClick={() => selectClip(null)}
            >
              ✕
            </button>
          </div>
        );
      })()}
    </div>
  );
}
