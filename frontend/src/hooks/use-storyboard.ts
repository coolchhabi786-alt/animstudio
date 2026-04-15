"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { apiFetch } from "@/lib/api-client";
import { useSignalR } from "@/hooks/use-signalr";
import type { StoryboardDto, ShotUpdatedPayload, JobDto } from "@/types";

// ── Query keys ────────────────────────────────────────────────────────────────
const KEYS = {
  detail: (episodeId: string) => ["storyboard", episodeId] as const,
};

// ── Queries ───────────────────────────────────────────────────────────────────

/** Fetches the storyboard for an episode (null if not yet generated). */
export function useStoryboard(episodeId: string | undefined) {
  return useQuery<StoryboardDto | null>({
    queryKey: KEYS.detail(episodeId ?? ""),
    queryFn: async () => {
      try {
        return await apiFetch<StoryboardDto>(
          `/api/v1/episodes/${episodeId}/storyboard`,
        );
      } catch (err: unknown) {
        if (err instanceof Error && err.message.includes("404")) return null;
        if (err instanceof Error && err.message.includes("NO_STORYBOARD"))
          return null;
        throw err;
      }
    },
    enabled: !!episodeId,
    staleTime: 60_000,
    retry: false,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

/** Enqueues a StoryboardPlan job. Returns the accepted JobDto on 202. */
export function useGenerateStoryboard(episodeId: string) {
  const qc = useQueryClient();

  return useMutation<JobDto, Error, { directorNotes?: string }>({
    mutationFn: (body) =>
      apiFetch<JobDto>(`/api/v1/episodes/${episodeId}/storyboard`, {
        method: "POST",
        body: JSON.stringify({ directorNotes: body.directorNotes ?? null }),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.detail(episodeId) });
    },
  });
}

/** Re-queues a single shot for regeneration, optionally with style override. */
export function useRegenerateShot(episodeId: string) {
  const qc = useQueryClient();

  return useMutation<JobDto, Error, { shotId: string; styleOverride?: string }>({
    mutationFn: ({ shotId, styleOverride }) =>
      apiFetch<JobDto>(`/api/v1/shots/${shotId}/regenerate`, {
        method: "POST",
        body: JSON.stringify({ styleOverride: styleOverride ?? null }),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.detail(episodeId) });
    },
  });
}

/** Persists a per-shot style override and re-queues the shot for rendering. */
export function useUpdateShotStyle(episodeId: string) {
  const qc = useQueryClient();

  return useMutation<JobDto, Error, { shotId: string; styleOverride: string | null }>({
    mutationFn: ({ shotId, styleOverride }) =>
      apiFetch<JobDto>(`/api/v1/shots/${shotId}/style`, {
        method: "PUT",
        body: JSON.stringify({ styleOverride }),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.detail(episodeId) });
    },
  });
}

// ── Real-time: subscribe to ShotUpdated broadcasts ───────────────────────────

/**
 * Joins the team's SignalR group and patches the storyboard cache on every
 * <c>ShotUpdated</c> broadcast. Returns connection state so UI can render a
 * subtle live indicator.
 */
export function useStoryboardRealtime(params: {
  episodeId: string | undefined;
  teamId: string | undefined;
  hubUrl: string;
}) {
  const { episodeId, teamId, hubUrl } = params;
  const qc = useQueryClient();
  const { connection, connected } = useSignalR(hubUrl);

  useEffect(() => {
    if (!connection || !connected || !teamId || !episodeId) return;

    void connection.invoke("JoinTeamGroup", teamId).catch(() => {
      /* handled by SignalR logger */
    });

    const handler = (payload: ShotUpdatedPayload) => {
      if (payload.episodeId !== episodeId) return;
      qc.setQueryData<StoryboardDto | null>(KEYS.detail(episodeId), (prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          shots: prev.shots.map((s) =>
            s.id === payload.shotId
              ? {
                  ...s,
                  imageUrl: payload.imageUrl ?? s.imageUrl,
                  regenerationCount: payload.regenerationCount,
                  updatedAt: new Date().toISOString(),
                }
              : s,
          ),
        };
      });
    };

    connection.on("ShotUpdated", handler);

    return () => {
      connection.off("ShotUpdated", handler);
      void connection.invoke("LeaveTeamGroup", teamId).catch(() => {
        /* ignore */
      });
    };
  }, [connection, connected, teamId, episodeId, qc]);

  return { connected, state: connection?.state ?? signalR.HubConnectionState.Disconnected };
}
