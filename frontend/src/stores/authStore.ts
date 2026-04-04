import { create } from "zustand";
import { UserDto, TeamDto } from "@/types";

type AuthState = {
  user: UserDto | null;
  team: TeamDto | null;
  loading: boolean;
  setUser: (user: UserDto | null) => void;
  setTeam: (team: TeamDto | null) => void;
};

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  team: null,
  loading: true,
  setUser: (user) => set({ user, loading: false }),
  setTeam: (team) => set({ team }),
}));