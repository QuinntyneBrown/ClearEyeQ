FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/gateway/ClearEyeQ.Gateway/ClearEyeQ.Gateway.csproj", "src/gateway/ClearEyeQ.Gateway/"]
RUN dotnet restore "src/gateway/ClearEyeQ.Gateway/ClearEyeQ.Gateway.csproj"

COPY . .
WORKDIR "/src/src/gateway/ClearEyeQ.Gateway"
RUN dotnet build "ClearEyeQ.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ClearEyeQ.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ClearEyeQ.Gateway.dll"]
