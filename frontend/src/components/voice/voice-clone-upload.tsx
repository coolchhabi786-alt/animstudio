"use client";

import { useState, useRef } from "react";
import { Upload, Lock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";

interface Props {
  characterId: string;
  onUpload: (file: File) => void;
  isTierLocked: boolean;
}

export function VoiceCloneUpload({ characterId: _characterId, onUpload, isTierLocked }: Props) {
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [progress, setProgress] = useState(0);
  const [isDone, setIsDone] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  function handleFile(file: File) {
    if (!file.type.startsWith("audio/")) return;
    setUploadedFile(file);
    setProgress(0);
    setIsDone(false);

    // Simulate upload to 100% over 2 seconds
    let pct = 0;
    const interval = setInterval(() => {
      pct += 10;
      setProgress(pct);
      if (pct >= 100) {
        clearInterval(interval);
        setIsDone(true);
        onUpload(file);
      }
    }, 200);
  }

  function handleDrop(e: React.DragEvent) {
    e.preventDefault();
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  }

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) handleFile(file);
  }

  if (isTierLocked) {
    return (
      <div className="border-2 border-dashed border-gray-200 rounded-lg p-6 text-center">
        <Lock className="h-8 w-8 mx-auto text-muted-foreground mb-2" />
        <p className="text-sm font-medium mb-1">Voice Cloning</p>
        <p className="text-xs text-muted-foreground mb-3">Available on Studio tier</p>
        <Badge variant="secondary">Upgrade to Studio tier</Badge>
      </div>
    );
  }

  return (
    <div
      className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-primary transition-colors cursor-pointer"
      onDragOver={(e) => e.preventDefault()}
      onDrop={handleDrop}
      onClick={() => !uploadedFile && inputRef.current?.click()}
    >
      <input
        ref={inputRef}
        type="file"
        accept="audio/*,.mp3,.wav,.m4a"
        className="hidden"
        onChange={handleFileChange}
      />

      {!uploadedFile ? (
        <>
          <Upload className="h-8 w-8 mx-auto text-muted-foreground mb-2" />
          <p className="text-sm text-muted-foreground mb-1">
            Drag & drop or click to upload
          </p>
          <p className="text-xs text-muted-foreground">MP3, WAV, M4A — max 10 MB</p>
        </>
      ) : (
        <div className="space-y-2" onClick={(e) => e.stopPropagation()}>
          <p className="text-sm font-medium truncate">{uploadedFile.name}</p>
          <Progress value={progress} className="h-2" />
          <p className="text-xs text-muted-foreground">
            {isDone ? "Upload complete" : `${progress}%`}
          </p>
          {isDone && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setUploadedFile(null);
                setProgress(0);
                setIsDone(false);
              }}
            >
              Upload another
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
