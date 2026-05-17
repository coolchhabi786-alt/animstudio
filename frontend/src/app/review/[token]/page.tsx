"use client";

import { useRef, useState } from "react";
import { Lock, AlertTriangle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { CommentPanel } from "@/components/review/comment-panel";
import { useReview, useReviewComments } from "@/hooks/use-review";

export default function ReviewPage({ params }: { params: { token: string } }) {
  const { token } = params;
  const videoRef = useRef<HTMLVideoElement>(null);
  const [submittedPassword, setSubmittedPassword] = useState<string | undefined>(undefined);
  const [passwordInput, setPasswordInput] = useState("");

  const { data: review, isLoading, error } = useReview(token, submittedPassword);
  const { data: comments = [] } = useReviewComments(token);

  function handleSeek(seconds: number) {
    if (videoRef.current) {
      videoRef.current.currentTime = seconds;
      videoRef.current.play();
    }
  }

  function handlePasswordSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmittedPassword(passwordInput.trim() || undefined);
  }

  if (isLoading) {
    return (
      <div className="min-h-screen p-6 flex items-center justify-center">
        <Skeleton className="h-12 w-48" />
      </div>
    );
  }

  // Show password gate when:
  // 1. We haven't submitted a password yet and the review requires one, OR
  // 2. An error occurred (likely 401/403 from wrong password)
  const needsPassword = !submittedPassword && review?.hasPassword;
  const passwordError = !!error && !!submittedPassword;

  if (!review || review.isRevoked || review.isExpired || needsPassword || passwordError) {
    // Expired or revoked
    if (review?.isRevoked || review?.isExpired) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center space-y-2">
            <AlertTriangle className="h-10 w-10 text-destructive mx-auto" />
            <p className="text-lg font-semibold">
              {review.isRevoked ? "This link has been revoked." : "This link has expired."}
            </p>
          </div>
        </div>
      );
    }

    // Password gate
    return (
      <div className="min-h-screen flex items-center justify-center bg-muted/30">
        <div className="w-full max-w-sm rounded-xl border bg-background p-6 shadow-sm space-y-4">
          <div className="flex items-center gap-2">
            <Lock className="h-5 w-5 text-violet-500" />
            <h1 className="text-lg font-semibold">Password Required</h1>
          </div>
          <p className="text-sm text-muted-foreground">
            This review link is password-protected.
          </p>
          {passwordError && (
            <p className="text-sm text-destructive">Incorrect password. Please try again.</p>
          )}
          <form onSubmit={handlePasswordSubmit} className="space-y-3">
            <div className="space-y-1">
              <Label htmlFor="pw">Password</Label>
              <Input
                id="pw"
                type="password"
                value={passwordInput}
                onChange={(e) => setPasswordInput(e.target.value)}
                placeholder="Enter password…"
                required
              />
            </div>
            <Button type="submit" className="w-full">
              View Review
            </Button>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b px-6 py-3 flex items-center gap-3">
        <span className="font-semibold">{review.episodeName}</span>
        <span className="text-xs text-muted-foreground">Review</span>
      </header>

      <div className="flex flex-1 overflow-hidden">
        {/* Video player — left 60% */}
        <div className="w-[60%] bg-black flex items-center justify-center">
          <video
            ref={videoRef}
            src={review.videoUrl}
            controls
            className="max-h-full max-w-full"
            preload="metadata"
          />
        </div>

        {/* Comment panel — right 40% */}
        <div className="w-[40%] border-l flex flex-col overflow-hidden">
          <div className="px-4 py-3 border-b">
            <h2 className="text-sm font-semibold">Comments ({comments.length})</h2>
          </div>
          <div className="flex-1 overflow-hidden p-4">
            <CommentPanel
              token={token}
              comments={comments}
              onSeek={handleSeek}
              isOwner={false}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
