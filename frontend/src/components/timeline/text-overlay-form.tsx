"use client";

import { useState } from "react";
import { X } from "lucide-react";
import { TextAnimation, type TextOverlay } from "@/types/timeline";
import {
  textOverlayUtils,
  GRID_POSITIONS,
  type GridPosition,
} from "@/lib/timeline/text-overlay-utils";
import { TextOverlayPreview } from "./text-overlay-preview";

// ── Constants ─────────────────────────────────────────────────────────────────

const FONT_SIZES = [12, 16, 20, 24, 32, 40, 48, 64];

const ANIMATION_OPTIONS: { value: TextAnimation; label: string }[] = [
  { value: TextAnimation.None,      label: "None"      },
  { value: TextAnimation.FadeIn,    label: "Fade In"   },
  { value: TextAnimation.SlideUp,   label: "Slide Up"  },
  { value: TextAnimation.SlideDown, label: "Slide Down"},
];

const GRID_LABELS: Record<GridPosition, string> = {
  "top-left":      "↖",
  "top-center":    "↑",
  "top-right":     "↗",
  "center-left":   "←",
  "center":        "·",
  "center-right":  "→",
  "bottom-left":   "↙",
  "bottom-center": "↓",
  "bottom-right":  "↘",
};

// ── Component ─────────────────────────────────────────────────────────────────

interface TextOverlayFormProps {
  episodeId: string;
  timelineDurationMs: number;
  /** Prefill values when editing an existing overlay. */
  initial?: TextOverlay;
  onSubmit: (overlay: Omit<TextOverlay, "id">) => void;
  onCancel: () => void;
}

