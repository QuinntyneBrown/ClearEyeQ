FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/services/treatment/ClearEyeQ.Treatment.API/ClearEyeQ.Treatment.API.csproj", "src/services/treatment/ClearEyeQ.Treatment.API/"]
COPY ["src/services/treatment/ClearEyeQ.Treatment.Infrastructure/ClearEyeQ.Treatment.Infrastructure.csproj", "src/services/treatment/ClearEyeQ.Treatment.Infrastructure/"]
COPY ["src/services/treatment/ClearEyeQ.Treatment.Application/ClearEyeQ.Treatment.Application.csproj", "src/services/treatment/ClearEyeQ.Treatment.Application/"]
COPY ["src/services/treatment/ClearEyeQ.Treatment.Domain/ClearEyeQ.Treatment.Domain.csproj", "src/services/treatment/ClearEyeQ.Treatment.Domain/"]
COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/services/treatment/ClearEyeQ.Treatment.API/ClearEyeQ.Treatment.API.csproj"
COPY . .
WORKDIR "/src/src/services/treatment/ClearEyeQ.Treatment.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Treatment.API.dll"]
