"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

const navLinks = [
  { name: "Dashboard", href: "/dashboard" },
  { name: "Settings", href: "/settings" },
  { name: "Team", href: "/settings/team" },
  { name: "Billing", href: "/settings/billing" },
];

export default function Sidebar() {
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const router = useRouter();

  return (
    <aside
      className={cn(
        "w-64 bg-gray-800 text-white md:flex md:flex-col px-4 py-6",
        {
          hidden: !isMobileOpen,
          block: isMobileOpen,
        }
      )}
    >
      <button
        className="block md:hidden mb-4"
        onClick={() => setIsMobileOpen(!isMobileOpen)}
      >
        {isMobileOpen ? "Close" : "Open"}
      </button>

      <nav>
        <ul className="space-y-4">
          {navLinks.map((link) => (
            <li key={link.name}>
              <Button
                asChild
                variant="ghost"
                onClick={() => router.push(link.href)}
              >
                <a>{link.name}</a>
              </Button>
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  );
}