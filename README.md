# Havila Kystruten Maritime Services - Container Demo

This repository demonstrates automated container builds for maritime microservices using Azure Container Registry Tasks.

## Architecture
- **Language**: C# (.NET 8.0)
- **Container**: Multi-stage Docker build
- **CI/CD**: Azure Container Registry Tasks
- **Deployment**: Azure Container Instances / AKS

## Maritime Service Features
- Health monitoring endpoints
- API versioning for service compatibility  
- Container-ready configuration
- Production logging and metrics

## ACR Tasks Workflow
1. **Code Push** → Triggers ACR Task
2. **Build Phase** → Restore dependencies, compile C# code
3. **Test Phase** → Run unit tests, validate APIs
4. **Container Phase** → Build optimized Docker image
5. **Deploy Phase** → Push to registry, tag for environments

## Quick Start
```bash
# Local development
dotnet restore
dotnet build
dotnet run

# Container testing
docker build -t maritime-api .
docker run -p 8080:8080 maritime-api
```

## Production Considerations
- Multi-region deployment for global vessel operations
- Security scanning for compliance
- Automated rollback capabilities
- Integration with maritime IoT sensors

---
*Built for Havila Kystruten's digital transformation of maritime operations*