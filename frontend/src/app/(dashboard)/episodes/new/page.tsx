"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { TemplateGallery } from "@/components/template/template-gallery";
import { StylePicker } from "@/components/template/style-picker";
import { useStylePresets } from "@/hooks/use-templates";
import { useCreateEpisode } from "@/hooks/use-episodes";
import { TemplateDto, Style } from "@/types";
import { toast } from "sonner";
import Link from "next/link";

export default function NewEpisodePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const projectId = searchParams.get("projectId") ?? "";

  // Form state
  const [name, setName] = useState("");
  const [idea, setIdea] = useState("");
  const [selectedTemplate, setSelectedTemplate] = useState<TemplateDto | undefined>(undefined);
  const [selectedStyle, setSelectedStyle] = useState<Style | undefined>(undefined);

  const { data: stylePresets, isLoading: stylesLoading } = useStylePresets();
  const createEpisode = useCreateEpisode(projectId);

  // When a template is selected, auto-populate the style picker with its default
  function handleTemplateSelect(template: TemplateDto) {
    if (selectedTemplate?.id === template.id) {
      // Deselect
      setSelectedTemplate(undefined);
      return;
    }
    setSelectedTemplate(template);
    setSelectedStyle(template.defaultStyle);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!name.trim()) {
      toast.error("Episode name is required.");
      return;
    }
    if (!projectId) {
      toast.error("No project selected. Please navigate from a project page.");
      return;
    }

    try {
      await createEpisode.mutateAsync({
        name: name.trim(),
        idea: idea.trim(),
        style: selectedStyle ?? "",
        templateId: selectedTemplate?.id,
      });
      toast.success("Episode created!");
      router.push(`/projects/${projectId}`);
    } catch {
      // apiFetch already toasts the error
    }
  }

  const isSubmitting = createEpisode.isPending;

  return (
    <main className="p-4 md:p-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        {projectId && (
          <Link
            href={`/projects/${projectId}`}
            className="text-sm text-blue-500 hover:underline mb-2 inline-block"
          >
            ← Back to project
          </Link>
        )}
        <h1 className="text-2xl font-bold">New Episode</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Describe your idea, pick a template, and choose a visual style.
        </p>
      </div>

      <form onSubmit={handleSubmit} noValidate>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* ── LEFT: Idea input + Template picker ──────────────────────────── */}
          <div className="flex flex-col gap-6">
            {/* Episode name */}
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="episode-name">
                Episode Name <span className="text-destructive">*</span>
              </Label>
              <Input
                id="episode-name"
                type="text"
                required
                maxLength={200}
                placeholder="e.g. The Midnight Museum Heist"
                value={name}
                onChange={(e) => setName(e.target.value)}
                aria-required="true"
              />
            </div>

            {/* Episode idea */}
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="episode-idea">Idea / Brief</Label>
              <Textarea
                id="episode-idea"
                rows={4}
                maxLength={5000}
                placeholder="Describe the story in a few sentences. The AI will use this when writing the script."
                value={idea}
                onChange={(e) => setIdea(e.target.value)}
              />
              <p className="text-xs text-muted-foreground text-right">{idea.length}/5000</p>
            </div>

            {/* Template picker */}
            <div className="flex flex-col gap-2">
              <div className="flex items-center justify-between">
                <h2 className="text-sm font-semibold">
                  Template{" "}
                  <span className="text-muted-foreground font-normal">(optional)</span>
                </h2>
                {selectedTemplate && (
                  <button
                    type="button"
                    onClick={() => setSelectedTemplate(undefined)}
                    className="text-xs text-muted-foreground hover:text-foreground underline"
                  >
                    Clear
                  </button>
                )}
              </div>
              {selectedTemplate && (
                <div className="rounded-md bg-blue-50 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 px-3 py-2 text-xs text-blue-700 dark:text-blue-300">
                  Selected: <strong>{selectedTemplate.title}</strong>
                </div>
              )}
              <TemplateGallery
                selectedTemplateId={selectedTemplate?.id}
                onSelect={handleTemplateSelect}
              />
            </div>
          </div>

          {/* ── RIGHT: Style swatches ────────────────────────────────────────── */}
          <div className="flex flex-col gap-4">
            <div className="lg:sticky lg:top-6">
              <h2 className="text-sm font-semibold mb-3">
                Visual Style{" "}
                <span className="text-muted-foreground font-normal">(optional)</span>
              </h2>

              <StylePicker
                presets={stylePresets ?? []}
                isLoading={stylesLoading}
                selectedStyle={selectedStyle}
                onSelect={setSelectedStyle}
              />

              {/* Selected style detail card */}
              {selectedStyle && stylePresets && (
                <div className="mt-4 rounded-lg border bg-muted/50 p-4">
                  {(() => {
                    const preset = stylePresets.find((p) => p.style === selectedStyle);
                    if (!preset) return null;
                    return (
                      <>
                        <p className="text-sm font-semibold mb-1">{preset.displayName}</p>
                        <p className="text-xs text-muted-foreground">{preset.description}</p>
                      </>
                    );
                  })()}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* ── Submit ──────────────────────────────────────────────────────────── */}
        <div className="mt-8 flex items-center justify-end gap-3 border-t pt-6">
          {projectId && (
            <Button
              type="button"
              variant="ghost"
              asChild
            >
              <Link href={`/projects/${projectId}`}>Cancel</Link>
            </Button>
          )}
          <Button type="submit" disabled={isSubmitting || !name.trim()}>
            {isSubmitting ? "Creating…" : "Create Episode"}
          </Button>
        </div>
      </form>
    </main>
  );
}
