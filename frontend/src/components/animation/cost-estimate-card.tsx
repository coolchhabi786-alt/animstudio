"use client";

import { Loader2 } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ANIMATION_BACKENDS } from "@/types";
import type {
  AnimationBackend,
  AnimationEstimateDto,
} from "@/types";

interface Props {
  backend: AnimationBackend;
  onBackendChange: (backend: AnimationBackend) => void;
  estimate: AnimationEstimateDto | undefined;
  isLoading: boolean;
}

const currency = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  minimumFractionDigits: 2,
  maximumFractionDigits: 4,
});

export function CostEstimateCard({
  backend,
  onBackendChange,
  estimate,
  isLoading,
}: Props) {
  return (
    <Card className="p-5 space-y-4">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-lg font-semibold">Render cost estimate</h2>
          <p className="text-xs text-muted-foreground mt-0.5">
            Pick a backend — review per-shot costs before approving.
          </p>
        </div>
        {isLoading && (
          <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
        )}
      </div>

      {/* Backend radio toggle */}
      <div
        role="radiogroup"
        aria-label="Animation backend"
        className="grid grid-cols-1 sm:grid-cols-2 gap-2"
      >
        {ANIMATION_BACKENDS.map((opt) => {
          const selected = backend === opt.value;
          return (
            <button
              key={opt.value}
              role="radio"
              aria-checked={selected}
              onClick={() => onBackendChange(opt.value)}
              className={
                "text-left rounded-lg border p-3 transition " +
                (selected
                  ? "border-indigo-500 bg-indigo-50"
                  : "border-gray-200 hover:border-gray-300")
              }
            >
              <div className="flex items-center justify-between">
                <span className="font-medium text-sm">{opt.label}</span>
                <Badge variant="outline" className="text-xs">
                  {opt.perClipUsd === 0
                    ? "Free"
                    : `${currency.format(opt.perClipUsd)} / shot`}
                </Badge>
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                {opt.description}
              </p>
            </button>
          );
        })}
      </div>

      {/* Breakdown */}
      <div className="rounded-lg border">
        <div className="px-3 py-2 bg-gray-50 text-xs font-medium text-gray-600 flex items-center justify-between border-b">
          <span>Shot breakdown</span>
          {estimate && (
            <span>
              {estimate.shotCount} shot{estimate.shotCount === 1 ? "" : "s"}
            </span>
          )}
        </div>

        {isLoading && !estimate ? (
          <div className="p-3 space-y-2">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-5/6" />
            <Skeleton className="h-4 w-2/3" />
          </div>
        ) : !estimate || estimate.shotCount === 0 ? (
          <p className="p-4 text-xs text-muted-foreground text-center">
            No shots found on this episode&apos;s storyboard.
          </p>
        ) : (
          <div className="max-h-56 overflow-y-auto text-sm">
            <table className="w-full text-xs">
              <thead className="bg-white sticky top-0">
                <tr className="border-b text-gray-500">
                  <th className="text-left font-medium px-3 py-1.5">Scene</th>
                  <th className="text-left font-medium px-3 py-1.5">Shot</th>
                  <th className="text-right font-medium px-3 py-1.5">Cost</th>
                </tr>
              </thead>
              <tbody>
                {estimate.breakdown.map((line) => (
                  <tr
                    key={`${line.sceneNumber}-${line.shotIndex}`}
                    className="border-b last:border-b-0"
                  >
                    <td className="px-3 py-1.5">S{line.sceneNumber}</td>
                    <td className="px-3 py-1.5">#{line.shotIndex}</td>
                    <td className="px-3 py-1.5 text-right tabular-nums">
                      {line.unitCostUsd === 0
                        ? "—"
                        : currency.format(line.unitCostUsd)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Total */}
      <div className="flex items-center justify-between pt-1">
        <span className="text-sm text-muted-foreground">Total</span>
        <span className="text-xl font-semibold tabular-nums">
          {estimate ? currency.format(estimate.totalCostUsd) : "—"}
        </span>
      </div>
    </Card>
  );
}
