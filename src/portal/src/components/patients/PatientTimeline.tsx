"use client";

import { Eye, Stethoscope, Pill, FileText } from "lucide-react";
import { cn } from "@/lib/utils";

interface TimelineEvent {
  id: string;
  type: "scan" | "diagnosis" | "treatment" | "note";
  title: string;
  description: string;
  date: string;
}

interface PatientTimelineProps {
  events: TimelineEvent[];
}

const eventConfig: Record<string, { icon: React.ComponentType<{ className?: string }>; color: string; bg: string }> = {
  scan: { icon: Eye, color: "text-primary", bg: "bg-primary-light" },
  diagnosis: { icon: Stethoscope, color: "text-warning", bg: "bg-warning/10" },
  treatment: { icon: Pill, color: "text-success", bg: "bg-success/10" },
  note: { icon: FileText, color: "text-textSecondary", bg: "bg-bgSurface" },
};

export function PatientTimeline({ events }: PatientTimelineProps) {
  if (events.length === 0) {
    return (
      <p className="text-sm text-textTertiary">No timeline events</p>
    );
  }

  return (
    <div className="relative space-y-0">
      {events.map((event, index) => {
        const config = eventConfig[event.type] || eventConfig.note;
        const Icon = config.icon;
        const isLast = index === events.length - 1;

        return (
          <div key={event.id} className="relative flex gap-4 pb-6">
            {/* Vertical Line */}
            {!isLast && (
              <div className="absolute left-[19px] top-10 h-[calc(100%-32px)] w-[2px] bg-border" />
            )}

            {/* Icon */}
            <div className={cn("relative z-10 flex h-10 w-10 shrink-0 items-center justify-center rounded-full", config.bg)}>
              <Icon className={cn("h-4 w-4", config.color)} />
            </div>

            {/* Content */}
            <div className="flex-1 pt-1">
              <div className="flex items-center justify-between">
                <h4 className="text-sm font-medium text-textPrimary">{event.title}</h4>
                <time className="text-xs text-textTertiary">{event.date}</time>
              </div>
              <p className="mt-1 text-sm text-textSecondary">{event.description}</p>
            </div>
          </div>
        );
      })}
    </div>
  );
}
