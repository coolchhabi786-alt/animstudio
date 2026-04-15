import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { UserDto } from "@/types";

export function useCurrentUser() {
  const { data, error, isLoading } = useQuery<UserDto>({
    queryKey: ["currentUser"],
    queryFn: () => apiFetch<UserDto>("/api/auth/me"),
  });

  return {
    data,
    user: data,
    error,
    isLoading,
    loading: isLoading,
  };
}