export function TextOverlayForm({
  episodeId,
  timelineDurationMs,
  initial,
  onSubmit,
  onCancel,
}: TextOverlayFormProps) {
  const initGrid = initial
    ? textOverlayUtils.percentToGrid(initial.positionX, initial.positionY)
    : "bottom-center" as GridPosition;

  const [text,          setText]          = useState(initial?.text ?? "");
  const [fontSize,      setFontSize]      = useState(initial?.fontSizePixels ?? 24);
  const [color,         setColor]         = useState(initial?.color ?? "#ffffff");
  const [animation,     setAnimation]     = useState<TextAnimation>(initial?.animation ?? TextAnimation.FadeIn);
  const [gridPos,       setGridPos]       = useState<GridPosition>(initGrid);
  const [startMmss,     setStartMmss]     = useState(textOverlayUtils.msToMmss(initial?.startMs ?? 0));
  const [durationSecs,  setDurationSecs]  = useState(Math.round((initial?.durationMs ?? 3000) / 1000));
  const [errors,        setErrors]        = useState<string[]>([]);

  const { positionX, positionY } = textOverlayUtils.gridToPercent(gridPos);

  function handleSubmit() {
    const startMs    = textOverlayUtils.mmssToMs(startMmss);
    const durationMs = durationSecs * 1000;

    const overlay: Omit<TextOverlay, "id"> = {
      episodeId,
      text,
      fontSizePixels: fontSize,
      color,
      positionX,
      positionY,
      startMs,
      durationMs,
      animation,
      zIndex: initial?.zIndex ?? 1,
    };

    const errs = textOverlayUtils.validateTextOverlay(overlay);
    if (startMs + durationMs > timelineDurationMs)
      errs.push("Overlay extends beyond timeline duration.");

    if (errs.length) { setErrors(errs); return; }
    setErrors([]);
    onSubmit(overlay);
  }

  return (
    // Modal backdrop
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="relative w-[480px] max-h-[90vh] overflow-y-auto bg-[#111827] border border-slate-700 rounded-xl shadow-2xl flex flex-col">

        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-slate-800">
          <h2 className="text-sm font-semibold text-slate-100">
            {initial ? "Edit Text Overlay" : "Add Text Overlay"}
          </h2>
          <button onClick={onCancel} className="text-slate-500 hover:text-slate-300">
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="flex flex-col gap-5 px-5 py-4">

          {/* Text */}
          <label className="flex flex-col gap-1.5">
            <span className="text-xs text-slate-400">Text</span>
            <textarea
              rows={3}
              value={text}
              onChange={(e) => setText(e.target.value)}
              placeholder="Enter overlay text…"
              className="bg-slate-800 text-slate-100 text-sm rounded border border-slate-700 px-3 py-2 resize-none focus:outline-none focus:border-purple-500"
            />
          </label>

          {/* Font size + color */}
          <div className="grid grid-cols-2 gap-3">
            <label className="flex flex-col gap-1.5">
              <span className="text-xs text-slate-400">Font size</span>
              <select
                value={fontSize}
                onChange={(e) => setFontSize(Number(e.target.value))}
                className="bg-slate-800 text-slate-100 text-sm rounded border border-slate-700 px-2 py-1.5 focus:outline-none focus:border-purple-500"
              >
                {FONT_SIZES.map((s) => (
                  <option key={s} value={s}>{s}px</option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1.5">
              <span className="text-xs text-slate-400">Color</span>
              <div className="flex items-center gap-2">
                <input
                  type="color"
                  value={color}
                  onChange={(e) => setColor(e.target.value)}
                  className="h-8 w-10 rounded cursor-pointer border border-slate-700 bg-slate-800 p-0.5"
                />
                <input
                  type="text"
                  value={color}
                  onChange={(e) => setColor(e.target.value)}
                  maxLength={7}
                  className="flex-1 bg-slate-800 text-slate-100 text-sm rounded border border-slate-700 px-2 py-1.5 focus:outline-none focus:border-purple-500 font-mono"
                />
              </div>
            </label>
          </div>

          {/* Animation */}
          <label className="flex flex-col gap-1.5">
            <span className="text-xs text-slate-400">Animation</span>
            <select
              value={animation}
              onChange={(e) => setAnimation(e.target.value as TextAnimation)}
              className="bg-slate-800 text-slate-100 text-sm rounded border border-slate-700 px-2 py-1.5 focus:outline-none focus:border-purple-500"
            >
              {ANIMATION_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </label>

          {/* Position 3×3 grid */}
          <div className="flex flex-col gap-1.5">
            <span className="text-xs text-slate-400">Position</span>
            <div className="grid grid-cols-3 gap-1 w-28">
              {GRID_POSITIONS.map((pos) => (
                <button
                  key={pos}
                  onClick={() => setGridPos(pos)}
                  title={pos.replace(/-/g, " ")}
                  className={`h-8 w-8 text-lg rounded flex items-center justify-center transition-colors ${
                    gridPos === pos
                      ? "bg-purple-600 text-white"
                      : "bg-slate-700 text-slate-400 hover:bg-slate-600"
                  }`}
                >
                  {GRID_LABELS[pos]}
                </button>
              ))}
            </div>
          </div>

          {/* Timing */}
          <div className="grid grid-cols-2 gap-3">
            <label className="flex flex-col gap-1.5">
              <span className="text-xs text-slate-400">Start time (MM:SS)</span>
              <input
                type="text"
                value={startMmss}
                onChange={(e) => setStartMmss(e.target.value)}
                placeholder="00:00"
                className="bg-slate-800 text-slate-100 text-sm rounded border border-slate-700 px-2 py-1.5 focus:outline-none focus:border-purple-500 font-mono"
              />
            </label>

            <label className="flex flex-col gap-1.5">
              <span className="text-xs text-slate-400">Duration (seconds)</span>
              <input
                type="number"
                min={1}
                max={Math.floor(timelineDurationMs / 1000)}
                value={durationSecs}
                onChange={(e) => setDurationSecs(Number(e.target.value))}
                className="bg-slate-800 text-slate-100 text-sm rounded border border-slate-700 px-2 py-1.5 focus:outline-none focus:border-purple-500"
              />
            </label>
          </div>

          {/* Preview */}
          <TextOverlayPreview
            text={text}
            fontSizePixels={fontSize}
            color={color}
            positionX={positionX}
            positionY={positionY}
            animation={animation}
          />

          {/* Errors */}
          {errors.length > 0 && (
            <ul className="text-xs text-red-400 space-y-0.5">
              {errors.map((err, i) => <li key={i}>• {err}</li>)}
            </ul>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-2 px-5 py-3.5 border-t border-slate-800">
          <button
            onClick={onCancel}
            className="text-xs px-4 py-1.5 rounded bg-slate-700 text-slate-300 hover:bg-slate-600 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            className="text-xs px-4 py-1.5 rounded bg-purple-600 text-white hover:bg-purple-500 transition-colors"
          >
            {initial ? "Save Changes" : "Add to Timeline"}
          </button>
        </div>
      </div>
    </div>
  );
}
