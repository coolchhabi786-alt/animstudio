import type { SceneDto } from "@/types";

interface ScriptStatsProps {
  scenes: SceneDto[];
}

function computeStats(scenes: SceneDto[]) {
  const totalLines = scenes.reduce((sum, s) => sum + s.dialogue.length, 0);

  // Estimated duration = sum of all end_times of last dialogue in each scene
  const estimatedSeconds = scenes.reduce((sum, s) => {
    if (s.dialogue.length === 0) return sum;
    const lastLine = s.dialogue[s.dialogue.length - 1];
    return sum + lastLine.endTime;
  }, 0);

  const minutes = Math.floor(estimatedSeconds / 60);
  const seconds = Math.round(estimatedSeconds % 60);
  const duration =
    minutes > 0
      ? `${minutes}m ${seconds}s`
      : `${seconds}s`;

  return { totalLines, estimatedSeconds, duration };
}

export function ScriptStats({ scenes }: ScriptStatsProps) {
  const { totalLines, duration } = computeStats(scenes);

  return (
    <div className="flex items-center gap-6 text-sm text-gray-600">
      <Stat label="Scenes" value={scenes.length} />
      <div className="h-4 w-px bg-gray-200" aria-hidden="true" />
      <Stat label="Lines" value={totalLines} />
      <div className="h-4 w-px bg-gray-200" aria-hidden="true" />
      <Stat label="Est. duration" value={duration} />
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="flex items-baseline gap-1.5">
      <span className="text-base font-semibold text-gray-900">{value}</span>
      <span className="text-xs text-gray-500">{label}</span>
    </div>
  );
}
