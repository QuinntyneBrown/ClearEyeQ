FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["src/services/predictive/ClearEyeQ.Predictive.Domain/ClearEyeQ.Predictive.Domain.csproj", "src/services/predictive/ClearEyeQ.Predictive.Domain/"]
COPY ["src/services/predictive/ClearEyeQ.Predictive.Application/ClearEyeQ.Predictive.Application.csproj", "src/services/predictive/ClearEyeQ.Predictive.Application/"]
COPY ["src/services/predictive/ClearEyeQ.Predictive.Infrastructure/ClearEyeQ.Predictive.Infrastructure.csproj", "src/services/predictive/ClearEyeQ.Predictive.Infrastructure/"]
COPY ["src/services/predictive/ClearEyeQ.Predictive.API/ClearEyeQ.Predictive.API.csproj", "src/services/predictive/ClearEyeQ.Predictive.API/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["tools/proto/predictive_ml.proto", "tools/proto/"]

RUN dotnet restore "src/services/predictive/ClearEyeQ.Predictive.API/ClearEyeQ.Predictive.API.csproj"

COPY src/shared/ src/shared/
COPY src/services/predictive/ src/services/predictive/

WORKDIR "/src/src/services/predictive/ClearEyeQ.Predictive.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Predictive.API.dll"]
