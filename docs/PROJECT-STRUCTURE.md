# Project Structure Documentation

## Havila Maritime Platform - Organized Structure

This document describes the organized folder structure of the Havila Maritime Platform.

## 📁 Root Directory Structure

```
├── src/                          # Source code (core application)
│   ├── Controllers/              # REST API controllers
│   ├── Services/                 # Business logic services
│   ├── Models/                   # Data models and DTOs
│   ├── Data/                     # Data access layer
│   └── Functions/                # Original Azure Functions (excluded from build)
├── config/                       # Configuration files
│   ├── appsettings.json          # Development configuration
│   └── appsettings.Production.json # Production configuration
├── deployment/                   # Deployment configurations
│   ├── docker/                   # Docker-related files
│   │   ├── Dockerfile            # Container definition
│   │   ├── docker-compose.yml    # Development environment
│   │   ├── docker-compose.prod.yml # Production environment
│   │   └── nginx.conf            # Web server configuration
│   └── kubernetes/               # Kubernetes manifests
│       └── havila-maritime-deployment.yaml
├── devops/                       # DevOps and automation
│   ├── pipelines/                # CI/CD pipeline definitions
│   │   └── azure-pipelines-enhanced.yml
│   └── scripts/                  # Deployment and automation scripts
│       └── deploy-havila-maritime-platform.sh
├── analytics/                    # Analytics and reporting
│   ├── powerbi/                  # Power BI configurations
│   │   └── havila-workspace-config.json
│   └── stream-analytics/         # Stream Analytics queries
│       └── havila-maritime-analytics.sql
├── docs/                         # Documentation
└── bin/, obj/                    # Build artifacts (auto-generated)
```

## 🎯 Folder Purposes

### `/config/`
**Purpose**: Centralized configuration management
- Environment-specific settings
- Application configuration files
- Secrets configuration (development)

### `/deployment/`
**Purpose**: All deployment-related configurations
- **`/docker/`**: Container definitions and orchestration
- **`/kubernetes/`**: K8s manifests for container orchestration

### `/devops/`
**Purpose**: DevOps automation and CI/CD
- **`/pipelines/`**: Azure DevOps, GitHub Actions, etc.
- **`/scripts/`**: Deployment automation scripts

### `/analytics/`
**Purpose**: Business intelligence and data processing
- **`/powerbi/`**: Power BI workspace configurations
- **`/stream-analytics/`**: Real-time data processing queries

### `/docs/` (Future)
**Purpose**: Technical documentation
- API documentation
- Architecture diagrams
- Deployment guides

## 🔧 Build Configuration

The project file (`MaritimeIQ.Platform.csproj`) has been updated to:
- Copy configuration files from `/config/` to output directory
- Exclude the `/Functions/` folder from compilation
- Maintain compatibility with the organized structure

## 🚀 Usage

### Development
```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Docker development
cd deployment/docker
docker-compose up -d
```

### Production Deployment
```bash
# Azure deployment
./devops/scripts/deploy-havila-maritime-platform.sh

# Kubernetes deployment
kubectl apply -f deployment/kubernetes/
```

## 📝 Benefits of This Structure

1. **Clear Separation of Concerns**: Each folder has a specific purpose
2. **Environment Management**: Configuration files centralized in `/config/`
3. **Deployment Flexibility**: All deployment options organized in `/deployment/`
4. **DevOps Ready**: CI/CD pipelines and scripts in `/devops/`
5. **Analytics Integration**: Business intelligence components in `/analytics/`
6. **Scalable**: Easy to add new components in appropriate folders

## 🔄 Migration Notes

If you need to reference files in their new locations:
- Configuration files: `config/appsettings.json`
- Docker files: `deployment/docker/Dockerfile`
- Pipeline files: `devops/pipelines/azure-pipelines-enhanced.yml`
- Deployment scripts: `devops/scripts/deploy-havila-maritime-platform.sh`

The application will continue to work as before, but with a much cleaner and more professional structure.