"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api-client";
import type { Timeline, TimelineClip, TimelineTrack, TextOverlay } from "@/types/timeline";
import { TrackType, TransitionType, TextAnimation } from "@/types/timeline";
import type { TextPosition } from "@/types/timeline";

// ── Raw API DTO types (flat, matches backend JSON exactly) ─────────────────────

interface ApiClipDto {
  id: string;
  trackId: string;
  type: string; // "video" | "audio" | "music" | "text"
  startMs: number;
  durationMs: number;
  sceneNumber?: number;
  shotIndex?: number;
  clipUrl?: string;
  thumbnailUrl?: string;
  transitionIn?: string;
  label?: string;
  audioUrl?: string;
  volumePercent?: number;
  fadeInMs?: number;
  fadeOutMs?: number;
  text?: string;
  fontSize?: number;
  color?: string;
  position?: string;
  animation?: string;
}

interface ApiTrackDto {
  id: string;
  trackType: string;
  label: string;
  isMuted: boolean;
  isLocked: boolean;
  volumePercent?: number;
  autoDuck?: boolean;
  clips: ApiClipDto[];
}

interface ApiOverlayDto {
  id: string;
  episodeId: string;
  text: string;
  fontSizePixels: number;
  color: string;
  positionX: number;
  positionY: number;
  startMs: number;
  durationMs: number;
  animation: string;
  zIndex: number;
}

interface ApiTimelineDto {
  id: string;
  episodeId: string;
  durationMs: number;
  fps: number;
  tracks: ApiTrackDto[];
  textOverlays: ApiOverlayDto[];
  updatedAt: string;
}

// ── Adapters: API DTO → canonical frontend types ───────────────────────────────

function adaptClip(dto: ApiClipDto): TimelineClip {
  // Backend stores music-track clips as type "music"; frontend uses "audio" for all audio.
  const type = dto.type === "music" ? "audio" : dto.type;

  if (type === "video") {
    return {
      type: "video",
      id: dto.id,
      trackId: dto.trackId,
      sceneNumber: dto.sceneNumber ?? 0,
      shotIndex: dto.shotIndex ?? 0,
      clipUrl: dto.clipUrl ?? "",
      thumbnailUrl: dto.thumbnailUrl,
      startMs: dto.startMs,
      durationMs: dto.durationMs,
      transitionIn: (dto.transitionIn as TransitionType) ?? TransitionType.Cut,
    };
  }

  if (type === "audio") {
    return {
      type: "audio",
      id: dto.id,
      trackId: dto.trackId,
      label: dto.label ?? "",
      audioUrl: dto.audioUrl ?? "",
      startMs: dto.startMs,
      durationMs: dto.durationMs,
      volumePercent: dto.volumePercent ?? 80,
      fadeInMs: dto.fadeInMs ?? 0,
      fadeOutMs: dto.fadeOutMs ?? 0,
    };
  }

  return {
    type: "text",
    id: dto.id,
    trackId: dto.trackId,
    text: dto.text ?? "",
    startMs: dto.startMs,
    durationMs: dto.durationMs,
    fontSize: dto.fontSize ?? 24,
    color: dto.color ?? "#ffffff",
    position: (dto.position as TextPosition) ?? "center",
    animation: (dto.animation as TextAnimation) ?? TextAnimation.None,
  };
}

function adaptTrack(dto: ApiTrackDto): TimelineTrack {
  return {
    id: dto.id,
    trackType: dto.trackType as TrackType,
    label: dto.label,
    isMuted: dto.isMuted,
    isSolo: false,
    isLocked: dto.isLocked,
    volumePercent: dto.volumePercent ?? undefined,
    autoDuck: dto.autoDuck ?? undefined,
    clips: dto.clips.map(adaptClip),
  };
}

