"use client";

import { useEffect, useRef } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import * as signalR from "@microsoft/signalr";
import { toast } from "sonner";
import { apiFetch } from "@/lib/api-client";
import type {
  CharacterDto,
  CharacterJobAcceptedDto,
  CharacterTrainingUpdatePayload,
  PagedResult,
} from "@/types";
import { normaliseTrainingStatus } from "@/types";

const BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5001";
const HUB_URL = `${BASE}/hubs/character-training`;

// ── Query keys ────────────────────────────────────────────────────────────────
const KEYS = {
  list: (page: number, pageSize: number) =>
    ["characters", "list", page, pageSize] as const,
  detail: (id: string) => ["characters", "detail", id] as const,
  episodeRoster: (episodeId: string) =>
    ["characters", "episode", episodeId] as const,
};

// ── Fetchers ──────────────────────────────────────────────────────────────────

/** Returns the team's character library (paginated). */
export function useCharacters(page = 1, pageSize = 20) {
  return useQuery<PagedResult<CharacterDto>>({
    queryKey: KEYS.list(page, pageSize),
    queryFn: async () => {
      const result = await apiFetch<PagedResult<CharacterDto>>(
        `/api/v1/characters?page=${page}&pageSize=${pageSize}`
      );
      // Normalise numeric trainingStatus enum → string
      return {
        ...result,
        items: result.items.map((c) => ({
          ...c,
          trainingStatus: normaliseTrainingStatus(c.trainingStatus as any),
        })),
      };
    },
    staleTime: 30_000, // 30 s — refreshed by SignalR events
  });
}

/** Returns a single character by ID. */
export function useCharacter(id: string | undefined) {
  return useQuery<CharacterDto>({
    queryKey: KEYS.detail(id ?? ""),
    queryFn: () => apiFetch<CharacterDto>(`/api/v1/characters/${id}`),
    enabled: !!id,
    staleTime: 30_000,
  });
}

/** Returns all characters attached to a specific episode. */
export function useEpisodeCharacters(episodeId: string | undefined) {
  return useQuery<CharacterDto[]>({
    queryKey: KEYS.episodeRoster(episodeId ?? ""),
    queryFn: async () => {
      const items = await apiFetch<CharacterDto[]>(`/api/v1/episodes/${episodeId}/characters`);
      return items.map((c) => ({
        ...c,
        trainingStatus: normaliseTrainingStatus(c.trainingStatus as any),
      }));
    },
    enabled: !!episodeId,
    staleTime: 30_000,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

/** Creates a new character and enqueues LoRA training. Returns 202 response. */
export function useCreateCharacter() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: {
      name: string;
      description?: string;
      styleDna?: string;
    }) =>
      apiFetch<CharacterJobAcceptedDto>("/api/v1/characters", {
        method: "POST",
        body: JSON.stringify(payload),
      }),
    onSuccess: () => {
      // Invalidate character list so the new Draft card appears
      qc.invalidateQueries({ queryKey: ["characters", "list"] });
    },
  });
}

/** Soft-deletes a character. */
export function useDeleteCharacter() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (characterId: string) =>
      apiFetch<void>(`/api/v1/characters/${characterId}`, { method: "DELETE" }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["characters", "list"] });
    },
    onError: (error: unknown) => {
      const msg = error instanceof Error ? error.message : String(error);
      const isInUse = msg.toLowerCase().includes("in use") || msg.includes("CHARACTER_IN_USE");
      toast.error(
        isInUse
          ? "Character is used in an episode and cannot be deleted."
          : "Failed to delete character. Please try again."
      );
    },
  });
}

/** Attaches a character to an episode. */
export function useAttachCharacter(episodeId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (characterId: string) =>
      apiFetch<void>(`/api/v1/episodes/${episodeId}/characters`, {
        method: "POST",
        body: JSON.stringify({ characterId }),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.episodeRoster(episodeId) });
    },
  });
}

/**
 * Advances all Draft characters in an episode to TrainingQueued and dispatches
 * design jobs. Called by the "Approve & Start Training" button on the script page.
 */
export function useApproveCharactersForTraining(episodeId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () =>
      apiFetch<{ approved: number }>(
        `/api/v1/episodes/${episodeId}/characters/approve-training`,
        { method: "POST" }
      ),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.episodeRoster(episodeId) });
    },
  });
}

/** Retries character training (restarts CharacterDesign or LoRA-only depending on state). */
export function useRetryCharacterTraining() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (characterId: string) =>
      apiFetch(`/api/v1/characters/${characterId}/retry-training`, { method: "POST" }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["characters", "list"] });
      toast.success("Training restarted.");
    },
    onError: () => toast.error("Failed to restart training. Please try again."),
  });
}

/** Re-generates all dataset pose images for a character, restarting LoRA training from scratch. */
export function useRegenerateDataset() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (characterId: string) =>
      apiFetch(`/api/v1/characters/${characterId}/regenerate-dataset`, { method: "POST" }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["characters", "list"] });
      toast.success("Dataset regeneration started.");
    },
    onError: () => toast.error("Failed to start regeneration. Please try again."),
  });
}

/** Detaches a character from an episode. */
export function useDetachCharacter(episodeId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (characterId: string) =>
      apiFetch<void>(
        `/api/v1/episodes/${episodeId}/characters/${characterId}`,
        { method: "DELETE" }
      ),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.episodeRoster(episodeId) });
    },
  });
}

// ── SignalR subscription ────────────────────────────────────────────────────────

/**
 * Subscribes to real-time character training updates for the current team.
 * Patches TanStack Query cache when a `CharacterTrainingUpdate` message arrives,
 * so the UI reflects progress without a manual refresh.
 *
 * Call this once in the Character Studio page (or root layout when the user is
 * authenticated). The connection is torn down when the component unmounts.
 */
export function useCharacterTrainingUpdates(teamId: string | undefined) {
  const qc = useQueryClient();
  const connRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!teamId) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connRef.current = connection;

    connection
      .start()
      .then(() => connection.invoke("JoinTeamGroup", teamId))
      .catch((err) =>
        console.error("[CharacterHub] Connection error:", err)
      );

    connection.on(
      "CharacterTrainingUpdate",
      (payload: CharacterTrainingUpdatePayload) => {
        const status = normaliseTrainingStatus(payload.status as any);

        // Update the detail query if it's in cache
        qc.setQueryData<CharacterDto>(
          KEYS.detail(payload.characterId),
          (old) =>
            old
              ? {
                  ...old,
                  trainingStatus: status,
                  trainingProgressPercent: payload.progressPercent,
                }
              : old
        );

        // Update the character in the list query cache
        qc.setQueriesData<PagedResult<CharacterDto>>(
          { queryKey: ["characters", "list"] },
          (old) => {
            if (!old) return old;
            return {
              ...old,
              items: old.items.map((c) =>
                c.id === payload.characterId
                  ? {
                      ...c,
                      trainingStatus: status,
                      trainingProgressPercent: payload.progressPercent,
                    }
                  : c
              ),
            };
          }
        );
      }
    );

    return () => {
      connection.stop();
    };
  }, [teamId, qc]);
}
