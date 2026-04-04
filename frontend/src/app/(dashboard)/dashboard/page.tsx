"use client";

import { Button } from "@/components/ui/button";
import { useSubscription } from "@/hooks/useSubscription";
import { Skeleton } from "@/components/ui/skeleton";

export default function DashboardPage() {
  const { subscription, loading, error } = useSubscription();

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
        <p className="text-red-500">An error occurred trying to fetch subscription information.</p>
      </main>
    );
  }

  return (
    <main className="p-4">
      <section className="mb-6">
        <h1 className="text-2xl font-bold mb-2">Welcome to AnimStudio!</h1>
        <p className="text-gray-700">Your subscription is: {subscription?.status}</p>
      </section>
      <section>
        <h2 className="text-xl font-medium mb-2">Quick Actions</h2>
        <Button asChild variant="default">
          <a href="/settings">Update Profile</a>
        </Button>
      </section>
    </main>
  );
}