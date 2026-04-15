"use client";

import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { TemplateDto, StylePresetDto } from "@/types";

const STALE_TIME = 60 * 60 * 1000; // 1 hour — mirrors server-side TTL

/** Returns all active episode templates optionally filtered by genre. */
export function useTemplates(genre?: string) {
  const params = genre ? `?genre=${encodeURIComponent(genre)}` : "";
  return useQuery<TemplateDto[]>({
    queryKey: ["templates", genre ?? "all"],
    queryFn: () => apiFetch<TemplateDto[]>(`/api/v1/templates${params}`),
    staleTime: STALE_TIME,
  });
}

/** Returns a single episode template by ID. */
export function useTemplate(id: string | undefined) {
  return useQuery<TemplateDto>({
    queryKey: ["templates", id],
    queryFn: () => apiFetch<TemplateDto>(`/api/v1/templates/${id}`),
    enabled: !!id,
    staleTime: STALE_TIME,
  });
}

/** Returns all active visual style presets. */
export function useStylePresets() {
  return useQuery<StylePresetDto[]>({
    queryKey: ["styles"],
    queryFn: () => apiFetch<StylePresetDto[]>("/api/v1/styles"),
    staleTime: STALE_TIME,
  });
}
