# MaritimeIQ Platform - Azure Deployment Guide

This directory contains all deployment configurations and scripts for deploying the complete MaritimeIQ Platform to Azure.

## 🚀 Quick Deployment

To deploy the entire platform to your Azure subscription:

```bash
# Navigate to project root
cd /path/to/MaritimeIQ_Platform

# Run the complete deployment script
./deployment/deploy-full-platform.sh
```

## 📋 What Gets Deployed

The deployment script creates the following Azure resources:

### **Core Infrastructure**
- **Resource Group**: `maritime-platform-rg`
- **Azure SQL Database**: Platform data storage
- **Azure Storage Account**: File and blob storage
- **Key Vault**: Secrets management
- **Application Insights**: Monitoring and analytics

### **Application Services**
- **Container Apps**: MaritimeIQ API (.NET 8)
- **Static Web Apps**: Maritime Intelligence Dashboard (Next.js)
- **Azure Functions**: Background processing and IoT data handling

### **Data & Analytics**
- **Event Hubs**: Real-time data streaming
- **Stream Analytics**: Live data processing
- **Service Bus**: Message queuing
- **Cognitive Services**: AI/ML capabilities

### **Optional Components**
- **Power BI**: Business intelligence dashboards
- **IoT Hub**: Device connectivity
- **Logic Apps**: Workflow automation

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     Azure Subscription                          │
├─────────────────────────────────────────────────────────────────┤
│  Resource Group: maritime-platform-rg                          │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  │   Container     │    │   Static Web    │    │   Azure         │
│  │   Apps          │    │   Apps          │    │   Functions     │
│  │   (API)         │    │   (Dashboard)   │    │   (Background)  │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘
│           │                       │                       │      │
│           └───────────────────────┼───────────────────────┘      │
│                                   │                              │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  │   SQL Database  │    │   Event Hubs    │    │   Service Bus   │
│  │                 │    │                 │    │                 │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  │   Key Vault     │    │   Storage       │    │   App Insights  │
│  │                 │    │   Account       │    │                 │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘
└─────────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
deployment/
├── deploy-full-platform.sh     # Complete deployment script
├── azure/
│   ├── azure-infrastructure.json   # ARM template for Azure resources
│   └── parameters.json             # Deployment parameters
├── docker/
│   ├── Dockerfile                  # API container configuration
│   ├── docker-compose.yml          # Local development setup
│   └── docker-compose.prod.yml     # Production configuration
├── kubernetes/                     # K8s deployment (optional)
├── data-factory/                   # Data pipeline definitions
├── monitoring/                     # Monitoring configuration
└── scripts/                        # Utility scripts
```

## 🔧 Prerequisites

Before deploying, ensure you have:

1. **Azure CLI** installed and configured
2. **.NET 8 SDK** for building the API
3. **Node.js 18+** for building the dashboard
4. **Azure subscription** with appropriate permissions
5. **Resource creation rights** in your subscription

## 🚀 Step-by-Step Deployment

### 1. Prepare Environment

```bash
# Clone/navigate to the project
cd MaritimeIQ_Platform

# Ensure all prerequisites are met
az --version
dotnet --version
node --version
```

### 2. Configure Deployment

Edit the configuration variables in `deploy-full-platform.sh` if needed:

```bash
# Main configuration
PLATFORM_NAME="maritime-platform"
RESOURCE_GROUP="maritime-platform-rg"
LOCATION="norwayeast"
```

### 3. Run Deployment

```bash
# Make script executable
chmod +x deployment/deploy-full-platform.sh

# Run deployment
./deployment/deploy-full-platform.sh
```

### 4. Monitor Progress

The script will:
1. ✅ Check prerequisites
2. ✅ Set up Azure authentication
3. ✅ Deploy Azure infrastructure (ARM template)
4. ✅ Build and deploy the API
5. ✅ Build and deploy the dashboard
6. ✅ Configure monitoring
7. ✅ Display deployment summary

## 🔗 Post-Deployment Access

After successful deployment, you'll receive:

- **API Endpoint**: `https://maritime-platform-api-xxxxx.norwayeast.azurecontainerapps.io`
- **Dashboard URL**: `https://maritime-platform-dashboard-xxxxx.azurestaticapps.net`
- **Swagger Documentation**: `https://[api-endpoint]/swagger`
- **Azure Portal Link**: Direct link to your resource group

## 🛠️ Manual Deployment Options

### Deploy Individual Components

```bash
# Deploy only infrastructure
az deployment group create --resource-group maritime-platform-rg --template-file deployment/azure/azure-infrastructure.json

# Deploy only API
az containerapp up --source . --name maritime-platform-api

# Deploy only dashboard
cd maritime-dashboard && npm run build
```

### Local Development

```bash
# Run locally with Docker Compose
docker-compose -f deployment/docker/docker-compose.yml up

# Or run components individually
dotnet run  # API
cd maritime-dashboard && npm run dev  # Dashboard
```

## 🔒 Security Considerations

- **SQL passwords** are auto-generated and stored in Key Vault
- **API keys** are managed through Azure managed identity
- **HTTPS** is enforced for all endpoints
- **CORS** is configured for dashboard-API communication

## 📊 Monitoring & Logging

- **Application Insights** tracks performance and errors
- **Azure Monitor** provides infrastructure metrics
- **Log Analytics** centralizes log data
- **Custom dashboards** available in Azure portal

## 🆘 Troubleshooting

### Common Issues

1. **Authentication Error**
   ```bash
   az login
   az account set --subscription "your-subscription-id"
   ```

2. **Resource Name Conflicts**
   - Resource names include unique identifiers
   - Check Azure portal for existing resources

3. **Permission Issues**
   - Ensure you have Owner/Contributor role
   - Contact your Azure administrator

### Support Resources

- **Azure Documentation**: https://docs.microsoft.com/azure/
- **Container Apps**: https://docs.microsoft.com/azure/container-apps/
- **Static Web Apps**: https://docs.microsoft.com/azure/static-web-apps/

## 🔄 Updates & Maintenance

To update the deployed platform:

```bash
# Update API
az containerapp revision copy --name maritime-platform-api --resource-group maritime-platform-rg

# Update dashboard
cd maritime-dashboard
npm run build
# Use SWA CLI or GitHub Actions for deployment
```

---

**🚢 Ready to deploy your MaritimeIQ Platform to Azure? Run the deployment script and set sail!**