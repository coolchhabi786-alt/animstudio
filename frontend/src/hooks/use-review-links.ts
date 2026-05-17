"use client";

import { apiFetch } from "@/lib/api-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ReviewLink } from "@/types";

export function useReviewLinks(episodeId: string) {
  return useQuery<ReviewLink[]>({
    queryKey: ["review-links", episodeId],
    queryFn: () => apiFetch<ReviewLink[]>(`/api/v1/episodes/${episodeId}/review-links`),
    enabled: !!episodeId,
    staleTime: 30_000,
  });
}

export function useCreateReviewLink() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      renderId,
      body,
    }: {
      renderId: string;
      body: { expiresInDays?: number; password?: string };
    }) =>
      apiFetch<ReviewLink>(`/api/v1/renders/${renderId}/review-links`, {
        method: "POST",
        body: JSON.stringify(body),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["review-links"] });
    },
  });
}

export function useRevokeReviewLink() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiFetch<void>(`/api/v1/review-links/${id}`, { method: "DELETE" }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["review-links"] });
    },
  });
}
