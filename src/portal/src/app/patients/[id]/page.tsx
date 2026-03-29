"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft, Eye } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { usePatientDetail, type PatientDetail } from "@/hooks/usePatients";
import { ScanGallery } from "@/components/patients/ScanGallery";
import { DiagnosisSummary } from "@/components/patients/DiagnosisSummary";
import { TreatmentTimeline } from "@/components/patients/TreatmentTimeline";
import { ClinicalNotes } from "@/components/patients/ClinicalNotes";
import { PatientTimeline } from "@/components/patients/PatientTimeline";
import { cn } from "@/lib/utils";

// Mock data for when API is unavailable
const mockPatient: PatientDetail = {
  id: "p1",
  firstName: "Sarah",
  lastName: "Chen",
  email: "sarah.chen@email.com",
  dateOfBirth: "1992-05-15",
  status: "Flagged",
  overallScore: 7.2,
  scans: [
    {
      id: "s1",
      capturedAt: "2026-03-27T10:30:00Z",
      rednessScore: 7.5,
      tearFilmScore: 6.8,
      overallScore: 7.2,
      imageUrl: "",
      status: "Completed",
    },
    {
      id: "s2",
      capturedAt: "2026-03-20T09:15:00Z",
      rednessScore: 6.2,
      tearFilmScore: 5.9,
      overallScore: 6.1,
      imageUrl: "",
      status: "Completed",
    },
    {
      id: "s3",
      capturedAt: "2026-03-13T14:45:00Z",
      rednessScore: 4.8,
      tearFilmScore: 5.1,
      overallScore: 5.0,
      imageUrl: "",
      status: "Completed",
    },
  ],
  diagnoses: [
    {
      id: "d1",
      condition: "Allergic Conjunctivitis",
      confidence: 0.92,
      severity: "Moderate",
      diagnosedAt: "2026-03-15T12:00:00Z",
    },
    {
      id: "d2",
      condition: "Dry Eye Syndrome",
      confidence: 0.78,
      severity: "Mild",
      diagnosedAt: "2026-03-15T12:00:00Z",
    },
    {
      id: "d3",
      condition: "Blepharitis",
      confidence: 0.45,
      severity: "Mild",
      diagnosedAt: "2026-03-15T12:00:00Z",
    },
  ],
  treatmentPlans: [
    {
      id: "t1",
      name: "Allergy Management Plan",
      status: "Active",
      startDate: "2026-03-16T00:00:00Z",
      medications: ["Olopatadine 0.1%", "Artificial Tears"],
      behavioralInterventions: ["Avoid rubbing eyes", "Cold compress 2x daily"],
      environmentalInterventions: ["HEPA air filter", "Reduce screen time"],
    },
    {
      id: "t2",
      name: "Initial Assessment Protocol",
      status: "Completed",
      startDate: "2026-03-01T00:00:00Z",
      endDate: "2026-03-15T00:00:00Z",
      medications: ["Artificial Tears PRN"],
      behavioralInterventions: ["Daily eye hygiene routine"],
      environmentalInterventions: [],
    },
  ],
  notes: [
    {
      id: "n1",
      authorName: "Dr. Thompson",
      content: "Patient reports increased redness and itching over the past week. Seasonal allergens likely contributing. Adjusting treatment plan.",
      createdAt: "2026-03-27T11:00:00Z",
    },
    {
      id: "n2",
      authorName: "Dr. Thompson",
      content: "Initial consultation completed. Patient presents with bilateral eye redness and intermittent tearing. Family history of allergies noted.",
      createdAt: "2026-03-01T10:30:00Z",
    },
  ],
};

function getStatusVariant(status: string) {
  switch (status) {
    case "Flagged": return "error" as const;
    case "Urgent": return "error" as const;
    case "Stable": return "success" as const;
    case "Active": return "default" as const;
    default: return "outline" as const;
  }
}

function getScoreColor(score: number | null): string {
  if (score === null) return "text-textTertiary";
  if (score <= 3) return "text-success";
  if (score <= 6) return "text-warning";
  return "text-error";
}

