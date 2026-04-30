import type { Timeline } from "@/types/timeline";

export type RenderStage = "compositing" | "mixing" | "exporting" | "complete";

export interface RenderProgress {
  percent: number;
  stage: RenderStage;
  label: string;
}

export interface RenderResult {
  videoUrl: string;
  durationSeconds: number;
}

function stageFromPercent(percent: number): { stage: RenderStage; label: string } {
  if (percent < 40) return { stage: "compositing", label: "Compositing video frames..." };
  if (percent < 70) return { stage: "mixing",       label: "Mixing audio tracks..." };
  if (percent < 100) return { stage: "exporting",   label: "Exporting video..." };
  return { stage: "complete", label: "Complete!" };
}

// Public-domain sample used as a stand-in for the generated preview
const MOCK_VIDEO_URL =
  "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

export const previewRenderService = {
  /**
   * Simulate a render job that emits progress at 1-second intervals
   * (0 → 25 → 50 → 75 → 100) then resolves with a mock video URL.
   */
  startRender(
    timeline: Timeline,
    onProgress: (p: RenderProgress) => void
  ): Promise<RenderResult> {
    const steps = [0, 25, 50, 75, 100];
    let i = 0;

    return new Promise((resolve) => {
      function emit() {
        const percent = steps[i];
        onProgress({ percent, ...stageFromPercent(percent) });
        i++;
        if (i < steps.length) {
          setTimeout(emit, 1000);
        } else {
          resolve({ videoUrl: MOCK_VIDEO_URL, durationSeconds: timeline.durationMs / 1000 });
        }
      }
      emit();
    });
  },
};
