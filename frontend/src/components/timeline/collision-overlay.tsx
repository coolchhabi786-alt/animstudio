"use client";

import { Rect } from "react-konva";

interface CollisionOverlayProps {
  /** Width of the clip in pixels (matches parent clip shape width). */
  width: number;
  /** Height of the clip body in pixels. */
  height: number;
  /** When false the overlay is not rendered at all. */
  visible: boolean;
}

/**
 * CollisionOverlay
 *
 * A semi-transparent red rectangle rendered on top of a clip shape when
 * an overlap (collision) is detected during a drag operation. The overlay
 * shares the same coordinate space as the parent ClipShape Group.
 */
export function CollisionOverlay({ width, height, visible }: CollisionOverlayProps) {
  if (!visible) return null;

  return (
    <Rect
      x={0}
      y={0}
      width={Math.max(1, width)}
      height={Math.max(1, height)}
      fill="rgba(239, 68, 68, 0.45)"
      stroke="#EF4444"
      strokeWidth={2}
      cornerRadius={4}
      listening={false}
    />
  );
}
