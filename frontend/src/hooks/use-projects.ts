"use client";

import { apiFetch } from "@/lib/api-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { ProjectDto, PagedResult } from "@/types";

export function useProjects() {
  return useQuery<ProjectDto[]>({
    queryKey: ["projects"],
    queryFn: async () => {
      const result = await apiFetch<PagedResult<ProjectDto>>("/api/v1/projects");
      return result.items;
    },
    staleTime: 30_000,
    enabled: true,
  });
}

export function useProject(id: string) {
  return useQuery<ProjectDto>({
    queryKey: ["projects", id],
    queryFn: () => apiFetch<ProjectDto>(`/api/v1/projects/${id}`),
    enabled: !!id,
    staleTime: 30_000,
  });
}

export function useCreateProject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: { name: string; description?: string }) =>
      apiFetch<ProjectDto>("/api/v1/projects", {
        method: "POST",
        body: JSON.stringify(body),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
}

export function useDeleteProject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiFetch<void>(`/api/v1/projects/${id}`, { method: "DELETE" }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      toast.success("Project deleted.");
    },
    onError: () => {
      toast.error("Failed to delete project. Please try again.");
    },
  });
}
