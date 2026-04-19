"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { apiFetch } from "@/lib/api-client";
import { useSignalR } from "@/hooks/use-signalr";
import type {
  AnimationBackend,
  AnimationClipDto,
  AnimationEstimateDto,
  AnimationJobDto,
  ApproveAnimationRequest,
  ClipReadyEvent,
  SignedClipUrlDto,
} from "@/types";

const KEYS = {
  estimate: (episodeId: string, backend: AnimationBackend) =>
    ["animation", "estimate", episodeId, backend] as const,
  clips: (episodeId: string) => ["animation", "clips", episodeId] as const,
};

/** GET /api/v1/episodes/{id}/animation/estimate */
export function useAnimationEstimate(
  episodeId: string | undefined,
  backend: AnimationBackend,
) {
  return useQuery<AnimationEstimateDto>({
    queryKey: KEYS.estimate(episodeId ?? "", backend),
    queryFn: () =>
      apiFetch<AnimationEstimateDto>(
        `/api/v1/episodes/${episodeId}/animation/estimate?backend=${backend}`,
      ),
    enabled: !!episodeId,
    staleTime: 30_000,
  });
}

/** GET /api/v1/episodes/{id}/animation */
export function useAnimationClips(episodeId: string | undefined) {
  return useQuery<AnimationClipDto[]>({
    queryKey: KEYS.clips(episodeId ?? ""),
    queryFn: () =>
      apiFetch<AnimationClipDto[]>(`/api/v1/episodes/${episodeId}/animation`),
    enabled: !!episodeId,
    staleTime: 15_000,
  });
}

/** POST /api/v1/episodes/{id}/animation — approve + enqueue job. */
export function useApproveAnimation(episodeId: string) {
  const qc = useQueryClient();
  return useMutation<AnimationJobDto, Error, ApproveAnimationRequest>({
    mutationFn: (body) =>
      apiFetch<AnimationJobDto>(`/api/v1/episodes/${episodeId}/animation`, {
        method: "POST",
        body: JSON.stringify(body),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.clips(episodeId) });
    },
  });
}

/** GET /api/v1/episodes/{id}/animation/clips/{clipId} */
export async function fetchSignedClipUrl(
  episodeId: string,
  clipId: string,
): Promise<SignedClipUrlDto> {
  return apiFetch<SignedClipUrlDto>(
    `/api/v1/episodes/${episodeId}/animation/clips/${clipId}`,
  );
}

/**
 * Joins the team's SignalR group and patches the clips cache whenever
 * a <c>ClipReady</c> event arrives for the current episode.
 */
export function useAnimationRealtime(params: {
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

    const handler = (payload: ClipReadyEvent) => {
      if (payload.episodeId !== episodeId) return;
      qc.setQueryData<AnimationClipDto[]>(
        KEYS.clips(episodeId),
        (prev) => {
          if (!prev) return prev;
          return prev.map((c) =>
            c.id === payload.clipId
              ? { ...c, clipUrl: payload.clipUrl, status: "Ready" }
              : c,
          );
        },
      );
    };

    connection.on("ClipReady", handler);

    return () => {
      connection.off("ClipReady", handler);
      void connection.invoke("LeaveTeamGroup", teamId).catch(() => {
        /* ignore */
      });
    };
  }, [connection, connected, teamId, episodeId, qc]);

  return {
    connected,
    state: connection?.state ?? signalR.HubConnectionState.Disconnected,
  };
}
