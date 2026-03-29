"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost } from "@/services/api";

export interface TreatmentReview {
  id: string;
  patientId: string;
  patientName: string;
  diagnosis: string;
  proposedPlan: {
    name: string;
    medications: string[];
    behavioralInterventions: string[];
    environmentalInterventions: string[];
  };
  status: "Pending" | "Approved" | "Rejected";
  submittedAt: string;
  urgency: "High" | "Medium" | "Low";
}

export function useReviewQueue() {
  return useQuery<TreatmentReview[]>({
    queryKey: ["treatment-reviews"],
    queryFn: () => apiGet<TreatmentReview[]>("/api/clinical/treatment-reviews"),
  });
}

export function useApproveReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      reviewId,
      notes,
    }: {
      reviewId: string;
      notes?: string;
    }) =>
      apiPost(`/api/clinical/treatment-reviews/${reviewId}/approve`, {
        notes,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["treatment-reviews"] });
    },
  });
}

export function useRejectReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      reviewId,
      reason,
    }: {
      reviewId: string;
      reason: string;
    }) =>
      apiPost(`/api/clinical/treatment-reviews/${reviewId}/reject`, {
        reason,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["treatment-reviews"] });
    },
  });
}
