"use client";

import React from "react";
import { Stage, Layer, Text as KonvaText, Rect } from "react-konva";
import type { Timeline } from "@/types/timeline";
import { timelineUtils } from "@/lib/timeline-utils";
import {
  useTimelineStore,
  selectTimeline,
  selectPlayhead,
  selectZoom,
  selectSelectedClipId,
  selectTextOverlays,
} from "@/stores/timelineStore";
import { TimelineRuler, RULER_HEIGHT } from "./timeline-ruler";
import { TrackLane } from "./track-lane";
import { PlayheadIndicator } from "./playhead-indicator";
import { TRACK_HEIGHTS } from "./timeline-constants";
export { TRACK_HEIGHTS };

// ── Track layout constants ────────────────────────────────────────────────────

function getTrackHeights(timeline: Timeline): number[] {
  return timeline.tracks.map((t) => TRACK_HEIGHTS[t.trackType] ?? 60);
}

function getTotalTracksHeight(timeline: Timeline): number {
  return getTrackHeights(timeline).reduce((s, h) => s + h, 0);
}

function getTrackYOffsets(timeline: Timeline): number[] {
  const heights = getTrackHeights(timeline);
  const offsets: number[] = [];
  let y = RULER_HEIGHT;
  for (const h of heights) {
    offsets.push(y);
    y += h;
  }
  return offsets;
}

// ── Component ─────────────────────────────────────────────────────────────────

interface TimelineContainerProps {
  /** Visible container width in px (from ResizeObserver). */
  containerWidth: number;
}

export function TimelineContainer({ containerWidth }: TimelineContainerProps) {
  const timeline         = useTimelineStore(selectTimeline);
  const playheadMs       = useTimelineStore(selectPlayhead);
  const zoom             = useTimelineStore(selectZoom);
  const selectedClipId   = useTimelineStore(selectSelectedClipId);
  const textOverlays     = useTimelineStore(selectTextOverlays);
  const selectClip       = useTimelineStore((s) => s.selectClip);
  const moveClip         = useTimelineStore((s) => s.moveClip);
  const trimClip         = useTimelineStore((s) => s.trimClip);
  const setPlayhead      = useTimelineStore((s) => s.setPlayheadPosition);

  if (!timeline) return null;

  const contentWidth  = Math.max(containerWidth, timelineUtils.msToPixels(timeline.durationMs, zoom));
  const tracksHeight  = getTotalTracksHeight(timeline);
  const totalHeight   = RULER_HEIGHT + tracksHeight;
  const trackOffsets  = getTrackYOffsets(timeline);
  const trackHeights  = getTrackHeights(timeline);

  return (
    <Stage width={contentWidth} height={totalHeight}>
      {/* ── Ruler ──────────────────────────────────────── */}
      <TimelineRuler
        totalWidth={contentWidth}
        zoom={zoom}
        durationMs={timeline.durationMs}
        onSeek={setPlayhead}
      />

      {/* ── Track backgrounds ──────────────────────────── */}
      <Layer name="backgrounds" listening={false}>
        {/* Rendered inside TrackLane via its own Rect */}
      </Layer>

      {/* ── Clips ──────────────────────────────────────── */}
      <Layer name="clips">
        {timeline.tracks.map((track, i) => (
          <TrackLane
            key={track.id}
            track={track}
            trackIndex={i}
            y={trackOffsets[i]}
            height={trackHeights[i]}
            totalWidth={contentWidth}
            zoom={zoom}
            timelineDurationMs={timeline.durationMs}
            selectedClipId={selectedClipId}
            onSelectClip={selectClip}
            onMoveClip={moveClip}
            onTrimClip={trimClip}
          />
        ))}
      </Layer>

      {/* ── Text overlays (show active ones at their canvas position) ── */}
      <Layer name="text-overlays" listening={false}>
        {textOverlays
          .filter(
            (o) => playheadMs >= o.startMs && playheadMs < o.startMs + o.durationMs
          )
          .sort((a, b) => a.zIndex - b.zIndex)
          .map((overlay) => {
            const ox = (overlay.positionX / 100) * contentWidth;
            const oy = (overlay.positionY / 100) * totalHeight;
            // Semi-transparent backing pill for readability
            const dims = {
              w: Math.min(overlay.fontSizePixels * overlay.text.length * 0.6, contentWidth * 0.8),
              h: overlay.fontSizePixels * 1.4,
            };
            return (
              <React.Fragment key={overlay.id}>
                <Rect
                  x={ox - dims.w / 2 - 6}
                  y={oy - dims.h / 2 - 3}
                  width={dims.w + 12}
                  height={dims.h + 6}
                  fill="rgba(0,0,0,0.55)"
                  cornerRadius={4}
                />
                <KonvaText
                  x={ox - dims.w / 2}
                  y={oy - dims.h / 2}
                  text={overlay.text}
                  fontSize={overlay.fontSizePixels}
                  fill={overlay.color}
                  width={dims.w}
                  align="center"
                  verticalAlign="middle"
                  height={dims.h}
                  wrap="none"
                  ellipsis
                />
              </React.Fragment>
            );
          })}
      </Layer>

      {/* ── Playhead ───────────────────────────────────── */}
      <Layer name="playhead">
        <PlayheadIndicator
          playheadPositionMs={playheadMs}
          totalHeight={totalHeight}
          totalWidth={contentWidth}
          zoom={zoom}
          durationMs={timeline.durationMs}
          onSeek={setPlayhead}
        />
      </Layer>
    </Stage>
  );
}

// Export track layout helpers for consumers (e.g. track panel sidebar)
export { getTotalTracksHeight, getTrackYOffsets, getTrackHeights };
