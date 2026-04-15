"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { useSubscription } from "@/hooks/useSubscription";
import { Skeleton } from "@/components/ui/skeleton";
import { LayoutDashboard, Settings, FolderKanban, Users } from "lucide-react";
import Link from "next/link";

export default function DashboardPage() {
  const { subscription, loading, error } = useSubscription();

  if (loading) {
    return (
      <main className="p-6 space-y-4">
        <Skeleton className="h-8 w-48" />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28 w-full" />
          ))}
        </div>
      </main>
    );
  }

  if (error) {
    return (
      <main className="p-6">
        <p className="text-destructive">An error occurred trying to fetch subscription information.</p>
      </main>
    );
  }

  const usagePercent =
    subscription && subscription.episodesPerMonth > 0
      ? Math.min(
          (subscription.episodesUsedThisMonth / subscription.episodesPerMonth) * 100,
          100
        )
      : 0;

  return (
    <main className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Welcome to AnimStudio!</h1>
        <p className="text-muted-foreground">
          Your subscription is{" "}
          <Badge variant={subscription?.status === "Active" ? "default" : "secondary"}>
            {subscription?.status ?? "Unknown"}
          </Badge>
        </p>
      </div>

      {/* Usage card */}
      {subscription && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Episode Usage This Month
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="flex items-baseline gap-2">
              <span className="text-2xl font-bold">{subscription.episodesUsedThisMonth}</span>
              <span className="text-sm text-muted-foreground">
                / {subscription.episodesPerMonth === -1 ? "∞" : subscription.episodesPerMonth}
              </span>
            </div>
            <Progress value={usagePercent} />
          </CardContent>
        </Card>
      )}

      {/* Quick actions */}
      <div>
        <h2 className="text-lg font-semibold mb-3">Quick Actions</h2>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <Button asChild variant="outline" className="h-auto py-4 flex flex-col gap-1.5 items-center">
            <Link href="/projects">
              <FolderKanban className="h-5 w-5" />
              <span className="text-sm">Projects</span>
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-auto py-4 flex flex-col gap-1.5 items-center">
            <Link href="/settings">
              <Settings className="h-5 w-5" />
              <span className="text-sm">Settings</span>
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-auto py-4 flex flex-col gap-1.5 items-center">
            <Link href="/settings/team">
              <Users className="h-5 w-5" />
              <span className="text-sm">Team</span>
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-auto py-4 flex flex-col gap-1.5 items-center">
            <Link href="/billing">
              <LayoutDashboard className="h-5 w-5" />
              <span className="text-sm">Billing</span>
            </Link>
          </Button>
        </div>
      </div>
    </main>
  );
}