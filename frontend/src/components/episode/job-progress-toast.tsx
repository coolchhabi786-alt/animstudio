"use client";

import { useEffect } from "react";
import { toast } from "sonner";
import { SagaStateDto } from "@/types";

const STAGE_LABELS: Record<string, string> = {
  ScriptGeneration: "Script generation",
  StoryboardGeneration: "Storyboard generation",
  VoiceGeneration: "Voice generation",
  AnimationGeneration: "Animation generation",
  MusicGeneration: "Music generation",
  FinalRendering: "Final rendering",
  Completed: "Episode complete",
};

interface JobProgressToastProps {
  sagaState: SagaStateDto | null;
}

export function JobProgressToast({ sagaState }: JobProgressToastProps) {
  useEffect(() => {
    if (!sagaState) return;

    if (sagaState.currentStage === "Completed") {
      toast.success("Episode processing complete!", {
        description: "Your episode is ready.",
        duration: 6000,
      });
    } else if (sagaState.lastError && sagaState.isCompensating) {
      toast.error("Processing error", {
        description: sagaState.lastError,
        duration: 8000,
      });
    } else if (sagaState.currentStage) {
      const label = STAGE_LABELS[sagaState.currentStage] ?? sagaState.currentStage;
      toast.info(`${label}…`, { id: "episode-progress", duration: 4000 });
    }
  }, [sagaState?.currentStage, sagaState?.lastError]);

  return null;
}
