"use client";

import { apiFetch } from "@/lib/api-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { BrandKit } from "@/types";

export function useBrandKit(teamId: string) {
  return useQuery<BrandKit | null>({
    queryKey: ["brand-kit", teamId],
    queryFn: () => apiFetch<BrandKit>(`/api/v1/teams/${teamId}/brand-kit`),
    enabled: !!teamId,
    staleTime: 60_000,
  });
}

export function useUpsertBrandKit(teamId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: Partial<BrandKit>) =>
      apiFetch<BrandKit>(`/api/v1/teams/${teamId}/brand-kit`, {
        method: "PUT",
        body: JSON.stringify(body),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["brand-kit", teamId] });
    },
  });
}
