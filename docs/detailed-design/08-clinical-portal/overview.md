# 08 - Clinical Portal

## Purpose

The Clinical Portal is the clinician-facing web portal for ClearEyeQ. It
operates as both a read-model/BFF (Backend for Frontend) and the **human review
boundary** for clinically significant treatment decisions. The context projects
read models from upstream bounded contexts via Azure Service Bus event
subscriptions, owns clinician-authored notes and assignments, and manages the
review queues for treatment proposals, treatment-adjustment proposals, and
escalation recommendations.

## Bounded Context Ownership

| Owned Aggregates       | Projected Read Models                              |
|------------------------|----------------------------------------------------|
| PatientAssignment      | PatientSummaryReadModel (from Scan, Diag)          |
| ReferralCase           | ScanResultReadModel (from Scan)                    |
| ClinicalNote           | DiagnosticReadModel (from Diagnostic)              |
| TreatmentReviewTask    | TreatmentPlanReadModel (from Treatment)            |
|                        | TreatmentReviewReadModel (from Treatment proposals) |

## Key Capabilities

- **Patient List & Dashboard** -- aggregated view of assigned patients with latest scan results, diagnostic summaries, and treatment plans.
- **Treatment Review Queue** -- clinicians approve or reject `TreatmentPlanProposed` and `TreatmentAdjustmentProposed` events with explicit rationale.
- **Referral Inbox** -- AI-escalated cases surface here for clinician review; clinicians accept or decline referrals.
- **Clinical Notes** -- clinicians author and manage notes attached to patient encounters.
- **Read-Model Projections** -- event-driven projections keep local read models eventually consistent with upstream contexts.
- **Idempotent Projection Processing** -- every consumed integration event is deduplicated before projections or review-task side effects are applied.

## Technology Stack

| Layer          | Technology                              |
|----------------|-----------------------------------------|
| Frontend       | React 18 + TypeScript, TanStack Query   |
| BFF API        | ASP.NET Core 9 Minimal API              |
| Messaging      | Azure Service Bus (subscriptions)        |
| Read Store     | PostgreSQL                               |
| Cache          | Redis                                    |
| Real-time      | SignalR                                  |
| Auth           | Microsoft Identity, JWT Bearer           |

## Domain Model

![Domain Model](class-domain-model.png)

## Application Services

![Application Services](class-application-services.png)

## Referral Flow

![Referral Flow](seq-referral-flow.png)

## Component Diagram (C4)

![C4 Component Diagram](c4-component.png)

## Integration Events Consumed

| Event                        | Source Context | Projection Target          |
|------------------------------|----------------|----------------------------|
| ScanCompleted                | Scan           | ScanResultReadModel        |
| DiagnosisCompleted           | Diagnostic     | DiagnosticReadModel        |
| TreatmentPlanProposed        | Treatment      | TreatmentReviewTask        |
| TreatmentPlanActivated       | Treatment      | TreatmentPlanReadModel     |
| TreatmentAdjustmentProposed  | Treatment      | TreatmentReviewTask        |
| EscalationRecommended        | Treatment      | ReferralCase (created)     |
| PatientRegistered            | Identity       | PatientSummaryReadModel    |

## Integration Events Published

| Event                        | Description                              |
|------------------------------|------------------------------------------|
| TreatmentPlanApproved        | Clinician approved a proposed treatment plan |
| TreatmentPlanRejected        | Clinician rejected a proposed treatment plan |
| TreatmentAdjustmentApproved  | Clinician approved a proposed adjustment |
| TreatmentAdjustmentRejected  | Clinician rejected a proposed adjustment |
| ReferralAccepted             | Clinician accepted an AI-escalated case  |
| ReferralDeclined             | Clinician declined a referral            |
| ClinicalNoteAdded            | A new clinical note was recorded         |
