"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

export default function AcceptInvitePage() {
  const router = useRouter();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = new URLSearchParams(window.location.search).get("token");

    if (!token) {
      setError("Invalid invite token.");
      setLoading(false);
      return;
    }

    fetch(`/api/v1/teams/invites/${token}/accept`, {
      method: "POST",
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.error) {
          setError(data.error);
          setLoading(false);
        } else {
          router.push(`/teams/${data.teamId}`);
        }
      })
      .catch(() => {
        setError("Failed to accept the invite. Please try again.");
        setLoading(false);
      });
  }, [router]);

  if (loading) {
    return (
      <main className="flex items-center justify-center min-h-screen">
        <Skeleton className="h-12 w-48" />
      </main>
    );
  }

  if (error) {
    return (
      <main className="flex items-center justify-center min-h-screen">
        <div className="p-6 bg-white rounded shadow">
          <p className="text-red-500 mb-4">{error}</p>
          <Button asChild variant="default">
            <a href="/">Go to Homepage</a>
          </Button>
        </div>
      </main>
    );
  }

  return null;
}