FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/services/clinical/ClearEyeQ.Clinical.API/ClearEyeQ.Clinical.API.csproj", "src/services/clinical/ClearEyeQ.Clinical.API/"]
COPY ["src/services/clinical/ClearEyeQ.Clinical.Application/ClearEyeQ.Clinical.Application.csproj", "src/services/clinical/ClearEyeQ.Clinical.Application/"]
COPY ["src/services/clinical/ClearEyeQ.Clinical.Infrastructure/ClearEyeQ.Clinical.Infrastructure.csproj", "src/services/clinical/ClearEyeQ.Clinical.Infrastructure/"]
COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/services/clinical/ClearEyeQ.Clinical.API/ClearEyeQ.Clinical.API.csproj"
COPY . .
WORKDIR "/src/src/services/clinical/ClearEyeQ.Clinical.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Clinical.API.dll"]
