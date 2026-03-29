# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution-level files
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Copy shared kernel
COPY src/shared/ClearEyeQ.SharedKernel/ src/shared/ClearEyeQ.SharedKernel/

# Copy identity service projects
COPY src/services/identity/ClearEyeQ.Identity.Domain/ src/services/identity/ClearEyeQ.Identity.Domain/
COPY src/services/identity/ClearEyeQ.Identity.Application/ src/services/identity/ClearEyeQ.Identity.Application/
COPY src/services/identity/ClearEyeQ.Identity.Infrastructure/ src/services/identity/ClearEyeQ.Identity.Infrastructure/
COPY src/services/identity/ClearEyeQ.Identity.API/ src/services/identity/ClearEyeQ.Identity.API/

# Restore and build
WORKDIR /src/src/services/identity/ClearEyeQ.Identity.API
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ClearEyeQ.Identity.API.dll"]
