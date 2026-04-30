"use client";

import { useState } from "react";
import { Plus, Pencil, Trash2, Type } from "lucide-react";
import type { TextOverlay, Timeline } from "@/types/timeline";
import { textOverlayUtils } from "@/lib/timeline/text-overlay-utils";
import { TextOverlayForm } from "./text-overlay-form";

interface TextOverlayPanelProps {
  timeline: Timeline;
  onAdd:    (overlay: Omit<TextOverlay, "id">) => void;
  onUpdate: (id: string, updates: Partial<TextOverlay>) => void;
  onDelete: (id: string) => void;
}

export function TextOverlayPanel({
  timeline,
  onAdd,
  onUpdate,
  onDelete,
}: TextOverlayPanelProps) {
  const [showForm,    setShowForm]    = useState(false);
  const [editTarget,  setEditTarget]  = useState<TextOverlay | null>(null);

  const overlays = timeline.textOverlays ?? [];

  function handleAdd(data: Omit<TextOverlay, "id">) {
    onAdd(data);
    setShowForm(false);
  }

  function handleEdit(overlay: TextOverlay, data: Omit<TextOverlay, "id">) {
    onUpdate(overlay.id, data);
    setEditTarget(null);
  }

  return (
    <>
      {/* Modal: add */}
      {showForm && (
        <TextOverlayForm
          episodeId={timeline.episodeId}
          timelineDurationMs={timeline.durationMs}
          onSubmit={handleAdd}
          onCancel={() => setShowForm(false)}
        />
      )}

      {/* Modal: edit */}
      {editTarget && (
        <TextOverlayForm
          episodeId={timeline.episodeId}
          timelineDurationMs={timeline.durationMs}
          initial={editTarget}
          onSubmit={(data) => handleEdit(editTarget, data)}
          onCancel={() => setEditTarget(null)}
        />
      )}

      {/* Panel strip */}
      <div className="shrink-0 flex flex-col bg-[#0d1421] border-t border-slate-800">
        {/* Header row */}
        <div className="flex items-center gap-2 px-4 py-2 border-b border-slate-800/60">
          <Type className="h-3.5 w-3.5 text-amber-400" />
          <span className="text-xs font-medium text-slate-300">Text Overlays</span>
          <span className="text-[10px] text-slate-600 ml-1">
            {overlays.length} {overlays.length === 1 ? "overlay" : "overlays"}
          </span>
          <button
            onClick={() => setShowForm(true)}
            className="ml-auto flex items-center gap-1 text-[10px] px-2.5 py-1 rounded bg-amber-500/80 text-white hover:bg-amber-500 transition-colors"
          >
            <Plus className="h-3 w-3" />
            Add Text
          </button>
        </div>

        {/* Overlay list */}
        {overlays.length === 0 ? (
          <div className="flex items-center justify-center py-3 text-[10px] text-slate-600">
            No text overlays yet — click "Add Text" to create one.
          </div>
        ) : (
          <div className="flex gap-2 px-4 py-2 overflow-x-auto">
            {overlays.map((overlay) => (
              <OverlayChip
                key={overlay.id}
                overlay={overlay}
                onEdit={() => setEditTarget(overlay)}
                onDelete={() => onDelete(overlay.id)}
              />
            ))}
          </div>
        )}
      </div>
    </>
  );
}

// ── Chip component ──────────────────────────────────────────────────────────

function OverlayChip({
  overlay,
  onEdit,
  onDelete,
}: {
  overlay: TextOverlay;
  onEdit:   () => void;
  onDelete: () => void;
}) {
  const start = textOverlayUtils.msToMmss(overlay.startMs);
  const end   = textOverlayUtils.msToMmss(overlay.startMs + overlay.durationMs);
  const pos   = textOverlayUtils.percentToGrid(overlay.positionX, overlay.positionY);

  return (
    <div className="flex items-center gap-2 shrink-0 bg-slate-800/70 border border-slate-700 rounded-lg px-3 py-1.5 min-w-[180px] max-w-[240px]">
      {/* Color swatch */}
      <span
        className="h-3 w-3 rounded-full shrink-0 border border-slate-600"
        style={{ background: overlay.color }}
      />

      {/* Info */}
      <div className="flex flex-col gap-0.5 flex-1 min-w-0">
        <span className="text-xs text-slate-200 truncate leading-tight">{overlay.text}</span>
        <span className="text-[9px] text-slate-500">
          {start} – {end} · {pos.replace(/-/g, " ")}
        </span>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-1 shrink-0">
        <button
          onClick={onEdit}
          className="text-slate-500 hover:text-slate-300 transition-colors"
          title="Edit overlay"
        >
          <Pencil className="h-3 w-3" />
        </button>
        <button
          onClick={onDelete}
          className="text-slate-500 hover:text-red-400 transition-colors"
          title="Delete overlay"
        >
          <Trash2 className="h-3 w-3" />
        </button>
      </div>
    </div>
  );
}
