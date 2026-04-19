"use client";

import { Upload, Lock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

interface Props {
  isStudioTier: boolean;
  onUpload?: (file: File) => void;
}

export function VoiceCloneUpload({ isStudioTier, onUpload }: Props) {
  function handleDrop(e: React.DragEvent) {
    e.preventDefault();
    if (!isStudioTier) return;
    const file = e.dataTransfer.files[0];
    if (file && onUpload) onUpload(file);
  }

  function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file && onUpload) onUpload(file);
  }

  if (!isStudioTier) {
    return (
      <div className="border-2 border-dashed border-gray-200 rounded-lg p-6 text-center">
        <Lock className="h-8 w-8 mx-auto text-muted-foreground mb-2" />
        <p className="text-sm text-muted-foreground mb-1">Voice Cloning</p>
        <Badge variant="secondary">Studio Tier Required</Badge>
      </div>
    );
  }

  return (
    <div
      className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-primary transition-colors cursor-pointer"
      onDragOver={(e) => e.preventDefault()}
      onDrop={handleDrop}
    >
      <Upload className="h-8 w-8 mx-auto text-muted-foreground mb-2" />
      <p className="text-sm text-muted-foreground mb-2">
        Drag & drop an audio sample for voice cloning
      </p>
      <p className="text-xs text-muted-foreground mb-3">
        WAV or MP3, max 10 MB
      </p>
      <label>
        <input
          type="file"
          accept=".wav,.mp3,audio/wav,audio/mpeg"
          className="hidden"
          onChange={handleFileSelect}
        />
        <Button variant="outline" size="sm" asChild>
          <span>Choose File</span>
        </Button>
      </label>
    </div>
  );
}
