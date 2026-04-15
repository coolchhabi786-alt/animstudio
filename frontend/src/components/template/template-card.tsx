"use client";

import { TemplateDto } from "@/types";
import { Badge } from "@/components/ui/badge";
import Image from "next/image";
import { cn } from "@/lib/utils";

interface TemplateCardProps {
  template: TemplateDto;
  selected: boolean;
  onSelect: (template: TemplateDto) => void;
}

export function TemplateCard({ template, selected, onSelect }: TemplateCardProps) {
  return (
    <button
      type="button"
      aria-pressed={selected}
      aria-label={`Select template: ${template.title}`}
      onClick={() => onSelect(template)}
      className={cn(
        "group relative flex flex-col rounded-lg border-2 text-left transition-all duration-150 overflow-hidden",
        "hover:border-blue-400 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500",
        selected
          ? "border-blue-500 shadow-md ring-2 ring-blue-500/30"
          : "border-border bg-card"
      )}
    >
      {/* Thumbnail */}
      <div className="relative h-36 w-full bg-muted overflow-hidden">
        {template.thumbnailUrl ? (
          <Image
            src={template.thumbnailUrl}
            alt={template.title}
            fill
            className="object-cover transition-transform duration-200 group-hover:scale-105"
            sizes="(max-width: 768px) 50vw, 25vw"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-muted-foreground text-xs">
            No preview
          </div>
        )}
        {/* Preview video overlay on hover */}
        {template.previewVideoUrl && (
          <video
            src={template.previewVideoUrl}
            muted
            loop
            playsInline
            aria-hidden="true"
            className="absolute inset-0 h-full w-full object-cover opacity-0 group-hover:opacity-100 transition-opacity duration-200"
            onMouseEnter={(e) => (e.currentTarget as HTMLVideoElement).play()}
            onMouseLeave={(e) => {
              const v = e.currentTarget as HTMLVideoElement;
              v.pause();
              v.currentTime = 0;
            }}
          />
        )}
      </div>

      {/* Content */}
      <div className="p-3 flex flex-col gap-1.5">
        <div className="flex items-start justify-between gap-2">
          <p className="text-sm font-semibold leading-tight line-clamp-2">{template.title}</p>
          <Badge variant="secondary" className="shrink-0 text-xs capitalize">
            {template.genre}
          </Badge>
        </div>
        <p className="text-xs text-muted-foreground line-clamp-2">{template.description}</p>
      </div>

      {/* Selected indicator */}
      {selected && (
        <div className="absolute top-2 right-2 flex h-5 w-5 items-center justify-center rounded-full bg-blue-500 text-white text-xs font-bold shadow">
          ✓
        </div>
      )}
    </button>
  );
}
