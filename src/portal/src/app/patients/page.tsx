"use client";

import { useState } from "react";
import { PatientSearchBar } from "@/components/patients/PatientSearchBar";
import { PatientTable } from "@/components/patients/PatientTable";
import { usePatientList, type PatientSummary } from "@/hooks/usePatients";
import { Skeleton } from "@/components/ui/skeleton";

// Mock data used when API is not available
const mockPatients: PatientSummary[] = [
  {
    id: "p1",
    firstName: "Sarah",
    lastName: "Chen",
    email: "sarah.chen@email.com",
    lastScanDate: "2026-03-27T10:30:00Z",
    overallScore: 7.2,
    status: "Flagged",
  },
  {
    id: "p2",
    firstName: "Michael",
    lastName: "Park",
    email: "michael.park@email.com",
    lastScanDate: "2026-03-26T14:15:00Z",
    overallScore: 3.1,
    status: "Stable",
  },
  {
    id: "p3",
    firstName: "Emily",
    lastName: "Rodriguez",
    email: "emily.r@email.com",
    lastScanDate: "2026-03-28T08:45:00Z",
    overallScore: 2.1,
    status: "Stable",
  },
  {
    id: "p4",
    firstName: "David",
    lastName: "Kim",
    email: "david.kim@email.com",
    lastScanDate: "2026-03-25T16:20:00Z",
    overallScore: 8.5,
    status: "Urgent",
  },
  {
    id: "p5",
    firstName: "Lisa",
    lastName: "Johnson",
    email: "l.johnson@email.com",
    lastScanDate: "2026-03-24T11:00:00Z",
    overallScore: 4.7,
    status: "Active",
  },
  {
    id: "p6",
    firstName: "Alex",
    lastName: "Martinez",
    email: "alex.m@email.com",
    lastScanDate: null,
    overallScore: null,
    status: "Active",
  },
];

export default function PatientsPage() {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");

  const { data, isLoading, isError } = usePatientList(search, statusFilter);

  // Use mock data as fallback
  const patients = data ?? mockPatients;

  // Client-side filter for mock data
  const filtered = patients.filter((p) => {
    const matchesSearch =
      !search ||
      `${p.firstName} ${p.lastName}`.toLowerCase().includes(search.toLowerCase()) ||
      p.email.toLowerCase().includes(search.toLowerCase());
    const matchesStatus =
      statusFilter === "All" || p.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6">
      <PatientSearchBar
        search={search}
        onSearchChange={setSearch}
        activeFilter={statusFilter}
        onFilterChange={setStatusFilter}
      />

      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-16 w-full" />
          ))}
        </div>
      ) : (
        <PatientTable patients={filtered} />
      )}
    </div>
  );
}
