"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useUiStore } from "@/stores/uiStore";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Sheet, SheetContent } from "@/components/ui/sheet";
import {
  LayoutDashboard,
  FolderKanban,
  Settings,
  CreditCard,
} from "lucide-react";

const NAV_LINKS = [
  { name: "Dashboard", href: "/dashboard", icon: LayoutDashboard },
  { name: "Projects", href: "/projects", icon: FolderKanban },
  { name: "Settings", href: "/settings", icon: Settings },
  { name: "Billing", href: "/billing", icon: CreditCard },
];

function SidebarNav() {
  const pathname = usePathname();

  return (
    <nav className="flex flex-col gap-1 p-4" aria-label="Main navigation">
      {NAV_LINKS.map((link) => {
        const isActive = pathname === link.href || pathname.startsWith(link.href + "/");
        const Icon = link.icon;
        return (
          <Button
            key={link.href}
            variant={isActive ? "secondary" : "ghost"}
            className={cn("justify-start gap-3", isActive && "font-semibold")}
            asChild
          >
            <Link href={link.href} aria-current={isActive ? "page" : undefined}>
              <Icon className="h-4 w-4" />
              {link.name}
            </Link>
          </Button>
        );
      })}
    </nav>
  );
}

const Sidebar = () => {
  const { sidebarOpen, setSidebarOpen } = useUiStore();

  return (
    <>
      {/* Desktop sidebar */}
      <aside className="hidden lg:flex lg:flex-col lg:w-64 lg:border-r lg:border-border lg:bg-background lg:min-h-screen">
        <div className="px-4 py-5">
          <span className="text-lg font-bold text-primary">AnimStudio</span>
        </div>
        <Separator />
        <SidebarNav />
      </aside>

      {/* Mobile sidebar (Sheet) */}
      <Sheet open={sidebarOpen} onOpenChange={setSidebarOpen}>
        <SheetContent side="left" className="w-64 p-0">
          <div className="px-4 py-5">
            <span className="text-lg font-bold text-primary">AnimStudio</span>
          </div>
          <Separator />
          <SidebarNav />
        </SheetContent>
      </Sheet>
    </>
  );
};

export default Sidebar;