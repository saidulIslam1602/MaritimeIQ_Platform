# Multi-stage Dockerfile for Havila Maritime API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AcrTasksDemo.csproj", "."]
COPY ["Functions/HavilaKystruten.Maritime.Functions.csproj", "Functions/"]

# Restore dependencies
RUN dotnet restore "AcrTasksDemo.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src"
RUN dotnet build "AcrTasksDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcrTasksDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && \
    chmod 755 /app/logs

# Add non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AcrTasksDemo.dll"]