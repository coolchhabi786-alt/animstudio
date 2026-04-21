"use client";

import { useState } from "react";
import { Mic } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { VoicePicker } from "@/components/voice/voice-picker";
import { LanguageSelector } from "@/components/voice/language-selector";
import { AudioPreviewPlayer } from "@/components/voice/audio-preview-player";
import { VoiceCloneUpload } from "@/components/voice/voice-clone-upload";
import { mockVoices } from "@/lib/mock-data";
import { BUILT_IN_VOICES } from "@/types";

// Free tier — voice cloning locked
const IS_STUDIO_TIER = false;

const VOICE_VALUES = BUILT_IN_VOICES.map((v) => v.value);

type AssignmentState = { voiceName: string; language: string };

export default function VoiceStudioPage({ params }: { params: { id: string } }) {
  const episodeId = params.id;

  const [assignments, setAssignments] = useState<Record<string, AssignmentState>>(
    () =>
      Object.fromEntries(
        mockVoices.map((v) => [v.characterId, { voiceName: v.voiceName, language: v.language }]),
      ),
  );

  function setVoice(characterId: string, voiceName: string) {
    setAssignments((prev) => ({
      ...prev,
      [characterId]: { ...prev[characterId], voiceName },
    }));
  }

  function setLanguage(characterId: string, language: string) {
    setAssignments((prev) => ({
      ...prev,
      [characterId]: { ...prev[characterId], language },
    }));
  }

  return (
    <main className="p-6 max-w-4xl mx-auto space-y-10">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 mb-1">
          <Mic className="h-5 w-5 text-rose-500" />
          <h1 className="text-2xl font-bold">Voice Studio</h1>
        </div>
        <p className="text-sm text-muted-foreground">
          Episode ID: <span className="font-mono">{episodeId}</span>
        </p>
      </div>

      {/* Section 1 — Character voice assignments */}
      <section>
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground mb-4">
          Character Voice Assignments
        </h2>

        <div className="rounded-lg border overflow-hidden">
          {/* Table header */}
          <div className="grid grid-cols-[1fr_auto_auto_auto] gap-4 items-center px-4 py-2 bg-muted/50 text-xs font-medium uppercase tracking-wide text-muted-foreground">
            <span>Character</span>
            <span>Voice</span>
            <span>Language</span>
            <span>Preview</span>
          </div>

          {/* Rows */}
          {mockVoices.map((v) => {
            const state = assignments[v.characterId] ?? { voiceName: v.voiceName, language: v.language };
            return (
              <div
                key={v.id}
                className="grid grid-cols-[1fr_auto_auto_auto] gap-4 items-center px-4 py-3 border-t"
              >
                {/* Character identity */}
                <div className="flex items-center gap-3 min-w-0">
                  <img
                    src={v.character.avatarUrl}
                    alt={v.character.name}
                    className="h-9 w-9 rounded-full object-cover flex-shrink-0"
                  />
                  <div className="min-w-0">
                    <p className="text-sm font-medium truncate">{v.character.name}</p>
                    <Badge variant="outline" className="text-[10px] capitalize">
                      {v.character.role}
                    </Badge>
                  </div>
                </div>

                {/* Voice picker */}
                <VoicePicker
                  value={state.voiceName}
                  onValueChange={(val) => setVoice(v.characterId, val)}
                />

                {/* Language selector */}
                <LanguageSelector
                  value={state.language}
                  onValueChange={(val) => setLanguage(v.characterId, val)}
                />

                {/* Audio preview */}
                <AudioPreviewPlayer
                  voiceName={state.voiceName}
                  characterName={v.character.name}
                  sampleText={`Hello, I am ${v.character.name}.`}
                  onPlay={() => {}}
                />
              </div>
            );
          })}
        </div>
      </section>

      {/* Section 2 — Voice cloning */}
      <section>
        <div className="flex items-center gap-2 mb-1">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Voice Cloning
          </h2>
          {!IS_STUDIO_TIER && (
            <Badge variant="secondary" className="text-[10px]">
              Studio Tier
            </Badge>
          )}
        </div>
        <p className="text-xs text-muted-foreground mb-4">
          Upload audio samples to clone a character&apos;s voice for more natural synthesis.
        </p>

        <div className="grid gap-4 sm:grid-cols-3">
          {mockVoices.slice(0, 3).map((v) => (
            <div key={v.characterId}>
              <p className="text-xs font-medium mb-2">{v.character.name}</p>
              <VoiceCloneUpload
                characterId={v.characterId}
                isTierLocked={!IS_STUDIO_TIER}
                onUpload={(file) => {
                  console.log(`[Voice Clone] ${v.character.name}:`, file.name);
                }}
              />
            </div>
          ))}
        </div>
      </section>

      {/* Voice list legend */}
      <section>
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground mb-3">
          Available Voices
        </h2>
        <div className="flex flex-wrap gap-2">
          {BUILT_IN_VOICES.map((v) => (
            <div
              key={v.value}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-full border text-xs"
            >
              <span className="font-medium">{v.label}</span>
              <span className="text-muted-foreground">— {v.gender}</span>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}
