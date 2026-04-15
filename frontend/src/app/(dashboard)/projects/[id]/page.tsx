"use client";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useProject } from "@/hooks/use-projects";
import { useEpisodes, useCreateEpisode } from "@/hooks/use-episodes";
import { Input } from "@/components/ui/input";
import Link from "next/link";
import { useState } from "react";

interface Props {
  params: { id: string };
}

export default function ProjectDetailPage({ params }: Props) {
  const { id } = params;
  const { data: project, isLoading: projectLoading } = useProject(id);
  const { data: episodes, isLoading: episodesLoading } = useEpisodes(id);
  const createEpisode = useCreateEpisode(id);
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState("");

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    await createEpisode.mutateAsync({ name: name.trim() });
    setName("");
    setShowForm(false);
  }

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
        <Button onClick={() => setShowForm((v) => !v)}>New Episode</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 flex gap-2">
          <Input
            placeholder="Episode name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoFocus
          />
          <Button type="submit" disabled={createEpisode.isPending}>
            {createEpisode.isPending ? "Creating…" : "Create"}
          </Button>
          <Button variant="ghost" type="button" onClick={() => setShowForm(false)}>
            Cancel
          </Button>
        </form>
      )}

      {!episodes?.length ? (
        <p className="text-gray-500">No episodes yet. Create your first one!</p>
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
