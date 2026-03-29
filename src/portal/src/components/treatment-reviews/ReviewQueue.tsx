"use client";

import { useState } from "react";
import { ReviewCard } from "./ReviewCard";
import type { TreatmentReview } from "@/hooks/useTreatmentReviews";

interface ReviewQueueProps {
  reviews: TreatmentReview[];
  onApprove: (id: string, notes?: string) => void;
  onReject: (id: string, reason: string) => void;
  approvingId?: string;
  rejectingId?: string;
}

type SortKey = "date" | "urgency";

const urgencyOrder: Record<string, number> = {
  High: 0,
  Medium: 1,
  Low: 2,
};

export function ReviewQueue({
  reviews,
  onApprove,
  onReject,
  approvingId,
  rejectingId,
}: ReviewQueueProps) {
  const [sortBy, setSortBy] = useState<SortKey>("date");

  const sorted = [...reviews].sort((a, b) => {
    if (sortBy === "urgency") {
      return (urgencyOrder[a.urgency] ?? 2) - (urgencyOrder[b.urgency] ?? 2);
    }
    return new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime();
  });

  return (
    <div className="space-y-4">
      {/* Sort Controls */}
      <div className="flex items-center gap-2">
        <span className="text-sm text-textSecondary">Sort by:</span>
        <button
          onClick={() => setSortBy("date")}
          className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
            sortBy === "date"
              ? "bg-primary text-white"
              : "bg-bgSurface text-textSecondary hover:bg-border/50"
          }`}
        >
          Date
        </button>
        <button
          onClick={() => setSortBy("urgency")}
          className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
            sortBy === "urgency"
              ? "bg-primary text-white"
              : "bg-bgSurface text-textSecondary hover:bg-border/50"
          }`}
        >
          Urgency
        </button>
      </div>

      {/* Review Cards */}
      {sorted.length === 0 ? (
        <div className="flex h-40 items-center justify-center rounded-lg border border-border bg-bgSurface">
          <p className="text-sm text-textTertiary">
            No treatment reviews pending
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {sorted.map((review) => (
            <ReviewCard
              key={review.id}
              review={review}
              onApprove={onApprove}
              onReject={onReject}
              isApproving={approvingId === review.id}
              isRejecting={rejectingId === review.id}
            />
          ))}
        </div>
      )}
    </div>
  );
}
