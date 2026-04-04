"use client";

import { Skeleton } from "@/components/ui/skeleton";
import TeamMemberList from "@/components/team/TeamMemberList";
import InviteMemberForm from "@/components/team/InviteMemberForm";
import { useTeam } from "@/hooks/useTeam";

export default function TeamSettingsPage() {
  const { team, members, loading, error } = useTeam();

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
        <p className="text-red-500">An error occurred while loading team information.</p>
      </main>
    );
  }

  return (
    <main className="p-4">
      <section className="mb-6">
        <h1 className="text-2xl font-bold mb-2">Team Settings</h1>
        <p className="text-gray-700">Manage your team members and roles.</p>
      </section>

      <section>
        <h2 className="text-lg font-medium mb-2">Invite a New Member</h2>
        <InviteMemberForm teamId={team?.id} />
      </section>

      <section className="mt-6">
        <h2 className="text-lg font-medium mb-2">Team Members</h2>
        <TeamMemberList members={members} />
      </section>
    </main>
  );
}