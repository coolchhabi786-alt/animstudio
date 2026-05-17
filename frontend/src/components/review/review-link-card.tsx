"use client";

import { Copy, Eye, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useRevokeReviewLink } from "@/hooks/use-review-links";
import { ReviewLink } from "@/types";

interface ReviewLinkCardProps {
  link: ReviewLink;
}

function getStatus(link: ReviewLink): { label: string; variant: "default" | "secondary" | "destructive" } {
  if (link.isRevoked) return { label: "Revoked", variant: "destructive" };
  if (link.expiresAt && new Date(link.expiresAt) < new Date()) {
    return { label: "Expired", variant: "secondary" };
  }
  return { label: "Active", variant: "default" };
}

export function ReviewLinkCard({ link }: ReviewLinkCardProps) {
  const revokeMutation = useRevokeReviewLink();
  const status = getStatus(link);

  async function handleCopy() {
    await navigator.clipboard.writeText(link.shareUrl);
    toast.success("Link copied.");
  }

  function handleRevoke() {
    revokeMutation.mutate(link.id, {
      onSuccess: () => toast.success("Link revoked."),
      onError: (err) => toast.error(err.message ?? "Failed to revoke link."),
    });
  }

  return (
    <div className="flex items-start justify-between gap-4 rounded-lg border p-4">
      <div className="min-w-0 space-y-1">
        <div className="flex items-center gap-2">
          <Badge variant={status.variant}>{status.label}</Badge>
          <span className="text-xs text-muted-foreground">
            Created {new Date(link.createdAt).toLocaleDateString()}
          </span>
        </div>
        <p className="text-sm font-mono truncate text-muted-foreground">{link.shareUrl}</p>
        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          <span className="flex items-center gap-1">
            <Eye className="h-3 w-3" />
            {link.viewCount} views
          </span>
          {link.expiresAt && (
            <span>Expires {new Date(link.expiresAt).toLocaleDateString()}</span>
          )}
        </div>
      </div>
      <div className="flex shrink-0 gap-2">
        <Button variant="ghost" size="icon" onClick={handleCopy} title="Copy link">
          <Copy className="h-4 w-4" />
        </Button>
        {!link.isRevoked && (
          <Button
            variant="ghost"
            size="icon"
            onClick={handleRevoke}
            disabled={revokeMutation.isPending}
            title="Revoke link"
            className="text-destructive hover:text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        )}
      </div>
    </div>
  );
}
