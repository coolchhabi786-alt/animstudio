"use client";

import { Bell, CheckCheck, AlertTriangle, Info, Zap, ShieldAlert } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { useNotifications, useMarkNotificationRead, useMarkAllNotificationsRead } from "@/hooks/use-notifications";
import { Notification, NotificationType } from "@/types";

const TYPE_ICONS: Record<NotificationType, React.ComponentType<{ className?: string }>> = {
  EpisodeComplete: Zap,
  JobFailed: AlertTriangle,
  UsageWarning: Info,
  SystemAlert: ShieldAlert,
};

function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60_000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}

function NotificationItem({ n, onRead }: { n: Notification; onRead: (id: string) => void }) {
  const Icon = TYPE_ICONS[n.type] ?? Bell;
  return (
    <div className={`flex gap-3 px-4 py-3 hover:bg-muted/50 transition-colors ${n.isRead ? "opacity-60" : ""}`}>
      <div className="mt-0.5 shrink-0">
        <Icon className="h-4 w-4 text-muted-foreground" />
      </div>
      <div className="flex-1 min-w-0 space-y-0.5">
        <p className="text-sm font-medium leading-tight">{n.title}</p>
        <p className="text-xs text-muted-foreground leading-snug">{n.body}</p>
        <p className="text-xs text-muted-foreground">{timeAgo(n.createdAt)}</p>
      </div>
      {!n.isRead && (
        <button
          onClick={() => onRead(n.id)}
          className="shrink-0 mt-0.5 text-xs text-violet-600 hover:text-violet-700 whitespace-nowrap"
          title="Mark as read"
        >
          <CheckCheck className="h-4 w-4" />
        </button>
      )}
    </div>
  );
}

export function NotificationPanel() {
  const { data: notifications = [], isLoading } = useNotifications();
  const markRead = useMarkNotificationRead();
  const markAllRead = useMarkAllNotificationsRead();

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  function handleMarkAllRead() {
    markAllRead.mutate(undefined, {
      onSuccess: () => toast.success("All notifications marked as read."),
    });
  }

  return (
    <div className="flex flex-col w-80 max-h-[480px]">
      <div className="flex items-center justify-between px-4 py-3 border-b">
        <span className="text-sm font-semibold">
          Notifications {unreadCount > 0 && <span className="text-muted-foreground font-normal">({unreadCount} unread)</span>}
        </span>
        {unreadCount > 0 && (
          <Button
            variant="ghost"
            size="sm"
            className="h-7 text-xs"
            onClick={handleMarkAllRead}
            disabled={markAllRead.isPending}
          >
            Mark all read
          </Button>
        )}
      </div>

      <div className="overflow-y-auto flex-1">
        {isLoading && (
          <p className="text-xs text-muted-foreground text-center py-8">Loading…</p>
        )}
        {!isLoading && notifications.length === 0 && (
          <div className="flex flex-col items-center justify-center gap-2 py-12">
            <Bell className="h-8 w-8 text-muted-foreground/40" />
            <p className="text-sm text-muted-foreground">No notifications</p>
          </div>
        )}
        {notifications.map((n, i) => (
          <div key={n.id}>
            {i > 0 && <Separator />}
            <NotificationItem
              n={n}
              onRead={(id) => markRead.mutate(id)}
            />
          </div>
        ))}
      </div>
    </div>
  );
}
