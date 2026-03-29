# Treatment Orchestration --- Detailed Design

## Overview

The Treatment Orchestration bounded context is the most clinically sensitive
domain in ClearEyeQ. It spans FR-300, FR-310, and FR-320, but it is implemented
as **clinician-supervised clinical decision support**, not autonomous care.
The context generates personalized treatment recommendations, tracks efficacy
over time, proposes evidence-backed adjustments, recommends escalation when
outcomes are insufficient, and supports maintenance and relapse prevention after
resolution.

A distinguishing capability is the Therapeutic Innovation Engine (FR-320), which
applies RAG-based literature synthesis over PubMed embeddings to generate novel
treatment hypotheses and compounding suggestions for clinician review. It may
surface options, but it does not place treatment into effect without an
explicit clinician decision.

## Clinical Safety Guardrails

1. **No autonomous prescribing** --- medication initiation, discontinuation,
   dose changes, compounding orders, and specialist-escalation decisions require
   clinician approval before activation.
2. **Guardrailed automation only** --- low-risk behavioral or environmental
   interventions may be scheduled automatically only if they were already
   approved as part of an active treatment plan.
3. **Human-verifiable evidence** --- every recommendation and adjustment
   includes the triggering evidence, contraindications considered, and model or
   ruleset version used to generate it.

## Responsibilities

- **Treatment Recommendation Generation** --- Upon receiving a
  `DiagnosisCompleted` event, assess severity, patient history, contraindications,
  and approved care pathways to assemble a draft multi-phase treatment plan.
- **Clinician Approval Workflow** --- Publish `TreatmentPlanProposed` and
  `TreatmentAdjustmentProposed` events so clinicians can approve or reject
  medication-affecting decisions in the Clinical Portal.
- **Guardrailed Behavioral Scheduling** --- Schedule non-pharmacological
  interventions such as screen breaks, hydration reminders, sleep hygiene, and
  environmental adjustments within the boundaries of an approved plan.
- **Novel Formulation Suggestions** --- Query a compounding pharmacy API to
  suggest custom formulations when standard treatments are insufficient or
  contraindicated. Suggestions remain advisory until clinician-approved.
- **Efficacy Tracking** --- Compute efficacy scores by comparing current
  scan/monitoring metrics against plan baselines, tracking improvement
  trajectories over time.
- **Adjustment Proposal Generation** --- When efficacy drops or risk rises,
  generate structured treatment-adjustment proposals rather than silently
  altering medication.
- **Escalation Recommendation** --- When efficacy thresholds are not met within
  configured timeframes, generate a referral package and publish
  `EscalationRecommended` for clinician triage.
- **30/60/90-Day Resolution Verification** --- Evaluate treatment outcomes at
  standard checkpoints to confirm resolution or recommend continuation.
- **Relapse Prevention** --- After resolution, maintain a lightweight approved
  monitoring and behavioral plan to detect early regression signals.
- **Cross-Patient De-identified Learning** --- Aggregate de-identified treatment
  outcome data to improve protocol selection for future patients.
- **Hypothesis Generation & Literature Synthesis** --- Use RAG over PubMed
  vector embeddings to surface relevant research findings and novel therapeutic
  hypotheses for clinician review.

## Boundaries

| In Scope | Out of Scope |
|----------|--------------|
| Draft treatment recommendation generation | Clinical diagnosis (Diagnostic Engine) |
| Clinician approval orchestration for plans and medication changes | Autonomous prescribing or medication dose changes |
| Guardrailed behavioral/environmental automations | Image capture and scoring (Scan Engine) |
| Efficacy tracking and resolution verification | Passive biometric monitoring (Passive Monitoring) |
| Specialist escalation recommendation and referral packaging | Predictive forecasting (Predictive Engine) |
| Compounding suggestion queries | Push notification delivery (Notifications & Alerts) |
| PubMed RAG and hypothesis generation | Billing and subscription gating (Subscription & Billing) |
| De-identified outcome learning | FHIR export mechanics (FHIR Interoperability) |

## Treatment Lifecycle

1. **Recommendation Drafting** --- A `DiagnosisCompleted` event triggers
   assessment of severity, history, contraindications, and available therapies.
   A draft `TreatmentPlan` is generated in `PendingApproval` status.
2. **Clinician Review** --- The service publishes `TreatmentPlanProposed`. A
   clinician reviews the rationale, evidence, and safety checks in the Clinical
   Portal and either approves or rejects the recommendation.
