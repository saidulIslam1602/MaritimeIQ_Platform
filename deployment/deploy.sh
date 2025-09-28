#!/bin/bash

# Maritime Platform Azure Deployment Script
# This script deploys the maritime platform infrastructure to Azure

set -e

# Change to the deployment directory if not already there
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Configuration
TEMPLATE_FILE="azure/azure-infrastructure.json"
APP_NAME="maritime-platform"
UNIQUE_ID=$(date +%s | tail -c 6)

# Resource group will be selected interactively
RESOURCE_GROUP=""
LOCATION=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚢 Maritime Platform Azure Deployment${NC}"
echo "=========================================="

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed. Please install it first.${NC}"
    echo "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in to Azure
echo -e "${YELLOW}🔐 Checking Azure login status...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Please login to Azure:${NC}"
    az login
fi

# Display current subscription
echo -e "${GREEN}📋 Current Azure subscription:${NC}"
az account show --query "{Name:name, SubscriptionId:id, TenantId:tenantId}" --output table

# Ask for confirmation
read -p "Continue with this subscription? (y/N): " -n 1 -r
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

# List available resource groups and let user select
echo -e "${YELLOW}📦 Available resource groups:${NC}"
az group list --query "[].{Name:name, Location:location}" --output table

echo ""
read -p "Enter the name of the existing resource group to use: " RESOURCE_GROUP

# Validate the resource group exists and get its location
echo -e "${YELLOW}🔍 Validating resource group: $RESOURCE_GROUP${NC}"
LOCATION=$(az group show --name "$RESOURCE_GROUP" --query "location" --output tsv 2>/dev/null)

if [ -z "$LOCATION" ]; then
    echo -e "${RED}❌ Error: Resource group '$RESOURCE_GROUP' not found.${NC}"
    echo "Please make sure the resource group exists and you have access to it."
    exit 1
fi

echo -e "${GREEN}✅ Using resource group: $RESOURCE_GROUP in location: $LOCATION${NC}"

# Check if template file exists
if [ ! -f "$TEMPLATE_FILE" ]; then
    echo -e "${RED}❌ Template file not found: $TEMPLATE_FILE${NC}"
    echo "Current directory: $(pwd)"
    echo "Looking for: $(realpath $TEMPLATE_FILE 2>/dev/null || echo $TEMPLATE_FILE)"
    exit 1
fi

# Validate template
echo -e "${YELLOW}✅ Validating ARM template...${NC}"
az deployment group validate \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$TEMPLATE_FILE" \
    --parameters appName="$APP_NAME" uniqueId="$UNIQUE_ID" sqlAdminPassword="$SQL_PASSWORD"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Template validation successful!${NC}"
else
    echo -e "${RED}❌ Template validation failed. Please check the template.${NC}"
    exit 1
fi

# Deploy infrastructure
echo -e "${YELLOW}🚀 Deploying maritime platform infrastructure...${NC}"
echo "This may take 10-15 minutes..."

DEPLOYMENT_NAME="maritime-platform-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --template-file "$TEMPLATE_FILE" \
    --parameters appName="$APP_NAME" uniqueId="$UNIQUE_ID" sqlAdminPassword="$SQL_PASSWORD" \
    --output table

if [ $? -eq 0 ]; then
    echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"
    
    # Show deployment outputs
    echo -e "${GREEN}📊 Deployment outputs:${NC}"
    az deployment group show \
        --resource-group "$RESOURCE_GROUP" \
        --name "$DEPLOYMENT_NAME" \
        --query "properties.outputs" \
        --output table
    
    echo -e "${GREEN}🔗 Useful commands:${NC}"
    echo "View resources: az resource list --resource-group $RESOURCE_GROUP --output table"
    echo "Monitor deployment: az deployment group show --resource-group $RESOURCE_GROUP --name $DEPLOYMENT_NAME"
    echo "Delete resources: az group delete --name $RESOURCE_GROUP --yes --no-wait"
    
else
    echo -e "${RED}❌ Deployment failed. Check the error messages above.${NC}"
    exit 1
fi