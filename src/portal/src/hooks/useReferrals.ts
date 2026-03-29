"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost } from "@/services/api";

export interface Referral {
  id: string;
  patientId: string;
  patientName: string;
  condition: string;
  urgency: "Urgent" | "Standard";
  status: "Pending" | "Accepted" | "Declined";
  referredBy: string;
  referredAt: string;
  notes: string;
  diagnosticSummary: string;
  recommendedActions: string[];
}

export function useReferralList(urgency?: string) {
  const params = new URLSearchParams();
  if (urgency && urgency !== "All") params.set("urgency", urgency);
  const query = params.toString();

  return useQuery<Referral[]>({
    queryKey: ["referrals", urgency],
    queryFn: () =>
      apiGet<Referral[]>(
        `/api/clinical/referrals${query ? `?${query}` : ""}`
      ),
  });
}

export function useAcceptReferral() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (referralId: string) =>
      apiPost(`/api/clinical/referrals/${referralId}/accept`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["referrals"] });
    },
  });
}

export function useDeclineReferral() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      referralId,
      reason,
    }: {
      referralId: string;
      reason: string;
    }) =>
      apiPost(`/api/clinical/referrals/${referralId}/decline`, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["referrals"] });
    },
  });
}
