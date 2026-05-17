"use client";

import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { PagedResult } from "@/types";

export interface AdminStats {
  dau: number;
  mau: number;
  activeJobs: number;
  errorRatePct: number;
}

export interface AdminUserRow {
  id: string;
  email: string;
  displayName: string;
  role: string;
  createdAt: string;
  lastActiveAt: string | null;
}

export interface HangfireJobRow {
  id: string;
  jobType: string;
  state: string;
  createdAt: string;
}

function useIsAdmin(): boolean {
  const { data: session } = useSession();
  return (session as any)?.user?.role === "Admin";
}

export function useAdminStats() {
  const isAdmin = useIsAdmin();
  return useQuery<AdminStats>({
    queryKey: ["admin", "stats"],
    queryFn: () => apiFetch<AdminStats>("/api/v1/admin/stats"),
    enabled: isAdmin,
    staleTime: 30_000,
  });
}

export function useAdminUsers(page = 1, pageSize = 20) {
  const isAdmin = useIsAdmin();
  return useQuery<PagedResult<AdminUserRow>>({
    queryKey: ["admin", "users", page, pageSize],
    queryFn: () =>
      apiFetch<PagedResult<AdminUserRow>>(
        `/api/v1/admin/users?page=${page}&pageSize=${pageSize}`,
      ),
    enabled: isAdmin,
    staleTime: 30_000,
  });
}

export function useAdminJobs() {
  const isAdmin = useIsAdmin();
  return useQuery<HangfireJobRow[]>({
    queryKey: ["admin", "jobs"],
    queryFn: () => apiFetch<HangfireJobRow[]>("/api/v1/admin/jobs"),
    enabled: isAdmin,
    staleTime: 15_000,
  });
}
