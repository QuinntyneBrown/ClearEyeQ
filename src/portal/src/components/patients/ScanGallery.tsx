"use client";

import { useState } from "react";
import { Eye, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { ScanResult } from "@/hooks/usePatients";

interface ScanGalleryProps {
  scans: ScanResult[];
}

function getScoreBadgeVariant(score: number) {
  if (score <= 3) return "success" as const;
  if (score <= 6) return "warning" as const;
  return "error" as const;
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function ScanGallery({ scans }: ScanGalleryProps) {
  const [expandedScan, setExpandedScan] = useState<ScanResult | null>(null);

  if (scans.length === 0) {
    return (
      <div className="flex h-40 items-center justify-center rounded-lg border border-border bg-bgSurface">
        <div className="text-center">
          <Eye className="mx-auto h-8 w-8 text-textTertiary" />
          <p className="mt-2 text-sm text-textTertiary">No scans recorded</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {scans.map((scan) => (
          <button
            key={scan.id}
            onClick={() => setExpandedScan(scan)}
            className="group relative overflow-hidden rounded-lg border border-border bg-bgSurface transition-shadow hover:shadow-md text-left"
          >
            {/* Placeholder scan image */}
            <div className="flex h-40 items-center justify-center bg-primary-light">
              <Eye className="h-12 w-12 text-primary/30" />
            </div>

            {/* Score overlay */}
            <div className="absolute top-2 right-2">
              <Badge variant={getScoreBadgeVariant(scan.overallScore)}>
                {scan.overallScore.toFixed(1)}
              </Badge>
            </div>

            {/* Info */}
            <div className="p-3">
              <p className="text-sm font-medium text-textPrimary">
                {formatDate(scan.capturedAt)}
              </p>
              <p className="text-xs text-textSecondary">
                Redness: {scan.rednessScore.toFixed(1)} | Tear Film: {scan.tearFilmScore.toFixed(1)}
              </p>
            </div>
          </button>
        ))}
      </div>

      {/* Expanded view */}
      {expandedScan && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="relative max-w-2xl w-full mx-4 rounded-lg bg-white p-6 shadow-xl">
            <button
              onClick={() => setExpandedScan(null)}
              className="absolute right-4 top-4 rounded-sm p-1 text-textSecondary hover:text-textPrimary"
            >
              <X className="h-5 w-5" />
            </button>

            <div className="flex h-64 items-center justify-center rounded-lg bg-primary-light mb-4">
              <Eye className="h-20 w-20 text-primary/30" />
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-textPrimary">
                  Scan - {formatDate(expandedScan.capturedAt)}
                </h3>
                <Badge variant={getScoreBadgeVariant(expandedScan.overallScore)}>
                  Overall: {expandedScan.overallScore.toFixed(1)}
                </Badge>
              </div>
              <div className="grid grid-cols-2 gap-4 mt-4">
                <div className="rounded-md bg-bgSurface p-3">
                  <p className="text-xs text-textSecondary">Redness Score</p>
                  <p className="text-lg font-semibold text-textPrimary">
                    {expandedScan.rednessScore.toFixed(1)}
                  </p>
                </div>
                <div className="rounded-md bg-bgSurface p-3">
                  <p className="text-xs text-textSecondary">Tear Film Score</p>
                  <p className="text-lg font-semibold text-textPrimary">
                    {expandedScan.tearFilmScore.toFixed(1)}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
