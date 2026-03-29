"use client";

import { useQuery } from "@tanstack/react-query";
import { apiGet } from "@/services/api";

export interface PatientSummary {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl?: string;
  lastScanDate: string | null;
  overallScore: number | null;
  status: "Active" | "Flagged" | "Stable" | "Urgent";
}

export interface ScanResult {
  id: string;
  capturedAt: string;
  rednessScore: number;
  tearFilmScore: number;
  overallScore: number;
  imageUrl: string;
  status: string;
}

export interface Diagnosis {
  id: string;
  condition: string;
  confidence: number;
  severity: "Mild" | "Moderate" | "Severe";
  diagnosedAt: string;
}

export interface TreatmentPlan {
  id: string;
  name: string;
  status: "Active" | "Completed" | "Pending";
  startDate: string;
  endDate?: string;
  medications: string[];
  behavioralInterventions: string[];
  environmentalInterventions: string[];
}

export interface ClinicalNote {
  id: string;
  authorName: string;
  content: string;
  createdAt: string;
}

export interface PatientDetail {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  dateOfBirth: string;
  avatarUrl?: string;
  status: "Active" | "Flagged" | "Stable" | "Urgent";
  overallScore: number | null;
  scans: ScanResult[];
  diagnoses: Diagnosis[];
  treatmentPlans: TreatmentPlan[];
  notes: ClinicalNote[];
}

export function usePatientList(search?: string, status?: string) {
  const params = new URLSearchParams();
  if (search) params.set("search", search);
  if (status && status !== "All") params.set("status", status);
  const query = params.toString();

  return useQuery<PatientSummary[]>({
    queryKey: ["patients", search, status],
    queryFn: () =>
      apiGet<PatientSummary[]>(
        `/api/clinical/patients${query ? `?${query}` : ""}`
      ),
  });
}

export function usePatientDetail(id: string) {
  return useQuery<PatientDetail>({
    queryKey: ["patient", id],
    queryFn: () => apiGet<PatientDetail>(`/api/clinical/patients/${id}`),
    enabled: !!id,
  });
}
