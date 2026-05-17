"use client";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useProjects, useCreateProject, useDeleteProject } from "@/hooks/use-projects";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { useState } from "react";
import Link from "next/link";

export default function ProjectsPage() {
  const { data: projects, isLoading, error } = useProjects();
  const createProject = useCreateProject();
  const deleteProject = useDeleteProject();
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState("");
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null);
  const pendingDeleteProject = projects?.find((p) => p.id === pendingDeleteId);

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    await createProject.mutateAsync({ name: name.trim() });
    setName("");
    setShowForm(false);
  }

  if (isLoading) {
    return (
      <main className="p-6 space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
      </main>
    );
  }

  if (error) {
    return (
      <main className="p-6">
        <p className="text-red-500">Failed to load projects.</p>
      </main>
    );
  }

  return (
    <main className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Projects</h1>
        <Button onClick={() => setShowForm((v) => !v)}>New Project</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 flex gap-2">
          <Input
            placeholder="Project name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoFocus
          />
          <Button type="submit" disabled={createProject.isPending}>
            {createProject.isPending ? "Creating…" : "Create"}
          </Button>
          <Button variant="ghost" type="button" onClick={() => setShowForm(false)}>
            Cancel
          </Button>
        </form>
      )}

      {!projects?.length ? (
        <p className="text-gray-500">No projects yet. Create your first one!</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {projects.map((project) => (
            <Card
              key={project.id}
              className="flex flex-col hover:shadow-md transition-shadow"
            >
              <CardContent className="flex flex-col gap-2 p-4 flex-1">
                <Link href={`/projects/${project.id}`} className="font-semibold hover:underline">
                  {project.name}
                </Link>
                {project.description && (
                  <p className="text-sm text-muted-foreground line-clamp-2">{project.description}</p>
                )}
                <div className="flex items-center justify-between mt-auto pt-2">
                  <span className="text-xs text-muted-foreground">
                    {new Date(project.createdAt).toLocaleDateString()}
                  </span>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:text-destructive"
                    onClick={() => setPendingDeleteId(project.id)}
                  >
                    Delete
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Dialog
        open={pendingDeleteId !== null}
        onOpenChange={(open) => { if (!open) setPendingDeleteId(null); }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete project?</DialogTitle>
            <DialogDescription>
              This will permanently delete &ldquo;{pendingDeleteProject?.name}&rdquo; and all its
              episodes. This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setPendingDeleteId(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteProject.isPending}
              onClick={async () => {
                if (!pendingDeleteId) return;
                await deleteProject.mutateAsync(pendingDeleteId);
                setPendingDeleteId(null);
              }}
            >
              {deleteProject.isPending ? "Deleting…" : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </main>
  );
}
