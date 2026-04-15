"use client";

import { useEffect, useState } from "react";
import { useSignalR } from "./use-signalr";
import { useSagaState } from "./use-saga-state";
import { SagaStateDto } from "@/types";

const SIGNALR_HUB_URL =
  (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5001") + "/hubs/progress";

export function useEpisodeProgress(episodeId: string) {
  const { data: sagaState, refetch } = useSagaState(episodeId);
  const { connection } = useSignalR(SIGNALR_HUB_URL);
  const [realtimeState, setRealtimeState] = useState<SagaStateDto | null>(null);

  useEffect(() => {
    if (!connection || !episodeId) return;

    const handler = (state: SagaStateDto) => {
      if (state.episodeId === episodeId) {
        setRealtimeState(state);
        refetch();
      }
    };

    connection.on("EpisodeProgressUpdated", handler);
    return () => {
      connection.off("EpisodeProgressUpdated", handler);
    };
  }, [connection, episodeId, refetch]);

  return realtimeState ?? sagaState ?? null;
}