export default function PatientDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data, isLoading } = usePatientDetail(id);
  const patient = data ?? mockPatient;

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-48" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  const timelineEvents = [
    ...patient.scans.map((s) => ({
      id: s.id,
      type: "scan" as const,
      title: `Eye Scan (Score: ${s.overallScore.toFixed(1)})`,
      description: `Redness: ${s.rednessScore.toFixed(1)}, Tear Film: ${s.tearFilmScore.toFixed(1)}`,
      date: new Date(s.capturedAt).toLocaleDateString("en-US", { month: "short", day: "numeric" }),
    })),
    ...patient.diagnoses.map((d) => ({
      id: d.id,
      type: "diagnosis" as const,
      title: d.condition,
      description: `${d.severity} severity, ${(d.confidence * 100).toFixed(0)}% confidence`,
      date: new Date(d.diagnosedAt).toLocaleDateString("en-US", { month: "short", day: "numeric" }),
    })),
    ...patient.treatmentPlans.map((t) => ({
      id: t.id,
      type: "treatment" as const,
      title: t.name,
      description: `Status: ${t.status}`,
      date: new Date(t.startDate).toLocaleDateString("en-US", { month: "short", day: "numeric" }),
    })),
  ].sort((a, b) => b.date.localeCompare(a.date));

  return (
    <div className="space-y-6">
      {/* Back Link */}
      <Link
        href="/patients"
        className="inline-flex items-center gap-1 text-sm text-textSecondary hover:text-textPrimary transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Patients
      </Link>

      {/* Patient Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary-light text-lg font-semibold text-primary">
            {patient.firstName.charAt(0)}
            {patient.lastName.charAt(0)}
          </div>
          <div>
            <div className="flex items-center gap-3">
              <h2 className="text-2xl font-bold text-textPrimary">
                {patient.firstName} {patient.lastName}
              </h2>
              <Badge variant={getStatusVariant(patient.status)}>
                {patient.status}
              </Badge>
            </div>
            <p className="text-sm text-textSecondary">{patient.email}</p>
          </div>
        </div>

        {patient.overallScore !== null && (
          <div className="text-right">
            <p className="text-xs text-textSecondary">Overall Score</p>
            <p className={cn("text-3xl font-bold", getScoreColor(patient.overallScore))}>
              {patient.overallScore.toFixed(1)}
            </p>
          </div>
        )}
      </div>

      {/* Tabs */}
      <Tabs defaultValue="overview" className="w-full">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="scans">Scans</TabsTrigger>
          <TabsTrigger value="diagnosis">Diagnosis</TabsTrigger>
          <TabsTrigger value="treatment">Treatment</TabsTrigger>
          <TabsTrigger value="notes">Notes</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6 mt-4">
          <div className="grid gap-6 lg:grid-cols-2">
            {/* Latest Scan */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Latest Scan</CardTitle>
              </CardHeader>
              <CardContent>
                {patient.scans.length > 0 ? (
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <Eye className="h-5 w-5 text-primary" />
                        <span className="text-sm text-textSecondary">
                          {new Date(patient.scans[0].capturedAt).toLocaleDateString("en-US", {
                            month: "long",
                            day: "numeric",
                            year: "numeric",
                          })}
                        </span>
                      </div>
                      <span className={cn("text-lg font-bold", getScoreColor(patient.scans[0].overallScore))}>
                        {patient.scans[0].overallScore.toFixed(1)}
                      </span>
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <div className="rounded-md bg-bgSurface p-3">
                        <p className="text-xs text-textSecondary">Redness</p>
                        <p className="text-sm font-semibold">{patient.scans[0].rednessScore.toFixed(1)}</p>
                      </div>
                      <div className="rounded-md bg-bgSurface p-3">
                        <p className="text-xs text-textSecondary">Tear Film</p>
                        <p className="text-sm font-semibold">{patient.scans[0].tearFilmScore.toFixed(1)}</p>
                      </div>
                    </div>
                  </div>
                ) : (
                  <p className="text-sm text-textTertiary">No scans available</p>
                )}
              </CardContent>
            </Card>

            {/* Diagnosis Summary */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Primary Diagnosis</CardTitle>
              </CardHeader>
              <CardContent>
                {patient.diagnoses.length > 0 ? (
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <h4 className="font-medium">{patient.diagnoses[0].condition}</h4>
                      <Badge variant={getSeverityVariant(patient.diagnoses[0].severity)}>
                        {patient.diagnoses[0].severity}
                      </Badge>
                    </div>
                    <p className="text-sm text-textSecondary">
                      Confidence: {(patient.diagnoses[0].confidence * 100).toFixed(0)}%
                    </p>
                  </div>
                ) : (
                  <p className="text-sm text-textTertiary">No diagnoses on record</p>
                )}
              </CardContent>
            </Card>
          </div>

          {/* Active Treatment */}
          {patient.treatmentPlans.filter((t) => t.status === "Active").length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Active Treatment</CardTitle>
              </CardHeader>
              <CardContent>
                <TreatmentTimeline
                  plans={patient.treatmentPlans.filter((t) => t.status === "Active")}
                />
              </CardContent>
            </Card>
          )}

          {/* Timeline */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Health Timeline</CardTitle>
            </CardHeader>
            <CardContent>
              <PatientTimeline events={timelineEvents} />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="scans" className="mt-4">
          <ScanGallery scans={patient.scans} />
        </TabsContent>

        <TabsContent value="diagnosis" className="mt-4">
          <DiagnosisSummary diagnoses={patient.diagnoses} />
        </TabsContent>

        <TabsContent value="treatment" className="mt-4">
          <TreatmentTimeline plans={patient.treatmentPlans} />
        </TabsContent>

        <TabsContent value="notes" className="mt-4">
          <ClinicalNotes notes={patient.notes} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

function getSeverityVariant(severity: string) {
  switch (severity) {
    case "Mild": return "success" as const;
    case "Moderate": return "warning" as const;
    case "Severe": return "error" as const;
    default: return "outline" as const;
  }
}
