# Base 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY "src" .
RUN dotnet restore "./CLI/ARCSummary.fsproj"
RUN dotnet build "./CLI/ARCSummary.fsproj" -c $BUILD_CONFIGURATION -o /build

# Publish stage
FROM build AS publish
RUN dotnet publish "./CLI/ARCSummary.fsproj" -c $BUILD_CONFIGURATION -o /publish

# Runtime Stage in minimal environment
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /tool
COPY --from=publish /publish .
RUN chmod +x /tool/ARCSummary
ENTRYPOINT ["/tool/ARCSummary"]