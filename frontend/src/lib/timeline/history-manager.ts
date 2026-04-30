import type { Timeline } from "@/types/timeline";

/**
 * Standalone history stack for timeline states.
 * The Zustand store already implements equivalent logic internally;
 * this class is exported for external consumers (e.g. dev tools, tests).
 */
export class HistoryManager {
  private stack: Timeline[] = [];
  private currentIndex = -1;
  private readonly maxHistorySize: number;

  constructor(maxHistorySize = 50) {
    this.maxHistorySize = maxHistorySize;
  }

  push(state: Timeline): void {
    // Discard any future states (the "redo branch")
    this.stack = this.stack.slice(0, this.currentIndex + 1);
    this.stack.push(state);
    if (this.stack.length > this.maxHistorySize) {
      this.stack.shift();
      // currentIndex stays the same: we removed one from the front
      // and added one to the end → net zero change in index
    } else {
      this.currentIndex = this.stack.length - 1;
    }
  }

  undo(): Timeline | null {
    if (!this.canUndo()) return null;
    this.currentIndex--;
    return this.stack[this.currentIndex];
  }

  redo(): Timeline | null {
    if (!this.canRedo()) return null;
    this.currentIndex++;
    return this.stack[this.currentIndex];
  }

  canUndo(): boolean { return this.currentIndex > 0; }
  canRedo(): boolean { return this.currentIndex < this.stack.length - 1; }

  current(): Timeline | null {
    return this.currentIndex >= 0 ? this.stack[this.currentIndex] : null;
  }

  getHistorySize(): { current: number; max: number } {
    return { current: this.stack.length, max: this.maxHistorySize };
  }

  clear(): void {
    this.stack = [];
    this.currentIndex = -1;
  }
}
