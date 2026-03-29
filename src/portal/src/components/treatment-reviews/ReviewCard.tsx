"use client";

import { useState } from "react";
import { Pill, Brain, Leaf } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import {
  DialogRoot,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogTrigger,
} from "@/components/ui/dialog";
import type { TreatmentReview } from "@/hooks/useTreatmentReviews";

interface ReviewCardProps {
  review: TreatmentReview;
  onApprove: (id: string, notes?: string) => void;
  onReject: (id: string, reason: string) => void;
  isApproving?: boolean;
  isRejecting?: boolean;
}

function getUrgencyVariant(urgency: string) {
  switch (urgency) {
    case "High":
      return "error" as const;
    case "Medium":
      return "warning" as const;
    case "Low":
      return "outline" as const;
    default:
      return "outline" as const;
  }
}

export function ReviewCard({
  review,
  onApprove,
  onReject,
  isApproving,
  isRejecting,
}: ReviewCardProps) {
  const [rejectReason, setRejectReason] = useState("");
  const [approveNotes, setApproveNotes] = useState("");
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [showApproveDialog, setShowApproveDialog] = useState(false);

  return (
    <div className="rounded-lg border border-border bg-white p-5">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-light text-sm font-medium text-primary">
            {review.patientName.split(" ").map((n) => n[0]).join("")}
          </div>
          <div>
            <h4 className="text-sm font-medium text-textPrimary">
              {review.patientName}
            </h4>
            <p className="text-xs text-textSecondary">{review.diagnosis}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant={getUrgencyVariant(review.urgency)}>
            {review.urgency}
          </Badge>
          <span className="text-xs text-textTertiary">
            {new Date(review.submittedAt).toLocaleDateString("en-US", {
              month: "short",
              day: "numeric",
            })}
          </span>
        </div>
      </div>

      {/* Proposed Plan */}
      <div className="space-y-3 mb-4">
        <h5 className="text-xs font-semibold text-textSecondary uppercase tracking-wide">
          Proposed Treatment: {review.proposedPlan.name}
        </h5>

        {review.proposedPlan.medications.length > 0 && (
          <div className="flex items-start gap-2">
            <Pill className="h-4 w-4 text-primary mt-0.5 shrink-0" />
            <div className="flex flex-wrap gap-1">
              {review.proposedPlan.medications.map((med) => (
                <span
                  key={med}
                  className="rounded-full bg-primary-light px-2 py-0.5 text-xs text-primary"
                >
                  {med}
                </span>
              ))}
            </div>
          </div>
        )}

        {review.proposedPlan.behavioralInterventions.length > 0 && (
          <div className="flex items-start gap-2">
            <Brain className="h-4 w-4 text-success mt-0.5 shrink-0" />
            <div className="flex flex-wrap gap-1">
              {review.proposedPlan.behavioralInterventions.map((item) => (
                <span
                  key={item}
                  className="rounded-full bg-success/10 px-2 py-0.5 text-xs text-success"
                >
                  {item}
                </span>
              ))}
            </div>
          </div>
        )}

        {review.proposedPlan.environmentalInterventions.length > 0 && (
          <div className="flex items-start gap-2">
            <Leaf className="h-4 w-4 text-warning mt-0.5 shrink-0" />
            <div className="flex flex-wrap gap-1">
              {review.proposedPlan.environmentalInterventions.map((item) => (
                <span
                  key={item}
                  className="rounded-full bg-warning/10 px-2 py-0.5 text-xs text-warning"
                >
                  {item}
                </span>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Actions */}
      {review.status === "Pending" && (
        <div className="flex gap-3">
          {/* Approve Dialog */}
          <DialogRoot open={showApproveDialog} onOpenChange={setShowApproveDialog}>
            <DialogTrigger asChild>
              <Button className="bg-success hover:bg-success/90">
                Approve
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Approve Treatment Plan</DialogTitle>
                <DialogDescription>
                  Approve the proposed treatment plan for {review.patientName}.
                  You can optionally add notes.
                </DialogDescription>
              </DialogHeader>
              <Textarea
                placeholder="Add optional notes..."
                value={approveNotes}
                onChange={(e) => setApproveNotes(e.target.value)}
              />
              <DialogFooter>
                <Button
                  variant="outline"
                  onClick={() => setShowApproveDialog(false)}
                >
                  Cancel
                </Button>
                <Button
                  className="bg-success hover:bg-success/90"
                  disabled={isApproving}
                  onClick={() => {
                    onApprove(review.id, approveNotes || undefined);
                    setShowApproveDialog(false);
                    setApproveNotes("");
                  }}
                >
                  {isApproving ? "Approving..." : "Confirm Approval"}
                </Button>
              </DialogFooter>
            </DialogContent>
          </DialogRoot>

          {/* Reject Dialog */}
          <DialogRoot open={showRejectDialog} onOpenChange={setShowRejectDialog}>
            <DialogTrigger asChild>
              <Button variant="destructive">Reject</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Reject Treatment Plan</DialogTitle>
                <DialogDescription>
                  Please provide a reason for rejecting the treatment plan for{" "}
                  {review.patientName}.
                </DialogDescription>
              </DialogHeader>
              <Textarea
                placeholder="Reason for rejection (required)..."
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
              />
              <DialogFooter>
                <Button
                  variant="outline"
                  onClick={() => setShowRejectDialog(false)}
                >
                  Cancel
                </Button>
                <Button
                  variant="destructive"
                  disabled={!rejectReason.trim() || isRejecting}
                  onClick={() => {
                    onReject(review.id, rejectReason);
                    setShowRejectDialog(false);
                    setRejectReason("");
                  }}
                >
                  {isRejecting ? "Rejecting..." : "Confirm Rejection"}
                </Button>
              </DialogFooter>
            </DialogContent>
          </DialogRoot>
        </div>
      )}

      {review.status !== "Pending" && (
        <Badge variant={review.status === "Approved" ? "success" : "error"}>
          {review.status}
        </Badge>
      )}
    </div>
  );
}
