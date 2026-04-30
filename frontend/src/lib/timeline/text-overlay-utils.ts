import type { CSSProperties } from "react";
import type { TextOverlay } from "@/types/timeline";

const POSITION_GRID: Record<string, { x: number; y: number }> = {
  "top-left":      { x: 5,  y: 5  },
  "top-center":    { x: 50, y: 5  },
  "top-right":     { x: 95, y: 5  },
  "center-left":   { x: 5,  y: 50 },
  "center":        { x: 50, y: 50 },
  "center-right":  { x: 95, y: 50 },
  "bottom-left":   { x: 5,  y: 90 },
  "bottom-center": { x: 50, y: 90 },
  "bottom-right":  { x: 95, y: 90 },
};

export type GridPosition = keyof typeof POSITION_GRID;

export const textOverlayUtils = {
  /** Map a 9-point grid name to positionX/positionY percent values. */
  gridToPercent(position: GridPosition): { positionX: number; positionY: number } {
    const p = POSITION_GRID[position] ?? { x: 50, y: 90 };
    return { positionX: p.x, positionY: p.y };
  },

  /** Infer the closest grid label from positionX/positionY values. */
  percentToGrid(positionX: number, positionY: number): GridPosition {
    let closest: GridPosition = "bottom-center";
    let minDist = Infinity;
    for (const [key, val] of Object.entries(POSITION_GRID)) {
      const dist = Math.hypot(positionX - val.x, positionY - val.y);
      if (dist < minDist) {
        minDist  = dist;
        closest  = key as GridPosition;
      }
    }
    return closest;
  },

  /** Build CSS props for rendering the overlay preview inside a container div. */
  formatTextForDisplay(overlay: Pick<TextOverlay, "fontSizePixels" | "color" | "positionX" | "positionY">): CSSProperties {
    return {
      position:  "absolute",
      left:      `${overlay.positionX}%`,
      top:       `${overlay.positionY}%`,
      transform: "translate(-50%, -50%)",
      fontSize:  overlay.fontSizePixels,
      color:     overlay.color,
      whiteSpace: "pre-wrap",
      textShadow: "0 1px 4px rgba(0,0,0,0.8)",
      pointerEvents: "none",
      userSelect: "none",
    };
  },

  /** Returns a list of validation error strings; empty array means valid. */
  validateTextOverlay(overlay: Partial<TextOverlay>): string[] {
    const errors: string[] = [];
    if (!overlay.text?.trim())          errors.push("Text is required.");
    if (!overlay.fontSizePixels || overlay.fontSizePixels < 8)
                                        errors.push("Font size must be at least 8px.");
    if (!overlay.color?.match(/^#[0-9a-fA-F]{6}$/))
                                        errors.push("Color must be a valid hex value (e.g. #FF0000).");
    if ((overlay.durationMs ?? 0) < 500)
                                        errors.push("Duration must be at least 500ms.");
    if ((overlay.startMs ?? 0) < 0)     errors.push("Start time cannot be negative.");
    return errors;
  },

  /** Estimate rendered text width/height (rough heuristic). */
  calculateTextDimensions(text: string, fontSizePixels: number): { width: number; height: number } {
    const lines   = text.split("\n");
    const longest = Math.max(...lines.map((l) => l.length));
    return {
      width:  longest * fontSizePixels * 0.6,
      height: lines.length * fontSizePixels * 1.4,
    };
  },

  /** Convert MM:SS string to milliseconds. Returns NaN on invalid input. */
  mmssToMs(value: string): number {
    const [mm, ss] = value.split(":").map(Number);
    if (isNaN(mm) || isNaN(ss)) return NaN;
    return (mm * 60 + ss) * 1000;
  },

  /** Format milliseconds as MM:SS for display in form fields. */
  msToMmss(ms: number): string {
    const totalSecs = Math.floor(ms / 1000);
    const m = Math.floor(totalSecs / 60).toString().padStart(2, "0");
    const s = (totalSecs % 60).toString().padStart(2, "0");
    return `${m}:${s}`;
  },
};

export const GRID_POSITIONS: GridPosition[] = Object.keys(POSITION_GRID) as GridPosition[];
