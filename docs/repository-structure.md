# ClearEyeQ — Repository Structure

This document defines the expected folder layout for the ClearEyeQ monorepo. The structure follows Clean Architecture per bounded context, with a hybrid .NET + Python runtime and React Native / React frontends.

```
ClearEyeQ/
│
├── .github/
│   ├── workflows/
│   │   ├── ci-dotnet.yml                  # Build + test all .NET services
│   │   ├── ci-python.yml                  # Lint + test all Python ML services
│   │   ├── ci-mobile.yml                  # Build + test React Native app
│   │   ├── ci-portal.yml                  # Build + test Clinical Portal web app
│   │   ├── cd-staging.yml                 # Deploy to AKS staging
│   │   └── cd-production.yml              # Deploy to AKS production (manual gate)
│   ├── CODEOWNERS
│   └── pull_request_template.md
│
├── docs/
│   ├── specs/
│   │   ├── L1.md                          # High-level requirements
│   │   └── L2.md                          # Detailed requirements with acceptance criteria
│   ├── deployment-strategy-azure-staging-production.md  # Azure staging -> production deployment strategy
│   ├── detailed-design/
│   │   ├── 00-system-architecture/        # C4 L1/L2, messaging map, shared kernel
│   │   ├── 01-identity-and-access/        # AuthN/AuthZ/HIPAA design
│   │   ├── 02-scan-engine/                # Eye scan + ML pipeline design
│   │   ├── 03-passive-monitoring/         # Wearable + on-device ML design
│   │   ├── 04-environmental-context/      # AQI/pollen/screen time design
│   │   ├── 05-diagnostic-engine/          # Differential diagnosis design
│   │   ├── 06-predictive-engine/          # Forecasting + flare-up design
│   │   ├── 07-treatment-orchestration/    # Treatment + closed-loop design
│   │   ├── 08-clinical-portal/            # Clinician BFF design
│   │   ├── 09-notifications-and-alerts/   # Multi-channel notification design
│   │   ├── 10-subscription-and-billing/   # Stripe + feature gating design
│   │   ├── 11-fhir-interoperability/      # FHIR R4 exchange design
│   │   └── render-puml.py                 # Render all .puml → .png
│   ├── adr/                               # Architecture Decision Records
│   │   └── 0001-hybrid-dotnet-python.md
│   ├── runbooks/                          # Operational runbooks
│   │   ├── incident-response.md
│   │   ├── database-failover.md
│   │   └── privacy-erasure.md
│   └── repository-structure.md            # This file
│
├── src/
│   │
│   ├── shared/
│   │   ├── ClearEyeQ.SharedKernel/        # .NET shared value objects, interfaces, base classes
│   │   │   ├── Domain/
│   │   │   │   ├── AggregateRoot.cs
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   ├── IAuditableEntity.cs
│   │   │   │   ├── ITenantScopedEntity.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── UserId.cs
│   │   │   │   │   ├── TenantId.cs
│   │   │   │   │   ├── ScanId.cs
│   │   │   │   │   ├── PartitionKey.cs
│   │   │   │   │   ├── ConfidenceScore.cs
│   │   │   │   │   ├── AuditMetadata.cs
│   │   │   │   │   └── Severity.cs
│   │   │   │   └── Events/
│   │   │   │       └── IntegrationEventEnvelope.cs
│   │   │   ├── Application/
│   │   │   │   ├── Behaviors/
│   │   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   │   ├── AuthorizationBehavior.cs
│   │   │   │   │   └── LoggingBehavior.cs
│   │   │   │   └── Interfaces/
│   │   │   │       ├── IRepository.cs
│   │   │   │       └── IOutboxStore.cs
│   │   │   └── Infrastructure/
│   │   │       ├── Messaging/
│   │   │       │   ├── OutboxRelay.cs
│   │   │       │   ├── InboxConsumer.cs
│   │   │       │   └── ServiceBusPublisher.cs
│   │   │       ├── Persistence/
│   │   │       │   └── CosmosDbContext.cs
│   │   │       └── Observability/
│   │   │           ├── TelemetryInitializer.cs
│   │   │           └── AuditLogger.cs
│   │   └── ClearEyeQ.SharedKernel.Tests/
│   │
│   ├── services/
│   │   │
│   │   ├── identity/                      # 01 — Identity & Access
│   │   │   ├── ClearEyeQ.Identity.Domain/
│   │   │   │   ├── Aggregates/
│   │   │   │   │   └── User.cs
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Tenant.cs
│   │   │   │   │   ├── Consent.cs
│   │   │   │   │   └── RefreshToken.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   ├── Events/
│   │   │   │   └── Enums/
│   │   │   │       ├── Role.cs
│   │   │   │       └── AccountStatus.cs
│   │   │   ├── ClearEyeQ.Identity.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── RegisterUser/
│   │   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   │   ├── RegisterUserHandler.cs
│   │   │   │   │   │   └── RegisterUserValidator.cs
│   │   │   │   │   ├── Authenticate/
│   │   │   │   │   ├── GrantConsent/
│   │   │   │   │   └── RevokeConsent/
│   │   │   │   ├── Queries/
│   │   │   │   │   └── GetUserProfile/
│   │   │   │   └── Interfaces/
│   │   │   │       ├── IUserRepository.cs
│   │   │   │       ├── ITokenProvider.cs
│   │   │   │       ├── IAuditLogger.cs
│   │   │   │       └── IConsentService.cs
│   │   │   ├── ClearEyeQ.Identity.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   └── CosmosUserRepository.cs
│   │   │   │   ├── Auth/
│   │   │   │   │   ├── JwtTokenProvider.cs
│   │   │   │   │   ├── AppleOAuthHandler.cs
│   │   │   │   │   └── GoogleOAuthHandler.cs
│   │   │   │   └── Audit/
│   │   │   │       └── CosmosAuditLogRepository.cs
│   │   │   ├── ClearEyeQ.Identity.API/
│   │   │   │   ├── Controllers/
│   │   │   │   │   ├── AuthController.cs
│   │   │   │   │   └── ConsentController.cs
│   │   │   │   ├── Middleware/
│   │   │   │   ├── Program.cs
│   │   │   │   ├── appsettings.json
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Identity.Tests/
│   │   │       ├── Unit/
│   │   │       └── Integration/
│   │   │
│   │   ├── scan/                          # 02 — Scan Engine
│   │   │   ├── ClearEyeQ.Scan.Domain/
│   │   │   │   ├── Aggregates/
│   │   │   │   │   └── Scan.cs
│   │   │   │   ├── Entities/
│   │   │   │   │   └── ScanImage.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── RednessScore.cs
│   │   │   │   │   ├── TearFilmMetrics.cs
│   │   │   │   │   ├── CaptureMetadata.cs
│   │   │   │   │   └── PositioningFeedback.cs
│   │   │   │   └── Enums/
│   │   │   │       └── ScanStatus.cs
│   │   │   ├── ClearEyeQ.Scan.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── InitiateScan/
│   │   │   │   │   └── ProcessScan/
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetScanResult/
│   │   │   │   │   └── CompareScan/
│   │   │   │   └── Interfaces/
│   │   │   │       ├── IScanRepository.cs
│   │   │   │       ├── IImageStore.cs
│   │   │   │       └── IMLInferenceClient.cs
│   │   │   ├── ClearEyeQ.Scan.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   ├── BlobStorage/
│   │   │   │   ├── ML/
│   │   │   │   │   └── GrpcMLInferenceClient.cs
│   │   │   │   └── SignalR/
│   │   │   │       └── ScanHub.cs
│   │   │   ├── ClearEyeQ.Scan.API/
│   │   │   │   ├── Controllers/
│   │   │   │   ├── Program.cs
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Scan.Tests/
│   │   │
│   │   ├── monitoring/                    # 03 — Passive Monitoring
│   │   │   ├── ClearEyeQ.Monitoring.Domain/
│   │   │   ├── ClearEyeQ.Monitoring.Application/
│   │   │   ├── ClearEyeQ.Monitoring.Infrastructure/
│   │   │   ├── ClearEyeQ.Monitoring.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Monitoring.Tests/
│   │   │
│   │   ├── environmental/                 # 04 — Environmental Context
│   │   │   ├── ClearEyeQ.Environmental.Domain/
│   │   │   ├── ClearEyeQ.Environmental.Application/
│   │   │   ├── ClearEyeQ.Environmental.Infrastructure/
│   │   │   ├── ClearEyeQ.Environmental.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Environmental.Tests/
│   │   │
│   │   ├── diagnostic/                    # 05 — Diagnostic Engine
│   │   │   ├── ClearEyeQ.Diagnostic.Domain/
│   │   │   ├── ClearEyeQ.Diagnostic.Application/
│   │   │   ├── ClearEyeQ.Diagnostic.Infrastructure/
│   │   │   ├── ClearEyeQ.Diagnostic.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Diagnostic.Tests/
│   │   │
│   │   ├── predictive/                    # 06 — Predictive Engine
│   │   │   ├── ClearEyeQ.Predictive.Domain/
│   │   │   ├── ClearEyeQ.Predictive.Application/
│   │   │   ├── ClearEyeQ.Predictive.Infrastructure/
│   │   │   ├── ClearEyeQ.Predictive.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Predictive.Tests/
│   │   │
│   │   ├── treatment/                     # 07 — Treatment Orchestration
│   │   │   ├── ClearEyeQ.Treatment.Domain/
│   │   │   ├── ClearEyeQ.Treatment.Application/
│   │   │   ├── ClearEyeQ.Treatment.Infrastructure/
│   │   │   ├── ClearEyeQ.Treatment.API/
│   │   │   │   └── Dockerfile
│   │   │   ├── ClearEyeQ.Treatment.Worker/          # Closed-loop background processor
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Treatment.Tests/
│   │   │
│   │   ├── clinical/                      # 08 — Clinical Portal (BFF)
│   │   │   ├── ClearEyeQ.Clinical.Application/
│   │   │   ├── ClearEyeQ.Clinical.Infrastructure/
│   │   │   ├── ClearEyeQ.Clinical.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Clinical.Tests/
│   │   │
│   │   ├── notifications/                 # 09 — Notifications & Alerts
│   │   │   ├── ClearEyeQ.Notifications.Domain/
│   │   │   ├── ClearEyeQ.Notifications.Application/
│   │   │   ├── ClearEyeQ.Notifications.Infrastructure/
│   │   │   ├── ClearEyeQ.Notifications.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Notifications.Tests/
│   │   │
│   │   ├── billing/                       # 10 — Subscription & Billing
│   │   │   ├── ClearEyeQ.Billing.Domain/
│   │   │   ├── ClearEyeQ.Billing.Application/
│   │   │   ├── ClearEyeQ.Billing.Infrastructure/
│   │   │   ├── ClearEyeQ.Billing.API/
│   │   │   │   └── Dockerfile
│   │   │   └── ClearEyeQ.Billing.Tests/
│   │   │
│   │   └── fhir/                          # 11 — FHIR Interoperability
│   │       ├── ClearEyeQ.Fhir.Application/
│   │       ├── ClearEyeQ.Fhir.Infrastructure/
│   │       ├── ClearEyeQ.Fhir.API/
│   │       │   └── Dockerfile
│   │       └── ClearEyeQ.Fhir.Tests/
│   │
│   ├── ml/                                # Python ML Services
│   │   │
│   │   ├── shared/                        # Shared Python utilities
│   │   │   ├── cleareyeq_ml_common/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── config.py
│   │   │   │   ├── logging.py
│   │   │   │   ├── grpc_utils.py
│   │   │   │   └── model_registry.py
│   │   │   ├── pyproject.toml
│   │   │   └── tests/
│   │   │
│   │   ├── scan-ml/                       # 02 — Scan ML Service
│   │   │   ├── scan_ml/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── service.py             # gRPC service entrypoint
│   │   │   │   ├── preprocessor.py        # OpenCV image pipeline
│   │   │   │   ├── positioning.py         # Eye detection + alignment
│   │   │   │   ├── redness_scorer.py      # ONNX redness inference
│   │   │   │   └── tear_film.py           # Tear film analysis
│   │   │   ├── models/                    # Trained model artifacts (.onnx)
│   │   │   ├── proto/
│   │   │   │   └── scan_ml.proto          # gRPC service definition
│   │   │   ├── tests/
│   │   │   ├── pyproject.toml
│   │   │   └── Dockerfile
│   │   │
│   │   ├── diagnostic-ml/                 # 05 — Diagnostic ML Service
│   │   │   ├── diagnostic_ml/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── service.py             # gRPC service entrypoint
│   │   │   │   ├── classifier.py          # PyTorch multi-label classifier
│   │   │   │   ├── causal_inference.py    # Bayesian network engine
│   │   │   │   ├── medication_checker.py  # Drug interaction analysis
│   │   │   │   ├── genetic_scorer.py      # Genetic risk scoring
│   │   │   │   └── embeddings.py          # Patient similarity (pgvector)
│   │   │   ├── models/
│   │   │   ├── proto/
│   │   │   │   └── diagnostic_ml.proto
│   │   │   ├── tests/
│   │   │   ├── pyproject.toml
│   │   │   └── Dockerfile
│   │   │
│   │   ├── predictive-ml/                 # 06 — Predictive ML Service
│   │   │   ├── predictive_ml/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── service.py
│   │   │   │   ├── forecaster.py          # Temporal Fusion Transformer
│   │   │   │   ├── flare_detector.py      # Flare-up risk model
│   │   │   │   ├── trajectory.py          # Long-term trajectory regression
│   │   │   │   └── similarity.py          # Patient similarity search
│   │   │   ├── models/
│   │   │   ├── proto/
│   │   │   │   └── predictive_ml.proto
│   │   │   ├── tests/
│   │   │   ├── pyproject.toml
│   │   │   └── Dockerfile
│   │   │
│   │   └── innovation-ml/                 # 07 — Therapeutic Innovation Service
│   │       ├── innovation_ml/
│   │       │   ├── __init__.py
│   │       │   ├── service.py             # FastAPI service entrypoint
│   │       │   ├── cross_patient.py       # Anonymized cross-patient learning
│   │       │   ├── hypothesis.py          # Novel treatment hypothesis generation
│   │       │   └── literature_rag.py      # PubMed RAG synthesis
│   │       ├── models/
│   │       ├── tests/
│   │       ├── pyproject.toml
│   │       └── Dockerfile
│   │
│   ├── gateway/                           # API Gateway
│   │   ├── ClearEyeQ.Gateway/
│   │   │   ├── Program.cs
│   │   │   ├── yarp.json                  # YARP reverse proxy config
│   │   │   ├── appsettings.json
│   │   │   └── Dockerfile
│   │   └── ClearEyeQ.Gateway.Tests/
│   │
│   ├── mobile/                            # React Native Mobile App
│   │   ├── app/
│   │   │   ├── (tabs)/                    # Expo Router tab layout
│   │   │   │   ├── _layout.tsx
│   │   │   │   ├── index.tsx              # Home / Dashboard
│   │   │   │   ├── scan.tsx               # Eye Scan flow
│   │   │   │   ├── timeline.tsx           # Timeline & Trends
│   │   │   │   ├── treatment.tsx          # Treatment Plan
│   │   │   │   └── settings.tsx           # Settings & Profile
│   │   │   ├── (auth)/
│   │   │   │   ├── login.tsx
│   │   │   │   ├── signup.tsx
│   │   │   │   └── onboarding.tsx
│   │   │   ├── diagnosis/
│   │   │   │   └── [id].tsx               # Diagnosis detail
│   │   │   ├── notifications.tsx
│   │   │   └── _layout.tsx
│   │   ├── components/
│   │   │   ├── ui/                        # Design system components
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Card.tsx
│   │   │   │   ├── Dialog.tsx
│   │   │   │   ├── Toast.tsx
│   │   │   │   ├── Input.tsx
│   │   │   │   ├── Toggle.tsx
│   │   │   │   └── Badge.tsx
│   │   │   ├── scan/                      # Scan-specific components
│   │   │   │   ├── CameraViewfinder.tsx
│   │   │   │   ├── PositioningGuide.tsx
│   │   │   │   └── ScanResults.tsx
│   │   │   ├── dashboard/
│   │   │   │   ├── HealthScoreCard.tsx
│   │   │   │   ├── EnvironmentCards.tsx
│   │   │   │   └── DailyTipBanner.tsx
│   │   │   └── charts/
│   │   │       ├── RednessTimeline.tsx
│   │   │       └── ForecastCards.tsx
│   │   ├── hooks/
│   │   │   ├── useAuth.ts
│   │   │   ├── useScan.ts
│   │   │   ├── useSignalR.ts
│   │   │   └── useHealthKit.ts
│   │   ├── services/
│   │   │   ├── api.ts                     # API client (axios/fetch)
│   │   │   ├── signalr.ts                 # SignalR connection manager
│   │   │   └── notifications.ts           # Push notification setup
│   │   ├── ml/
│   │   │   ├── BlinkDetector.ts           # TFLite blink rate model bridge
│   │   │   └── FatigueEstimator.ts        # On-device fatigue model
│   │   ├── stores/                        # Zustand or similar state management
│   │   │   ├── authStore.ts
│   │   │   ├── scanStore.ts
│   │   │   └── settingsStore.ts
│   │   ├── theme/
│   │   │   ├── colors.ts
│   │   │   ├── typography.ts
│   │   │   └── spacing.ts
│   │   ├── assets/
│   │   ├── app.json
│   │   ├── package.json
│   │   ├── tsconfig.json
│   │   └── eas.json                       # Expo Application Services config
│   │
│   └── portal/                            # Clinical Portal Web App
│       ├── src/
│       │   ├── app/
│       │   │   ├── layout.tsx
│       │   │   ├── page.tsx               # Dashboard
│       │   │   ├── patients/
│       │   │   │   ├── page.tsx            # Patient list
│       │   │   │   └── [id]/
│       │   │   │       └── page.tsx        # Patient detail (tabs: scans, diagnosis, treatment)
│       │   │   ├── referrals/
│       │   │   │   └── page.tsx            # Referral inbox
│       │   │   └── settings/
│       │   │       └── page.tsx
│       │   ├── components/
│       │   │   ├── ui/                    # Shadcn/UI or similar
│       │   │   ├── patients/
│       │   │   │   ├── PatientTable.tsx
│       │   │   │   └── PatientTimeline.tsx
│       │   │   ├── referrals/
│       │   │   │   └── ReferralCard.tsx
│       │   │   └── layout/
│       │   │       ├── Sidebar.tsx
│       │   │       └── TopBar.tsx
│       │   ├── hooks/
│       │   ├── services/
│       │   └── lib/
│       ├── public/
│       ├── package.json
│       ├── tsconfig.json
│       ├── next.config.js                 # Next.js config
│       └── Dockerfile
│
├── infra/                                 # Infrastructure as Code
│   ├── terraform/
│   │   ├── environments/
│   │   │   ├── dev/
│   │   │   │   └── main.tf
│   │   │   ├── staging/
│   │   │   │   └── main.tf
│   │   │   └── production/
│   │   │       └── main.tf
│   │   ├── modules/
│   │   │   ├── aks/                       # Azure Kubernetes Service
│   │   │   │   ├── main.tf
│   │   │   │   ├── variables.tf
│   │   │   │   └── outputs.tf
│   │   │   ├── cosmos/                    # Cosmos DB accounts + databases
│   │   │   ├── postgres/                  # PostgreSQL + pgvector
│   │   │   ├── redis/                     # Azure Redis Cache
│   │   │   ├── servicebus/                # Service Bus namespace + topics
│   │   │   ├── storage/                   # Blob Storage accounts
│   │   │   ├── keyvault/                  # Azure Key Vault
│   │   │   └── monitoring/                # App Insights + Log Analytics
│   │   └── backend.tf                     # Terraform state backend (Azure Storage)
│   │
│   └── k8s/                               # Kubernetes manifests
│       ├── base/
│       │   ├── namespace.yaml
│       │   ├── configmap.yaml
│       │   └── secrets.yaml               # ExternalSecret references
│       ├── services/
│       │   ├── identity/
│       │   │   ├── deployment.yaml
│       │   │   ├── service.yaml
│       │   │   └── hpa.yaml
│       │   ├── scan/
│       │   ├── scan-ml/
│       │   ├── monitoring/
│       │   ├── environmental/
│       │   ├── diagnostic/
│       │   ├── diagnostic-ml/
│       │   ├── predictive/
│       │   ├── predictive-ml/
│       │   ├── treatment/
│       │   ├── treatment-worker/
│       │   ├── innovation-ml/
│       │   ├── clinical/
│       │   ├── notifications/
│       │   ├── billing/
│       │   ├── fhir/
│       │   ├── gateway/
│       │   └── portal/
│       ├── ingress/
│       │   └── ingress.yaml               # NGINX ingress with TLS
│       └── kustomization.yaml
│
├── tools/                                 # Developer tooling
│   ├── db-migrations/
│   │   ├── cosmos/                        # Cosmos DB seed/migration scripts
│   │   └── postgres/                      # PostgreSQL migrations (Flyway or EF)
│   ├── proto/                             # Shared gRPC proto definitions
│   │   ├── scan_ml.proto
│   │   ├── diagnostic_ml.proto
│   │   └── predictive_ml.proto
│   ├── scripts/
│   │   ├── dev-setup.sh                   # Local development environment setup
│   │   ├── run-all-tests.sh
│   │   └── generate-grpc.sh               # Generate gRPC stubs from .proto files
│   └── docker-compose.yml                 # Local development: all services + deps
│
├── tests/
│   ├── integration/                       # Cross-service integration tests
│   │   ├── ScanToDignosticFlowTests.cs
│   │   ├── TreatmentClosedLoopTests.cs
│   │   └── EndToEndScanTests.cs
│   ├── load/                              # Load/performance tests
│   │   ├── k6/
│   │   │   ├── scan-api.js
│   │   │   └── forecast-api.js
│   │   └── README.md
│   └── contract/                          # Event schema contract tests
│       ├── ScanCompletedContractTests.cs
│       └── DiagnosisCompletedContractTests.cs
│
│
│   └── admin/                             # Admin Portal (Blazor)
│       ├── ClearEyeQ.Admin/
│       │   ├── Components/
│       │   │   ├── Layout/
│       │   │   │   ├── MainLayout.razor
│       │   │   │   └── NavMenu.razor
│       │   │   ├── Pages/
│       │   │   │   ├── Dashboard.razor
│       │   │   │   ├── Tenants/
│       │   │   │   │   ├── TenantList.razor
│       │   │   │   │   └── TenantDetail.razor
│       │   │   │   ├── Users/
│       │   │   │   │   └── UserManagement.razor
│       │   │   │   ├── Subscriptions/
│       │   │   │   │   └── SubscriptionOverview.razor
│       │   │   │   ├── System/
│       │   │   │   │   ├── HealthDashboard.razor
│       │   │   │   │   └── FeatureFlags.razor
│       │   │   │   └── Audit/
│       │   │   │       └── AuditLogViewer.razor
│       │   │   └── Shared/
│       │   ├── Services/
│       │   │   ├── TenantService.cs
│       │   │   ├── SubscriptionService.cs
│       │   │   └── AuditService.cs
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   └── Dockerfile
│       └── ClearEyeQ.Admin.Tests/
│
├── .editorconfig
├── .gitignore
├── .dockerignore
├── Directory.Build.props                  # Shared .NET build properties
├── Directory.Packages.props               # Central .NET package versioning
├── ClearEyeQ.sln                          # .NET solution file
├── CLAUDE.md                              # Claude Code project instructions
├── LICENSE
└── README.md
```

