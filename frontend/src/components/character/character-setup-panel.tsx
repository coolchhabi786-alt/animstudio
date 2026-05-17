"use client";

import { useState } from "react";
import { Users, Plus, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useCharacters } from "@/hooks/use-characters";
import type { CharacterSelection } from "@/types";

interface Props {
  onConfirm: (selection: CharacterSelection) => void;
}

const ROLE_COLOURS: Record<string, string> = {
  Ready: "bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300",
};

export function CharacterSetupPanel({ onConfirm }: Props) {
  const { data, isLoading } = useCharacters(1, 100);
  const readyCharacters = (data?.items ?? []).filter(
    (c) => c.trainingStatus === "Ready"
  );

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [allowNew, setAllowNew] = useState(false);
  const [newCount, setNewCount] = useState<string>("");
  const [newNameInput, setNewNameInput] = useState("");
  const [newNames, setNewNames] = useState<string[]>([]);

  function toggleCharacter(id: string) {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function addName() {
    const trimmed = newNameInput.trim();
    if (trimmed && !newNames.includes(trimmed)) {
      setNewNames((prev) => [...prev, trimmed]);
    }
    setNewNameInput("");
  }

  function handleConfirm() {
    onConfirm({
      existingCharacterIds: Array.from(selectedIds),
      allowNewCharacters: selectedIds.size === 0 ? true : allowNew,
      newCharacterCount: allowNew && newCount ? parseInt(newCount, 10) : undefined,
      newCharacterNames: allowNew && newNames.length > 0 ? newNames : undefined,
    });
  }

  if (isLoading) {
    return (
      <div className="rounded-xl border border-gray-200 dark:border-gray-800 p-5 animate-pulse">
        <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-1/3 mb-3" />
        <div className="space-y-2">
          {[1, 2].map((n) => (
            <div key={n} className="h-10 bg-gray-100 dark:bg-gray-800 rounded" />
          ))}
        </div>
      </div>
    );
  }

  if (readyCharacters.length === 0) return null;

  return (
    <section className="rounded-xl border border-violet-200 dark:border-violet-800 bg-violet-50/60 dark:bg-violet-950/20 p-5 space-y-4">
      {/* Header */}
      <div className="flex items-center gap-2">
        <div className="h-8 w-8 rounded-full bg-violet-100 dark:bg-violet-900/50 flex items-center justify-center">
          <Users className="h-4 w-4 text-violet-600 dark:text-violet-400" />
        </div>
        <div>
          <h2 className="text-sm font-semibold text-violet-900 dark:text-violet-100">
            Use Existing Characters
          </h2>
          <p className="text-xs text-violet-600 dark:text-violet-400">
            Select trained characters to feature in this episode's story.
            Unselected characters will not appear.
          </p>
        </div>
      </div>

      {/* Character checkboxes */}
      <div className="space-y-2">
        {readyCharacters.map((char) => {
          const checked = selectedIds.has(char.id);
          return (
            <label
              key={char.id}
              className={`flex items-center gap-3 px-4 py-3 rounded-lg border cursor-pointer transition-colors ${
                checked
                  ? "border-violet-400 dark:border-violet-600 bg-violet-50 dark:bg-violet-950/40"
                  : "border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 hover:border-violet-300 dark:hover:border-violet-700"
              }`}
            >
              <input
                type="checkbox"
                className="accent-violet-600"
                checked={checked}
                onChange={() => toggleCharacter(char.id)}
              />
              <div className="h-8 w-8 rounded-full bg-gradient-to-br from-violet-200 to-indigo-300 dark:from-violet-800 dark:to-indigo-700 flex items-center justify-center shrink-0 text-sm font-bold text-violet-700 dark:text-violet-200">
                {char.name.charAt(0).toUpperCase()}
              </div>
              <div className="flex-1 min-w-0">
                <span className="text-sm font-medium">{char.name}</span>
                {char.description && (
                  <p className="text-xs text-muted-foreground truncate mt-0.5">
                    {char.description}
                  </p>
                )}
              </div>
              <span className={`text-[11px] px-2 py-0.5 rounded-full font-medium ${ROLE_COLOURS.Ready}`}>
                Ready
              </span>
            </label>
          );
        })}
      </div>

      {/* "Add new characters?" toggle — only shown when at least one existing is selected */}
      {selectedIds.size > 0 && (
        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            className="accent-violet-600"
            checked={allowNew}
            onChange={(e) => setAllowNew(e.target.checked)}
          />
          <span className="text-sm text-gray-700 dark:text-gray-300">
            Introduce new characters alongside the selected ones
          </span>
        </label>
      )}

      {/* New character options (count + names) */}
      {(selectedIds.size === 0 || allowNew) && (
        <div className="space-y-3 pt-1">
          <div className="flex gap-3">
            <div className="flex flex-col gap-1.5 w-32">
              <Label htmlFor="new-char-count" className="text-xs">
                How many new?
              </Label>
              <Input
                id="new-char-count"
                type="number"
                min={1}
                max={10}
                placeholder="Auto"
                value={newCount}
                onChange={(e) => setNewCount(e.target.value)}
              />
            </div>
            <div className="flex flex-col gap-1.5 flex-1">
              <Label htmlFor="new-char-name-input" className="text-xs">
                New character names (optional)
              </Label>
              <div className="flex gap-2">
                <Input
                  id="new-char-name-input"
                  type="text"
                  placeholder="e.g. Mia, Rex…"
                  value={newNameInput}
                  onChange={(e) => setNewNameInput(e.target.value)}
                  onKeyDown={(e) => {
                    if ((e.key === "Enter" || e.key === ",") && newNameInput.trim()) {
                      e.preventDefault();
                      addName();
                    }
                  }}
                />
                <Button type="button" variant="outline" size="sm" onClick={addName}>
                  <Plus className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          </div>
          {newNames.length > 0 && (
            <div className="flex flex-wrap gap-1.5">
              {newNames.map((n) => (
                <span
                  key={n}
                  className="inline-flex items-center gap-1 rounded-full bg-blue-100 dark:bg-blue-900/40 px-2.5 py-0.5 text-xs text-blue-700 dark:text-blue-300"
                >
                  {n}
                  <button
                    type="button"
                    onClick={() => setNewNames((prev) => prev.filter((x) => x !== n))}
                    className="ml-0.5 opacity-60 hover:opacity-100"
                    aria-label={`Remove ${n}`}
                  >
                    <X className="h-3 w-3" />
                  </button>
                </span>
              ))}
            </div>
          )}
        </div>
      )}

      {/* CTA */}
      <div className="flex items-center justify-between pt-1">
        <p className="text-xs text-muted-foreground">
          {selectedIds.size > 0
            ? `${selectedIds.size} existing character${selectedIds.size !== 1 ? "s" : ""} selected`
            : "No existing characters selected — AI will invent all characters"}
        </p>
        <Button
          type="button"
          onClick={handleConfirm}
          className="bg-violet-600 hover:bg-violet-700 text-white"
        >
          Confirm & Generate
        </Button>
      </div>
    </section>
  );
}
