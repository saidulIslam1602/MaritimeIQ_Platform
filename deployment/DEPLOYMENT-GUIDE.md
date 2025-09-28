# Maritime Platform Azure Deployment Guide

This guide will help you deploy the Maritime Data Engineering Platform infrastructure to your Azure subscription.

## Prerequisites

1. **Azure CLI installed**: Download from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
2. **Azure subscription**: Active Azure subscription with Owner or Contributor permissions
3. **Resource Group**: Create or identify target resource group

## Quick Deployment Steps

### 1. Login to Azure CLI

```bash
az login
```

### 2. Set Your Subscription (if you have multiple)

```bash
az account list --output table
az account set --subscription "your-subscription-id"
```

### 3. Create Resource Group (if needed)

```bash
az group create --name "maritime-platform-rg" --location "East US"
```

### 4. Deploy the Infrastructure

Navigate to the deployment directory and deploy:

```bash
cd deployment/azure
az deployment group create \
  --resource-group "maritime-platform-rg" \
  --template-file azure-infrastructure.json \
  --parameters appName="maritime-platform" \
               uniqueId="$(date +%s | tail -c 6)" \
               sqlAdminPassword="YourSecurePassword123!"
```

### Alternative: Use Azure Portal

1. **Login to Azure Portal**: https://portal.azure.com
2. **Create Resource** → **Template deployment (deploy using custom templates)**
3. **Build your own template in the editor**
4. Copy and paste the contents of `azure-infrastructure.json`
5. **Save** and **Purchase**
6. Fill in the parameters:
   - **Resource Group**: Select or create new
   - **App Name**: `maritime-platform`
   - **Unique Id**: Use a 5-6 digit number (e.g., `123456`)
   - **Sql Admin Password**: Strong password for SQL server
   - **Location**: Select your preferred region

## Deployed Resources

The template will create:

- **SQL Server & Database**: For maritime data storage
- **Storage Account**: Blob storage for data files
- **Event Hub Namespace**: Real-time data streaming
- **Stream Analytics Job**: Data processing pipeline  
- **Application Insights**: Monitoring and telemetry
- **Key Vault**: Secret management
- **Service Bus**: Message queuing
- **Cognitive Services**: AI/ML capabilities

## Post-Deployment Steps

1. **Configure Event Hub**: Add consumer groups and access policies
2. **Set up Stream Analytics**: Configure inputs/outputs and queries
3. **Key Vault Access**: Grant permissions to applications
4. **Application Insights**: Link to your applications
5. **Service Bus**: Create additional queues/topics as needed

## Monitoring Deployment

Check deployment status:

```bash
az deployment group show \
  --resource-group "maritime-platform-rg" \
  --name "azure-infrastructure"
```

## Troubleshooting

- **Unique naming conflicts**: Increase the `uniqueId` parameter
- **SQL password requirements**: Ensure password meets complexity requirements
- **Resource quotas**: Check subscription limits for deployed services
- **Permissions**: Verify you have sufficient permissions in the resource group

## Clean Up Resources

To remove all deployed resources:

```bash
az group delete --name "maritime-platform-rg" --yes --no-wait
```

## Security Notes

- Change default SQL admin password immediately after deployment
- Configure Key Vault access policies
- Enable Azure Security Center recommendations
- Review NSG rules and firewall settings

## Dashboard Deployment

Your maritime platform includes a Next.js dashboard for real-time monitoring and visualization.

### Option 1: Azure Static Web Apps (Recommended)

```bash
# Navigate to dashboard directory
cd maritime-dashboard

# Make deployment script executable
chmod +x deploy.sh

# Deploy to Azure Static Web Apps
./deploy.sh
```

### Option 2: Azure App Service

```bash
# Build the dashboard
cd maritime-dashboard
npm install
npm run build
npm run export

# Deploy to App Service (requires existing App Service)
az webapp deployment source config-zip \
  --resource-group "maritime-platform-rg" \
  --name "your-app-service-name" \
  --src out.zip
```

### Option 3: Local Development

```bash
cd maritime-dashboard
npm install
npm run dev
# Access at http://localhost:3000
```

### Dashboard Features

- 🌊 **Real-time Fleet Monitoring**: Live vessel tracking and status
- 📊 **Environmental Compliance**: Emissions and environmental data
- 🗺️ **Route Optimization**: Interactive maps with route insights
- 📈 **System Health**: Infrastructure monitoring dashboards
- 🔔 **Alert Management**: Real-time notifications and alerts

## Next Steps

After successful deployment:
1. **Deploy the Dashboard**: Follow dashboard deployment instructions above
2. Configure application connections using output values
3. Set up monitoring dashboards in Application Insights
4. Configure Stream Analytics queries for your data processing needs
5. Set up CI/CD pipelines for application deployments
6. **Connect Dashboard to Backend**: Update API endpoints in dashboard configuration