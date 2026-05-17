"use client";

import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { EpisodeAnalytics, TeamAnalytics } from "@/types";

export function useEpisodeAnalytics(episodeId: string) {
  return useQuery<EpisodeAnalytics>({
    queryKey: ["analytics", "episode", episodeId],
    queryFn: () => apiFetch<EpisodeAnalytics>(`/api/v1/episodes/${episodeId}/analytics`),
    enabled: !!episodeId,
    staleTime: 60_000,
  });
}

export function useTeamAnalytics(teamId: string) {
  return useQuery<TeamAnalytics>({
    queryKey: ["analytics", "team", teamId],
    queryFn: () => apiFetch<TeamAnalytics>(`/api/v1/teams/${teamId}/analytics`),
    enabled: !!teamId,
    staleTime: 60_000,
  });
}
