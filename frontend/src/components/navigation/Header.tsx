"use client";

import { Button } from "@/components/ui/button";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/stores/authStore";

type Props = {
  onSearch?: (query: string) => void;
};

export default function Header({ onSearch }: Props) {
  const router = useRouter();
  const { user } = useAuthStore();

  return (
    <header className="h-16 flex items-center justify-between px-4 bg-gray-800 text-white">
      <input
        type="search"
        placeholder="Search..."
        className="p-2 rounded bg-gray-700 text-white"
        onChange={(e) => onSearch?.(e.target.value)}
      />

      <div className="flex items-center space-x-4">
        <button
          aria-label="Notifications"
          className="text-lg hover:text-yellow-500 focus:text-yellow-500"
        >
          🔔
        </button>

        <Button
          variant="ghost"
          aria-label="User Menu"
          onClick={() => router.push("/settings")}
        >
          {user?.displayName || "User"}
        </Button>
      </div>
    </header>
  );
}