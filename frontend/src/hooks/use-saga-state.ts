"use client";

import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { SagaStateDto } from "@/types";

export function useSagaState(episodeId: string) {
  return useQuery<SagaStateDto>({
    queryKey: ["saga-state", episodeId],
    queryFn: () => apiFetch<SagaStateDto>(`/api/v1/episodes/${episodeId}/saga`),
    enabled: !!episodeId,
    refetchInterval: 5000,
  });
}