## Conventions

### .NET Services (src/services/*)

Every bounded context follows Clean Architecture with four projects:

| Project | Purpose | Dependencies |
|---------|---------|-------------|
| `Domain` | Aggregates, entities, value objects, domain events, enums | SharedKernel only |
| `Application` | Commands, queries, handlers, validators, interfaces | Domain |
| `Infrastructure` | Repositories, external clients, messaging, DB config | Application, Domain |
| `API` | Controllers, SignalR hubs, middleware, DI setup, Dockerfile | All above |
| `Tests` | Unit + integration tests | All above |

Each command/query lives in its own folder with handler and validator co-located:
```
Commands/
  RegisterUser/
    RegisterUserCommand.cs
    RegisterUserHandler.cs
    RegisterUserValidator.cs
```

### Python ML Services (src/ml/*)

Each ML service is a standalone Python package with:
- `pyproject.toml` for dependency management (uv or poetry)
- gRPC or FastAPI entrypoint
- `models/` directory for trained artifacts (`.onnx`, `.pt`)
- `proto/` for gRPC definitions (shared copies in `tools/proto/`)
- `Dockerfile` for containerization
- `tests/` with pytest

### Mobile App (src/mobile/)

- **Expo Router** file-based routing with tab layout
- **Component hierarchy**: `ui/` (design system) → feature folders → screens
- **On-device ML**: TFLite models via React Native bridge in `ml/`
- **State management**: Zustand stores per domain
- **Theme**: design tokens matching the `.pen` design system

### Clinical Portal (src/portal/) — React + TypeScript

- **Next.js** with App Router, SSR for initial load
- **TypeScript** strict mode throughout
- **Component library**: Shadcn/UI (Radix-based)
- **Layout**: sidebar navigation matching the designed LG portal screens

### Admin Portal (src/admin/) — Blazor Server

- **Blazor Server** for low-latency interaction with Azure-hosted backend
- **Full-stack C#** — shares .NET domain models from SharedKernel
- **Razor components** organized by feature (Tenants, Users, Subscriptions, System, Audit)
- **Admin role only** — desktop layout (LG breakpoint), no mobile/tablet support

### Infrastructure (infra/)

- **Terraform modules** per Azure resource type, composed per environment
- **Kubernetes manifests** per service with Kustomize overlays
- **Each service** gets: deployment, service, HPA, and optional ingress

### Testing Levels

| Level | Location | Scope |
|-------|----------|-------|
| Unit | `src/services/*/Tests/Unit/` | Domain logic, handlers |
| Integration | `src/services/*/Tests/Integration/` | DB, external clients |
| Cross-service | `tests/integration/` | Multi-service flows |
| Contract | `tests/contract/` | Event schema compatibility |
| Load | `tests/load/` | k6 performance benchmarks |
