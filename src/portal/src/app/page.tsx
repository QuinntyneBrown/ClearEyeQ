"use client";

import { StatCards } from "@/components/dashboard/StatCards";
import { RecentActivity } from "@/components/dashboard/RecentActivity";

const mockActivities = [
  {
    id: "1",
    type: "referral" as const,
    description: "New urgent referral received for Sarah Chen — suspected allergic conjunctivitis",
    timestamp: "10 minutes ago",
  },
  {
    id: "2",
    type: "review" as const,
    description: "Treatment plan approved for Michael Park — dry eye management",
    timestamp: "25 minutes ago",
  },
  {
    id: "3",
    type: "scan" as const,
    description: "New scan results available for Emily Rodriguez — redness score improved to 2.1",
    timestamp: "1 hour ago",
  },
  {
    id: "4",
    type: "referral" as const,
    description: "Standard referral received for David Kim — chronic dry eye follow-up",
    timestamp: "2 hours ago",
  },
  {
    id: "5",
    type: "note" as const,
    description: "Clinical note added for Lisa Johnson by Dr. Thompson",
    timestamp: "3 hours ago",
  },
  {
    id: "6",
    type: "scan" as const,
    description: "Scan completed for Alex Martinez — tear film metrics captured",
    timestamp: "4 hours ago",
  },
];

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <StatCards
        totalPatients={1284}
        flaggedCases={23}
        pendingReferrals={8}
        treatmentReviews={5}
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <RecentActivity activities={mockActivities.slice(0, 3)} />
        <RecentActivity activities={mockActivities.slice(3)} />
      </div>
    </div>
  );
}
