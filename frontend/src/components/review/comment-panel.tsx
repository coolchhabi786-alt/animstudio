"use client";

import { useState } from "react";
import { CheckCheck, Clock } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useAddReviewComment, useResolveComment } from "@/hooks/use-review";
import { ReviewComment } from "@/types";

interface CommentPanelProps {
  token: string;
  comments: ReviewComment[];
  onSeek?: (seconds: number) => void;
  isOwner?: boolean;
}

function formatTimestamp(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = Math.floor(seconds % 60);
  return `${m}:${s.toString().padStart(2, "0")}`;
}

export function CommentPanel({ token, comments, onSeek, isOwner = false }: CommentPanelProps) {
  const [authorName, setAuthorName] = useState("");
  const [text, setText] = useState("");
  const [timestampSeconds, setTimestampSeconds] = useState(0);

  const addMutation = useAddReviewComment(token);
  const resolveMutation = useResolveComment(token);

  function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!authorName.trim() || !text.trim()) return;
    addMutation.mutate(
      { authorName: authorName.trim(), text: text.trim(), timestampSeconds },
      {
        onSuccess: () => {
          setText("");
          toast.success("Comment added.");
        },
        onError: (err) => toast.error(err.message ?? "Failed to add comment."),
      },
    );
  }

  function handleResolve(id: string) {
    resolveMutation.mutate(id, {
      onSuccess: () => toast.success("Comment resolved."),
      onError: (err) => toast.error(err.message ?? "Failed to resolve comment."),
    });
  }

  return (
    <div className="flex flex-col h-full gap-4">
      <div className="flex-1 overflow-y-auto space-y-3 min-h-0">
        {comments.length === 0 && (
          <p className="text-sm text-muted-foreground text-center py-8">No comments yet.</p>
        )}
        {comments.map((c) => (
          <div
            key={c.id}
            className={`rounded-lg border p-3 space-y-1 ${c.isResolved ? "opacity-50" : ""}`}
          >
            <div className="flex items-center justify-between gap-2">
              <span className="text-sm font-medium">{c.authorName}</span>
              <button
                onClick={() => onSeek?.(c.timestampSeconds)}
                className="flex items-center gap-1 text-xs text-violet-600 hover:text-violet-700 transition-colors"
              >
                <Clock className="h-3 w-3" />
                {formatTimestamp(c.timestampSeconds)}
              </button>
            </div>
            <p className="text-sm">{c.text}</p>
            <div className="flex items-center justify-between">
              <span className="text-xs text-muted-foreground">
                {new Date(c.createdAt).toLocaleString([], { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" })}
              </span>
              {isOwner && !c.isResolved && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-6 text-xs"
                  onClick={() => handleResolve(c.id)}
                  disabled={resolveMutation.isPending}
                >
                  <CheckCheck className="h-3 w-3 mr-1" />
                  Resolve
                </Button>
              )}
              {c.isResolved && (
                <span className="text-xs text-muted-foreground flex items-center gap-1">
                  <CheckCheck className="h-3 w-3" /> Resolved
                </span>
              )}
            </div>
          </div>
        ))}
      </div>

      <form onSubmit={handleAdd} className="space-y-2 border-t pt-4">
        <div className="space-y-1">
          <Label htmlFor="author-name">Your name</Label>
          <Input
            id="author-name"
            placeholder="Name"
            value={authorName}
            onChange={(e) => setAuthorName(e.target.value)}
            required
          />
        </div>
        <div className="space-y-1">
          <Label htmlFor="comment-text">Comment</Label>
          <Textarea
            id="comment-text"
            placeholder="Add a comment…"
            rows={3}
            value={text}
            onChange={(e) => setText(e.target.value)}
            required
          />
        </div>
        <div className="space-y-1">
          <Label htmlFor="timestamp">Timestamp (seconds)</Label>
          <Input
            id="timestamp"
            type="number"
            min={0}
            value={timestampSeconds}
            onChange={(e) => setTimestampSeconds(Number(e.target.value))}
          />
        </div>
        <Button type="submit" className="w-full" disabled={addMutation.isPending}>
          {addMutation.isPending ? "Adding…" : "Add Comment"}
        </Button>
      </form>
    </div>
  );
}
