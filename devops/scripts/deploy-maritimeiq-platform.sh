#!/bin/bash

# MaritimeIQ Platform - Complete Azure Deployment Script
# This script demonstrates the comprehensive Azure services integration
# for the maritime operations platform

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
RESOURCE_GROUP="maritimeiq-platform-rg"
LOCATION="westeurope"
SUBSCRIPTION_ID="${AZURE_SUBSCRIPTION_ID}"
PROJECT_NAME="maritimeiq-platform"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  MaritimeIQ Platform                  ${NC}"
echo -e "${BLUE}  Complete Azure Services Deployment   ${NC}"
echo -e "${BLUE}========================================${NC}"

# Function to print status
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check Azure CLI login
print_status "Checking Azure CLI authentication..."
if ! az account show &> /dev/null; then
    print_error "Please login to Azure CLI first: az login"
    exit 1
fi

# Set subscription
if [ ! -z "$SUBSCRIPTION_ID" ]; then
    print_status "Setting Azure subscription: $SUBSCRIPTION_ID"
    az account set --subscription "$SUBSCRIPTION_ID"
fi

# Create resource group
print_status "Creating resource group: $RESOURCE_GROUP"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --tags Environment=Production Project=MaritimeIQ-Platform Owner=maritime-platform@example.com

echo -e "\n${BLUE}1. Core Infrastructure Services${NC}"

# Key Vault
print_status "Creating Key Vault for secure configuration..."
KEY_VAULT_NAME="${PROJECT_NAME}-kv-$(date +%s | tail -c 6)"

az keyvault create \
    --name "$KEY_VAULT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku Standard \
    --enabled-for-template-deployment true \
    --enabled-for-disk-encryption true

# Application Insights
print_status "Creating Application Insights for monitoring..."
APP_INSIGHTS_NAME="${PROJECT_NAME}-insights"

az monitor app-insights component create \
    --app "$APP_INSIGHTS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --kind web \
    --application-type web

echo -e "\n${BLUE}2. Messaging and Event Streaming${NC}"

# Service Bus Namespace
print_status "Creating Service Bus namespace..."
SERVICE_BUS_NAME="${PROJECT_NAME}-servicebus"

az servicebus namespace create \
    --name "$SERVICE_BUS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku Standard

# Create Service Bus Queues
print_status "Creating Service Bus queues for maritime operations..."
QUEUES=("vessel-data-queue" "route-updates-queue" "safety-alerts-queue" 
        "maintenance-requests-queue" "passenger-notifications-queue" "environmental-data-queue")

for queue in "${QUEUES[@]}"; do
    print_status "Creating queue: $queue"
    az servicebus queue create \
        --name "$queue" \
        --namespace-name "$SERVICE_BUS_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --max-size 1024 \
        --default-message-time-to-live P14D
done

# Event Hub Namespace
print_status "Creating Event Hub namespace for real-time data streaming..."
EVENT_HUB_NAMESPACE="${PROJECT_NAME}-events"

az eventhubs namespace create \
    --name "$EVENT_HUB_NAMESPACE" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku Standard \
    --capacity 1

# Create Event Hubs
print_status "Creating Event Hubs for vessel telemetry..."
EVENT_HUBS=("vessel-telemetry" "ais-data-stream" "environmental-sensors")

for hub in "${EVENT_HUBS[@]}"; do
    print_status "Creating Event Hub: $hub"
    az eventhubs eventhub create \
        --name "$hub" \
        --namespace-name "$EVENT_HUB_NAMESPACE" \
        --resource-group "$RESOURCE_GROUP" \
        --partition-count 4 \
        --message-retention 7
done

echo -e "\n${BLUE}3. IoT and Device Management${NC}"

# IoT Hub
print_status "Creating IoT Hub for vessel device management..."
IOT_HUB_NAME="${PROJECT_NAME}-iothub"

az iot hub create \
    --name "$IOT_HUB_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku S1 \
    --unit 1

# Register IoT Devices (Maritime Vessels)
print_status "Registering maritime vessels as IoT devices..."
VESSELS=("vessel-castor-device" "vessel-pollux-device" "vessel-capella-device" "vessel-polaris-device")

for vessel in "${VESSELS[@]}"; do
    print_status "Registering vessel device: $vessel"
    az iot hub device-identity create \
        --hub-name "$IOT_HUB_NAME" \
        --device-id "$vessel" \
        --auth-method shared_private_key
done

echo -e "\n${BLUE}4. AI and Cognitive Services${NC}"

# Cognitive Services Multi-Service Account
print_status "Creating Cognitive Services for AI capabilities..."
COGNITIVE_NAME="${PROJECT_NAME}-cognitive"

az cognitiveservices account create \
    --name "$COGNITIVE_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --kind CognitiveServices \
    --sku S0 \
    --yes

