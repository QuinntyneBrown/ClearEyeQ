# ClearEyeQ

ClearEyeQ is a monorepo for an AI-assisted ocular health platform. It contains backend services, shared libraries, mobile and web application workspaces, local development infrastructure, automated tests, and the supporting requirements, architecture, and deployment documentation for the platform.

## Overview

ClearEyeQ is organized around bounded contexts and supporting applications:

- backend services for identity, scan capture, passive monitoring, environmental context, diagnostics, prediction, treatment orchestration, clinical workflows, notifications, billing, and FHIR interoperability
- a shared .NET kernel for common domain and application primitives
- a YARP-based API gateway
- a React Native + Expo mobile app workspace
- a Next.js clinician portal workspace
- a Blazor admin workspace

The target deployment model is Azure-based and is documented in [`docs/deployment-strategy-azure-staging-production.md`](docs/deployment-strategy-azure-staging-production.md).

## Architecture At A Glance

- `.NET 9` is the primary backend runtime, with nullable reference types enabled and warnings treated as errors
- services follow a bounded-context and Clean Architecture structure
- local development infrastructure is provided through Docker Compose
- asynchronous integration is modeled in the detailed design set and targeted at Azure-native deployment
- ML-facing contracts and platform shape are documented alongside the service architecture

Core local dependencies today:

- Azure Cosmos DB Emulator
- PostgreSQL with `pgvector`
- Redis
- Azurite
- RabbitMQ

## Repository Layout

```text
.
|-- docs/
|   |-- detailed-design/
|   |-- deployment-strategy-azure-staging-production.md
|   |-- repository-structure.md
|   `-- specs/
|-- src/
|   |-- admin/
|   |-- gateway/
|   |-- mobile/
|   |-- portal/
|   |-- services/
|   `-- shared/
|-- tests/
|   `-- e2e/
|-- tools/
|   |-- docker-compose.yml
|   `-- proto/
|-- ClearEyeQ.slnx
|-- Directory.Build.props
`-- Directory.Packages.props
```

For the fuller intended structure, conventions, and planned infrastructure layout, see [`docs/repository-structure.md`](docs/repository-structure.md).

## Getting Started

### Prerequisites

- `.NET 9 SDK`
- `Node.js 20+`
- `Docker` with Compose support
- `Python 3.11+` if you want to render PlantUML diagrams locally

### Start Local Infrastructure

```bash
docker compose -f tools/docker-compose.yml up -d
```

### Restore, Build, And Test The .NET Solution

```bash
dotnet restore ClearEyeQ.slnx
dotnet build ClearEyeQ.slnx
dotnet test ClearEyeQ.slnx
```

### Start The Mobile App Workspace

```bash
cd src/mobile
npm install
npm run start
```

### Start The Clinician Portal Workspace

```bash
cd src/portal
npm install
npm run dev
```

### Render PlantUML Diagrams

```bash
python -m pip install plantuml
python docs/detailed-design/render-puml.py docs/detailed-design
```

The render helper uses the PlantUML server configured in [`docs/detailed-design/render-puml.py`](docs/detailed-design/render-puml.py).

## Documentation

- Product requirements: [`docs/specs/L1.md`](docs/specs/L1.md), [`docs/specs/L2.md`](docs/specs/L2.md)
- System and bounded-context architecture: [`docs/detailed-design`](docs/detailed-design)
- Azure deployment and promotion strategy: [`docs/deployment-strategy-azure-staging-production.md`](docs/deployment-strategy-azure-staging-production.md)
- Repository structure and conventions: [`docs/repository-structure.md`](docs/repository-structure.md)

Recommended starting points:

- System architecture overview: [`docs/detailed-design/00-system-architecture/overview.md`](docs/detailed-design/00-system-architecture/overview.md)
- Clinical workflow design: [`docs/detailed-design/08-clinical-portal/overview.md`](docs/detailed-design/08-clinical-portal/overview.md)
- Treatment orchestration design: [`docs/detailed-design/07-treatment-orchestration/overview.md`](docs/detailed-design/07-treatment-orchestration/overview.md)

## Project Status

ClearEyeQ is under active development.

The repository already includes:

- a solution-based .NET codebase with shared kernel, gateway, and service projects
- mobile, portal, and admin application workspaces
- local infrastructure definitions and test projects
- detailed product, architecture, and deployment documentation

Some areas described in the documentation are still being expanded or wired together, including broader CI/CD automation, environment provisioning, and full end-to-end application integration.

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for contribution and pull request expectations.

## License

This repository is licensed under the MIT License. See [`LICENSE`](LICENSE).

## Disclaimer

ClearEyeQ is a software repository. Nothing in this repository is medical advice, a diagnostic claim, or a production-ready medical device implementation.
