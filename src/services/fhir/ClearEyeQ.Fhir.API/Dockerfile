FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/services/fhir/ClearEyeQ.Fhir.API/ClearEyeQ.Fhir.API.csproj", "src/services/fhir/ClearEyeQ.Fhir.API/"]
COPY ["src/services/fhir/ClearEyeQ.Fhir.Application/ClearEyeQ.Fhir.Application.csproj", "src/services/fhir/ClearEyeQ.Fhir.Application/"]
COPY ["src/services/fhir/ClearEyeQ.Fhir.Infrastructure/ClearEyeQ.Fhir.Infrastructure.csproj", "src/services/fhir/ClearEyeQ.Fhir.Infrastructure/"]
COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/services/fhir/ClearEyeQ.Fhir.API/ClearEyeQ.Fhir.API.csproj"
COPY . .
WORKDIR "/src/src/services/fhir/ClearEyeQ.Fhir.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Fhir.API.dll"]
