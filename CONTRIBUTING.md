# Contributing to ClearEyeQ

## Ground Rules

- Keep changes focused. Small, reviewable pull requests are preferred over broad repo-wide edits.
- Align code, docs, and diagrams. If you change architecture, contracts, or behavior, update the corresponding documentation in `docs/`.
- Do not commit secrets, credentials, real patient data, or vendor keys. Use synthetic data only.
- Match the repository conventions in [`.editorconfig`](.editorconfig).

## Before You Start

- For non-trivial work, open or reference an issue so the intended change is clear before implementation starts.
- If you are changing architecture or requirements, identify the relevant files in `docs/specs/` and `docs/detailed-design/` up front.
- If you are adding a new service or major module, follow the bounded-context layout already used under `src/services/`.

## Development Expectations

### .NET

- Projects target `net9.0`.
- Nullable reference types are enabled.
- Warnings are treated as errors through `Directory.Build.props`.
- Shared package versions are managed centrally through `Directory.Packages.props`.

### Repository State

- The repository does not yet have a root `.sln` file.
- Build and test the specific projects you touch instead of assuming a single top-level command exists.
- Some directories are scaffolding-first. Avoid overstating maturity in docs or pull requests.

## Validation

Run the checks that match your change before opening a pull request.

### Code Changes

- Restore, build, and test the impacted projects with `dotnet restore`, `dotnet build`, and `dotnet test`.
- If you modify gateway routing or service startup code, validate the application starts cleanly where practical.
- If you add or update dependencies, keep them aligned with central package management.

Examples:

```bash
dotnet restore src/gateway/ClearEyeQ.Gateway/ClearEyeQ.Gateway.csproj
dotnet build src/gateway/ClearEyeQ.Gateway/ClearEyeQ.Gateway.csproj
dotnet test src/shared/ClearEyeQ.SharedKernel.Tests/ClearEyeQ.SharedKernel.Tests.csproj
```

### Documentation and Diagram Changes

- Update the relevant markdown files in `docs/`.
- If you change a `*.puml` file, regenerate the matching `*.png` output.

```bash
python -m pip install plantuml
python docs/detailed-design/render-puml.py
```

## Pull Requests

Each pull request should include:

- A clear summary of what changed
- The reason for the change
- Linked issue or discussion when applicable
- Notes on validation performed
- Screenshots or rendered diagrams when the change affects visuals or design artifacts

## Commit Guidance

- Use clear, descriptive commit messages.
- Separate unrelated refactors from functional changes.
- Avoid mixing formatting-only edits with logic or architecture changes unless there is a good reason.

## Review Criteria

Reviewers will generally look for:

- Correctness and clarity
- Consistency with the existing architecture and folder layout
- Adequate tests for code changes
- Accurate, updated documentation for architecture or workflow changes
- No secrets, real data, or unnecessary generated artifacts

## Questions

If repository structure, ownership, or direction is unclear, ask in the issue or pull request before expanding the scope of the change.
