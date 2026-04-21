"use client";

interface SceneRow {
  sceneNumber: number;
  shotCount: number;
}

interface Props {
  scenes: SceneRow[];
  ratePerShot: number;
  backend: string;
}

const usd = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  minimumFractionDigits: 3,
});

export function CostBreakdownTable({ scenes, ratePerShot }: Props) {
  const total = scenes.reduce((sum, s) => sum + s.shotCount * ratePerShot, 0);
  const isFree = ratePerShot === 0;

  return (
    <div className="rounded-lg border overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-muted/50">
          <tr className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
            <th className="text-left px-4 py-2">Scene</th>
            <th className="text-center px-4 py-2">Shots</th>
            <th className="text-right px-4 py-2">Rate ($)</th>
            <th className="text-right px-4 py-2">Subtotal ($)</th>
          </tr>
        </thead>
        <tbody>
          {scenes.map((s) => (
            <tr key={s.sceneNumber} className="border-t">
              <td className="px-4 py-2">Scene {s.sceneNumber}</td>
              <td className="px-4 py-2 text-center tabular-nums">{s.shotCount}</td>
              <td className="px-4 py-2 text-right tabular-nums">
                {isFree ? "Free" : usd.format(ratePerShot)}
              </td>
              <td className="px-4 py-2 text-right tabular-nums">
                {isFree ? "—" : usd.format(s.shotCount * ratePerShot)}
              </td>
            </tr>
          ))}
        </tbody>
        <tfoot className="border-t bg-muted/30">
          <tr>
            <td colSpan={3} className="px-4 py-3 font-bold text-sm">
              TOTAL COST
            </td>
            <td className="px-4 py-3 text-right font-bold text-sm tabular-nums">
              {isFree ? "Free" : usd.format(total)}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
