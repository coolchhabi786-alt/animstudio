"use client";

import { useState } from "react";
import { QRCodeSVG } from "qrcode.react";
import { toast } from "sonner";
import { Copy, Link2, QrCode } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useCreateReviewLink } from "@/hooks/use-review-links";
import { ReviewLink } from "@/types";

interface ReviewLinkGeneratorProps {
  renderId: string;
}

const EXPIRY_OPTIONS = [
  { label: "7 days", value: 7 },
  { label: "30 days", value: 30 },
  { label: "90 days", value: 90 },
  { label: "Never", value: 0 },
];

export function ReviewLinkGenerator({ renderId }: ReviewLinkGeneratorProps) {
  const [expiresInDays, setExpiresInDays] = useState("30");
  const [password, setPassword] = useState("");
  const [generated, setGenerated] = useState<ReviewLink | null>(null);
  const [showQr, setShowQr] = useState(false);

  const createMutation = useCreateReviewLink();

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const days = parseInt(expiresInDays, 10);
    createMutation.mutate(
      {
        renderId,
        body: {
          expiresInDays: days === 0 ? undefined : days,
          password: password.trim() || undefined,
        },
      },
      {
        onSuccess: (link) => {
          setGenerated(link);
          toast.success("Review link created.");
        },
        onError: (err) => {
          toast.error(err.message ?? "Failed to create review link.");
        },
      },
    );
  }

  async function handleCopy() {
    if (!generated) return;
    await navigator.clipboard.writeText(generated.shareUrl);
    toast.success("Link copied to clipboard.");
  }

  return (
    <div className="space-y-4">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <Label htmlFor="expiry">Expiry</Label>
            <Select value={expiresInDays} onValueChange={setExpiresInDays}>
              <SelectTrigger id="expiry">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {EXPIRY_OPTIONS.map((opt) => (
                  <SelectItem key={opt.value} value={String(opt.value)}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="password">Password (optional)</Label>
            <Input
              id="password"
              type="text"
              placeholder="Leave blank for open access"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
        </div>

        <Button type="submit" disabled={createMutation.isPending} className="w-full">
          <Link2 className="h-4 w-4 mr-2" />
          {createMutation.isPending ? "Generating…" : "Generate Review Link"}
        </Button>
      </form>

      {generated && (
        <div className="rounded-lg border bg-muted/40 p-4 space-y-3">
          <div className="flex items-center gap-2">
            <Input readOnly value={generated.shareUrl} className="font-mono text-sm" />
            <Button variant="outline" size="icon" onClick={handleCopy} title="Copy link">
              <Copy className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="icon"
              onClick={() => setShowQr((v) => !v)}
              title="Toggle QR code"
            >
              <QrCode className="h-4 w-4" />
            </Button>
          </div>
          {showQr && (
            <div className="flex justify-center p-4 bg-white rounded-lg">
              <QRCodeSVG value={generated.shareUrl} size={160} />
            </div>
          )}
        </div>
      )}
    </div>
  );
}
