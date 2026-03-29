"use client";

import { useState } from "react";
import { ChevronDown, ChevronUp, Clock } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { Referral } from "@/hooks/useReferrals";

interface ReferralCardProps {
  referral: Referral;
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
  isAccepting?: boolean;
  isDeclining?: boolean;
}

export function ReferralCard({
  referral,
  onAccept,
  onDecline,
  isAccepting,
  isDeclining,
}: ReferralCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  return (
    <div className="rounded-lg border border-border bg-white overflow-hidden">
      {/* Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="flex w-full items-center justify-between p-4 text-left hover:bg-bgSurface/50 transition-colors"
      >
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-light text-sm font-medium text-primary">
            {referral.patientName.split(" ").map((n) => n[0]).join("")}
          </div>
          <div>
            <h4 className="text-sm font-medium text-textPrimary">
              {referral.patientName}
            </h4>
            <p className="text-xs text-textSecondary">{referral.condition}</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <Badge variant={referral.urgency === "Urgent" ? "error" : "warning"}>
            {referral.urgency}
          </Badge>
          <div className="flex items-center gap-1 text-xs text-textTertiary">
            <Clock className="h-3 w-3" />
            {new Date(referral.referredAt).toLocaleDateString("en-US", {
              month: "short",
              day: "numeric",
              hour: "numeric",
              minute: "2-digit",
            })}
          </div>
          {isExpanded ? (
            <ChevronUp className="h-4 w-4 text-textTertiary" />
          ) : (
            <ChevronDown className="h-4 w-4 text-textTertiary" />
          )}
        </div>
      </button>

      {/* Expanded Content */}
      {isExpanded && (
        <div className="border-t border-border p-4 space-y-4">
          <div>
            <h5 className="text-xs font-medium text-textSecondary mb-1">
              Referred By
            </h5>
            <p className="text-sm text-textPrimary">{referral.referredBy}</p>
          </div>

          <div>
            <h5 className="text-xs font-medium text-textSecondary mb-1">
              Diagnostic Summary
            </h5>
            <p className="text-sm text-textSecondary">
              {referral.diagnosticSummary}
            </p>
          </div>

          {referral.notes && (
            <div>
              <h5 className="text-xs font-medium text-textSecondary mb-1">
                Notes
              </h5>
              <p className="text-sm text-textSecondary">{referral.notes}</p>
            </div>
          )}

          {referral.recommendedActions.length > 0 && (
            <div>
              <h5 className="text-xs font-medium text-textSecondary mb-1">
                Recommended Actions
              </h5>
              <ul className="list-disc list-inside space-y-1">
                {referral.recommendedActions.map((action, i) => (
                  <li key={i} className="text-sm text-textSecondary">
                    {action}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {referral.status === "Pending" && (
            <div className="flex gap-3 pt-2">
              <Button
                onClick={() => onAccept(referral.id)}
                disabled={isAccepting}
                className="bg-success hover:bg-success/90"
              >
                {isAccepting ? "Accepting..." : "Accept Referral"}
              </Button>
              <Button
                variant="destructive"
                onClick={() => onDecline(referral.id)}
                disabled={isDeclining}
              >
                {isDeclining ? "Declining..." : "Decline"}
              </Button>
            </div>
          )}

          {referral.status !== "Pending" && (
            <Badge variant={referral.status === "Accepted" ? "success" : "error"}>
              {referral.status}
            </Badge>
          )}
        </div>
      )}
    </div>
  );
}
