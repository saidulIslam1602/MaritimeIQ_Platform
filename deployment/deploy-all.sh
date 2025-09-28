#!/bin/bash

# Complete Maritime Platform Deployment Script
# Deploys both infrastructure and dashboard to Azure

set -e

# Change to the project root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

# Configuration
TEMPLATE_FILE="deployment/azure/azure-infrastructure.json"
APP_NAME="maritime-platform"
UNIQUE_ID=$(date +%s | tail -c 6)

# Resource group will be selected interactively
RESOURCE_GROUP=""
LOCATION=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚢 Complete Maritime Platform Deployment${NC}"
echo "=============================================="

# Check prerequisites
echo -e "${YELLOW}🔍 Checking prerequisites...${NC}"

# Check Azure CLI
if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed.${NC}"
    exit 1
fi

# Check Node.js for dashboard
if ! command -v npm &> /dev/null; then
    echo -e "${RED}❌ Node.js/npm is not installed. Required for dashboard deployment.${NC}"
    exit 1
fi

# Check if logged in to Azure
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Please login to Azure:${NC}"
    az login
fi

echo -e "${GREEN}✅ Prerequisites check passed${NC}"

# Display current subscription
echo -e "${BLUE}📋 Current Azure subscription:${NC}"
az account show --query "{Name:name, SubscriptionId:id}" --output table

# List existing resource groups
echo -e "${YELLOW}📦 Available Resource Groups:${NC}"
az group list --query "[].{Name:name, Location:location}" --output table

echo
read -p "Enter the name of your existing resource group: " RESOURCE_GROUP

# Validate resource group exists
if ! az group show --name "$RESOURCE_GROUP" &> /dev/null; then
    echo -e "${RED}❌ Resource group '$RESOURCE_GROUP' not found.${NC}"
    exit 1
fi

# Get resource group location
LOCATION=$(az group show --name "$RESOURCE_GROUP" --query "location" --output tsv)
echo -e "${GREEN}✅ Using resource group: $RESOURCE_GROUP (Location: $LOCATION)${NC}"

# Ask for confirmation
read -p "Continue with resource group '$RESOURCE_GROUP'? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Deployment cancelled."
    exit 1
fi

# Get SQL admin password
echo -e "${YELLOW}🔑 SQL Server admin password setup:${NC}"
while true; do
    read -s -p "Enter SQL admin password (min 8 chars, complex): " SQL_PASSWORD
    echo
    read -s -p "Confirm password: " SQL_PASSWORD_CONFIRM
    echo
    
    if [ "$SQL_PASSWORD" = "$SQL_PASSWORD_CONFIRM" ]; then
        if [ ${#SQL_PASSWORD} -ge 8 ]; then
            break
        else
            echo -e "${RED}Password must be at least 8 characters long.${NC}"
        fi
    else
        echo -e "${RED}Passwords do not match. Please try again.${NC}"
    fi
done

# Phase 1: Deploy Infrastructure
echo -e "${BLUE}🏗️  Phase 1: Deploying Infrastructure${NC}"
echo "=================================="

echo -e "${YELLOW}📦 Using existing resource group: $RESOURCE_GROUP${NC}"

# Deploy infrastructure
echo -e "${YELLOW}🚀 Deploying maritime platform infrastructure...${NC}"
DEPLOYMENT_NAME="maritime-platform-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --template-file "$TEMPLATE_FILE" \
    --parameters appName="$APP_NAME" uniqueId="$UNIQUE_ID" location="$LOCATION" sqlAdminPassword="$SQL_PASSWORD" \
    --output table

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Infrastructure deployment completed!${NC}"
else
    echo -e "${RED}❌ Infrastructure deployment failed. Stopping.${NC}"
    exit 1
fi

# Phase 2: Deploy Dashboard
echo -e "${BLUE}🖥️  Phase 2: Deploying Dashboard${NC}"
echo "=============================="

cd maritime-dashboard

# Install dependencies and build
echo -e "${YELLOW}📦 Installing dashboard dependencies...${NC}"
npm install --silent

echo -e "${YELLOW}🔨 Building dashboard...${NC}"
npm run build

# Deploy dashboard to Azure Static Web Apps
echo -e "${YELLOW}🌐 Deploying dashboard to Azure Static Web Apps...${NC}"
chmod +x deploy.sh
# Pass resource group and location to dashboard deployment
export PRESET_RESOURCE_GROUP="$RESOURCE_GROUP"
export PRESET_LOCATION="$LOCATION"
./deploy.sh

cd ..

# Phase 3: Configuration Summary
echo -e "${BLUE}📋 Deployment Summary${NC}"
echo "===================="

echo -e "${GREEN}🎉 Complete Maritime Platform Deployment Finished!${NC}"
echo
echo -e "${YELLOW}🏗️  Infrastructure Resources:${NC}"
az resource list --resource-group "$RESOURCE_GROUP" --query "[].{Name:name, Type:type, Location:location}" --output table

echo
echo -e "${YELLOW}🔗 Access Points:${NC}"
STATIC_WEB_APP_URL=$(az staticwebapp show --name "havila-maritime-dashboard" --resource-group "$RESOURCE_GROUP" --query "defaultHostname" -o tsv 2>/dev/null || echo "Check Azure Portal")
echo "Dashboard URL: https://$STATIC_WEB_APP_URL"
echo "Azure Portal: https://portal.azure.com"

echo
echo -e "${YELLOW}📊 Get Connection Strings:${NC}"
echo "az deployment group show --resource-group $RESOURCE_GROUP --name $DEPLOYMENT_NAME --query 'properties.outputs'"

echo
echo -e "${YELLOW}🧹 Cleanup Command:${NC}"
echo "az group delete --name $RESOURCE_GROUP --yes --no-wait"

echo
echo -e "${GREEN}✨ Your Maritime Platform is ready to use!${NC}"