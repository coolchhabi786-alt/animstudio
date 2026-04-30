"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api-client";
import type {
  RenderDto,
  StartRenderRequest,
} from "@/types";

const KEYS = {
  history: (episodeId: string) => ["renders", "history", episodeId] as const,
  single:  (renderId: string)  => ["renders", "single",  renderId]  as const,
};

/** GET /api/v1/episodes/{id}/renders — render history, newest-first. */
export function useRenderHistory(episodeId: string | undefined) {
  return useQuery<RenderDto[]>({
    queryKey: KEYS.history(episodeId ?? ""),
    queryFn: () =>
      apiFetch<RenderDto[]>(`/api/v1/episodes/${episodeId}/renders`),
    enabled: !!episodeId,
    staleTime: 10_000,
    refetchInterval: (query) => {
      // Keep polling while any render is still in-flight.
      const data = query.state.data;
      const active = data?.some(
        (r) => r.status === "Pending" || r.status === "Rendering",
      );
      return active ? 3_000 : false;
    },
  });
}

/** GET /api/v1/renders/{id} — single render polling. */
export function useRender(renderId: string | undefined) {
  return useQuery<RenderDto>({
    queryKey: KEYS.single(renderId ?? ""),
    queryFn: () => apiFetch<RenderDto>(`/api/v1/renders/${renderId}`),
    enabled: !!renderId,
    staleTime: 5_000,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status === "Pending" || status === "Rendering" ? 2_000 : false;
    },
  });
}

/** POST /api/v1/episodes/{id}/renders — start a new render. */
export function useStartRender(episodeId: string) {
  const qc = useQueryClient();
  return useMutation<RenderDto, Error, StartRenderRequest>({
    mutationFn: (body) =>
      apiFetch<RenderDto>(`/api/v1/episodes/${episodeId}/renders`, {
        method: "POST",
        body: JSON.stringify(body),
      }),
    onSuccess: (newRender) => {
      // Optimistically prepend to history
      qc.setQueryData<RenderDto[]>(
        KEYS.history(episodeId),
        (prev) => (prev ? [newRender, ...prev] : [newRender]),
      );
      qc.invalidateQueries({ queryKey: KEYS.history(episodeId) });
    },
  });
}
