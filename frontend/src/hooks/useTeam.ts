import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { TeamDto, TeamMemberDto } from "@/types";

export function useTeam() {
  const { data: team, error: teamError, isLoading: teamLoading } = useQuery<TeamDto>({
    queryKey: ["team"],
    queryFn: () => apiFetch<TeamDto>("/api/v1/teams/me"),
  });

  const {
    data: members,
    error: membersError,
    isLoading: membersLoading,
  } = useQuery<TeamMemberDto[]>({
    queryKey: ["teamMembers", team?.id],
    queryFn: () => apiFetch<TeamMemberDto[]>(`/api/v1/teams/${team!.id}/members`),
    enabled: !!team,
  });

  return {
    team,
    members: members ?? [],
    loading: teamLoading || membersLoading,
    error: teamError || membersError,
  };
}