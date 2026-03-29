# ClearEyeQ Deployment Strategy - Azure Staging to Production

## 1. Purpose

This document defines the deployment strategy for ClearEyeQ on Azure with:

- a long-lived staging environment that mirrors production closely
- a controlled promotion path from staging to production
- a secure, auditable release process for backend services, web apps, and the React Native mobile app

This strategy aligns with the planned repository structure in [repository-structure.md](./repository-structure.md) and the mobile stack in [specs/L2.md](./specs/L2.md).

Promotion in this document means promoting a verified release candidate from staging into production. It does not mean renaming or reusing staging Azure resources as production resources.

## 2. Deployment Principles

1. Build once, promote many. The exact artifact validated in staging must be the artifact deployed to production.
2. Staging and production are separate Azure environments with separate identities, secrets, data stores, and network boundaries.
3. Deploy by immutable version and image digest, not by mutable tags such as `latest`.
4. Production deployments require both automated validation and a manual approval gate.
5. Database and event contract changes must be backward-compatible during rollout.
6. Rollback must be fast for stateless services and controlled for stateful changes.
7. Native mobile promotion is different from backend promotion:
   - JavaScript-only updates can be promoted if runtime compatibility and environment handling allow it.
   - Native changes require a separate production binary built from the same approved source revision.

## 3. Azure Environment Model

### 3.1 Environment Topology

ClearEyeQ should run at least these Azure environments:

| Environment | Azure Scope | Purpose |
|-------------|-------------|---------|
| Staging | Dedicated non-production subscription | Final pre-production validation, release candidate testing, integration validation |
| Production | Dedicated production subscription | Live patient, clinician, billing, and interoperability traffic |

Recommended topology:

| Component | Staging | Production | Strategy |
|-----------|---------|------------|----------|
| Subscription | Separate non-prod subscription | Separate prod subscription | Do not mix staging and production in one subscription unless forced by platform constraints |
| AKS | Dedicated staging cluster | Dedicated prod cluster | No shared cluster between staging and production |
| Cosmos DB | Dedicated account/database | Dedicated account/database | No shared data plane |
| PostgreSQL | Dedicated server/database | Dedicated server/database | Separate credentials and backups |
| Service Bus | Dedicated namespace | Dedicated namespace | Prevent cross-environment event leakage |
| Storage | Dedicated account(s) | Dedicated account(s) | Separate blob containers, keys, retention |
| Key Vault | Dedicated vault | Dedicated vault | Separate secrets and certificates |
| Redis | Dedicated instance | Dedicated instance | Separate cache invalidation domains |
| Monitoring | Dedicated App Insights / Log Analytics | Dedicated App Insights / Log Analytics | Separate alert noise and retention controls |
| DNS / ingress | `staging-*` hostnames | production hostnames | No production traffic routed to staging endpoints |

### 3.2 Data Separation

- Staging must never use live production PHI.
- If staging requires realistic data, use synthetic data or privacy-scrubbed copies with a documented de-identification process.
- Staging identities, client registrations, push credentials, and callback URLs must be separate from production.

## 4. Promotion Model

### 4.1 What Gets Promoted

The promotable unit is a release manifest. It should include:

- git commit SHA
- container image digests for every deployable service
- infrastructure module version or Terraform revision
- Kubernetes manifest or overlay version
- database migration version(s)
- mobile app version, build numbers, and EAS metadata
- EAS Update group ID(s), if OTA updates are part of the release

Example:

```yaml
releaseId: 2026.03.29-rc.4
gitSha: 8f3d7b1
images:
  identity: cleareyeq.azurecr.io/identity@sha256:...
  scan: cleareyeq.azurecr.io/scan@sha256:...
  diagnostic: cleareyeq.azurecr.io/diagnostic@sha256:...
  gateway: cleareyeq.azurecr.io/gateway@sha256:...
k8sRevision: 8f3d7b1
terraformRevision: 8f3d7b1
migrations:
  postgres: 202603291015_add_referral_indexes
  cosmos: 20260329_partition_policy_v2
mobile:
  stagingBuild:
    ios: 1.8.0(104)
    android: 1.8.0(104)
  productionBuild:
    ios: 1.8.0(204)
    android: 1.8.0(204)
  ota:
    runtimeVersion: 1.8.0
    stagingUpdateGroup: 7d8b...
```

