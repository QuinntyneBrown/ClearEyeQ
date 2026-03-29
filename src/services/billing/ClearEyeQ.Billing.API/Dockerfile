FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/services/billing/ClearEyeQ.Billing.API/ClearEyeQ.Billing.API.csproj", "src/services/billing/ClearEyeQ.Billing.API/"]
COPY ["src/services/billing/ClearEyeQ.Billing.Infrastructure/ClearEyeQ.Billing.Infrastructure.csproj", "src/services/billing/ClearEyeQ.Billing.Infrastructure/"]
COPY ["src/services/billing/ClearEyeQ.Billing.Application/ClearEyeQ.Billing.Application.csproj", "src/services/billing/ClearEyeQ.Billing.Application/"]
COPY ["src/services/billing/ClearEyeQ.Billing.Domain/ClearEyeQ.Billing.Domain.csproj", "src/services/billing/ClearEyeQ.Billing.Domain/"]
COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/services/billing/ClearEyeQ.Billing.API/ClearEyeQ.Billing.API.csproj"
COPY . .
WORKDIR "/src/src/services/billing/ClearEyeQ.Billing.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Billing.API.dll"]
