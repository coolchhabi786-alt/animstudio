"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api-client";
import type { ScriptDto, ScreenplayDto, JobDto } from "@/types";

// ── Query keys ────────────────────────────────────────────────────────────────
const KEYS = {
  detail: (episodeId: string) => ["script", episodeId] as const,
};

// ── Queries ────────────────────────────────────────────────────────────────────

/** Fetches the current script for an episode (null if not yet generated). */
export function useScript(episodeId: string | undefined) {
  return useQuery<ScriptDto | null>({
    queryKey: KEYS.detail(episodeId ?? ""),
    queryFn: async () => {
      try {
        return await apiFetch<ScriptDto>(`/api/v1/episodes/${episodeId}/script`);
      } catch (err: unknown) {
        // 404 means no script yet — return null instead of throwing
        if (err instanceof Error && err.message.includes("404")) return null;
        if (err instanceof Error && err.message.includes("NO_SCRIPT")) return null;
        throw err;
      }
    },
    enabled: !!episodeId,
    staleTime: 60_000,
    retry: false,
  });
}

// ── Mutations ──────────────────────────────────────────────────────────────────

/** Enqueues an AI scriptwriting job. Returns the accepted JobDto on 202. */
export function useGenerateScript(episodeId: string) {
  const qc = useQueryClient();

  return useMutation<JobDto, Error, { directorNotes?: string }>({
    mutationFn: (body) =>
      apiFetch<JobDto>(`/api/v1/episodes/${episodeId}/script`, {
        method: "POST",
        body: JSON.stringify({ directorNotes: body.directorNotes ?? null }),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.detail(episodeId) });
    },
  });
}

/** Saves manual edits to the screenplay. Marks the script as manually edited. */
export function useSaveScript(episodeId: string) {
  const qc = useQueryClient();

  return useMutation<ScriptDto, Error, ScreenplayDto>({
    mutationFn: (screenplay) =>
      apiFetch<ScriptDto>(`/api/v1/episodes/${episodeId}/script`, {
        method: "PUT",
        body: JSON.stringify({ screenplay }),
      }),
    onSuccess: (updated) => {
      qc.setQueryData(KEYS.detail(episodeId), updated);
    },
  });
}

/** Re-enqueues a scriptwriting job with optional director notes. */
export function useRegenerateScript(episodeId: string) {
  const qc = useQueryClient();

  return useMutation<JobDto, Error, { directorNotes?: string }>({
    mutationFn: (body) =>
      apiFetch<JobDto>(`/api/v1/episodes/${episodeId}/script/regenerate`, {
        method: "POST",
        body: JSON.stringify({ directorNotes: body.directorNotes ?? null }),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.detail(episodeId) });
    },
  });
}
