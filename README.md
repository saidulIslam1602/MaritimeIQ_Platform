# MaritimeIQ Platform

A comprehensive, unified maritime operations platform for coastal fleet operations, featuring real-time vessel tracking, environmental compliance monitoring, passenger communications, and AI-driven route optimization.

## 🚢 Platform Overview

This consolidated platform integrates all maritime operations into a single, high-performance Web API application, providing:

- **Real-time AIS vessel tracking and analytics**
- **Environmental compliance monitoring** (CO2, NOx, SOx emissions)
- **Passenger notification system** (boarding, delays, Northern Lights alerts)
- **AI-driven route optimization** with weather and aurora integration
- **Comprehensive REST API** for all maritime operations

## 🏗️ Architecture

- **Framework**: .NET 8.0 Web API
- **Pattern**: Service-oriented architecture with dependency injection
- **Container**: Multi-stage Docker build optimized for production
- **CI/CD**: Azure DevOps Pipeline with enhanced security scanning
- **Deployment**: Azure Container Apps / Kubernetes ready

## 🌟 Key Features

### AIS Processing Service
- Real-time vessel position tracking
- Fleet analytics and performance metrics
- Safety alerts and geofence monitoring
- MMSI-based vessel identification

### Environmental Monitoring Service
- Real-time emission tracking (CO2, NOx, SOx)
- Hybrid battery optimization monitoring
- Regulatory compliance reporting
- Environmental alert system

### Passenger Notification Service
- Automated boarding notifications
- Northern Lights viewing alerts
- Delay and schedule update communications
- Multi-channel passenger engagement

### Route Optimization Service
- AI-driven route planning
- Weather condition integration
- Aurora viewing opportunity optimization
- Fuel efficiency maximization
- Passenger comfort prioritization

## 🚀 Quick Start

### Local Development
```bash
# Clone and build
git clone <repository-url>
cd azure_container_service
dotnet restore
dotnet build
dotnet run

# Access Swagger UI at: http://localhost:5000/swagger
```

### Using Docker
```bash
# Build and run with Docker
docker build -f deployment/docker/Dockerfile -t maritime-platform .
docker run -p 5000:8080 maritime-platform

# Or use Docker Compose for development environment
cd deployment/docker && docker-compose up -d
```

## 📡 API Endpoints

### AIS Processing API
- `GET /api/ais/analytics` - Get fleet AIS analytics
- `POST /api/ais/process-data` - Process AIS vessel data

### Environmental Monitoring API  
- `GET /api/environmental/compliance-report` - Get compliance status
- `POST /api/environmental/process-environmental-data` - Process emissions data
- `GET /api/environmental/alerts` - Get environmental alerts

### Passenger Notification API
- `GET /api/passengernotification/summary` - Get notification summary
- `GET /api/passengernotification/northern-lights-conditions` - Check aurora conditions
- `POST /api/passengernotification/send-delay-notification` - Send delay alerts

### Route Optimization API
- `POST /api/routeoptimization/optimize-fleet-routes` - Optimize all fleet routes
- `GET /api/routeoptimization/optimization-status` - Get optimization status
- `GET /api/routeoptimization/weather-impact` - Get weather impact analysis

## 🗂️ Project Structure

```
├── Controllers/                    # REST API controllers (4 controllers)
├── Services/                      # Business logic services (4 services)  
├── Models/                        # Data models and DTOs
├── Data/                         # Data access layer
├── config/                       # Configuration files
│   ├── appsettings.json          # Development settings
│   └── appsettings.Production.json # Production settings
├── deployment/                   # Deployment configurations
│   ├── docker/                   # Docker files and compose
│   └── kubernetes/               # K8s manifests
├── devops/                       # CI/CD and automation
│   ├── pipelines/                # Pipeline definitions
│   └── scripts/                  # Deployment scripts
├── analytics/                    # Business intelligence
│   ├── powerbi/                  # Power BI configurations
│   └── stream-analytics/         # Stream processing
└── docs/                         # Documentation
```

📋 **See [PROJECT-STRUCTURE.md](docs/PROJECT-STRUCTURE.md) for detailed folder organization.**

## 🌊 Maritime Features

- **Real-time vessel tracking** for maritime fleet operations
- **Environmental compliance** monitoring (CO2, NOx, SOx)
- **Weather-based alerts** for enhanced passenger experience
- **AI route optimization** with weather integration
- **Hybrid propulsion** monitoring and battery optimization

## 🚀 Production Deployment

The platform includes comprehensive deployment automation:
- Azure DevOps pipeline with security scanning
- Container orchestration with Docker Compose
- Kubernetes manifests for scalable deployment
- Environment-specific configuration management

---
*Consolidated maritime platform for digital fleet operations*
```

## Production Considerations
- Multi-region deployment for global vessel operations
- Security scanning for compliance
- Automated rollback capabilities
- Integration with maritime IoT sensors

---
*Built for digital transformation of maritime operations*