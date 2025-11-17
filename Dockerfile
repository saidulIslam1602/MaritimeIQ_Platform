# MaritimeIQ Platform Docker Container Configuration
# Multi-stage build for optimized production deployment
# Supports .NET 8 with Azure integration for maritime operations

# Stage 1: Base runtime image for production deployment
# Uses official Microsoft .NET 8 runtime for optimal performance
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# HTTP port for API endpoints
EXPOSE 8080

# Stage 2: Build environment with .NET 8 SDK
# Includes all build tools and dependencies for compilation
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first for better Docker layer caching
# This allows Docker to cache the restore step if only source code changes
COPY ["MaritimeIQ.Platform.csproj", "."]
RUN dotnet restore "MaritimeIQ.Platform.csproj"

# Copy all source code and build the application
COPY . .
RUN dotnet build "MaritimeIQ.Platform.csproj" -c Release -o /app/build

# Stage 3: Publish stage for optimized deployment package
# Creates self-contained deployment package (not self-contained for smaller image)
FROM build AS publish
RUN dotnet publish "MaritimeIQ.Platform.csproj" -c Release -o /app/publish --self-contained false

# Stage 4: Final production image
# Combines runtime base with published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment configuration for production deployment
# Bind to all interfaces for container networking
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# Production environment settings
ENV ASPNETCORE_ENVIRONMENT=Production

# Application entry point
# Starts the MaritimeIQ Platform API server
ENTRYPOINT ["dotnet", "MaritimeIQ.Platform.dll"]
