"use client";

import { useState } from "react";
import { ReferralList } from "@/components/referrals/ReferralList";
import {
  useReferralList,
  useAcceptReferral,
  useDeclineReferral,
  type Referral,
} from "@/hooks/useReferrals";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "@/components/ui/toast";

const mockReferrals: Referral[] = [
  {
    id: "r1",
    patientId: "p1",
    patientName: "Sarah Chen",
    condition: "Allergic Conjunctivitis",
    urgency: "Urgent",
    status: "Pending",
    referredBy: "Dr. Williams (Primary Care)",
    referredAt: "2026-03-29T08:15:00Z",
    notes: "Patient experiencing acute flare-up with significant discomfort. Requires specialist evaluation.",
    diagnosticSummary: "Bilateral eye redness with conjunctival injection, tearing, and itching. Redness score 7.5. Elevated IgE levels. Seasonal allergen exposure confirmed.",
    recommendedActions: [
      "Comprehensive eye examination",
      "Allergy panel testing",
      "Consider topical antihistamine therapy",
      "Evaluate for corticosteroid treatment if severe",
    ],
  },
  {
    id: "r2",
    patientId: "p4",
    patientName: "David Kim",
    condition: "Chronic Dry Eye",
    urgency: "Standard",
    status: "Pending",
    referredBy: "Dr. Garcia (Optometry)",
    referredAt: "2026-03-28T14:30:00Z",
    notes: "Ongoing dry eye symptoms despite initial treatment. Follow-up recommended.",
    diagnosticSummary: "Persistent dry eye with reduced tear film break-up time (4 seconds). Meibomian gland dysfunction suspected. Current artificial tears providing insufficient relief.",
    recommendedActions: [
      "Tear film analysis",
      "Meibomian gland evaluation",
      "Consider punctal plugs",
      "Review current medication regimen",
    ],
  },
  {
    id: "r3",
    patientId: "p5",
    patientName: "Lisa Johnson",
    condition: "Recurring Eye Irritation",
    urgency: "Standard",
    status: "Pending",
    referredBy: "Dr. Martinez (Family Medicine)",
    referredAt: "2026-03-27T10:00:00Z",
    notes: "Patient reports recurring episodes of eye irritation, potentially related to screen time.",
    diagnosticSummary: "Intermittent eye redness and fatigue symptoms. Computer Vision Syndrome suspected. Average screen time 10+ hours/day.",
    recommendedActions: [
      "Digital eye strain assessment",
      "Environmental factor evaluation",
      "Screen time management plan",
    ],
  },
  {
    id: "r4",
    patientId: "p2",
    patientName: "Michael Park",
    condition: "Post-Treatment Follow-Up",
    urgency: "Urgent",
    status: "Pending",
    referredBy: "Dr. Lee (Ophthalmology)",
    referredAt: "2026-03-29T06:45:00Z",
    notes: "Urgent follow-up needed. Patient showing unexpected reaction to prescribed medication.",
    diagnosticSummary: "Adverse reaction to olopatadine — periorbital swelling and increased redness noted 48 hours after starting treatment. Medication discontinued.",
    recommendedActions: [
      "Immediate clinical assessment",
      "Alternative treatment evaluation",
      "Document adverse reaction",
      "Consider patch testing",
    ],
  },
];

export default function ReferralsPage() {
  const [activeTab, setActiveTab] = useState("All");
  const [acceptingId, setAcceptingId] = useState<string | undefined>();
  const [decliningId, setDecliningId] = useState<string | undefined>();

  const { data, isLoading } = useReferralList();
  const acceptMutation = useAcceptReferral();
  const declineMutation = useDeclineReferral();

  const referrals = data ?? mockReferrals;

  function handleAccept(id: string) {
    setAcceptingId(id);
    acceptMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Referral accepted", variant: "success" });
        setAcceptingId(undefined);
      },
      onError: () => {
        toast({ title: "Failed to accept referral", variant: "error" });
        setAcceptingId(undefined);
      },
    });
  }

  function handleDecline(id: string) {
    setDecliningId(id);
    declineMutation.mutate(
      { referralId: id, reason: "Not accepting at this time" },
      {
        onSuccess: () => {
          toast({ title: "Referral declined", variant: "default" });
          setDecliningId(undefined);
        },
        onError: () => {
          toast({ title: "Failed to decline referral", variant: "error" });
          setDecliningId(undefined);
        },
      }
    );
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-64" />
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-20 w-full" />
        ))}
      </div>
    );
  }

  return (
    <ReferralList
      referrals={referrals}
      onAccept={handleAccept}
      onDecline={handleDecline}
      acceptingId={acceptingId}
      decliningId={decliningId}
      activeTab={activeTab}
      onTabChange={setActiveTab}
    />
  );
}
