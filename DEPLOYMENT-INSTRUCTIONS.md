# 🚢 MaritimeIQ Platform - Azure Deployment Instructions

## 📋 Complete Platform Deployment to Your Azure Portal

This guide will deploy the **entire MaritimeIQ Platform** (not just the dashboard) to your Azure subscription linked to: **[https://portal.azure.com/#home](https://portal.azure.com/#home)**

## 🎯 What Will Be Deployed

### **Core Platform Components**
✅ **API Backend** (.NET 8) - All controllers, services, and business logic  
✅ **Maritime Dashboard** (Next.js React) - Organized with our new structure  
✅ **Azure Functions** - Background processing and IoT handling  
✅ **SQL Database** - All maritime data storage  
✅ **Event Hubs** - Real-time data streaming  
✅ **Application Insights** - Monitoring and analytics  
✅ **Key Vault** - Secure secrets management  
✅ **Container Apps** - Scalable application hosting  
✅ **Static Web Apps** - Dashboard hosting  

### **Enhanced Features Included**
✅ All organized components and services  
✅ Updated TypeScript types and utilities  
✅ Clean path mappings and imports  
✅ Generic maritime platform (non-Havila specific)  
✅ Modern deployment architecture  

## 🚀 Quick Start Deployment

### **Option 1: One-Click Full Deployment (Recommended)**

```bash
# Navigate to your project directory
cd /home/saidul/Desktop/Portfolio_Project/MaritimeIQ_Platform

# Run the complete deployment script
./deployment/deploy-full-platform.sh
```

This single script will:
1. ✅ Check all prerequisites
2. ✅ Authenticate with your Azure account
3. ✅ Create all Azure resources
4. ✅ Build and deploy the API
5. ✅ Build and deploy the dashboard
6. ✅ Configure monitoring and alerts
7. ✅ Provide you with all access URLs

### **Option 2: Step-by-Step Deployment**

If you prefer more control:

```bash
# 1. Deploy Azure infrastructure
cd deployment
./deploy.sh

# 2. Deploy the dashboard separately
cd ../maritime-dashboard
./deploy.sh
```

## 📊 Expected Azure Resources

After deployment, your Azure Portal will contain:

```
📁 Resource Group: maritime-platform-rg
├── 🏗️  Container App: maritime-platform-api
├── 🌐 Static Web App: maritime-platform-dashboard
├── 🗄️  SQL Database: maritime-platform-db
├── 📡 Event Hub Namespace: ehns-maritime-platform
├── 🔑 Key Vault: kv-maritime-platform
├── 📊 Application Insights: maritime-platform-insights
├── 💾 Storage Account: stmaritimeplatform
├── 🚌 Service Bus Namespace: sb-maritime-platform
├── ⚡ Function App: maritime-platform-functions
└── 🧠 Cognitive Services: csmaritimeplatform
```

## 🔗 Post-Deployment Access URLs

You'll receive URLs like:
- **API**: `https://maritime-platform-api-xxxxx.norwayeast.azurecontainerapps.io`
- **Dashboard**: `https://maritime-platform-dashboard-xxxxx.azurestaticapps.net`
- **Swagger Docs**: `https://[api-url]/swagger`
- **Azure Portal**: Direct link to your resource group

## 📋 Prerequisites Check

Before running deployment, ensure you have:

```bash
# Check Azure CLI
az --version
# Should show version 2.0+

# Check .NET SDK
dotnet --version
# Should show version 8.0+

# Check Node.js
node --version
# Should show version 18.0+

# Login to Azure
az login
# This will open browser for authentication
```

## 🔧 Configuration Options

The deployment script uses these default settings (you can modify in `deploy-full-platform.sh`):

```bash
PLATFORM_NAME="maritime-platform"
RESOURCE_GROUP="maritime-platform-rg"
LOCATION="norwayeast"  # Close to your region
```

## 🛠️ Troubleshooting

### **Common Issues & Solutions**

1. **"Resource already exists" error**
   ```bash
   # The script handles existing resources gracefully
   # Re-run the script - it will skip existing resources
   ```

2. **Authentication issues**
   ```bash
   az logout
   az login
   az account set --subscription "your-subscription-name"
   ```

3. **Permission errors**
   - Ensure you have Owner or Contributor role on the subscription
   - Contact your Azure administrator if needed

## 🎉 Success Indicators

✅ **Successful deployment** shows:
- Green success messages for each component
- Working API URL with Swagger documentation
- Functional dashboard with real-time data
- All Azure resources visible in your portal

## 📚 Next Steps After Deployment

1. **Test the API**: Visit the Swagger URL to test endpoints
2. **Access Dashboard**: Open the dashboard URL to see the organized interface
3. **Configure Alerts**: Set up monitoring in Application Insights
4. **Custom Domain**: Configure custom domains if needed
5. **CI/CD Setup**: Set up automated deployments

## 🆘 Support

If you encounter issues:
1. Check the deployment logs for error messages
2. Verify all prerequisites are installed
3. Ensure your Azure subscription has available quota
4. Review the Azure Portal for any failed deployments

---

## 🚢 Ready to Deploy?

Run this command to start your complete MaritimeIQ Platform deployment:

```bash
./deployment/deploy-full-platform.sh
```

Your entire maritime intelligence platform will be live in Azure within 15-20 minutes! 🎯