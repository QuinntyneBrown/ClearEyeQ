"use client";

import { Check, Clock, Loader2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { TreatmentPlan } from "@/hooks/usePatients";

interface TreatmentTimelineProps {
  plans: TreatmentPlan[];
}

function getStatusIcon(status: string) {
  switch (status) {
    case "Completed":
      return <Check className="h-4 w-4 text-white" />;
    case "Active":
      return <Loader2 className="h-4 w-4 text-white animate-spin" />;
    default:
      return <Clock className="h-4 w-4 text-white" />;
  }
}

function getStatusColor(status: string) {
  switch (status) {
    case "Completed":
      return "bg-success";
    case "Active":
      return "bg-primary";
    default:
      return "bg-textTertiary";
  }
}

function getStatusBadgeVariant(status: string) {
  switch (status) {
    case "Completed":
      return "success" as const;
    case "Active":
      return "default" as const;
    default:
      return "outline" as const;
  }
}

export function TreatmentTimeline({ plans }: TreatmentTimelineProps) {
  if (plans.length === 0) {
    return (
      <p className="text-sm text-textTertiary">No treatment plans on record</p>
    );
  }

  return (
    <div className="space-y-0">
      {plans.map((plan, index) => {
        const isLast = index === plans.length - 1;

        return (
          <div key={plan.id} className="relative flex gap-4 pb-6">
            {/* Vertical Line */}
            {!isLast && (
              <div className="absolute left-[15px] top-9 h-[calc(100%-28px)] w-[2px] bg-border" />
            )}

            {/* Status Circle */}
            <div
              className={cn(
                "relative z-10 flex h-8 w-8 shrink-0 items-center justify-center rounded-full",
                getStatusColor(plan.status)
              )}
            >
              {getStatusIcon(plan.status)}
            </div>

            {/* Content */}
            <div className="flex-1 rounded-lg border border-border p-4">
              <div className="flex items-center justify-between mb-2">
                <h4 className="text-sm font-medium text-textPrimary">{plan.name}</h4>
                <Badge variant={getStatusBadgeVariant(plan.status)}>
                  {plan.status}
                </Badge>
              </div>

              <div className="text-xs text-textSecondary mb-3">
                {new Date(plan.startDate).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
                {plan.endDate && (
                  <>
                    {" — "}
                    {new Date(plan.endDate).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
                  </>
                )}
              </div>

              {plan.medications.length > 0 && (
                <div className="mb-2">
                  <p className="text-xs font-medium text-textSecondary mb-1">Medications</p>
                  <div className="flex flex-wrap gap-1">
                    {plan.medications.map((med) => (
                      <span key={med} className="rounded-full bg-primary-light px-2 py-0.5 text-xs text-primary">
                        {med}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {plan.behavioralInterventions.length > 0 && (
                <div className="mb-2">
                  <p className="text-xs font-medium text-textSecondary mb-1">Behavioral</p>
                  <div className="flex flex-wrap gap-1">
                    {plan.behavioralInterventions.map((item) => (
                      <span key={item} className="rounded-full bg-success/10 px-2 py-0.5 text-xs text-success">
                        {item}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {plan.environmentalInterventions.length > 0 && (
                <div>
                  <p className="text-xs font-medium text-textSecondary mb-1">Environmental</p>
                  <div className="flex flex-wrap gap-1">
                    {plan.environmentalInterventions.map((item) => (
                      <span key={item} className="rounded-full bg-warning/10 px-2 py-0.5 text-xs text-warning">
                        {item}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
