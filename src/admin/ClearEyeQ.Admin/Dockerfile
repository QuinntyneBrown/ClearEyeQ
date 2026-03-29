FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/shared/ClearEyeQ.SharedKernel/ClearEyeQ.SharedKernel.csproj", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["src/admin/ClearEyeQ.Admin/ClearEyeQ.Admin.csproj", "src/admin/ClearEyeQ.Admin/"]

RUN dotnet restore "src/admin/ClearEyeQ.Admin/ClearEyeQ.Admin.csproj"

COPY ["src/shared/ClearEyeQ.SharedKernel/", "src/shared/ClearEyeQ.SharedKernel/"]
COPY ["src/admin/ClearEyeQ.Admin/", "src/admin/ClearEyeQ.Admin/"]

WORKDIR "/src/src/admin/ClearEyeQ.Admin"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClearEyeQ.Admin.dll"]
