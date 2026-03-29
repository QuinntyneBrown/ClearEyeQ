# ClearEyeQ

ClearEyeQ is a design-first monorepo for an AI-assisted ocular health platform. The repository combines product requirements, system and bounded-context architecture, UI design assets, and early backend scaffolding for identity, scan processing, monitoring, diagnostics, prediction, treatment orchestration, clinical workflows, notifications, billing, and interoperability.

> [!IMPORTANT]
> This repository is in an early stage. Today it contains substantial design documentation and initial .NET service scaffolding, but it does not yet provide a single root solution or fully wired mobile, web, and ML application workspaces.

## What Is In This Repository

- Product requirements in [`docs/specs`](docs/specs)
- Detailed architecture and PlantUML diagrams in [`docs/detailed-design`](docs/detailed-design)
- Shared .NET building blocks in [`src/shared`](src/shared)
- An API gateway based on YARP in [`src/gateway`](src/gateway)
- Bounded-context service scaffolding in [`src/services`](src/services)
- Local development infrastructure and proto contracts in [`tools`](tools)

## Architecture Snapshot

- Backend services target `.NET 9` with nullable reference types enabled and warnings treated as errors
- Architecture follows a bounded-context and Clean Architecture approach
- gRPC contracts for ML integration live under [`tools/proto`](tools/proto)
- Local infrastructure currently centers on Cosmos DB Emulator, PostgreSQL with `pgvector`, Redis, Azurite, and RabbitMQ
- Product requirements define three primary user applications:
  - Patient mobile app: React Native + Expo
  - Clinical portal: Next.js + React + TypeScript
  - Admin portal: Blazor

## Repository Layout

```text
.
|-- docs/
|   |-- specs/
|   |-- detailed-design/
|   `-- repository-structure.md
|-- src/
|   |-- gateway/
|   |-- services/
|   `-- shared/
|-- tools/
|   |-- docker-compose.yml
|   `-- proto/
|-- Directory.Build.props
`-- Directory.Packages.props
```

For a more detailed target structure, see [`docs/repository-structure.md`](docs/repository-structure.md).

## Documentation Map

- High-level requirements: [`docs/specs/L1.md`](docs/specs/L1.md)
- Detailed requirements and acceptance criteria: [`docs/specs/L2.md`](docs/specs/L2.md)
- System architecture overview: [`docs/detailed-design/00-system-architecture/overview.md`](docs/detailed-design/00-system-architecture/overview.md)
- Identity and access design: [`docs/detailed-design/01-identity-and-access/overview.md`](docs/detailed-design/01-identity-and-access/overview.md)
- Scan engine design: [`docs/detailed-design/02-scan-engine/overview.md`](docs/detailed-design/02-scan-engine/overview.md)
- Clinical portal design: [`docs/detailed-design/08-clinical-portal/overview.md`](docs/detailed-design/08-clinical-portal/overview.md)

## Getting Started

### Prerequisites

- `.NET 9 SDK`
- `Docker` with Compose support
- `Python 3.11+` if you want to render PlantUML diagrams locally

### Start Local Infrastructure

```bash
docker compose -f tools/docker-compose.yml up -d
```

This brings up the current local dependencies used by the repository:

- Cosmos DB Emulator
- PostgreSQL + `pgvector`
- Redis
- Azurite
- RabbitMQ

### Restore, Build, and Test

There is not yet a root `.sln` file, so work is currently done project-by-project.

```bash
dotnet restore src/gateway/ClearEyeQ.Gateway/ClearEyeQ.Gateway.csproj
dotnet build src/gateway/ClearEyeQ.Gateway/ClearEyeQ.Gateway.csproj
dotnet test src/shared/ClearEyeQ.SharedKernel.Tests/ClearEyeQ.SharedKernel.Tests.csproj
```

Use the same pattern for the specific service or test project you are changing under `src/services`.

### Render Architecture Diagrams

The repository includes a helper script to render `*.puml` files to `*.png`:

```bash
python -m pip install plantuml
python docs/detailed-design/render-puml.py
```

The current script uses the public PlantUML server configured in [`docs/detailed-design/render-puml.py`](docs/detailed-design/render-puml.py).

## Current State

The repository already includes:

- A mature requirements and architecture document set
- Initial .NET projects for the gateway, shared kernel, and several services
- Proto contracts and local infrastructure definitions

The repository is still assembling:

- A unified root solution/workspace
- End-to-end mobile, clinical portal, and admin portal application code
- Fully wired CI/CD, deployment manifests, and operational documentation promised by the target structure

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for branch, validation, and pull request expectations.

## License

This repository is licensed under the MIT License. See [`LICENSE`](LICENSE).

## Disclaimer

ClearEyeQ is a software and architecture repository. Nothing in this repository is medical advice, a diagnostic claim, or a production-ready medical device implementation.
