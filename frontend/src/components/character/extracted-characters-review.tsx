"use client";

import { useState } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { Users, Sparkles, ChevronDown, ChevronUp, ExternalLink } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { useApproveCharactersForTraining, useDetachCharacter } from "@/hooks/use-characters";
import type { CharacterDto } from "@/types";

const ROLE_COLOURS: Record<string, string> = {
  lead: "bg-indigo-100 text-indigo-700 dark:bg-indigo-900/40 dark:text-indigo-300",
  side: "bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300",
  other: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400",
};

interface Props {
  projectId: string;
  episodeId: string;
  characters: CharacterDto[];
  onApproved: () => void;
}

export function ExtractedCharactersReview({ projectId, episodeId, characters, onApproved }: Props) {
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [pendingRemoveId, setPendingRemoveId] = useState<string | null>(null);
  const approve = useApproveCharactersForTraining(episodeId);
  const detach = useDetachCharacter(episodeId);

  const pendingRemoveChar = characters.find((c) => c.id === pendingRemoveId);

  async function handleApprove() {
    try {
      const result = await approve.mutateAsync();
      toast.success(
        `Training started for ${result.approved} character${result.approved !== 1 ? "s" : ""}.`
      );
      onApproved();
    } catch {
      // apiFetch already shows a toast
    }
  }

  async function confirmRemove() {
    if (!pendingRemoveId) return;
    try {
      await detach.mutateAsync(pendingRemoveId);
    } catch {
      // error already toasted
    } finally {
      setPendingRemoveId(null);
    }
  }

  return (
    <section className="rounded-xl border border-blue-200 dark:border-blue-800 bg-blue-50/60 dark:bg-blue-950/20 p-5 space-y-4">
      {/* Header */}
      <div className="flex items-start gap-2">
        <div className="h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-900/50 flex items-center justify-center shrink-0">
          <Users className="h-4 w-4 text-blue-600 dark:text-blue-400" />
        </div>
        <div className="flex-1 min-w-0">
          <h2 className="text-sm font-semibold text-blue-900 dark:text-blue-100">
            Characters Found — Review Before Training
          </h2>
          <p className="text-xs text-blue-600 dark:text-blue-400">
            {characters.length} character{characters.length !== 1 ? "s" : ""} identified by the AI.
            Edit or remove any before starting LoRA training.
          </p>
        </div>
        <Link
          href={`/projects/${projectId}/characters`}
          className="inline-flex items-center gap-1 text-xs text-blue-600 hover:text-blue-800 hover:underline shrink-0"
        >
          <ExternalLink className="h-3 w-3" />
          Character Studio
        </Link>
      </div>

      {/* Character cards */}
      <div className="space-y-2">
        {characters.map((char) => {
          const isExpanded = expandedId === char.id;
          const roleKey = (char.description?.match(/^(lead|side|other)/i)?.[0] ?? "other").toLowerCase();

          return (
            <div
              key={char.id}
              className="rounded-lg border bg-white dark:bg-gray-900 border-blue-100 dark:border-blue-900/50"
            >
              <div className="flex items-center gap-3 px-4 py-3">
                {/* Avatar placeholder */}
                <div className="h-9 w-9 rounded-full bg-gradient-to-br from-blue-200 to-indigo-300 dark:from-blue-800 dark:to-indigo-700 flex items-center justify-center shrink-0 text-sm font-bold text-blue-700 dark:text-blue-200">
                  {char.name.charAt(0).toUpperCase()}
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="text-sm font-medium truncate">{char.name}</span>
                    <span
                      className={`text-[11px] px-2 py-0.5 rounded-full font-medium ${ROLE_COLOURS[roleKey] ?? ROLE_COLOURS.other}`}
                    >
                      {roleKey}
                    </span>
                  </div>
                  {char.description && (
                    <p className="text-xs text-muted-foreground truncate mt-0.5">
                      {char.description}
                    </p>
                  )}
                </div>

                <div className="flex items-center gap-1 shrink-0">
                  <button
                    type="button"
                    onClick={() => setExpandedId(isExpanded ? null : char.id)}
                    className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 text-muted-foreground"
                    aria-label={isExpanded ? "Collapse" : "Expand style DNA"}
                  >
                    {isExpanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
                  </button>
                  <button
                    type="button"
                    onClick={() => setPendingRemoveId(char.id)}
                    className="p-1.5 rounded hover:bg-red-50 dark:hover:bg-red-950/30 text-muted-foreground hover:text-red-500 transition-colors"
                    aria-label={`Remove ${char.name}`}
                  >
                    ×
                  </button>
                </div>
              </div>

              {isExpanded && char.styleDna && (
                <div className="px-4 pb-3 border-t border-blue-100 dark:border-blue-900/50 pt-2">
                  <p className="text-[11px] font-medium text-muted-foreground mb-1">Style DNA</p>
                  <p className="text-xs text-gray-600 dark:text-gray-400 leading-relaxed">
                    {char.styleDna}
                  </p>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* CTA */}
      <div className="flex items-center justify-between pt-1">
        <p className="text-xs text-muted-foreground">
          Training takes ~15 minutes per character.
        </p>
        <Button
          onClick={handleApprove}
          disabled={approve.isPending || characters.length === 0}
          className="bg-indigo-600 hover:bg-indigo-700 text-white gap-1.5"
        >
          <Sparkles className="h-4 w-4" />
          {approve.isPending ? "Starting…" : "Approve & Start Training"}
        </Button>
      </div>

      <Dialog
        open={pendingRemoveId !== null}
        onOpenChange={(open) => { if (!open) setPendingRemoveId(null); }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Remove character from script?</DialogTitle>
            <DialogDescription>
              Removing &ldquo;{pendingRemoveChar?.name}&rdquo; will detach them from this episode.
              They will not appear in the storyboard or animation.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setPendingRemoveId(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={detach.isPending}
              onClick={confirmRemove}
            >
              {detach.isPending ? "Removing…" : "Remove"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </section>
  );
}
