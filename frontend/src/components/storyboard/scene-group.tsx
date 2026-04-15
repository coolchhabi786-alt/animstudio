"use client";

import { ShotCard } from "@/components/storyboard/shot-card";
import type { StoryboardShotDto } from "@/types";

interface Props {
  sceneNumber: number;
  shots: StoryboardShotDto[];
  onRegenerate: (shot: StoryboardShotDto) => void;
  onChangeStyle: (shot: StoryboardShotDto) => void;
  pendingShotId?: string | null;
}

export function SceneGroup({
  sceneNumber,
  shots,
  onRegenerate,
  onChangeStyle,
  pendingShotId,
}: Props) {
  return (
    <section>
      <h2 className="text-sm font-semibold text-gray-800 mb-3">
        Scene {sceneNumber}
        <span className="ml-2 text-xs font-normal text-gray-500">
          {shots.length} shot{shots.length === 1 ? "" : "s"}
        </span>
      </h2>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {shots.map((shot) => (
          <ShotCard
            key={shot.id}
            shot={shot}
            onRegenerate={onRegenerate}
            onChangeStyle={onChangeStyle}
            isPending={pendingShotId === shot.id}
          />
        ))}
      </div>
    </section>
  );
}