function adaptTimeline(dto: ApiTimelineDto): Timeline {
  return {
    id: dto.id,
    episodeId: dto.episodeId,
    durationMs: dto.durationMs,
    fps: dto.fps,
    tracks: dto.tracks.map(adaptTrack),
    textOverlays: dto.textOverlays.map((o): TextOverlay => ({
      id: o.id,
      episodeId: o.episodeId,
      text: o.text,
      fontSizePixels: o.fontSizePixels,
      color: o.color,
      positionX: o.positionX,
      positionY: o.positionY,
      startMs: o.startMs,
      durationMs: o.durationMs,
      animation: o.animation as TextAnimation,
      zIndex: o.zIndex,
    })),
    updatedAt: dto.updatedAt,
  };
}

// ── Adapters: canonical frontend types → API DTO ───────────────────────────────

function clipToDto(clip: TimelineClip): ApiClipDto {
  const base = {
    id: clip.id,
    trackId: clip.trackId,
    startMs: clip.startMs,
    durationMs: clip.durationMs,
  };

  if (clip.type === "video") {
    return {
      ...base,
      type: "video",
      sceneNumber: clip.sceneNumber,
      shotIndex: clip.shotIndex,
      clipUrl: clip.clipUrl,
      thumbnailUrl: clip.thumbnailUrl,
      transitionIn: clip.transitionIn,
    };
  }

  if (clip.type === "audio") {
    return {
      ...base,
      type: "audio",
      label: clip.label,
      audioUrl: clip.audioUrl,
      volumePercent: clip.volumePercent,
      fadeInMs: clip.fadeInMs,
      fadeOutMs: clip.fadeOutMs,
    };
  }

  return {
    ...base,
    type: "text",
    text: clip.text,
    fontSize: clip.fontSize,
    color: clip.color,
    position: clip.position,
    animation: clip.animation,
  };
}

function timelineToSaveRequest(t: Timeline) {
  return {
    durationMs: t.durationMs,
    fps: t.fps,
    tracks: t.tracks.map((tr) => ({
      id: tr.id,
      trackType: tr.trackType,
      label: tr.label,
      isMuted: tr.isMuted,
      isLocked: tr.isLocked,
      volumePercent: tr.volumePercent ?? null,
      autoDuck: tr.autoDuck ?? null,
      clips: tr.clips.map(clipToDto),
    })),
    textOverlays: t.textOverlays.map((o) => ({
      id: o.id,
      episodeId: o.episodeId,
      text: o.text,
      fontSizePixels: o.fontSizePixels,
      color: o.color,
      positionX: o.positionX,
      positionY: o.positionY,
      startMs: o.startMs,
      durationMs: o.durationMs,
      animation: o.animation,
      zIndex: o.zIndex,
    })),
  };
}

// ── Query key ──────────────────────────────────────────────────────────────────

const KEY = (id: string) => ["timeline", id] as const;

// ── Hooks ──────────────────────────────────────────────────────────────────────

/** Fetches the timeline for an episode and adapts it to the canonical Timeline type. */
export function useTimeline(episodeId: string | undefined) {
  return useQuery<Timeline>({
    queryKey: KEY(episodeId ?? ""),
    queryFn: async () => {
      const dto = await apiFetch<ApiTimelineDto>(`/api/v1/episodes/${episodeId}/timeline`);
      return adaptTimeline(dto);
    },
    enabled: !!episodeId,
    staleTime: 30_000,
    retry: (failureCount, error) => {
      // Don't retry on 404 — timeline just doesn't exist yet.
      if (error instanceof Error && error.message.includes("NOT_FOUND")) return false;
      return failureCount < 2;
    },
  });
}

/** Saves (PUT) the timeline to the backend and updates the query cache. */
export function useSaveTimeline(episodeId: string) {
  const qc = useQueryClient();
  return useMutation<Timeline, Error, Timeline>({
    mutationFn: async (timeline) => {
      const dto = await apiFetch<ApiTimelineDto>(`/api/v1/episodes/${episodeId}/timeline`, {
        method: "PUT",
        body: JSON.stringify(timelineToSaveRequest(timeline)),
      });
      return adaptTimeline(dto);
    },
    onSuccess: (fresh) => {
      qc.setQueryData(KEY(episodeId), fresh);
    },
  });
}
