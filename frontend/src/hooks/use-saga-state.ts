"use client";

import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { SagaStateDto } from "@/types";

export function useSagaState(episodeId: string) {
  return useQuery<SagaStateDto | null>({
    queryKey: ["saga-state", episodeId],
    queryFn: async () => {
      try {
        return await apiFetch<SagaStateDto>(`/api/v1/episodes/${episodeId}/saga`);
      } catch {
        return null;
      }
    },
    enabled: !!episodeId,
    refetchInterval: 5000,
    retry: false,
  });
}
