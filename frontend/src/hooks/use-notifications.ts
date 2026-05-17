"use client";

import { useEffect } from "react";
import { apiFetch } from "@/lib/api-client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSignalR } from "@/hooks/use-signalr";
import { Notification } from "@/types";

const NOTIFICATIONS_HUB_URL =
  (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5001") + "/hubs/notifications";

const QUERY_KEY = ["notifications"] as const;

export function useNotifications(unreadOnly?: boolean) {
  const queryClient = useQueryClient();
  const { connection } = useSignalR(NOTIFICATIONS_HUB_URL);

  // Invalidate the list whenever a new notification arrives via SignalR.
  useEffect(() => {
    if (!connection) return;

    const handler = () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    };

    connection.on("NewNotification", handler);
    return () => {
      connection.off("NewNotification", handler);
    };
  }, [connection, queryClient]);

  const qs = unreadOnly ? "?unreadOnly=true" : "";
  return useQuery<Notification[]>({
    queryKey: [...QUERY_KEY, { unreadOnly }],
    queryFn: () => apiFetch<Notification[]>(`/api/v1/notifications${qs}`),
    staleTime: 30_000,
  });
}

export function useMarkNotificationRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiFetch<void>(`/api/v1/notifications/${id}/read`, { method: "PATCH" }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
}

export function useMarkAllNotificationsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () =>
      apiFetch<void>("/api/v1/notifications/read-all", { method: "PATCH" }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
}
