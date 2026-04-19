"use client";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import type { AnimationBackend, AnimationEstimateDto } from "@/types";

interface Props {
  isOpen: boolean;
  isLoading: boolean;
  backend: AnimationBackend;
  estimate: AnimationEstimateDto | undefined;
  onConfirm: () => void;
  onClose: () => void;
}

const currency = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  minimumFractionDigits: 2,
  maximumFractionDigits: 4,
});

export function ApprovalDialog({
  isOpen,
  isLoading,
  backend,
  estimate,
  onConfirm,
  onClose,
}: Props) {
  const shotCount = estimate?.shotCount ?? 0;
  const totalCost = estimate?.totalCostUsd ?? 0;

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Approve animation render?</DialogTitle>
          <DialogDescription>
            This queues a render job for every shot on the storyboard. Approvals
            are final — cancel later from the job list if needed.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-3 text-sm">
          <Row label="Backend" value={<Badge variant="outline">{backend}</Badge>} />
          <Row
            label="Shots to render"
            value={<span className="font-medium tabular-nums">{shotCount}</span>}
          />
          <Row
            label="Estimated total"
            value={
              <span className="font-semibold tabular-nums">
                {currency.format(totalCost)}
              </span>
            }
          />
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button onClick={onConfirm} disabled={isLoading || shotCount === 0}>
            {isLoading ? "Approving…" : `Approve & render ${shotCount} shots`}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function Row({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between border-b last:border-b-0 py-2">
      <span className="text-muted-foreground">{label}</span>
      {value}
    </div>
  );
}
