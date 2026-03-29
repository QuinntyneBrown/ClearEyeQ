"use client";

import Link from "next/link";
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { PatientSummary } from "@/hooks/usePatients";

interface PatientTableProps {
  patients: PatientSummary[];
}

function getStatusVariant(status: string) {
  switch (status) {
    case "Flagged":
      return "error" as const;
    case "Urgent":
      return "error" as const;
    case "Stable":
      return "success" as const;
    case "Active":
      return "default" as const;
    default:
      return "outline" as const;
  }
}

function getScoreColor(score: number | null): string {
  if (score === null) return "text-textTertiary";
  if (score <= 3) return "text-success";
  if (score <= 6) return "text-warning";
  return "text-error";
}

function formatDate(dateStr: string | null): string {
  if (!dateStr) return "No scans";
  const date = new Date(dateStr);
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function PatientTable({ patients }: PatientTableProps) {
  if (patients.length === 0) {
    return (
      <div className="flex h-40 items-center justify-center rounded-lg border border-border bg-bgSurface">
        <p className="text-sm text-textTertiary">No patients found</p>
      </div>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Patient</TableHead>
          <TableHead>Last Scan</TableHead>
          <TableHead>Score</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="text-right">Action</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {patients.map((patient) => (
          <TableRow key={patient.id}>
            <TableCell>
              <div className="flex items-center gap-3">
                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary-light text-sm font-medium text-primary">
                  {patient.firstName.charAt(0)}
                  {patient.lastName.charAt(0)}
                </div>
                <div>
                  <p className="font-medium text-textPrimary">
                    {patient.firstName} {patient.lastName}
                  </p>
                  <p className="text-xs text-textSecondary">{patient.email}</p>
                </div>
              </div>
            </TableCell>
            <TableCell>
              <span className="text-sm text-textSecondary">
                {formatDate(patient.lastScanDate)}
              </span>
            </TableCell>
            <TableCell>
              <span
                className={cn(
                  "text-sm font-semibold",
                  getScoreColor(patient.overallScore)
                )}
              >
                {patient.overallScore !== null
                  ? patient.overallScore.toFixed(1)
                  : "--"}
              </span>
            </TableCell>
            <TableCell>
              <Badge variant={getStatusVariant(patient.status)}>
                {patient.status}
              </Badge>
            </TableCell>
            <TableCell className="text-right">
              <Link
                href={`/patients/${patient.id}`}
                className="text-sm font-medium text-primary hover:underline"
              >
                View
              </Link>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
