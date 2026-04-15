"use client";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useEpisode, useDispatchEpisodeJob } from "@/hooks/use-episodes";
import { useEpisodeProgress } from "@/hooks/use-episode-progress";
import { ProgressStepper } from "@/components/episode/progress-stepper";
import { JobProgressToast } from "@/components/episode/job-progress-toast";
import Link from "next/link";

const JOB_TYPES = [
  { key: "CharacterDesign", label: "Character Design" },
  { key: "LoraTraining", label: "LoRA Training" },
  { key: "Script", label: "Script" },
  { key: "StoryboardPlan", label: "Storyboard Plan" },
  { key: "StoryboardGen", label: "Storyboard Gen" },
  { key: "Voice", label: "Voice" },
  { key: "Animation", label: "Animation" },
  { key: "PostProd", label: "Post Production" },
];

interface Props {
  params: { id: string; episodeId: string };
}

export default function EpisodeDetailPage({ params }: Props) {
  const { id: projectId, episodeId } = params;
  const { data: episode, isLoading } = useEpisode(episodeId);
  const progress = useEpisodeProgress(episodeId);
  const dispatch = useDispatchEpisodeJob(episodeId);

  if (isLoading) {
    return (
      <main className="p-6 space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-32 w-full" />
      </main>
    );
  }

  const isProcessing =
    progress?.currentStage &&
    progress.currentStage !== "Done" &&
    progress.currentStage !== "Idle" &&
    !progress.isCompensating;

  return (
    <main className="p-6 max-w-3xl mx-auto">
      <JobProgressToast sagaState={progress} />

      <div className="mb-4">
        <Link href={`/projects/${projectId}`} className="text-sm text-blue-500 hover:underline">
          ← Back to project
        </Link>
      </div>

      <h1 className="text-2xl font-bold mb-1">{episode?.name ?? "Episode"}</h1>
      <p className="text-sm text-gray-500 mb-6">Status: {episode?.status ?? "—"}</p>

      {/* Progress stepper */}
      <section className="mb-8">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500 mb-2">
          Production Pipeline
        </h2>
        <ProgressStepper
          currentStage={progress?.currentStage}
          isCompensating={progress?.isCompensating}
        />
        {progress?.lastError && (
          <p className="text-xs text-red-500 mt-2">Last error: {progress.lastError}</p>
        )}
      </section>

      {/* Episode details */}
      {(episode?.idea || episode?.style) && (
        <section className="mb-8 grid gap-3 sm:grid-cols-2">
          {episode.idea && (
            <div className="border rounded p-3">
              <p className="text-xs font-semibold uppercase text-gray-400 mb-1">Idea</p>
              <p className="text-sm">{episode.idea}</p>
            </div>
          )}
          {episode.style && (
            <div className="border rounded p-3">
              <p className="text-xs font-semibold uppercase text-gray-400 mb-1">Style</p>
              <p className="text-sm">{episode.style}</p>
            </div>
          )}
        </section>
      )}

      {/* Dispatch controls */}
      <section>
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500 mb-3">
          Dispatch Job
        </h2>
        <div className="flex flex-wrap gap-2">
          {JOB_TYPES.map(({ key, label }) => (
            <Button
              key={key}
              variant="outline"
              size="sm"
              disabled={!!isProcessing || dispatch.isPending}
              onClick={() => dispatch.mutate(key)}
            >
              {label}
            </Button>
          ))}
        </div>
        {isProcessing && (
          <p className="text-xs text-blue-500 mt-2">Processing: {progress?.currentStage}…</p>
        )}
      </section>
    </main>
  );
}