# Text Analytics
print_status "Creating Text Analytics for maritime document processing..."
TEXT_ANALYTICS_NAME="${PROJECT_NAME}-textanalytics"

az cognitiveservices account create \
    --name "$TEXT_ANALYTICS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --kind TextAnalytics \
    --sku S \
    --yes

# Computer Vision
print_status "Creating Computer Vision for maritime image analysis..."
VISION_NAME="${PROJECT_NAME}-vision"

az cognitiveservices account create \
    --name "$VISION_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --kind ComputerVision \
    --sku S1 \
    --yes

echo -e "\n${BLUE}5. Data Analytics and Reporting${NC}"

# Stream Analytics Job
print_status "Creating Stream Analytics job for real-time data processing..."
STREAM_ANALYTICS_NAME="${PROJECT_NAME}-analytics"

az stream-analytics job create \
    --name "$STREAM_ANALYTICS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output-error-policy Drop \
    --events-outoforder-policy Adjust \
    --events-outoforder-max-delay 00:00:05 \
    --events-late-arrival-max-delay 00:00:05 \
    --data-locale en-US \
    --compatibility-level 1.2

# SQL Database for analytics data
print_status "Creating SQL Database for analytics storage..."
SQL_SERVER_NAME="${PROJECT_NAME}-sqlserver-$(date +%s | tail -c 6)"
SQL_DB_NAME="${PROJECT_NAME}-database"
SQL_ADMIN_USER="maritimeadmin"
SQL_ADMIN_PASSWORD="MaritimePlatform2024!"

az sql server create \
    --name "$SQL_SERVER_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --admin-user "$SQL_ADMIN_USER" \
    --admin-password "$SQL_ADMIN_PASSWORD"

az sql db create \
    --name "$SQL_DB_NAME" \
    --server "$SQL_SERVER_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --service-objective S2

# Configure SQL Server firewall
print_status "Configuring SQL Server firewall..."
az sql server firewall-rule create \
    --name "AllowAzureServices" \
    --server "$SQL_SERVER_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

echo -e "\n${BLUE}6. Container and Web Services${NC}"

# Container Registry
print_status "Creating Azure Container Registry..."
ACR_NAME="${PROJECT_NAME}acr$(date +%s | tail -c 6)"

az acr create \
    --name "$ACR_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku Standard \
    --admin-enabled true

# App Service Plan
print_status "Creating App Service Plan..."
APP_SERVICE_PLAN="${PROJECT_NAME}-plan"

az appservice plan create \
    --name "$APP_SERVICE_PLAN" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku P1V2 \
    --is-linux

# Web App
print_status "Creating Web App for maritime platform..."
WEB_APP_NAME="${PROJECT_NAME}-webapp-$(date +%s | tail -c 6)"

az webapp create \
    --name "$WEB_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --plan "$APP_SERVICE_PLAN" \
    --runtime "DOTNETCORE:8.0"

echo -e "\n${BLUE}7. Security and Secrets Management${NC}"

# Store connection strings in Key Vault
print_status "Storing connection strings in Key Vault..."

