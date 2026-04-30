"use client";

import { Group, Rect } from "react-konva";
import type { TimelineTrack, TimelineClip } from "@/types/timeline";
import { ClipShape } from "./clip-shape";

/** Track background shades (alternating rows). */
const TRACK_BG = ["#1e293b", "#172033"];

interface TrackLaneProps {
  track: TimelineTrack;
  trackIndex: number;
  /** Absolute y-coordinate of this lane on the Konva Stage. */
  y: number;
  height: number;
  totalWidth: number;
  zoom: number;
  timelineDurationMs: number;
  selectedClipId: string | null;
  onSelectClip: (clipId: string) => void;
  onMoveClip:   (clipId: string, newStartMs: number) => void;
  onTrimClip:   (clipId: string, trimDeltaStartMs: number, newDurationMs: number) => void;
}

export function TrackLane({
  track,
  trackIndex,
  y,
  height,
  totalWidth,
  zoom,
  timelineDurationMs,
  selectedClipId,
  onSelectClip,
  onMoveClip,
  onTrimClip,
}: TrackLaneProps) {
  return (
    <Group y={y}>
      {/* ── Track background ───────────────────────────── */}
      <Rect
        x={0}
        y={0}
        width={totalWidth}
        height={height}
        fill={TRACK_BG[trackIndex % 2]}
        listening={false}
      />
      {/* ── Bottom separator line ──────────────────────── */}
      <Rect
        x={0}
        y={height - 1}
        width={totalWidth}
        height={1}
        fill="#334155"
        listening={false}
      />

      {/* ── Clips ──────────────────────────────────────── */}
      {track.clips.map((clip: TimelineClip) => (
        <ClipShape
          key={clip.id}
          clip={clip}
          trackType={track.trackType}
          trackHeight={height}
          trackAbsY={y}
          zoom={zoom}
          timelineDurationMs={timelineDurationMs}
          isSelected={clip.id === selectedClipId}
          otherClips={track.clips.filter((c) => c.id !== clip.id)}
          onSelect={onSelectClip}
          onMove={onMoveClip}
          onTrim={onTrimClip}
        />
      ))}
    </Group>
  );
}
