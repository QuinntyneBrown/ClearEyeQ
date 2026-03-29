FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/services/notifications/ClearEyeQ.Notifications.API/ClearEyeQ.Notifications.API.csproj", "src/services/notifications/ClearEyeQ.Notifications.API/"]
COPY ["src/services/notifications/ClearEyeQ.Notifications.Infrastructure/ClearEyeQ.Notifications.Infrastructure.csproj", "src/services/notifications/ClearEyeQ.Notifications.Infrastructure/"]
COPY ["src/services/notifications/ClearEyeQ.Notifications.Application/ClearEyeQ.Notifications.Application.csproj", "src/services/notifications/ClearEyeQ.Notifications.Application/"]
COPY ["src/services/notifications/ClearEyeQ.Notifications.Domain/ClearEyeQ.Notifications.Domain.csproj", "src/services/notifications/ClearEyeQ.Notifications.Domain/"]
COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/services/notifications/ClearEyeQ.Notifications.API/ClearEyeQ.Notifications.API.csproj"
COPY . .
WORKDIR "/src/src/services/notifications/ClearEyeQ.Notifications.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Notifications.API.dll"]