### 4.2 What Does Not Get Promoted

The following are not promoted from staging to production:

- staging databases or message history
- staging secrets or certificates
- staging DNS or ingress configuration
- staging mobile binaries

Production receives the same approved release content, but it runs against production-only infrastructure and identities.

## 5. Artifact Strategy

### 5.1 Containers

All backend services, workers, and web apps should be containerized and versioned with:

- semantic release version
- git SHA
- immutable manifest digest

Deployment manifests must pin images by digest, not tag.

Recommended flow:

1. CI builds the container image once.
2. CI generates SBOM and provenance metadata.
3. CI pushes the image to Azure Container Registry.
4. Staging deploys that digest.
5. Production deploys that same digest after approval.

If production isolation policy requires a separate registry, import the approved digest into the production registry before deploy. Azure Container Registry supports importing images across subscriptions and registries.

### 5.2 Infrastructure as Code

- Terraform is the source of truth for Azure resources under `infra/terraform`.
- Kubernetes manifests under `infra/k8s` are the source of truth for workload deployment.
- Environment-specific values must live in environment overlays, variables, or parameter files rather than being hard-coded into shared manifests.

Infrastructure promotion rule:

- the same Terraform module version validated in staging is the version applied in production
- production applies production-specific variables only

## 6. CI/CD Workflow Responsibilities

The planned workflows in the repository should operate as follows.

### 6.1 Continuous Integration

| Workflow | Responsibility |
|----------|----------------|
| `ci-dotnet.yml` | Build and test .NET services and shared libraries |
| `ci-python.yml` | Lint, test, and package Python ML services |
| `ci-portal.yml` | Build and test the clinician portal |
| `ci-mobile.yml` | Validate React Native app, typecheck, test, and verify Expo config |

All CI workflows should publish artifacts, test results, and security scan outputs. A release candidate is not eligible for staging unless CI is green.

### 6.2 Staging Deployment

`cd-staging.yml` should:

1. authenticate to Azure using GitHub Actions OIDC, not static cloud secrets
2. resolve the exact release manifest for the candidate build
3. apply Terraform changes to the staging subscription
4. run database migrations in staging
5. deploy the approved image digests to AKS staging
6. run smoke tests, contract tests, and critical integration flows
7. verify telemetry, alerting, and background workers
8. publish a staging release report and mark the release candidate as ready or failed

### 6.3 Production Deployment

`cd-production.yml` must be manually triggered or released only after approval of the exact staging-tested release manifest.

It should:

1. require the GitHub `production` environment approval gate
2. use the exact approved release manifest from staging
3. apply Terraform only for the approved production change set
4. run production-safe migrations
5. deploy the exact approved image digests
6. execute post-deploy smoke checks and synthetic probes
7. complete traffic cutover
8. publish a signed deployment record for auditability

Production must not rebuild containers, re-run package restore into new artifacts, or re-bundle the web application during promotion.

## 7. Production Rollout Pattern on AKS

### 7.1 Public HTTP Workloads

For `gateway`, `portal`, and public APIs, use blue/green rollout inside the production cluster:

- active namespace: current production release
- idle namespace: candidate release
- deploy candidate to idle namespace
- run smoke tests against idle namespace host or internal route
- switch ingress to the idle namespace after approval
- keep the previous namespace available for quick rollback

This approach is preferred for higher-risk releases and aligns with Azure guidance that blue/green is the safest pattern when minimal downtime is required.

### 7.2 Event Consumers and Scheduled Workers

Background processors need a stricter cutover model because running both old and new consumers in parallel can cause duplicate work even with idempotent handlers.

For `treatment-worker`, notification dispatchers, export workers, and scheduled jobs:

- deploy the new version in a disabled or zero-replica state
- cut over HTTP and API traffic first if applicable
- stop or scale down the previous consumers
- enable the new consumers
- validate queue lag, DLQ rate, and processing health

If a rollout requires both versions to coexist temporarily, the change must be reviewed for:

- message schema compatibility
- idempotent consumption
- safe retry behavior
- exactly-once business semantics where required

