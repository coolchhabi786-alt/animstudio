"use client";

import { apiFetch } from "@/lib/api-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { EpisodeDto } from "@/types";

export function useEpisodes(projectId: string) {
  return useQuery<EpisodeDto[]>({
    queryKey: ["episodes", projectId],
    queryFn: () =>
      apiFetch<EpisodeDto[]>(`/api/v1/projects/${projectId}/episodes`),
    enabled: !!projectId,
    staleTime: 30_000,
  });
}

export function useEpisode(episodeId: string) {
  return useQuery<EpisodeDto>({
    queryKey: ["episodes", "detail", episodeId],
    queryFn: () => apiFetch<EpisodeDto>(`/api/v1/episodes/${episodeId}`),
    enabled: !!episodeId,
    staleTime: 30_000,
  });
}

export function useCreateEpisode(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: { name: string; idea?: string; style?: string; templateId?: string; characterPreferences?: string }) =>
      apiFetch<EpisodeDto>(`/api/v1/projects/${projectId}/episodes`, {
        method: "POST",
        body: JSON.stringify(body),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["episodes", projectId] });
    },
  });
}

export function useUpdateEpisode(episodeId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: { idea: string; style?: string; characterPreferences?: string }) =>
      apiFetch<EpisodeDto>(`/api/v1/episodes/${episodeId}`, {
        method: "PATCH",
        body: JSON.stringify(body),
      }),
    onSuccess: (updated) => {
      queryClient.setQueryData(["episodes", "detail", episodeId], updated);
    },
  });
}

export function useDispatchEpisodeJob(episodeId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (jobType: string) =>
      apiFetch<void>(`/api/v1/episodes/${episodeId}/dispatch`, {
        method: "POST",
        body: JSON.stringify({ jobType }),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["saga-state", episodeId] });
    },
  });
}
