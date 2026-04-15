"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import type { StoryboardShotDto } from "@/types";

interface Props {
  shot: StoryboardShotDto | null;
  isOpen: boolean;
  isLoading: boolean;
  onConfirm: (styleOverride: string | null) => void;
  onClose: () => void;
}

export function StyleDialog({ shot, isOpen, isLoading, onConfirm, onClose }: Props) {
  const [value, setValue] = useState("");

  useEffect(() => {
    setValue(shot?.styleOverride ?? "");
  }, [shot]);

  function handleConfirm() {
    const trimmed = value.trim();
    onConfirm(trimmed.length > 0 ? trimmed : null);
  }

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Shot style</DialogTitle>
          <DialogDescription>
            Override the style for this shot only. Leave blank to fall back to the
            episode-level style. The shot will be re-rendered.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="style-override">Style override</Label>
          <Input
            id="style-override"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            placeholder="e.g. Studio Ghibli watercolor, golden hour lighting"
            maxLength={500}
          />
          <p className="text-xs text-muted-foreground text-right">
            {value.length}/500
          </p>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button onClick={handleConfirm} disabled={isLoading}>
            {isLoading ? "Applying…" : "Apply & regenerate"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
