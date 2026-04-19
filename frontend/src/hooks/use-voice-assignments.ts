"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api-client";
import type {
  VoiceAssignmentDto,
  VoiceAssignmentRequest,
  VoicePreviewResponse,
} from "@/types";

// ── Query keys ────────────────────────────────────────────────────────────────
const KEYS = {
  list: (episodeId: string) => ["voiceAssignments", episodeId] as const,
};

// ── Queries ───────────────────────────────────────────────────────────────────

/** Fetches all voice assignments for an episode. */
export function useVoiceAssignments(episodeId: string | undefined) {
  return useQuery<VoiceAssignmentDto[]>({
    queryKey: KEYS.list(episodeId ?? ""),
    queryFn: () =>
      apiFetch<VoiceAssignmentDto[]>(
        `/api/v1/episodes/${episodeId}/voices`,
      ),
    enabled: !!episodeId,
    staleTime: 30_000,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

/** Batch update voice assignments for an episode. */
export function useUpdateVoiceAssignments(episodeId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (assignments: VoiceAssignmentRequest[]) =>
      apiFetch<VoiceAssignmentDto[]>(
        `/api/v1/episodes/${episodeId}/voices`,
        {
          method: "PUT",
          body: JSON.stringify({ assignments }),
        },
      ),
    onSuccess: (data) => {
      qc.setQueryData(KEYS.list(episodeId), data);
    },
  });
}

/** Generate a TTS audio preview for a voice + text. */
export function useVoicePreview() {
  return useMutation({
    mutationFn: (req: { text: string; voiceName: string; language?: string }) =>
      apiFetch<VoicePreviewResponse>("/api/v1/voices/preview", {
        method: "POST",
        body: JSON.stringify(req),
      }),
  });
}
