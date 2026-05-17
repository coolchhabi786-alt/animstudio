"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { ShieldCheck, Users, Briefcase, Activity } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { MetricCard } from "@/components/analytics/metric-card";
import { useAdminStats, useAdminUsers, useAdminJobs } from "@/hooks/use-admin";

export default function AdminPage() {
  const router = useRouter();
  const { data: session, status } = useSession();
  const isAdmin = (session as any)?.user?.role === "Admin";

  const [page, setPage] = useState(1);

  const { data: stats, isLoading: statsLoading } = useAdminStats();
  const { data: usersPage, isLoading: usersLoading } = useAdminUsers(page);
  const { data: jobs = [], isLoading: jobsLoading } = useAdminJobs();

  // Redirect non-admins once session is resolved
  useEffect(() => {
    if (status === "authenticated" && !isAdmin) {
      router.replace("/dashboard");
    }
  }, [status, isAdmin, router]);

  if (status === "loading") {
    return (
      <div className="p-6 flex items-center justify-center min-h-[40vh]">
        <Skeleton className="h-12 w-48" />
      </div>
    );
  }

  if (!isAdmin) return null;

  return (
    <main className="p-6 max-w-6xl mx-auto space-y-10">
      <div className="flex items-center gap-2">
        <ShieldCheck className="h-5 w-5 text-violet-500" />
        <h1 className="text-2xl font-bold">Admin</h1>
      </div>

      {/* Section 1 — System Stats */}
      <section className="space-y-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          System Stats
        </h2>
        {statsLoading ? (
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-28 rounded-xl" />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <MetricCard label="DAU" value={stats?.dau ?? 0} icon={Users} />
            <MetricCard label="MAU" value={stats?.mau ?? 0} icon={Activity} />
            <MetricCard label="Active Jobs" value={stats?.activeJobs ?? 0} icon={Briefcase} />
            <MetricCard
              label="Error Rate"
              value={`${stats?.errorRatePct ?? 0}%`}
              icon={Activity}
            />
          </div>
        )}
      </section>

      {/* Section 2 — User List */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <Users className="h-4 w-4 text-muted-foreground" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Users
          </h2>
        </div>
        <div className="rounded-xl border bg-background shadow-sm overflow-hidden">
          <table className="w-full">
            <thead className="bg-muted/40">
              <tr>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">Name</th>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">Email</th>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">Role</th>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">Joined</th>
              </tr>
            </thead>
            <tbody>
              {usersLoading ? (
                <tr>
                  <td colSpan={4} className="py-8 text-center text-sm text-muted-foreground">Loading…</td>
                </tr>
              ) : (usersPage?.items ?? []).map((u) => (
                <tr key={u.id} className="border-t">
                  <td className="py-3 px-4 text-sm font-medium">{u.displayName}</td>
                  <td className="py-3 px-4 text-sm text-muted-foreground">{u.email}</td>
                  <td className="py-3 px-4 text-sm">{u.role}</td>
                  <td className="py-3 px-4 text-sm text-muted-foreground">
                    {new Date(u.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {usersPage && usersPage.totalPages && usersPage.totalPages > 1 && (
          <div className="flex items-center justify-between">
            <span className="text-xs text-muted-foreground">
              Page {page} of {usersPage.totalPages} · {usersPage.totalCount} users
            </span>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= (usersPage.totalPages ?? 1)}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </section>

      {/* Section 3 — Job Queue */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <Briefcase className="h-4 w-4 text-muted-foreground" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Job Queue
          </h2>
        </div>
        <div className="rounded-xl border bg-background shadow-sm overflow-hidden">
          <table className="w-full">
            <thead className="bg-muted/40">
              <tr>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">Job Type</th>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">State</th>
                <th className="py-3 px-4 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground">Created</th>
              </tr>
            </thead>
            <tbody>
              {jobsLoading ? (
                <tr>
                  <td colSpan={3} className="py-8 text-center text-sm text-muted-foreground">Loading…</td>
                </tr>
              ) : jobs.length === 0 ? (
                <tr>
                  <td colSpan={3} className="py-8 text-center text-sm text-muted-foreground">No active jobs.</td>
                </tr>
              ) : jobs.map((j) => (
                <tr key={j.id} className="border-t">
                  <td className="py-3 px-4 text-sm font-medium">{j.jobType}</td>
                  <td className="py-3 px-4 text-sm">
                    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                      j.state === "Processing"
                        ? "bg-violet-100 text-violet-700"
                        : j.state === "Failed"
                        ? "bg-red-100 text-red-700"
                        : "bg-muted text-muted-foreground"
                    }`}>
                      {j.state}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-sm text-muted-foreground">
                    {new Date(j.createdAt).toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </main>
  );
}
