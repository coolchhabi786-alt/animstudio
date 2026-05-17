"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { useProject } from "@/hooks/use-projects";
import { useEpisodes } from "@/hooks/use-episodes";
import Link from "next/link";
import { PlusCircle } from "lucide-react";

interface Props {
  params: { id: string };
}

export default function ProjectDetailPage({ params }: Props) {
  const { id } = params;
  const { data: project, isLoading: projectLoading } = useProject(id);
  const { data: episodes, isLoading: episodesLoading } = useEpisodes(id);

  if (projectLoading || episodesLoading) {
    return (
      <main className="p-6 space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-24 w-full" />
      </main>
    );
  }

  return (
    <main className="p-6">
      <div className="mb-4">
        <Link href="/projects" className="text-sm text-blue-500 hover:underline">
          ← Projects
        </Link>
      </div>

      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">{project?.name ?? "Project"}</h1>
          {project?.description && (
            <p className="text-gray-500 text-sm mt-1">{project.description}</p>
          )}
        </div>
        <Button asChild className="gap-2">
          <Link href={`/episodes/new?projectId=${id}`}>
            <PlusCircle className="h-4 w-4" />
            New Episode
          </Link>
        </Button>
      </div>

      {!episodes?.length ? (
        <div className="rounded-xl border border-dashed border-gray-300 py-16 flex flex-col items-center gap-3 text-center">
          <p className="font-medium text-gray-700">No episodes yet</p>
          <p className="text-sm text-gray-500">
            Create your first episode to start writing scripts and producing animations.
          </p>
          <Button asChild className="mt-2 gap-2">
            <Link href={`/episodes/new?projectId=${id}`}>
              <PlusCircle className="h-4 w-4" />
              New Episode
            </Link>
          </Button>
        </div>
      ) : (
        <div className="divide-y border rounded-lg">
          {episodes.map((ep) => (
            <div key={ep.id} className="flex items-center justify-between p-4 hover:bg-gray-50">
              <div>
                <Link
                  href={`/projects/${id}/episodes/${ep.id}`}
                  className="font-medium hover:underline"
                >
                  {ep.name}
                </Link>
                <p className="text-xs text-gray-400 mt-0.5">
                  Status: <span className="font-semibold">{ep.status}</span>
                  {ep.idea && (
                    <span className="ml-2 text-gray-400">· {ep.idea.slice(0, 60)}{ep.idea.length > 60 ? "…" : ""}</span>
                  )}
                </p>
              </div>
              <span className="text-xs text-gray-400">
                {new Date(ep.createdAt).toLocaleDateString()}
              </span>
            </div>
          ))}
        </div>
      )}
    </main>
  );
}
