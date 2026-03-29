FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj src/shared/ClearEyeQ.SharedKernel/
COPY src/services/scan/ClearEyeQ.Scan.Domain/ClearEyeQ.Scan.Domain.csproj src/services/scan/ClearEyeQ.Scan.Domain/
COPY src/services/scan/ClearEyeQ.Scan.Application/ClearEyeQ.Scan.Application.csproj src/services/scan/ClearEyeQ.Scan.Application/
COPY src/services/scan/ClearEyeQ.Scan.Infrastructure/ClearEyeQ.Scan.Infrastructure.csproj src/services/scan/ClearEyeQ.Scan.Infrastructure/
COPY src/services/scan/ClearEyeQ.Scan.API/ClearEyeQ.Scan.API.csproj src/services/scan/ClearEyeQ.Scan.API/

RUN dotnet restore src/services/scan/ClearEyeQ.Scan.API/ClearEyeQ.Scan.API.csproj

COPY src/shared/ src/shared/
COPY src/services/scan/ src/services/scan/
COPY tools/proto/ tools/proto/

RUN dotnet publish src/services/scan/ClearEyeQ.Scan.API/ClearEyeQ.Scan.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Scan.API.dll"]