## 8. Database and Schema Change Strategy

Database changes must use expand-and-contract migrations.

Rules:

1. Additive schema changes happen before code depends on them.
2. The old and new application versions must both work during the promotion window.
3. Destructive cleanup happens only after the new version is stable in production.
4. Rollback assumes code rollback first and data forward-fix second. Direct database rollback is not the default strategy.

For Cosmos DB and Service Bus contract changes:

- version event schemas
- maintain backward-compatible consumers during rollout
- update producers only after consumers can handle both old and new shapes

## 9. Security Controls for Deployment

### 9.1 CI/CD Identity

- Use GitHub Actions with OpenID Connect to authenticate to Azure.
- Do not store long-lived Azure deployment credentials in repository secrets if OIDC can be used.
- Use separate Azure principals or federated credentials for staging and production scopes.

### 9.2 Runtime Identity and Secrets

- Use Microsoft Entra Workload Identity for AKS workloads that access Azure resources.
- Use Azure Key Vault for secrets, certificates, signing material, and connection strings.
- Mount or inject secrets through the Key Vault CSI provider or equivalent workload identity-based access path.
- Do not bake secrets into container images or mobile bundles.

### 9.3 Environment Protection

- Protect the GitHub `production` environment with required reviewers and prevent self-approval.
- Keep staging and production secrets in environment-scoped secret stores.
- Restrict production deployments to protected branches or signed release tags.
- Use private networking and disable public access on data stores where practical.

## 10. Native App Deployment and Promotion Pattern

ClearEyeQ uses React Native + Expo with EAS Build.

The mobile release model must distinguish between:

- preview builds for internal QA
- staging builds for pre-production validation
- production builds for live distribution

### 10.1 Mobile Build Profiles

Recommended EAS profiles:

| Profile | Distribution Target | App Variant | Channel | Purpose |
|---------|---------------------|-------------|---------|---------|
| `preview` | Internal distribution | `preview` | `preview` | PR review, QA, design validation |
| `staging` | TestFlight / Google Play closed or internal track | `staging` | `staging` | Release candidate validation |
| `production` | App Store / Google Play production track | `production` | `production` | Live release |

Recommended Expo configuration pattern:

- use `app.config.js` rather than only `app.json`
- drive app identity from `APP_VARIANT`
- use distinct bundle/package identifiers for staging and production

Example identifiers:

| Variant | iOS bundle ID | Android package |
|---------|---------------|-----------------|
| Staging | `com.cleareyeq.app.staging` | `com.cleareyeq.app.staging` |
| Production | `com.cleareyeq.app` | `com.cleareyeq.app` |

This separation allows staging and production to be installed on the same device and prevents accidental cross-environment use.

### 10.2 Mobile Environment Configuration

The mobile app should separate at least these values by environment:

- API base URL
- SignalR endpoint
- notification sender IDs and push configuration
- Apple and Google auth client IDs
- Sentry or telemetry DSN
- feature flag environment

Recommended variables:

- `APP_VARIANT`
- `EXPO_PUBLIC_ENVIRONMENT`
- `EXPO_PUBLIC_API_BASE_URL`
- `EXPO_PUBLIC_SIGNALR_URL`

### 10.3 Native Binary Promotion Pattern

Native changes include:

- new native modules
- SDK upgrades with native impact
- permission changes
- entitlements or capabilities changes
- config plugin changes
- runtime version changes

For native changes:

1. build staging binaries from the release candidate commit
2. distribute them to TestFlight and Google Play closed testing, or to internal distribution earlier in QA
3. validate against the staging backend
4. after approval, build production binaries from the same approved git tag or commit
5. submit production binaries to the production app store tracks

Important: the production binary is not the same binary as the staging binary because the app identifiers, signing, and store targets differ.

The promotion guarantee is therefore:

- same source revision
- same app version and runtime intent
- same approved release contents
- different production signing and store packaging

### 10.4 OTA Update Promotion Pattern

For JavaScript-only updates that do not require a new native binary:

1. publish to the `staging` channel
2. validate on staging builds with the same runtime version
3. promote to `production` only after approval

