"use client";

import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { ReferralCard } from "./ReferralCard";
import type { Referral } from "@/hooks/useReferrals";

interface ReferralListProps {
  referrals: Referral[];
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
  acceptingId?: string;
  decliningId?: string;
  activeTab: string;
  onTabChange: (tab: string) => void;
}

export function ReferralList({
  referrals,
  onAccept,
  onDecline,
  acceptingId,
  decliningId,
  activeTab,
  onTabChange,
}: ReferralListProps) {
  const filtered =
    activeTab === "All"
      ? referrals
      : referrals.filter((r) => r.urgency === activeTab);

  return (
    <div className="space-y-4">
      <Tabs value={activeTab} onValueChange={onTabChange}>
        <TabsList>
          <TabsTrigger value="All">
            All ({referrals.length})
          </TabsTrigger>
          <TabsTrigger value="Urgent">
            Urgent ({referrals.filter((r) => r.urgency === "Urgent").length})
          </TabsTrigger>
          <TabsTrigger value="Standard">
            Standard ({referrals.filter((r) => r.urgency === "Standard").length})
          </TabsTrigger>
        </TabsList>
      </Tabs>

      <div className="space-y-3">
        {filtered.length === 0 ? (
          <div className="flex h-40 items-center justify-center rounded-lg border border-border bg-bgSurface">
            <p className="text-sm text-textTertiary">No referrals found</p>
          </div>
        ) : (
          filtered.map((referral) => (
            <ReferralCard
              key={referral.id}
              referral={referral}
              onAccept={onAccept}
              onDecline={onDecline}
              isAccepting={acceptingId === referral.id}
              isDeclining={decliningId === referral.id}
            />
          ))
        )}
      </div>
    </div>
  );
}
