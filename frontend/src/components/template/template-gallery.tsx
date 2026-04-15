"use client";

import { Genre, TemplateDto } from "@/types";
import { useTemplates } from "@/hooks/use-templates";
import { TemplateCard } from "./template-card";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { useState } from "react";

const GENRES: Array<{ label: string; value: Genre | "All" }> = [
  { label: "All", value: "All" },
  { label: "Kids", value: "Kids" },
  { label: "Comedy", value: "Comedy" },
  { label: "Drama", value: "Drama" },
  { label: "Horror", value: "Horror" },
  { label: "Romance", value: "Romance" },
  { label: "Sci-Fi", value: "SciFi" },
  { label: "Marketing", value: "Marketing" },
  { label: "Fantasy", value: "Fantasy" },
];

interface TemplateGalleryProps {
  selectedTemplateId: string | undefined;
  onSelect: (template: TemplateDto) => void;
}

export function TemplateGallery({ selectedTemplateId, onSelect }: TemplateGalleryProps) {
  const [activeGenre, setActiveGenre] = useState<Genre | "All">("All");
  const { data: templates, isLoading } = useTemplates(
    activeGenre === "All" ? undefined : activeGenre
  );

  return (
    <div className="flex flex-col gap-4">
      {/* Genre filter tabs */}
      <div
        role="tablist"
        aria-label="Filter by genre"
        className="flex flex-wrap gap-1.5"
      >
        {GENRES.map(({ label, value }) => (
          <button
            key={value}
            type="button"
            role="tab"
            aria-selected={activeGenre === value}
            onClick={() => setActiveGenre(value)}
            className={cn(
              "rounded-full px-3 py-1 text-xs font-medium transition-colors duration-150",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500",
              activeGenre === value
                ? "bg-blue-500 text-white"
                : "bg-muted text-muted-foreground hover:bg-muted/80"
            )}
          >
            {label}
          </button>
        ))}
      </div>

      {/* Template grid */}
      {isLoading ? (
        <div className="grid grid-cols-2 gap-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-48 w-full rounded-lg" />
          ))}
        </div>
      ) : !templates?.length ? (
        <p className="text-sm text-muted-foreground py-4 text-center">
          No templates found for this genre.
        </p>
      ) : (
        <div className="grid grid-cols-2 gap-3">
          {templates.map((t) => (
            <TemplateCard
              key={t.id}
              template={t}
              selected={selectedTemplateId === t.id}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </div>
  );
}
