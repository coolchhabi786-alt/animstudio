"use client";

import { useState } from "react";
import { Share2, Palette, Link2 } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { ReviewLinkGenerator } from "@/components/review/review-link-generator";
import { ReviewLinkCard } from "@/components/review/review-link-card";
import { useRenderHistory } from "@/hooks/use-render";
import { useReviewLinks } from "@/hooks/use-review-links";
import { useBrandKit, useUpsertBrandKit } from "@/hooks/use-brand-kit";
import { useTeam } from "@/hooks/useTeam";
import type { BrandKit } from "@/types";

const WATERMARK_POSITIONS = [
  { label: "Top Left", value: "TopLeft" },
  { label: "Top Right", value: "TopRight" },
  { label: "Bottom Left", value: "BottomLeft" },
  { label: "Bottom Right", value: "BottomRight" },
  { label: "Center", value: "Center" },
] as const;

export default function SharePage({ params }: { params: { id: string } }) {
  const episodeId = params.id;
  const { team } = useTeam();

  const { data: renders = [], isLoading: rendersLoading } = useRenderHistory(episodeId);
  const { data: reviewLinks = [], isLoading: linksLoading } = useReviewLinks(episodeId);
  const { data: brandKit, isLoading: kitLoading } = useBrandKit(team?.id ?? "");
  const upsertBrandKit = useUpsertBrandKit(team?.id ?? "");

  const completedRender = renders.find((r) => r.status === "Complete");

  // Brand kit local form state
  const [primaryColor, setPrimaryColor] = useState<string>("");
  const [secondaryColor, setSecondaryColor] = useState<string>("");
  const [watermarkPosition, setWatermarkPosition] = useState<BrandKit["watermarkPosition"]>("BottomRight");
  const [watermarkOpacity, setWatermarkOpacity] = useState<string>("0.5");

  // Populate form when brand kit loads (only once)
  const [kitLoaded, setKitLoaded] = useState(false);
  if (brandKit && !kitLoaded) {
    setPrimaryColor(brandKit.primaryColor);
    setSecondaryColor(brandKit.secondaryColor);
    setWatermarkPosition(brandKit.watermarkPosition);
    setWatermarkOpacity(String(brandKit.watermarkOpacity));
    setKitLoaded(true);
  }

  function handleSaveBrandKit(e: React.FormEvent) {
    e.preventDefault();
    if (!team) return;
    upsertBrandKit.mutate(
      {
        primaryColor,
        secondaryColor,
        watermarkPosition,
        watermarkOpacity: parseFloat(watermarkOpacity),
      },
      {
        onSuccess: () => toast.success("Brand kit saved."),
        onError: (err) => toast.error(err.message ?? "Failed to save brand kit."),
      },
    );
  }

  return (
    <main className="p-6 max-w-4xl mx-auto space-y-10">
      <div>
        <div className="flex items-center gap-2 mb-1">
          <Share2 className="h-5 w-5 text-violet-500" />
          <h1 className="text-2xl font-bold">Share & Review</h1>
        </div>
        <p className="text-sm text-muted-foreground">
          Episode ID: <span className="font-mono">{episodeId}</span>
        </p>
      </div>

      {/* Section 1 — Review Link Generator */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <Link2 className="h-4 w-4 text-muted-foreground" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Create Review Link
          </h2>
        </div>

        {rendersLoading ? (
          <Skeleton className="h-24 w-full rounded-lg" />
        ) : !completedRender ? (
          <div className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">
            A completed render is required to generate a review link.
            <br />
            Go to the Render page to create one.
          </div>
        ) : (
          <ReviewLinkGenerator renderId={completedRender.id} />
        )}
      </section>

      {/* Section 2 — Active Links */}
      <section className="space-y-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Active Links
        </h2>
        {linksLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 2 }).map((_, i) => (
              <Skeleton key={i} className="h-20 w-full rounded-lg" />
            ))}
          </div>
        ) : reviewLinks.length === 0 ? (
          <p className="text-sm text-muted-foreground">No review links created yet.</p>
        ) : (
          <div className="space-y-3">
            {reviewLinks.map((link) => (
              <ReviewLinkCard key={link.id} link={link} />
            ))}
          </div>
        )}
      </section>

      {/* Section 3 — Brand Kit */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <Palette className="h-4 w-4 text-muted-foreground" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Brand Kit
          </h2>
        </div>

        {kitLoading ? (
          <Skeleton className="h-48 w-full rounded-lg" />
        ) : (
          <form onSubmit={handleSaveBrandKit} className="space-y-4 rounded-lg border p-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="primary-color">Primary Color</Label>
                <div className="flex gap-2 items-center">
                  <input
                    id="primary-color"
                    type="color"
                    value={primaryColor || "#7c3aed"}
                    onChange={(e) => setPrimaryColor(e.target.value)}
                    className="h-9 w-12 cursor-pointer rounded border border-input"
                  />
                  <Input
                    value={primaryColor}
                    onChange={(e) => setPrimaryColor(e.target.value)}
                    placeholder="#7c3aed"
                    className="font-mono"
                  />
                </div>
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="secondary-color">Secondary Color</Label>
                <div className="flex gap-2 items-center">
                  <input
                    id="secondary-color"
                    type="color"
                    value={secondaryColor || "#a78bfa"}
                    onChange={(e) => setSecondaryColor(e.target.value)}
                    className="h-9 w-12 cursor-pointer rounded border border-input"
                  />
                  <Input
                    value={secondaryColor}
                    onChange={(e) => setSecondaryColor(e.target.value)}
                    placeholder="#a78bfa"
                    className="font-mono"
                  />
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="watermark-position">Watermark Position</Label>
                <Select
                  value={watermarkPosition}
                  onValueChange={(v) => setWatermarkPosition(v as BrandKit["watermarkPosition"])}
                >
                  <SelectTrigger id="watermark-position">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {WATERMARK_POSITIONS.map((pos) => (
                      <SelectItem key={pos.value} value={pos.value}>
                        {pos.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="watermark-opacity">
                  Watermark Opacity ({Math.round(parseFloat(watermarkOpacity || "0") * 100)}%)
                </Label>
                <input
                  id="watermark-opacity"
                  type="range"
                  min="0"
                  max="1"
                  step="0.05"
                  value={watermarkOpacity}
                  onChange={(e) => setWatermarkOpacity(e.target.value)}
                  className="w-full"
                />
              </div>
            </div>

            <Button type="submit" disabled={upsertBrandKit.isPending || !team}>
              {upsertBrandKit.isPending ? "Saving…" : "Save Brand Kit"}
            </Button>
          </form>
        )}
      </section>
    </main>
  );
}
