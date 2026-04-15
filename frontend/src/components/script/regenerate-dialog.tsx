"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface RegenerateDialogProps {
  isOpen: boolean;
  isLoading: boolean;
  onConfirm: (directorNotes: string) => void;
  onClose: () => void;
}

export function RegenerateDialog({
  isOpen,
  isLoading,
  onConfirm,
  onClose,
}: RegenerateDialogProps) {
  const [notes, setNotes] = useState("");

  function handleConfirm() {
    onConfirm(notes);
    setNotes("");
  }

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Regenerate Script</DialogTitle>
          <DialogDescription>
            The AI will rewrite the full screenplay. Any manual edits will be lost.
            Optionally leave director notes to guide the regeneration.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="director-notes">
            Director notes <span className="text-muted-foreground">(optional)</span>
          </Label>
          <Textarea
            id="director-notes"
            rows={4}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="e.g. Make the dialogue snappier. Add more humour in scene 2."
            maxLength={5000}
          />
          <p className="text-xs text-muted-foreground text-right">{notes.length}/5000</p>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button onClick={handleConfirm} disabled={isLoading}>
            {isLoading ? "Regenerating…" : "Regenerate"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
