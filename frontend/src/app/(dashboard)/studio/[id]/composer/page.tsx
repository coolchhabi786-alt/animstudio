"use client";

/**
 * Konva Compositor — production-style video + text overlay canvas.
 *
 * HOW KONVA WORKS (read this once, then it clicks):
 *
 *   Stage  →  the <canvas> element.  One stage per page.
 *   Layer  →  a group of shapes drawn on the canvas.  Like Photoshop layers.
 *             Add multiple layers to control draw order (bottom → top).
 *   Image  →  a Konva shape that can hold ANY HTMLImageElement OR HTMLVideoElement.
 *             We create a hidden <video> tag, attach it to a Konva.Image, and
 *             then use Konva.Animation to redraw that shape every frame → live video.
 *   Text   →  text drawn on the canvas.  Position, drag, resize with Transformer.
 *   Transformer → the selection handles (resize / rotate) that appear when you
 *                 click a shape.
 *   Anim   →  a requestAnimationFrame loop.  Call layer.batchDraw() inside it
 *             to refresh the canvas every frame (needed for video).
 *
 * PRODUCTION EXPORT FLOW:
 *   stage.toDataURL() → PNG snapshot of the current canvas frame.
 *   For full video export you would pipe each frame (via requestVideoFrameCallback)
 *   through a WebCodecs VideoEncoder and mux with an audio track.
 */

import { useEffect, useRef, useState } from "react";
import dynamic from "next/dynamic";

// Konva uses browser APIs — import only on the client side.
const KonvaCompositor = dynamic(() => import("@/components/composer/konva-compositor"), {
  ssr: false,
  loading: () => (
    <div className="flex items-center justify-center h-full text-slate-400 text-sm">
      Loading compositor…
    </div>
  ),
});

export default function ComposerPage() {
  return (
    <div className="flex flex-col h-full bg-[#0a0f1a] text-white overflow-hidden">
      {/* Header */}
      <div className="shrink-0 flex items-center gap-3 px-4 py-3 bg-[#111827] border-b border-slate-800">
        <span className="text-xs font-semibold tracking-widest text-purple-400 uppercase">
          Konva Compositor
        </span>
        <span className="text-xs text-slate-500">
          — click a shot thumbnail to preview · drag text overlays · export frame
        </span>
      </div>

      {/* Canvas area */}
      <div className="flex-1 overflow-hidden">
        <KonvaCompositor />
      </div>
    </div>
  );
}
