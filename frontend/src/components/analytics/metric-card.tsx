import { TrendingDown, TrendingUp, type LucideIcon } from "lucide-react";

interface MetricCardProps {
  label: string;
  value: string | number;
  trend?: number;
  icon: LucideIcon;
}

export function MetricCard({ label, value, trend, icon: Icon }: MetricCardProps) {
  return (
    <div className="rounded-xl border bg-background p-5 shadow-sm space-y-3">
      <div className="flex items-center justify-between">
        <span className="text-sm text-muted-foreground">{label}</span>
        <Icon className="h-4 w-4 text-muted-foreground" />
      </div>
      <p className="text-3xl font-bold tracking-tight">{value}</p>
      {trend !== undefined && (
        <div
          className={`flex items-center gap-1 text-xs font-medium ${
            trend >= 0 ? "text-emerald-600" : "text-red-500"
          }`}
        >
          {trend >= 0 ? (
            <TrendingUp className="h-3.5 w-3.5" />
          ) : (
            <TrendingDown className="h-3.5 w-3.5" />
          )}
          {Math.abs(trend)}% vs last period
        </div>
      )}
    </div>
  );
}
