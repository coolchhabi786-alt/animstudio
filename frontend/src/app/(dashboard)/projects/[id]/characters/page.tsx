"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { Skeleton } from "@/components/ui/skeleton";
import { CharacterForm } from "@/components/character/character-form";
import { CharacterCard } from "@/components/character/character-card";
import { useCharacters, useCharacterTrainingUpdates, useDeleteCharacter } from "@/hooks/use-characters";
import { useSession } from "next-auth/react";
import { toast } from "sonner";

/**
 * Character Studio page — left panel "Create Character" form,
 * right panel character card gallery with real-time training updates.
 *
 * Route: /projects/[id]/characters
 */
export default function CharactersPage() {
  const params = useParams<{ id: string }>();
  const projectId = params.id;

  // teamId lives in the JWT session (set by auth.ts JWT callback)
  const { data: session } = useSession();
  const teamId = (session?.user as any)?.teamId as string | undefined;

  // Real-time training updates via SignalR
  useCharacterTrainingUpdates(teamId);

  const { data: pagedData, isLoading } = useCharacters(1, 50);
  const deleteMutation = useDeleteCharacter();

  const [showForm, setShowForm] = useState(true);

  async function handleDelete(characterId: string) {
    const confirmed = window.confirm(
      "Delete this character? This cannot be undone."
    );
    if (!confirmed) return;

    try {
      await deleteMutation.mutateAsync(characterId);
      toast.success("Character deleted.");
    } catch {
      // apiFetch already shows a toast on error
    }
  }

  return (
    <main className="flex h-full min-h-screen flex-col">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        className="flex items-center gap-2 border-b bg-white px-6 py-3 text-sm text-gray-500"
      >
        <Link href="/projects" className="hover:text-gray-700 hover:underline">
          Projects
        </Link>
        <span aria-hidden="true">/</span>
        <Link
          href={`/projects/${projectId}`}
          className="hover:text-gray-700 hover:underline"
        >
          Project
        </Link>
        <span aria-hidden="true">/</span>
        <span className="font-medium text-gray-900">Characters</span>
      </nav>

      <div className="flex flex-1 flex-col gap-6 p-6 lg:flex-row">
        {/* ── Left panel: Create Character ────────────────────────────────── */}
        <aside
          className="w-full shrink-0 lg:w-80"
          aria-label="Create character panel"
        >
          <div className="sticky top-6 rounded-xl border bg-white p-5 shadow-sm">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-base font-semibold text-gray-900">
                Create Character
              </h2>
              <button
                onClick={() => setShowForm((p) => !p)}
                aria-expanded={showForm}
                className="text-sm text-purple-600 hover:underline"
              >
                {showForm ? "Hide" : "Show"}
              </button>
            </div>

            {showForm && (
              <CharacterForm onSuccess={() => setShowForm(false)} />
            )}
          </div>
        </aside>

        {/* ── Right panel: Character gallery ─────────────────────────────── */}
        <section aria-label="Character gallery" className="flex-1">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-base font-semibold text-gray-900">
              Team Characters
              {pagedData && (
                <span className="ml-2 text-sm font-normal text-gray-400">
                  ({pagedData.totalCount})
                </span>
              )}
            </h2>
          </div>

          {isLoading ? (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="overflow-hidden rounded-xl border bg-white">
                  <Skeleton className="aspect-square w-full" />
                  <div className="p-3">
                    <Skeleton className="mb-2 h-4 w-3/4" />
                    <Skeleton className="h-3 w-1/2" />
                  </div>
                </div>
              ))}
            </div>
          ) : !pagedData?.items.length ? (
            <div className="flex min-h-[200px] flex-col items-center justify-center gap-2 rounded-xl border border-dashed bg-gray-50 p-8 text-center">
              <p className="text-sm text-gray-500">
                No characters yet. Create your first character to get started.
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {pagedData.items.map((character) => (
                <CharacterCard
                  key={character.id}
                  character={character}
                  onDelete={handleDelete}
                />
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
