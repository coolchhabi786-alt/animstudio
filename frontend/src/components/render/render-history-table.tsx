"use client";

import { Download, RefreshCw, Play, MoreHorizontal } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { MockRender, RenderStatus } from "@/lib/mock-data";

interface Props {
  renders: MockRender[];
  onRerender?: (render: MockRender) => void;
  onPreview?: (render: MockRender) => void;
}

const STATUS_STYLES: Record<RenderStatus, string> = {
  complete:   "bg-emerald-100 text-emerald-700 border-emerald-200",
  queued:     "bg-gray-100 text-gray-600 border-gray-200",
  assembling: "bg-indigo-100 text-indigo-700 border-indigo-200",
  mixing:     "bg-violet-100 text-violet-700 border-violet-200",
  failed:     "bg-red-100 text-red-700 border-red-200",
};

const STATUS_LABEL: Record<RenderStatus, string> = {
  complete:   "Complete",
  queued:     "Queued",
  assembling: "Assembling",
  mixing:     "Mixing",
  failed:     "Failed",
};

function formatDate(iso: string) {
  return new Date(iso).toLocaleString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

function formatDuration(secs: number) {
  const m = Math.floor(secs / 60);
  const s = secs % 60;
  return m > 0 ? `${m}m ${s}s` : `${s}s`;
}

function triggerDownload(url: string, filename: string) {
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
}

export function RenderHistoryTable({ renders, onRerender, onPreview }: Props) {
  const sorted = [...renders].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );

  if (sorted.length === 0) {
    return (
      <div className="rounded-lg border py-10 text-center text-sm text-muted-foreground">
        No renders yet. Start rendering to see history here.
      </div>
    );
  }

  return (
    <div className="rounded-lg border overflow-hidden">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b bg-muted/50">
            <th className="text-left px-4 py-3 font-semibold text-muted-foreground">Date Created</th>
            <th className="text-left px-4 py-3 font-semibold text-muted-foreground">Duration</th>
            <th className="text-left px-4 py-3 font-semibold text-muted-foreground">Aspect</th>
            <th className="text-left px-4 py-3 font-semibold text-muted-foreground">Resolution</th>
            <th className="text-left px-4 py-3 font-semibold text-muted-foreground">Status</th>
            <th className="px-4 py-3" />
          </tr>
        </thead>
        <tbody>
          {sorted.map((r) => (
            <tr key={r.id} className="border-b last:border-b-0 hover:bg-muted/30 transition-colors">
              <td className="px-4 py-3 text-muted-foreground">{formatDate(r.createdAt)}</td>
              <td className="px-4 py-3 tabular-nums">{formatDuration(r.durationSeconds)}</td>
              <td className="px-4 py-3 font-mono text-xs">{r.aspectRatio}</td>
              <td className="px-4 py-3 text-xs text-muted-foreground uppercase">{r.resolution}</td>
              <td className="px-4 py-3">
                <Badge
                  variant="outline"
                  className={`text-[10px] border ${STATUS_STYLES[r.status]}`}
                >
                  {STATUS_LABEL[r.status]}
                </Badge>
              </td>
              <td className="px-4 py-3 text-right">
                <div className="flex items-center justify-end gap-1">
                  {r.finalVideoUrl && onPreview && (
                    <Button
                      size="sm"
                      variant="ghost"
                      className="h-7 gap-1 text-xs"
                      onClick={() => onPreview(r)}
                    >
                      <Play className="h-3 w-3" />
                      Preview
                    </Button>
                  )}
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button size="icon" variant="ghost" className="h-7 w-7">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      {r.finalVideoUrl && (
                        <DropdownMenuItem
                          onClick={() =>
                            triggerDownload(r.finalVideoUrl!, `render-${r.id.slice(0, 8)}.mp4`)
                          }
                        >
                          <Download className="h-4 w-4 mr-2" />
                          Download MP4
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuItem onClick={() => onRerender?.(r)}>
                        <RefreshCw className="h-4 w-4 mr-2" />
                        Re-render
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
