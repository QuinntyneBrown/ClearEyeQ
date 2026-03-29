"use client";

import { Inbox, ClipboardCheck, Eye, FileText } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface ActivityItem {
  id: string;
  type: "referral" | "review" | "scan" | "note";
  description: string;
  timestamp: string;
}

const iconMap: Record<string, { icon: React.ComponentType<{ className?: string }>; color: string; bg: string }> = {
  referral: { icon: Inbox, color: "text-warning", bg: "bg-warning/10" },
  review: { icon: ClipboardCheck, color: "text-success", bg: "bg-success/10" },
  scan: { icon: Eye, color: "text-primary", bg: "bg-primary-light" },
  note: { icon: FileText, color: "text-textSecondary", bg: "bg-bgSurface" },
};

interface RecentActivityProps {
  activities: ActivityItem[];
}

export function RecentActivity({ activities }: RecentActivityProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Recent Activity</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {activities.length === 0 && (
            <p className="text-sm text-textTertiary">No recent activity</p>
          )}
          {activities.map((activity) => {
            const config = iconMap[activity.type] || iconMap.note;
            const Icon = config.icon;
            return (
              <div key={activity.id} className="flex items-start gap-3">
                <div className={cn("rounded-md p-2 mt-0.5", config.bg)}>
                  <Icon className={cn("h-4 w-4", config.color)} />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm text-textPrimary">{activity.description}</p>
                  <p className="text-xs text-textTertiary mt-0.5">{activity.timestamp}</p>
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
