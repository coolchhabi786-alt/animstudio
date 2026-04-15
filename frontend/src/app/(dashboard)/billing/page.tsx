"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { useSubscription } from "@/hooks/useSubscription";

export default function BillingPage() {
  const { subscription, plans, loading, error } = useSubscription();

  if (loading) {
    return (
      <main className="p-6 space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-48 w-full" />
      </main>
    );
  }

  if (error) {
    return (
      <main className="p-6">
        <p className="text-destructive">An error occurred while fetching billing information.</p>
      </main>
    );
  }

  const usagePercent = subscription && subscription.episodesPerMonth > 0
    ? Math.min((subscription.episodesUsedThisMonth / subscription.episodesPerMonth) * 100, 100)
    : 0;
  const limitLabel = subscription?.episodesPerMonth === -1 ? "Unlimited" : String(subscription?.episodesPerMonth ?? 0);

  return (
    <main className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Billing Settings</h1>
        <p className="text-muted-foreground">Manage your subscription and plan.</p>
      </div>

      {/* Current Subscription */}
      {subscription && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Current Subscription</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center gap-3">
              <h3 className="text-xl font-bold">{subscription.planName}</h3>
              <Badge variant={subscription.status === "Active" ? "default" : "secondary"}>
                {subscription.status}
              </Badge>
            </div>

            {/* Usage */}
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Episodes used this month</span>
                <span className="font-medium">{subscription.episodesUsedThisMonth} / {limitLabel}</span>
              </div>
              <Progress value={usagePercent} />
            </div>

            <Button variant="outline" onClick={() => (window.location.href = "/manage-portal")}>
              Manage Subscription
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Available Plans */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Available Plans</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {plans.map((plan) => (
            <Card key={plan.id} className="flex flex-col">
              <CardHeader>
                <CardTitle className="text-lg">{plan.name}</CardTitle>
              </CardHeader>
              <CardContent className="flex-1 flex flex-col gap-3">
                <p className="text-3xl font-bold text-primary">
                  ${plan.price}<span className="text-sm font-normal text-muted-foreground">/mo</span>
                </p>
                <ul className="space-y-1 text-sm text-muted-foreground flex-1">
                  <li>{plan.episodesPerMonth === -1 ? "Unlimited" : plan.episodesPerMonth} episodes/month</li>
                  <li>{plan.maxCharacters} characters</li>
                  <li>{plan.maxTeamMembers} team members</li>
                </ul>
                <Button
                  className="w-full mt-auto"
                  variant={plan.isDefault ? "default" : "outline"}
                  onClick={() => (window.location.href = "/checkout")}
                >
                  {plan.isDefault ? "Current Plan" : "Choose Plan"}
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </main>
  );
}