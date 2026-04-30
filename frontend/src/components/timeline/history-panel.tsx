"use client";

import { useTimelineStore } from "@/stores/timelineStore";

interface HistoryPanelProps {
  isDev?: boolean;
}

/**
 * Dev-only floating panel that displays the full undo/redo stack.
 * Render it with isDev={process.env.NODE_ENV === "development"}.
 */
export function HistoryPanel({ isDev = false }: HistoryPanelProps) {
  const history      = useTimelineStore((s) => s.history);
  const historyIndex = useTimelineStore((s) => s.historyIndex);
  const undo         = useTimelineStore((s) => s.undo);
  const redo         = useTimelineStore((s) => s.redo);

  if (!isDev) return null;

  function jumpTo(target: number) {
    if (target < historyIndex) {
      for (let i = historyIndex; i > target; i--) undo();
    } else {
      for (let i = historyIndex; i < target; i++) redo();
    }
  }

  return (
    <div className="fixed right-4 top-1/2 -translate-y-1/2 z-50 w-52 bg-[#0f1623] border border-slate-700 rounded-lg shadow-xl overflow-hidden select-none">
      {/* Header */}
      <div className="px-3 py-2 border-b border-slate-700 flex items-center justify-between">
        <span className="text-[10px] font-semibold text-slate-400 uppercase tracking-wider">
          History
        </span>
        <span className="text-[9px] text-slate-600">
          {historyIndex + 1} / {history.length} (max 50)
        </span>
      </div>

      {/* Stack entries */}
      <div className="overflow-y-auto max-h-72">
        {history.map((_, i) => (
          <button
            key={i}
            className={`w-full text-left px-3 py-1.5 text-[10px] transition-colors hover:bg-slate-800 ${
              i === historyIndex
                ? "text-blue-300 font-semibold bg-blue-500/10"
                : i < historyIndex
                ? "text-slate-400"
                : "text-slate-600"
            }`}
            onClick={() => jumpTo(i)}
          >
            State #{i + 1}
            {i === historyIndex && (
              <span className="ml-1.5 text-blue-500">← current</span>
            )}
            {i > historyIndex && (
              <span className="ml-1.5 text-slate-700">(redo)</span>
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
