"use client";

import { Layer, Rect, Line, Text } from "react-konva";
import { timelineUtils } from "@/lib/timeline-utils";

export const RULER_HEIGHT = 40;
const TICK_INTERVAL_MS    = 5_000; // mark every 5 s
const MINOR_TICK_MS       = 1_000; // minor tick every 1 s

interface TimelineRulerProps {
  totalWidth: number;
  zoom: number;
  durationMs: number;
  onSeek: (ms: number) => void;
}

export function TimelineRuler({
  totalWidth,
  zoom,
  durationMs,
  onSeek,
}: TimelineRulerProps) {
  // Build tick arrays
  const majorTicks: number[] = [];
  const minorTicks: number[] = [];

  for (let ms = 0; ms <= durationMs; ms += MINOR_TICK_MS) {
    if (ms % TICK_INTERVAL_MS === 0) {
      majorTicks.push(ms);
    } else {
      minorTicks.push(ms);
    }
  }

  function handleClick(e: import("konva/lib/Node").KonvaEventObject<MouseEvent>) {
    const stage = e.target.getStage();
    if (!stage) return;
    const pos = stage.getPointerPosition();
    if (!pos) return;
    const ms = Math.round(timelineUtils.pixelsToMs(pos.x, zoom));
    onSeek(Math.max(0, Math.min(ms, durationMs)));
  }

  return (
    <Layer name="ruler" onClick={handleClick}>
      {/* Background */}
      <Rect
        x={0}
        y={0}
        width={totalWidth}
        height={RULER_HEIGHT}
        fill="#0f172a"
        listening={false}
      />

      {/* Minor ticks */}
      {minorTicks.map((ms) => {
        const x = timelineUtils.msToPixels(ms, zoom);
        return (
          <Line
            key={`m-${ms}`}
            points={[x, RULER_HEIGHT - 8, x, RULER_HEIGHT]}
            stroke="#334155"
            strokeWidth={1}
            listening={false}
          />
        );
      })}

      {/* Major ticks + labels */}
      {majorTicks.map((ms) => {
        const x = timelineUtils.msToPixels(ms, zoom);
        return (
          <Line
            key={`M-${ms}`}
            points={[x, 20, x, RULER_HEIGHT]}
            stroke="#475569"
            strokeWidth={1}
            listening={false}
          />
        );
      })}
      {majorTicks.map((ms) => {
        const x = timelineUtils.msToPixels(ms, zoom);
        return (
          <Text
            key={`L-${ms}`}
            x={x + 3}
            y={6}
            text={timelineUtils.formatMs(ms)}
            fontSize={11}
            fill="#94a3b8"
            listening={false}
          />
        );
      })}

      {/* Bottom border */}
      <Line
        points={[0, RULER_HEIGHT, totalWidth, RULER_HEIGHT]}
        stroke="#1e293b"
        strokeWidth={2}
        listening={false}
      />
    </Layer>
  );
}
