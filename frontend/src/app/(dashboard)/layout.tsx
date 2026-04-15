"use client";

import { useSession } from "next-auth/react";
import Sidebar from "@/components/navigation/Sidebar";
import Header from "@/components/navigation/Header";
import { Skeleton } from "@/components/ui/skeleton";

type Props = {
  children: React.ReactNode;
};

export default function DashboardLayout({ children }: Props) {
  // Middleware already redirects unauthenticated users to /login in production.
  // In development the middleware is bypassed, so we render even without a session.
  const { status } = useSession();

  if (status === "loading") {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Skeleton className="h-12 w-48" />
      </div>
    );
  }

  // In development, allow rendering without auth so every page is navigable.
  // In production the middleware guarantees users are authenticated by this point.

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <div className="flex flex-grow">
        <Sidebar />
        <main className="flex-grow p-4 bg-muted/50">{children}</main>
      </div>
    </div>
  );
}