Expo supports promoting a previously staged update by republishing it to the production channel. This is the preferred path only if the staged bundle is environment-neutral or if staging and production use compatible configuration handling for the update.

Recommended rules:

- Set `runtimeVersion` policy to `appVersion`.
- Prefer environment resolution outside the OTA payload when possible.
- If the update bundle contains environment-specific values, publish separately to staging and production from the same commit instead of republishing the exact staged bundle.

### 10.5 Suggested Mobile Release Commands

Examples:

```bash
# Preview build for QA
eas build --profile preview --platform all

# Staging release candidate binary
eas build --profile staging --platform all --auto-submit

# Staging OTA update
eas update --channel staging

# Promote the exact OTA update, only when the staging-tested bundle is safe to reuse
eas update:republish --destination-channel production

# Production binary from the same approved revision
eas build --profile production --platform all --auto-submit
```

## 11. Release Gates

A release can move from staging to production only when all of the following are true:

1. CI is green for backend, ML, portal, and mobile.
2. Security scanning and dependency policy checks pass.
3. Staging infrastructure apply and workload deployment succeed.
4. Database migrations succeed in staging.
5. Smoke tests and critical end-to-end flows pass in staging.
6. Observability shows stable error rate, latency, queue lag, and no unexpected DLQ growth.
7. Manual approval is given by the required production reviewers.
8. For mobile releases, staging testers sign off on the release candidate build.

## 12. Rollback Strategy

### 12.1 Backend and Web

- Roll back by redeploying the previous release manifest.
- For blue/green, switch ingress back to the previous namespace.
- Scale consumers back to the previous version if queue processing regresses.
- Do not rely on destructive database rollback. Use forward-fix migrations where needed.

### 12.2 Mobile

- For OTA regressions, use Expo rollback or republish the previous stable update.
- For binary regressions, halt store rollout, keep the previous production binary live where store controls allow it, and ship a hotfix build.
- Use staged or phased store rollout for higher-risk production binaries.

## 13. Operational Recommendations

- Keep staging always deployable and within one release of production.
- Run scheduled synthetic probes against both staging and production.
- Generate a deployment record for every release with approver, manifest, timestamp, and outcome.
- Record mobile build IDs, store submission IDs, and OTA update group IDs in the same release record as the backend release.
- Test disaster recovery, secret rotation, and rollback paths quarterly.

## 14. Summary Decision

ClearEyeQ should use:

- separate staging and production Azure subscriptions
- dedicated AKS clusters and data services per environment
- immutable release-manifest promotion from staging to production
- GitHub environment approvals plus Azure OIDC authentication
- blue/green cutover for public production workloads
- controlled worker cutover for queue consumers
- Expo app variants for staging and production
- OTA promotion only when runtime compatibility and environment handling make exact promotion safe

This gives a release process that is auditable, repeatable, secure, and suitable for regulated production software.

## 15. References

- Azure landing zones application environments:
  - https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/design-area/management-application-environments
- Azure subscription separation guidance:
  - https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/scale-subscriptions
- AKS production upgrade strategies:
  - https://learn.microsoft.com/en-us/azure/aks/aks-production-upgrade-strategies
- Azure mission-critical operations:
  - https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-mission-critical/mission-critical-operations
- Azure Container Registry image promotion and digests:
  - https://learn.microsoft.com/en-us/azure/container-registry/container-registry-import-images
  - https://learn.microsoft.com/en-us/azure/container-registry/container-registry-image-tag-version
- GitHub deployment approvals and protected environments:
  - https://docs.github.com/en/enterprise-cloud@latest/actions/reference/workflows-and-actions/deployments-and-environments
- Azure authentication from GitHub Actions:
  - https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure
- AKS workload identity:
  - https://learn.microsoft.com/en-us/azure/aks/workload-identity-overview
- Expo app variants:
  - https://docs.expo.dev/build-reference/variants/
  - https://docs.expo.dev/tutorial/eas/multiple-app-variants/
- Expo EAS Update deployment:
  - https://docs.expo.dev/eas-update/deployment/
- Expo internal distribution and store submission:
  - https://docs.expo.dev/build/internal-distribution/
  - https://docs.expo.dev/submit/introduction/
