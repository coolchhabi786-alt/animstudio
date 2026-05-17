"use client";

import { apiFetch } from "@/lib/api-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ReviewComment } from "@/types";

export interface ReviewPublicDto {
  token: string;
  episodeName: string;
  videoUrl: string;
  hasPassword: boolean;
  isExpired: boolean;
  isRevoked: boolean;
}

export function useReview(token: string, password?: string) {
  const qs = password ? `?password=${encodeURIComponent(password)}` : "";
  return useQuery<ReviewPublicDto>({
    queryKey: ["review", token, password ?? ""],
    queryFn: () => apiFetch<ReviewPublicDto>(`/api/v1/review/${token}${qs}`),
    enabled: !!token,
    staleTime: 60_000,
    retry: false,
  });
}

export function useReviewComments(token: string) {
  return useQuery<ReviewComment[]>({
    queryKey: ["review-comments", token],
    queryFn: () => apiFetch<ReviewComment[]>(`/api/v1/review/${token}/comments`),
    enabled: !!token,
    staleTime: 30_000,
  });
}

export function useAddReviewComment(token: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: { authorName: string; text: string; timestampSeconds: number }) =>
      apiFetch<ReviewComment>(`/api/v1/review/${token}/comments`, {
        method: "POST",
        body: JSON.stringify(body),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["review-comments", token] });
    },
  });
}

export function useResolveComment(token: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (commentId: string) =>
      apiFetch<void>(`/api/v1/review/${token}/comments/${commentId}/resolve`, {
        method: "PATCH",
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["review-comments", token] });
    },
  });
}
