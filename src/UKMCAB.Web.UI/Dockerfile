FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["UKMCAB.Web.UI/UKMCAB.Web.UI.csproj", "UKMCAB.Web.UI/"]
COPY ["UKMCAB.Common/UKMCAB.Common.csproj", "UKMCAB.Common/"]
COPY ["UKMCAB.Core/UKMCAB.Core.csproj", "UKMCAB.Core/"]
COPY ["UKMCAB.Data/UKMCAB.Data.csproj", "UKMCAB.Data/"]
COPY ["UKMCAB.Infrastructure/UKMCAB.Infrastructure.csproj", "UKMCAB.Infrastructure/"]
COPY ["UKMCAB.Web/UKMCAB.Web.csproj", "UKMCAB.Web/"]
RUN dotnet restore "./UKMCAB.Web.UI/UKMCAB.Web.UI.csproj"
COPY . .
WORKDIR "/src/UKMCAB.Web.UI"
RUN dotnet build "./UKMCAB.Web.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UKMCAB.Web.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UKMCAB.Web.UI.dll"]