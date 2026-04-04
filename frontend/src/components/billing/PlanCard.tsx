import { Button } from "@/components/ui/button";

type PlanCardProps = {
  planName: string;
  price?: number;
  onManage: () => void;
};

export default function PlanCard({ planName, price, onManage }: PlanCardProps) {
  return (
    <div className="p-4 bg-white rounded shadow">
      <h3 className="text-xl font-bold mb-2">{planName}</h3>
      {price !== undefined && (
        <p className="text-lg font-medium">${price} / month</p>
      )}
      <Button onClick={onManage} className="mt-6">
        Manage Subscription
      </Button>
    </div>
  );
}