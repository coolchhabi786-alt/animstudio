"use client";

import { Undo2, Redo2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  useTimelineStore,
  selectCanUndo,
  selectCanRedo,
} from "@/stores/timelineStore";

export function UndoRedoToolbar() {
  const canUndo      = useTimelineStore(selectCanUndo);
  const canRedo      = useTimelineStore(selectCanRedo);
  const undo         = useTimelineStore((s) => s.undo);
  const redo         = useTimelineStore((s) => s.redo);
  const historyIndex = useTimelineStore((s) => s.historyIndex);
  const historyLen   = useTimelineStore((s) => s.history.length);

  const undoTip = canUndo
    ? `Undo (${historyIndex} action${historyIndex !== 1 ? "s" : ""} available)  Ctrl+Z`
    : "Nothing to undo";

  const redoTip = canRedo
    ? `Redo (${historyLen - historyIndex - 1} action${historyLen - historyIndex - 1 !== 1 ? "s" : ""} available)  Ctrl+Shift+Z`
    : "Nothing to redo";

  return (
    <>
      <Button
        size="icon"
        variant="ghost"
        className="h-8 w-8 text-slate-300"
        onClick={undo}
        disabled={!canUndo}
        title={undoTip}
      >
        <Undo2 className="h-4 w-4" />
      </Button>
      <Button
        size="icon"
        variant="ghost"
        className="h-8 w-8 text-slate-300"
        onClick={redo}
        disabled={!canRedo}
        title={redoTip}
      >
        <Redo2 className="h-4 w-4" />
      </Button>
    </>
  );
}
