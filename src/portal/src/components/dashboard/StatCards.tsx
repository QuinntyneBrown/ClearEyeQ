"use client";

import { Users, AlertTriangle, Inbox, ClipboardCheck, TrendingUp, TrendingDown } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface StatCardData {
  label: string;
  value: string | number;
  icon: React.ComponentType<{ className?: string }>;
  trend?: { value: number; isPositive: boolean };
  iconColor: string;
  iconBg: string;
}

interface StatCardsProps {
  totalPatients: number;
  flaggedCases: number;
  pendingReferrals: number;
  treatmentReviews: number;
}

export function StatCards({
  totalPatients,
  flaggedCases,
  pendingReferrals,
  treatmentReviews,
}: StatCardsProps) {
  const stats: StatCardData[] = [
    {
      label: "Total Patients",
      value: totalPatients,
      icon: Users,
      trend: { value: 12, isPositive: true },
      iconColor: "text-primary",
      iconBg: "bg-primary-light",
    },
    {
      label: "Flagged Cases",
      value: flaggedCases,
      icon: AlertTriangle,
      trend: { value: 3, isPositive: false },
      iconColor: "text-error",
      iconBg: "bg-error/10",
    },
    {
      label: "Pending Referrals",
      value: pendingReferrals,
      icon: Inbox,
      trend: { value: 5, isPositive: true },
      iconColor: "text-warning",
      iconBg: "bg-warning/10",
    },
    {
      label: "Treatment Reviews",
      value: treatmentReviews,
      icon: ClipboardCheck,
      trend: { value: 2, isPositive: false },
      iconColor: "text-success",
      iconBg: "bg-success/10",
    },
  ];

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {stats.map((stat) => {
        const Icon = stat.icon;
        return (
          <Card key={stat.label}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div className={cn("rounded-md p-2", stat.iconBg)}>
                  <Icon className={cn("h-5 w-5", stat.iconColor)} />
                </div>
                {stat.trend && (
                  <div
                    className={cn(
                      "flex items-center gap-1 text-xs font-medium",
                      stat.trend.isPositive ? "text-success" : "text-error"
                    )}
                  >
                    {stat.trend.isPositive ? (
                      <TrendingUp className="h-3 w-3" />
                    ) : (
                      <TrendingDown className="h-3 w-3" />
                    )}
                    {stat.trend.value}%
                  </div>
                )}
              </div>
              <div className="mt-4">
                <p className="text-2xl font-bold text-textPrimary">{stat.value}</p>
                <p className="text-sm text-textSecondary">{stat.label}</p>
              </div>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