3. **Plan Activation** --- On `TreatmentPlanApproved`, the Treatment context
   activates the plan and publishes `TreatmentPlanActivated` for downstream
   consumers such as Notifications, Clinical Portal projections, and FHIR.
4. **Closed-Loop Monitoring** --- Incoming `ScanCompleted` or
   `ForecastGenerated` events trigger efficacy computation. Low-risk behavioral
   changes already covered by approved guardrails may be applied automatically.
5. **Adjustment Review** --- Medication-affecting changes produce
   `TreatmentAdjustmentProposed` and await clinician approval before any active
   plan is modified.
6. **Escalation Recommendation** --- If efficacy remains below threshold after
   the configured number of days, the system generates a referral package and
   publishes `EscalationRecommended`.
7. **Resolution and Maintenance** --- At 30-, 60-, and 90-day checkpoints, the
   system evaluates whether the condition has resolved. Successful resolution
   transitions the plan to maintenance mode and activates relapse prevention.

## Domain Concepts

| Concept | Description |
|---------|-------------|
| **TreatmentPlan** | Aggregate root representing a clinician-approved or pending treatment program for a diagnosed condition. |
| **TreatmentPhase** | A time-bounded stage within a plan containing a set of approved interventions. |
| **Intervention** | Abstract base for a therapeutic action: medication, behavioral, or environmental. |
| **ApprovalDecision** | Value object capturing clinician approval or rejection, rationale, and decision timestamp. |
| **SafetyGuardrail** | Value object encoding whether an intervention may be auto-applied and what boundaries are enforced. |
| **EfficacyMeasurement** | Value object capturing a point-in-time efficacy score against the plan baseline. |
| **EscalationRule** | Value object encoding the threshold, timeframe, and referral specialty for escalation review. |
| **TreatmentStatus** | Lifecycle enum: Draft, PendingApproval, Active, PendingAdjustmentApproval, EscalationRecommended, Resolved, Maintenance, Rejected. |

## Integration Points

| Direction | System | Protocol | Payload |
|-----------|--------|----------|---------|
| Inbound | Diagnostic Engine | Service Bus | `DiagnosisCompleted` |
| Inbound | Scan Engine | Service Bus | `ScanCompleted` |
| Inbound | Predictive Engine | Service Bus | `ForecastGenerated` |
| Inbound | Clinical Portal | Service Bus | `TreatmentPlanApproved`, `TreatmentPlanRejected`, `TreatmentAdjustmentApproved`, `TreatmentAdjustmentRejected` |
| Outbound | Azure Service Bus | AMQP | `TreatmentPlanProposed`, `TreatmentPlanActivated`, `TreatmentAdjustmentProposed`, `InterventionAdjusted`, `EscalationRecommended` |
| Outbound | Compounding Pharmacy API | HTTPS REST | Formulation suggestions only |
| Outbound | Notifications & Alerts | Service Bus | Approved-plan and escalation notifications |
| Outbound | Clinical Portal | Service Bus | Treatment review queue and escalation recommendations |
| Internal | PostgreSQL + pgvector | SQL | PubMed embeddings for RAG |
| Internal | Cosmos DB | SDK | Treatment plans, efficacy measurements, outbox records |

## Reliability and Isolation

- **Tenant-rooted scope** --- every treatment plan, review decision, projection,
  and event envelope carries `TenantId`.
- **Cosmos partitioning** --- high-cardinality treatment documents use the
  synthetic partition key `TenantId|UserId`.
- **Transactional outbox** --- proposal, activation, adjustment, and escalation
  events are written to the local outbox in the same commit as the plan change.
- **Inbox deduplication** --- clinician-decision consumers and closed-loop
  processors persist processed-message markers before side effects.
- **Privacy erasure** --- operational treatment plans are deleted or
  crypto-shredded where legally permissible; immutable audit and safety records
  retain only irreversible subject tokens.

## Diagrams

### Domain Model
![Domain Model](class-domain-model.png)

### Closed-Loop Services
![Closed-Loop Services](class-closed-loop.png)

### Application Services
![Application Services](class-application-services.png)

### Treatment Generation Sequence
![Treatment Generation](seq-treatment-generation.png)

### Closed-Loop Optimization Sequence
![Closed-Loop Optimization](seq-closed-loop.png)

### Escalation Sequence
![Escalation](seq-escalation.png)

### C4 Component Diagram
![C4 Component](c4-component.png)