# Get connection strings
SERVICE_BUS_CONNECTION=$(az servicebus namespace authorization-rule keys list \
    --namespace-name "$SERVICE_BUS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --name RootManageSharedAccessKey \
    --query primaryConnectionString -o tsv)

EVENT_HUB_CONNECTION=$(az eventhubs namespace authorization-rule keys list \
    --namespace-name "$EVENT_HUB_NAMESPACE" \
    --resource-group "$RESOURCE_GROUP" \
    --name RootManageSharedAccessKey \
    --query primaryConnectionString -o tsv)

IOT_HUB_CONNECTION=$(az iot hub connection-string show \
    --hub-name "$IOT_HUB_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query connectionString -o tsv)

COGNITIVE_KEY=$(az cognitiveservices account keys list \
    --name "$COGNITIVE_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query key1 -o tsv)

APP_INSIGHTS_KEY=$(az monitor app-insights component show \
    --app "$APP_INSIGHTS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query instrumentationKey -o tsv)

APP_INSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show \
    --app "$APP_INSIGHTS_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query connectionString -o tsv)

# Store secrets
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "maritime-servicebus-connection" --value "$SERVICE_BUS_CONNECTION"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "maritime-eventhub-connection" --value "$EVENT_HUB_CONNECTION"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "maritime-iothub-connection" --value "$IOT_HUB_CONNECTION"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "maritime-cognitive-key" --value "$COGNITIVE_KEY"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "maritime-appinsights-key" --value "$APP_INSIGHTS_KEY"

# Duplicate secrets using configuration-friendly naming for automatic binding
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "ServiceBus--ConnectionString" --value "$SERVICE_BUS_CONNECTION"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "EventHub--ConnectionString" --value "$EVENT_HUB_CONNECTION"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "IoTHub--ConnectionString" --value "$IOT_HUB_CONNECTION"
az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "ApplicationInsights--ConnectionString" --value "$APP_INSIGHTS_CONNECTION_STRING"

echo -e "\n${BLUE}8. Configuration Summary${NC}"

print_status "Deployment completed successfully!"
echo ""
echo -e "${GREEN}Resource Group:${NC} $RESOURCE_GROUP"
echo -e "${GREEN}Location:${NC} $LOCATION"
echo ""
echo -e "${YELLOW}Created Resources:${NC}"
echo "  🔐 Key Vault: $KEY_VAULT_NAME"
echo "  📊 Application Insights: $APP_INSIGHTS_NAME"
echo "  🚌 Service Bus: $SERVICE_BUS_NAME"
echo "  📡 Event Hub Namespace: $EVENT_HUB_NAMESPACE"
echo "  🔌 IoT Hub: $IOT_HUB_NAME"
echo "  🧠 Cognitive Services: $COGNITIVE_NAME"
echo "  📈 Stream Analytics: $STREAM_ANALYTICS_NAME"
echo "  🗄️  SQL Server: $SQL_SERVER_NAME"
echo "  🗃️  SQL Database: $SQL_DB_NAME"
echo "  📦 Container Registry: $ACR_NAME"
echo "  🌐 Web App: $WEB_APP_NAME"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Configure Power BI workspace for maritime analytics"
echo "2. Deploy the MaritimeIQ application to the Web App"
echo "3. Set up monitoring and alerting rules"
echo "4. Configure vessel IoT devices to send telemetry"
echo "5. Create Stream Analytics queries for real-time processing"
echo ""
echo -e "${GREEN}Maritime Platform Services:${NC}"
echo "  ⚓ Vessel Management & Tracking"
echo "  🗺️  Route Planning & Optimization"
echo "  🚨 Safety Monitoring & Alerts"
echo "  🌊 Environmental Compliance"
echo "  👥 Passenger Services"
echo "  📱 IoT Device Integration"
echo "  🤖 AI-Powered Analytics"
echo "  📊 Real-time Reporting"
echo ""
echo -e "${BLUE}MaritimeIQ Platform - Ready for Operations!${NC}"

# Create environment configuration file
cat > "${PROJECT_NAME}-environment.env" << EOF
# MaritimeIQ Platform - Environment Configuration
# Generated on: $(date)

# Azure Resources
AZURE_SUBSCRIPTION_ID=${SUBSCRIPTION_ID}
AZURE_RESOURCE_GROUP=${RESOURCE_GROUP}
AZURE_LOCATION=${LOCATION}

# Key Vault
KEYVAULT_NAME=${KEY_VAULT_NAME}
KEYVAULT_URI=https://${KEY_VAULT_NAME}.vault.azure.net/

# Service Bus
SERVICEBUS_NAMESPACE=${SERVICE_BUS_NAME}
SERVICEBUS_CONNECTION_STRING=${SERVICE_BUS_CONNECTION}

# Event Hub
EVENTHUB_NAMESPACE=${EVENT_HUB_NAMESPACE}
EVENTHUB_CONNECTION_STRING=${EVENT_HUB_CONNECTION}

# IoT Hub
IOTHUB_NAME=${IOT_HUB_NAME}
IOTHUB_CONNECTION_STRING=${IOT_HUB_CONNECTION}

# Cognitive Services
COGNITIVE_SERVICES_NAME=${COGNITIVE_NAME}
COGNITIVE_SERVICES_KEY=${COGNITIVE_KEY}

# Application Insights
APPINSIGHTS_NAME=${APP_INSIGHTS_NAME}
APPINSIGHTS_INSTRUMENTATION_KEY=${APP_INSIGHTS_KEY}
APPINSIGHTS_CONNECTION_STRING=${APP_INSIGHTS_CONNECTION_STRING}

# SQL Database
SQL_SERVER_NAME=${SQL_SERVER_NAME}
SQL_DATABASE_NAME=${SQL_DB_NAME}
SQL_CONNECTION_STRING=Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Database=${SQL_DB_NAME};User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};Encrypt=true;Connection Timeout=30;

# Container Registry
ACR_NAME=${ACR_NAME}
ACR_LOGIN_SERVER=${ACR_NAME}.azurecr.io

# Web App
WEBAPP_NAME=${WEB_APP_NAME}
WEBAPP_URL=https://${WEB_APP_NAME}.azurewebsites.net

# Maritime Fleet Configuration
FLEET_VESSELS=vessel-castor-device,vessel-pollux-device,vessel-capella-device,vessel-polaris-device
MARITIME_ROUTE=Bergen-Kirkenes
SERVICE_REGION=Norwegian Coastal Route
EOF

print_status "Environment configuration saved to: ${PROJECT_NAME}-environment.env"
echo -e "${GREEN}✅ MaritimeIQ Platform deployment completed successfully!${NC}"