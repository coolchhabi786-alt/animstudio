"use client";

import { useRef, useState } from "react";
import { Group, Rect, Text } from "react-konva";
import type { TimelineClip } from "@/types/timeline";
import { timelineUtils } from "@/lib/timeline-utils";
import { clipDragHandler } from "@/lib/timeline/clip-drag-handler";
import { clipTrimHandler } from "@/lib/timeline/clip-trim-handler";
import { CollisionOverlay } from "./collision-overlay";

// ── Visual constants ──────────────────────────────────────────────────────────

const CLIP_COLORS: Record<string, string> = {
  video: "#3B82F6",
  audio: "#10B981",
  music: "#8B5CF6",
  text:  "#F59E0B",
};

const CLIP_BORDER_COLORS: Record<string, string> = {
  video: "#1D4ED8",
  audio: "#047857",
  music: "#6D28D9",
  text:  "#B45309",
};

const HANDLE_W = 8;
const V_PAD    = 10;

// ── Props ─────────────────────────────────────────────────────────────────────

interface ClipShapeProps {
  clip: TimelineClip;
  trackType: string;
  trackHeight: number;
  /** Absolute y-coordinate of the parent TrackLane Group on the stage. */
  trackAbsY: number;
  zoom: number;
  isSelected: boolean;
  /** All other clips in the same track (used for collision detection). */
  otherClips: TimelineClip[];
  timelineDurationMs: number;
  onSelect: (clipId: string) => void;
  onMove:   (clipId: string, newStartMs: number) => void;
  onTrim:   (clipId: string, trimDeltaStartMs: number, newDurationMs: number) => void;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function getLabel(clip: TimelineClip): string {
  if (clip.type === "video") return `S${clip.sceneNumber}.${clip.shotIndex}`;
  if (clip.type === "audio") return clip.label;
  if (clip.type === "text")  return clip.text.replace(/\n/g, " ").slice(0, 24);
  return "";
}

// ── Component ─────────────────────────────────────────────────────────────────

export function ClipShape({
  clip,
  trackType,
  trackHeight,
  trackAbsY,
  zoom,
  isSelected,
  otherClips,
  timelineDurationMs,
  onSelect,
  onMove,
  onTrim,
}: ClipShapeProps) {
  // ── Derived layout values ───────────────────────────────────────────────────
  const x     = timelineUtils.msToPixels(clip.startMs, zoom);
  const clipW = Math.max(4, timelineUtils.msToPixels(clip.durationMs, zoom));
  const clipH = trackHeight - V_PAD * 2;

  const fill   = CLIP_COLORS[trackType]       ?? "#6B7280";
  const border = CLIP_BORDER_COLORS[trackType] ?? "#374151";

  // ── Collision state ─────────────────────────────────────────────────────────
  // Use a ref for the dragEnd handler (avoids stale-closure bug),
  // and a state var to drive the CollisionOverlay render.
  const hasCollisionRef  = useRef(false);
  const [hasCollision, setHasCollision] = useState(false);
  // Brief red flash when drag is rejected (collision or out of bounds)
  const [rejected, setRejected] = useState(false);

  function updateCollision(val: boolean) {
    hasCollisionRef.current = val;
    setHasCollision(val);
  }

  function flashRejected() {
    setRejected(true);
    setTimeout(() => setRejected(false), 500);
  }

  // ── Drag: snap + constrain + collision ─────────────────────────────────────

  function handleDragMove(e: import("konva/lib/Node").KonvaEventObject<DragEvent>) {
    // e.target.x() is already snapped (dragBoundFunc ran first)
    const provisionalMs = timelineUtils.pixelsToMs(e.target.x(), zoom);
    const provisional   = { ...clip, startMs: provisionalMs };
    updateCollision(clipDragHandler.detectClipOverlap(provisional, otherClips));
  }

  function handleDragEnd(e: import("konva/lib/Node").KonvaEventObject<DragEvent>) {
    if (hasCollisionRef.current) {
      // Reject — snap back to original position and flash red
      e.target.x(x);
      e.target.y(V_PAD);
      updateCollision(false);
      flashRejected();
      return;
    }

    // x() is already snapped by dragBoundFunc — compute final ms
    const newStartMs = clipDragHandler.calculateNewStartMs(
      e.target.x(),
      zoom,
      clip.durationMs,
      timelineDurationMs
    );
    onMove(clip.id, newStartMs);

    // Reset to store-driven position (re-render will reposition)
    e.target.x(x);
    e.target.y(V_PAD);
    updateCollision(false);
  }

  // ── Render ──────────────────────────────────────────────────────────────────

  return (
    <Group
      x={x}
      y={V_PAD}
      draggable
      dragBoundFunc={(pos) => {
        // pos is absolute stage coords. Snap x, lock y to track.
        const snappedMs = clipDragHandler.calculateNewStartMs(
          pos.x,
          zoom,
          clip.durationMs,
          timelineDurationMs
        );
        return {
          x: timelineUtils.msToPixels(snappedMs, zoom),
          y: trackAbsY + V_PAD,
        };
      }}
      onDragMove={handleDragMove}
      onDragEnd={handleDragEnd}
      onClick={() => onSelect(clip.id)}
      onMouseEnter={(e) => {
        const stage = e.target.getStage();
        if (stage) stage.container().style.cursor = "grab";
      }}
      onMouseLeave={(e) => {
        const stage = e.target.getStage();
        if (stage) stage.container().style.cursor = "default";
      }}
    >
      {/* ── Clip body ─────────────────────────────────── */}
      <Rect
        width={clipW}
        height={clipH}
        fill={rejected ? "#EF4444" : fill}
        stroke={rejected ? "#B91C1C" : isSelected ? "#FFFFFF" : border}
        strokeWidth={isSelected ? 2 : 1}
        cornerRadius={4}
        opacity={isSelected ? 1 : 0.88}
        shadowEnabled={isSelected || rejected}
        shadowColor={rejected ? "rgba(239,68,68,0.6)" : "rgba(255,255,255,0.35)"}
        shadowBlur={isSelected || rejected ? 10 : 0}
        shadowOffsetX={0}
        shadowOffsetY={0}
      />

      {/* ── Collision overlay (red tint while dragging over another clip) ── */}
      <CollisionOverlay width={clipW} height={clipH} visible={hasCollision} />

      {/* ── Label ─────────────────────────────────────── */}
      <Text
        x={HANDLE_W + 4}
        y={6}
        width={Math.max(0, clipW - HANDLE_W * 2 - 8)}
        height={clipH - 12}
        text={getLabel(clip)}
        fontSize={11}
        fontStyle="500"
        fill="#FFFFFF"
        ellipsis
        wrap="none"
        verticalAlign="middle"
        listening={false}
      />

      {/* ── Left trim handle ──────────────────────────── */}
      <Rect
        x={0}
        y={4}
        width={HANDLE_W}
        height={clipH - 8}
        fill="#FBBF24"
        cornerRadius={[4, 0, 0, 4]}
        opacity={0.75}
        draggable
        // Lock y during trim-handle drag
        dragBoundFunc={(pos) => ({ x: pos.x, y: trackAbsY + V_PAD + 4 })}
        onDragEnd={(e) => {
          // e.target.x() is the handle's position relative to the clip Group.
          // After a left-trim, the handle moved from x=0 to x=deltaX.
          const { startMs, durationMs } = clipTrimHandler.calculateTrimStart(
            e.target.x(),
            zoom,
            clip
          );
          if (
            clipTrimHandler.validateTrimRange(startMs, durationMs, timelineDurationMs)
          ) {
            const trimDelta = startMs - clip.startMs;
            onTrim(clip.id, trimDelta, durationMs);
          }
          e.target.x(0);
          e.target.y(4);
        }}
        onMouseEnter={(e) => {
          const stage = e.target.getStage();
          if (stage) stage.container().style.cursor = "ew-resize";
        }}
        onMouseLeave={(e) => {
          const stage = e.target.getStage();
          if (stage) stage.container().style.cursor = "default";
        }}
      />

      {/* ── Right trim handle ─────────────────────────── */}
      <Rect
        x={clipW - HANDLE_W}
        y={4}
        width={HANDLE_W}
        height={clipH - 8}
        fill="#FBBF24"
        cornerRadius={[0, 4, 4, 0]}
        opacity={0.75}
        draggable
        dragBoundFunc={(pos) => ({ x: pos.x, y: trackAbsY + V_PAD + 4 })}
        onDragEnd={(e) => {
          // deltaX = how far the right handle moved from its initial position
          const deltaX    = e.target.x() - (clipW - HANDLE_W);
          const newDuration = clipTrimHandler.calculateTrimEnd(
            deltaX,
            zoom,
            clip,
            timelineDurationMs
          );
          if (newDuration >= 500) {
            onTrim(clip.id, 0, newDuration);
          }
          e.target.x(clipW - HANDLE_W);
          e.target.y(4);
        }}
        onMouseEnter={(e) => {
          const stage = e.target.getStage();
          if (stage) stage.container().style.cursor = "ew-resize";
        }}
        onMouseLeave={(e) => {
          const stage = e.target.getStage();
          if (stage) stage.container().style.cursor = "default";
        }}
      />

      {/* ── Selection corner handles ──────────────────── */}
      {isSelected && (
        <>
          <Rect x={-2}        y={-2}        width={7} height={7} fill="#FFFFFF" cornerRadius={1} listening={false} />
          <Rect x={clipW - 5} y={-2}        width={7} height={7} fill="#FFFFFF" cornerRadius={1} listening={false} />
          <Rect x={-2}        y={clipH - 5} width={7} height={7} fill="#FFFFFF" cornerRadius={1} listening={false} />
          <Rect x={clipW - 5} y={clipH - 5} width={7} height={7} fill="#FFFFFF" cornerRadius={1} listening={false} />
        </>
      )}
    </Group>
  );
}
