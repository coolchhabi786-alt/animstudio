import { create } from "zustand";

type UiState = {
  sidebarOpen: boolean;
  activeNav: string;
  setSidebarOpen: (open: boolean) => void;
  setActiveNav: (path: string) => void;
};

export const useUiStore = create<UiState>((set) => ({
  sidebarOpen: false,
  activeNav: "",
  setSidebarOpen: (open) => set({ sidebarOpen: open }),
  setActiveNav: (path) => set({ activeNav: path }),
}));
