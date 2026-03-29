"use client";

import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { Diagnosis } from "@/hooks/usePatients";

interface DiagnosisSummaryProps {
  diagnoses: Diagnosis[];
}

function getSeverityVariant(severity: string) {
  switch (severity) {
    case "Mild":
      return "success" as const;
    case "Moderate":
      return "warning" as const;
    case "Severe":
      return "error" as const;
    default:
      return "outline" as const;
  }
}

export function DiagnosisSummary({ diagnoses }: DiagnosisSummaryProps) {
  if (diagnoses.length === 0) {
    return (
      <p className="text-sm text-textTertiary">No diagnoses on record</p>
    );
  }

  return (
    <div className="space-y-4">
      {diagnoses.map((diagnosis) => (
        <div
          key={diagnosis.id}
          className="rounded-lg border border-border p-4"
        >
          <div className="flex items-center justify-between mb-3">
            <h4 className="text-sm font-medium text-textPrimary">
              {diagnosis.condition}
            </h4>
            <Badge variant={getSeverityVariant(diagnosis.severity)}>
              {diagnosis.severity}
            </Badge>
          </div>

          {/* Confidence bar */}
          <div className="space-y-1">
            <div className="flex items-center justify-between">
              <span className="text-xs text-textSecondary">Confidence</span>
              <span className="text-xs font-medium text-textPrimary">
                {(diagnosis.confidence * 100).toFixed(0)}%
              </span>
            </div>
            <div className="h-2 w-full overflow-hidden rounded-full bg-bgSurface">
              <div
                className={cn(
                  "h-full rounded-full transition-all",
                  diagnosis.confidence >= 0.8
                    ? "bg-success"
                    : diagnosis.confidence >= 0.5
                    ? "bg-warning"
                    : "bg-error"
                )}
                style={{ width: `${diagnosis.confidence * 100}%` }}
              />
            </div>
          </div>

          <p className="mt-2 text-xs text-textTertiary">
            Diagnosed on{" "}
            {new Date(diagnosis.diagnosedAt).toLocaleDateString("en-US", {
              month: "short",
              day: "numeric",
              year: "numeric",
            })}
          </p>
        </div>
      ))}
    </div>
  );
}
