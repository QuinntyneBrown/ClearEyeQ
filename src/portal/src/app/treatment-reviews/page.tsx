"use client";

import { useState } from "react";
import { ReviewQueue } from "@/components/treatment-reviews/ReviewQueue";
import {
  useReviewQueue,
  useApproveReview,
  useRejectReview,
  type TreatmentReview,
} from "@/hooks/useTreatmentReviews";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "@/components/ui/toast";

const mockReviews: TreatmentReview[] = [
  {
    id: "tr1",
    patientId: "p1",
    patientName: "Sarah Chen",
    diagnosis: "Allergic Conjunctivitis",
    proposedPlan: {
      name: "Enhanced Allergy Management",
      medications: ["Olopatadine 0.2%", "Prednisolone 0.12%", "Artificial Tears"],
      behavioralInterventions: [
        "Cold compress 3x daily",
        "Avoid allergen exposure",
        "Eye hygiene routine",
      ],
      environmentalInterventions: [
        "HEPA air purifier",
        "Hypoallergenic bedding",
        "Screen time reduction to 6hrs/day",
      ],
    },
    status: "Pending",
    submittedAt: "2026-03-29T09:00:00Z",
    urgency: "High",
  },
  {
    id: "tr2",
    patientId: "p4",
    patientName: "David Kim",
    diagnosis: "Chronic Dry Eye Syndrome",
    proposedPlan: {
      name: "Intensive Dry Eye Protocol",
      medications: ["Cyclosporine 0.05%", "Lifitegrast 5%"],
      behavioralInterventions: [
        "20-20-20 rule adherence",
        "Blink exercises",
        "Omega-3 supplementation",
      ],
      environmentalInterventions: [
        "Desktop humidifier",
        "Monitor distance adjustment",
        "Anti-glare screen filter",
      ],
    },
    status: "Pending",
    submittedAt: "2026-03-28T15:30:00Z",
    urgency: "Medium",
  },
  {
    id: "tr3",
    patientId: "p5",
    patientName: "Lisa Johnson",
    diagnosis: "Digital Eye Strain",
    proposedPlan: {
      name: "Screen Fatigue Reduction Plan",
      medications: ["Artificial Tears PRN"],
      behavioralInterventions: [
        "20-20-20 rule",
        "Regular breaks every 45 minutes",
        "Eye yoga exercises",
      ],
      environmentalInterventions: [
        "Blue light filtering glasses",
        "Ambient lighting adjustment",
        "Monitor height calibration",
      ],
    },
    status: "Pending",
    submittedAt: "2026-03-27T11:00:00Z",
    urgency: "Low",
  },
  {
    id: "tr4",
    patientId: "p2",
    patientName: "Michael Park",
    diagnosis: "Medication Adverse Reaction",
    proposedPlan: {
      name: "Alternative Treatment Protocol",
      medications: ["Ketotifen 0.025%", "Sodium Cromoglycate 2%"],
      behavioralInterventions: [
        "Daily symptom tracking",
        "Cold compress as needed",
      ],
      environmentalInterventions: [
        "Allergen-free environment assessment",
      ],
    },
    status: "Pending",
    submittedAt: "2026-03-29T07:15:00Z",
    urgency: "High",
  },
];

export default function TreatmentReviewsPage() {
  const [approvingId, setApprovingId] = useState<string | undefined>();
  const [rejectingId, setRejectingId] = useState<string | undefined>();

  const { data, isLoading } = useReviewQueue();
  const approveMutation = useApproveReview();
  const rejectMutation = useRejectReview();

  const reviews = data ?? mockReviews;

  function handleApprove(id: string, notes?: string) {
    setApprovingId(id);
    approveMutation.mutate(
      { reviewId: id, notes },
      {
        onSuccess: () => {
          toast({
            title: "Treatment plan approved",
            description: "The clinician has been notified.",
            variant: "success",
          });
          setApprovingId(undefined);
        },
        onError: () => {
          toast({ title: "Failed to approve", variant: "error" });
          setApprovingId(undefined);
        },
      }
    );
  }

  function handleReject(id: string, reason: string) {
    setRejectingId(id);
    rejectMutation.mutate(
      { reviewId: id, reason },
      {
        onSuccess: () => {
          toast({
            title: "Treatment plan rejected",
            description: "Feedback has been sent.",
            variant: "default",
          });
          setRejectingId(undefined);
        },
        onError: () => {
          toast({ title: "Failed to reject", variant: "error" });
          setRejectingId(undefined);
        },
      }
    );
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-48 w-full" />
        ))}
      </div>
    );
  }

  return (
    <ReviewQueue
      reviews={reviews}
      onApprove={handleApprove}
      onReject={handleReject}
      approvingId={approvingId}
      rejectingId={rejectingId}
    />
  );
}
