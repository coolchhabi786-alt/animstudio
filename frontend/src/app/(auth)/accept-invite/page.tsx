"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { apiFetch } from "@/lib/api-client";

export default function AcceptInvitePage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const token = searchParams.get("token");

  if (!token) {
    toast.error("Invalid invite token.");
    return <Skeleton className="h-10 w-36" />;
  }

  const acceptInvite = async () => {
    try {
      // Backend: POST /api/teams/invites/accept with { token } in body
      await apiFetch("/api/teams/invites/accept", {
        method: "POST",
        body: JSON.stringify({ token }),
      });

      toast.success("Invite accepted successfully.");
      router.push("/dashboard");
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to accept invite");
    }
  };

  return (
    <div className="text-center">
      <h1 className="mb-4 text-xl font-semibold">Team Invite</h1>
      <p className="mb-6 text-gray-600">Accept your invite to join the team.</p>
      <Button
        className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md focus:ring focus:ring-blue-300"
        onClick={acceptInvite}
        aria-label="Accept team invite"
      >
        Accept Invite
      </Button>
    </div>
  );
}