"use client";

import { Skeleton } from "@/components/ui/skeleton";
import PlanCard from "@/components/billing/PlanCard";
import UsageBar from "@/components/billing/UsageBar";
import { useSubscription } from "@/hooks/useSubscription";

export default function BillingPage() {
  const { subscription, plans, loading, error } = useSubscription();

  if (loading) {
    return (
      <main className="p-4">
        <Skeleton className="h-12 w-full mb-4" />
        <Skeleton className="h-48 w-full" />
      </main>
    );
  }

  if (error) {
    return (
      <main className="p-4">
        <p className="text-red-500">An error occurred while fetching billing information.</p>
      </main>
    );
  }

  return (
    <main className="p-4">
      <section className="mb-6">
        <h1 className="text-2xl font-bold mb-2">Billing Settings</h1>
        <p className="text-gray-700">Manage your subscription and plan.</p>
      </section>

      <section>
        <h2 className="text-xl font-medium mb-2">Current Subscription</h2>
        {subscription && (
          <PlanCard
            planName={subscription.planName}
            onManage={() => (window.location.href = "/manage-portal")}
          />
        )}
      </section>

      <section className="mt-6">
        <h2 className="text-xl font-medium mb-2">Usage</h2>
        <UsageBar
          current={subscription?.episodesUsedThisMonth ?? 0}
          limit={subscription?.episodesPerMonth ?? 0}
        />
      </section>

      <section className="mt-6">
        <h2 className="text-xl font-medium mb-2">Available Plans</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {plans.map((plan) => (
            <PlanCard
              key={plan.id}
              planName={plan.name}
              price={plan.price}
              onManage={() => (window.location.href = "/checkout")}
            />
          ))}
        </div>
      </section>
    </main>
  );
}