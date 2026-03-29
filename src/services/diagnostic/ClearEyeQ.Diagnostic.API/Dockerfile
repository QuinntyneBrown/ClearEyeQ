FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["src/services/diagnostic/ClearEyeQ.Diagnostic.Domain/ClearEyeQ.Diagnostic.Domain.csproj", "src/services/diagnostic/ClearEyeQ.Diagnostic.Domain/"]
COPY ["src/services/diagnostic/ClearEyeQ.Diagnostic.Application/ClearEyeQ.Diagnostic.Application.csproj", "src/services/diagnostic/ClearEyeQ.Diagnostic.Application/"]
COPY ["src/services/diagnostic/ClearEyeQ.Diagnostic.Infrastructure/ClearEyeQ.Diagnostic.Infrastructure.csproj", "src/services/diagnostic/ClearEyeQ.Diagnostic.Infrastructure/"]
COPY ["src/services/diagnostic/ClearEyeQ.Diagnostic.API/ClearEyeQ.Diagnostic.API.csproj", "src/services/diagnostic/ClearEyeQ.Diagnostic.API/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["tools/proto/diagnostic_ml.proto", "tools/proto/"]

RUN dotnet restore "src/services/diagnostic/ClearEyeQ.Diagnostic.API/ClearEyeQ.Diagnostic.API.csproj"

COPY src/shared/ src/shared/
COPY src/services/diagnostic/ src/services/diagnostic/

WORKDIR "/src/src/services/diagnostic/ClearEyeQ.Diagnostic.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Diagnostic.API.dll"]
