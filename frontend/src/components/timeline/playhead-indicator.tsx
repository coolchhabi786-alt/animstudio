"use client";

import { Group, Line, Rect } from "react-konva";
import { timelineUtils } from "@/lib/timeline-utils";

const PLAYHEAD_COLOR  = "#EF4444";
const HEAD_WIDTH      = 12;
const HEAD_HEIGHT     = 14;
const RULER_HEIGHT    = 40;

interface PlayheadIndicatorProps {
  playheadPositionMs: number;
  totalHeight: number;
  totalWidth: number;
  zoom: number;
  durationMs: number;
  onSeek: (ms: number) => void;
}

export function PlayheadIndicator({
  playheadPositionMs,
  totalHeight,
  totalWidth,
  zoom,
  durationMs,
  onSeek,
}: PlayheadIndicatorProps) {
  const x = timelineUtils.msToPixels(playheadPositionMs, zoom);

  return (
    <Group
      x={x}
      draggable
      dragBoundFunc={(pos) => ({
        x: Math.max(0, Math.min(pos.x, timelineUtils.msToPixels(durationMs, zoom))),
        y: 0,
      })}
      onDragMove={(e) => {
        const newMs = Math.round(timelineUtils.pixelsToMs(e.target.x(), zoom));
        onSeek(Math.max(0, Math.min(newMs, durationMs)));
      }}
      onDragEnd={(e) => {
        const newMs = Math.round(timelineUtils.pixelsToMs(e.target.x(), zoom));
        onSeek(Math.max(0, Math.min(newMs, durationMs)));
        // Reset group x – position is driven by store state on re-render
        e.target.x(x);
      }}
    >
      {/* ── Diamond / arrow head in ruler ─────────────── */}
      <Rect
        x={-HEAD_WIDTH / 2}
        y={RULER_HEIGHT - HEAD_HEIGHT}
        width={HEAD_WIDTH}
        height={HEAD_HEIGHT}
        fill={PLAYHEAD_COLOR}
        cornerRadius={[2, 2, 0, 0]}
      />

      {/* ── Vertical line ─────────────────────────────── */}
      <Line
        points={[0, RULER_HEIGHT - HEAD_HEIGHT, 0, totalHeight]}
        stroke={PLAYHEAD_COLOR}
        strokeWidth={2}
        listening={false}
      />
    </Group>
  );
}
