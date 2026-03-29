# ClearEyeQ — System Architecture

## Overview

ClearEyeQ is an AI-powered autonomous ocular health platform that transforms smartphone cameras into clinical-grade diagnostic instruments. The system uses a **hybrid .NET + Python architecture** orchestrated through Azure Service Bus, deployed on Azure Kubernetes Service.

## Architecture Style

- **Microservices** — each bounded context is an independently deployable service
- **Clean Architecture** — Domain → Application → Infrastructure → Presentation layers per service
- **CQRS + MediatR** — Command/Query separation within each .NET service
- **Event-Driven** — Azure Service Bus for asynchronous inter-service communication
- **Hybrid Runtime** — .NET 9 for API/orchestration, Python for ML inference

## Bounded Contexts

| # | Context | Primary Responsibility | Runtime |
|---|---------|----------------------|---------|
| 01 | Identity & Access | AuthN, AuthZ, HIPAA audit | .NET |
| 02 | Scan Engine | Eye scan capture, image analysis | .NET + Python |
| 03 | Passive Monitoring | Continuous monitoring, wearable sync | .NET (+ on-device TFLite) |
| 04 | Environmental Context | AQI, pollen, screen time collection | .NET |
| 05 | Diagnostic Engine | Differential diagnosis, causal graphs | .NET + Python |
| 06 | Predictive Engine | 72h forecasts, flare-up detection | .NET + Python |
| 07 | Treatment Orchestration | Treatment plans, closed-loop optimization | .NET + Python |
| 08 | Clinical Portal | Clinician-facing patient management | .NET (BFF) |
| 09 | Notifications & Alerts | Multi-channel notification delivery | .NET |
| 10 | Subscription & Billing | Plans, feature gating, Stripe | .NET |
| 11 | FHIR Interoperability | HL7 FHIR R4 data exchange | .NET |

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Mobile Client | React Native + Expo | Cross-platform app, TFLite on-device ML |
| API Gateway | .NET 9 / ASP.NET Core | Routing, rate limiting, auth |
| Backend Services | .NET 9, MediatR, EF Core | Business logic, CQRS handlers |
| ML Services | Python 3.12, PyTorch, ONNX Runtime | Model training and inference |
| Real-time | SignalR | Scan feedback, live notifications |
| Messaging | Azure Service Bus | Async domain events |
| Primary DB | Azure Cosmos DB | Patient data, scan records |
| Analytics DB | PostgreSQL + pgvector | Knowledge graph, embeddings |
| Cache | Redis | Forecasts, sessions, rate limiting |
| Object Storage | Azure Blob Storage | Eye scan images |
| Infrastructure | AKS, Terraform, GitHub Actions | Deployment, IaC, CI/CD |

## Clean Architecture Convention

Each .NET service follows this project structure:

```
ClearEyeQ.{Context}.Domain/         # Entities, Value Objects, Aggregates, Domain Events
ClearEyeQ.{Context}.Application/    # Commands, Queries, Handlers, Interfaces
ClearEyeQ.{Context}.Infrastructure/ # Repositories, External Clients, DB Config
ClearEyeQ.{Context}.API/            # Controllers, SignalR Hubs, Middleware
```

## CQRS Convention

- **Commands** mutate state, return void or an ID
- **Queries** read state, return DTOs
- **MediatR pipeline** behaviors: Validation → Authorization → Logging → Handler
- **Domain Events** raised within aggregates, dispatched after SaveChanges

## Shared Kernel

Value objects and interfaces shared across all bounded contexts:

- `UserId`, `TenantId`, `ScanId` — strongly-typed identifiers
- `Severity` — enum: None, Mild, Moderate, Severe, Critical
- `ConfidenceScore` — value object [0.0, 1.0]
- `AuditMetadata` — CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
- `IDomainEvent` — base interface for all domain events
- `IAuditableEntity` — interface for HIPAA-auditable entities

## HIPAA Compliance Strategy

1. **Encryption at rest** — AES-256 via Azure Storage Service Encryption + Cosmos DB encryption
2. **Encryption in transit** — TLS 1.3 enforced on all endpoints
3. **Audit logging** — every data access logged with user, timestamp, resource, action
4. **Access controls** — RBAC + tenant isolation at query level
5. **Data retention** — configurable per tenant, automated purge jobs
6. **Right to deletion** — cascading delete across all bounded contexts within 72 hours

## Diagrams

### System Context (C4 Level 1)
![System Context](c4-system-context.png)

### Container Diagram (C4 Level 2)
![Container Diagram](c4-container.png)

### Async Messaging Map
![Messaging](async-messaging.png)

### Shared Kernel
![Shared Kernel](shared-kernel.png)
