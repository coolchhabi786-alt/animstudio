"use client";

import { useState, useCallback } from "react";
import { toast } from "sonner";
import { Save, Volume2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { VoicePicker } from "@/components/voice/voice-picker";
import { LanguageSelector } from "@/components/voice/language-selector";
import { AudioPreviewPlayer } from "@/components/voice/audio-preview-player";
import { VoiceCloneUpload } from "@/components/voice/voice-clone-upload";
import {
  useVoiceAssignments,
  useUpdateVoiceAssignments,
} from "@/hooks/use-voice-assignments";
import { useSubscription } from "@/hooks/useSubscription";
import type { VoiceAssignmentDto, VoiceAssignmentRequest } from "@/types";

interface Props {
  params: { id: string; episodeId: string };
}

/** Local editing state for one row. */
interface VoiceRow {
  characterId: string;
  characterName: string;
  voiceName: string;
  language: string;
  voiceCloneUrl?: string;
  previewUrl?: string;
}

export default function VoiceStudioPage({ params }: Props) {
  const { episodeId } = params;

  const { data: assignments, isLoading } = useVoiceAssignments(episodeId);
  const updateMutation = useUpdateVoiceAssignments(episodeId);
  const { subscription } = useSubscription();
  const isStudioTier = subscription?.planName === "Studio";

  // Local editing state — initialised from server data
  const [rows, setRows] = useState<VoiceRow[]>([]);
  const [initialized, setInitialized] = useState(false);

  // Sync server data into local state (once)
  if (assignments && !initialized) {
    setRows(
      assignments.map((a) => ({
        characterId: a.characterId,
        characterName: a.characterName,
        voiceName: a.voiceName,
        language: a.language,
        voiceCloneUrl: a.voiceCloneUrl,
      })),
    );
    setInitialized(true);
  }

  const updateRow = useCallback(
    (characterId: string, patch: Partial<VoiceRow>) => {
      setRows((prev) =>
        prev.map((r) =>
          r.characterId === characterId ? { ...r, ...patch } : r,
        ),
      );
    },
    [],
  );

  async function handleSaveAll() {
    const payloads: VoiceAssignmentRequest[] = rows.map((r) => ({
      characterId: r.characterId,
      voiceName: r.voiceName,
      language: r.language,
      voiceCloneUrl: r.voiceCloneUrl,
    }));

    try {
      await updateMutation.mutateAsync(payloads);
      toast.success("Voice assignments saved");
    } catch {
      toast.error("Failed to save voice assignments");
    }
  }

  // ── Loading state ─────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="space-y-4 p-6">
        <Skeleton className="h-8 w-48" />
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-20 w-full" />
        ))}
      </div>
    );
  }

  // ── Empty state ───────────────────────────────────────────────────────
  if (!assignments || assignments.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-12 text-center">
        <Volume2 className="h-12 w-12 text-muted-foreground mb-4" />
        <h2 className="text-xl font-semibold mb-2">No Characters Assigned</h2>
        <p className="text-muted-foreground max-w-md">
          Add characters to this episode first, then come back to assign voices.
          Each character needs a voice before the episode can be rendered.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Voice Studio</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Assign voices to each character and preview TTS audio
          </p>
        </div>
        <Button
          onClick={handleSaveAll}
          disabled={updateMutation.isPending}
        >
          <Save className="h-4 w-4 mr-2" />
          {updateMutation.isPending ? "Saving..." : "Save All"}
        </Button>
      </div>

      {/* Character voice rows */}
      <div className="space-y-3">
        {rows.map((row) => (
          <Card key={row.characterId} className="p-4">
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
              {/* Avatar + Name */}
              <div className="flex items-center gap-3 min-w-[160px]">
                <Avatar className="h-10 w-10">
                  <AvatarFallback className="bg-primary/10 text-sm font-medium">
                    {row.characterName.charAt(0).toUpperCase()}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-medium text-sm">{row.characterName}</p>
                  <Badge variant="outline" className="text-xs mt-0.5">
                    {row.voiceName || "No voice"}
                  </Badge>
                </div>
              </div>

              {/* Voice Picker */}
              <VoicePicker
                value={row.voiceName}
                onValueChange={(v) =>
                  updateRow(row.characterId, { voiceName: v })
                }
              />

              {/* Language Selector */}
              <LanguageSelector
                value={row.language}
                onValueChange={(v) =>
                  updateRow(row.characterId, { language: v })
                }
              />

              {/* Preview Player */}
              <AudioPreviewPlayer
                voiceName={row.voiceName}
                characterName={row.characterName}
                sampleText={`Hello, I am ${row.characterName}. This is a voice preview.`}
                onPlay={() => {}}
              />
            </div>
          </Card>
        ))}
      </div>

      {/* Voice Clone Section */}
      <div className="mt-8">
        <h2 className="text-lg font-semibold mb-3">Voice Cloning</h2>
        <VoiceCloneUpload
          characterId="episode-clone"
          isTierLocked={!isStudioTier}
          onUpload={(file) => {
            toast.info(`Voice cloning for "${file.name}" — feature coming soon`);
          }}
        />
      </div>
    </div>
  );
}
