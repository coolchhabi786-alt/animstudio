"use client";

import { usePathname } from "next/navigation";
import Link from "next/link";
import { cn } from "@/lib/utils";
import { useEpisode } from "@/hooks/use-episodes";
import { useEpisodeProgress } from "@/hooks/use-episode-progress";
import { ProgressStepper } from "@/components/episode/progress-stepper";
import { JobProgressToast } from "@/components/episode/job-progress-toast";
import { ArrowLeft, FileText, Layers, Mic, Film } from "lucide-react";

interface Props {
  params: { id: string; episodeId: string };
  children: React.ReactNode;
}

const NAV_TABS = [
  { label: "Script", icon: FileText, segment: "script" },
  { label: "Storyboard", icon: Layers, segment: "storyboard" },
  { label: "Voice", icon: Mic, segment: "voice" },
  { label: "Animation", icon: Film, segment: "animation" },
] as const;

export default function EpisodeLayout({ params, children }: Props) {
  const { id: projectId, episodeId } = params;
  const pathname = usePathname();
  const { data: episode } = useEpisode(episodeId);
  const progress = useEpisodeProgress(episodeId);

  return (
    <div className="flex flex-col min-h-screen bg-gray-50/40">
      <JobProgressToast sagaState={progress} />

      {/* Episode header */}
      <div className="border-b bg-white px-6 py-3">
        <div className="max-w-5xl mx-auto">
          <Link
            href={`/projects/${projectId}`}
            className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-900 transition-colors mb-1"
          >
            <ArrowLeft className="h-3.5 w-3.5" />
            Back to project
          </Link>
          <h1 className="text-lg font-semibold text-gray-900">
            {episode?.name ?? "Episode"}
          </h1>
          {progress?.lastError && (
            <p className="text-xs text-red-500 mt-0.5">
              Last error: {progress.lastError}
            </p>
          )}
        </div>
      </div>

      {/* Production pipeline stepper */}
      <div className="border-b bg-white px-6 py-3">
        <div className="max-w-5xl mx-auto">
          <p className="text-xs font-medium uppercase tracking-wide text-gray-400 mb-2">
            Production Pipeline
          </p>
          <ProgressStepper
            currentStage={progress?.currentStage}
            isCompensating={progress?.isCompensating}
          />
        </div>
      </div>

      {/* Tab navigation */}
      <div className="border-b bg-white sticky top-0 z-10">
        <div className="max-w-5xl mx-auto px-6">
          <nav className="flex gap-0 -mb-px">
            {NAV_TABS.map((tab) => {
              const href = `/projects/${projectId}/episodes/${episodeId}/${tab.segment}`;
              const isActive = pathname.startsWith(href);
              return (
                <Link
                  key={tab.segment}
                  href={href}
                  className={cn(
                    "inline-flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors whitespace-nowrap",
                    isActive
                      ? "border-indigo-600 text-indigo-600"
                      : "border-transparent text-gray-500 hover:text-gray-900 hover:border-gray-300"
                  )}
                >
                  <tab.icon className="h-4 w-4" />
                  {tab.label}
                </Link>
              );
            })}
          </nav>
        </div>
      </div>

      {/* Page content */}
      <div className="flex-1">{children}</div>
    </div>
  );
}
