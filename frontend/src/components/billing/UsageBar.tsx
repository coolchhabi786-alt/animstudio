import { Progress } from "@/components/ui/progress";

type UsageBarProps = {
  current: number;
  limit: number;
};

export default function UsageBar({ current, limit }: UsageBarProps) {
  // When limit is -1 the plan is unlimited — show 0% usage
  const percentage = limit <= 0 ? 0 : Math.min((current / limit) * 100, 100);
  const limitLabel = limit === -1 ? "Unlimited" : String(limit);

  return (
    <div className="w-full">
      <Progress value={percentage} className="mb-2" />
      <p className="text-sm text-gray-600">
        {current} / {limitLabel} episodes used
      </p>
    </div>
  );
}