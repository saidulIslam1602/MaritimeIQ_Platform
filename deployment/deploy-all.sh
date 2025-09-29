#!/bin/bash

# Complete Maritime Platform Deployment Script
# Deploys both infrastructure and dashboard to Azure

set -e

# Change to the project root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

# Source shared utilities
source "${SCRIPT_DIR}/shared/azure-common.sh"

# Configuration
TEMPLATE_FILE="deployment/azure/azure-infrastructure.json"
APP_NAME="maritime-platform"
UNIQUE_ID=$(date +%s | tail -c 6)

# Resource group will be selected interactively
RESOURCE_GROUP=""
LOCATION=""

print_success "🚢 Complete Maritime Platform Deployment"
echo "=============================================="

# Check prerequisites
print_info "🔍 Checking prerequisites..."

# Check Node.js for dashboard
if ! command -v npm &> /dev/null; then
    print_error "Node.js/npm is not installed. Required for dashboard deployment."
    exit 1
fi

# Use shared Azure setup
check_prerequisites
setup_azure_auth

print_success "Prerequisites check passed"

# Use shared functions for subscription and resource group selection
confirm_azure_subscription
select_resource_group

# Get SQL admin password using shared function
get_sql_password

# Phase 1: Deploy Infrastructure
print_info "🏗️  Phase 1: Deploying Infrastructure"
echo "=================================="

print_info "📦 Using existing resource group: $RESOURCE_GROUP"

# Deploy infrastructure using shared function
DEPLOYMENT_NAME="maritime-platform-$(date +%Y%m%d-%H%M%S)"
validate_arm_template "$TEMPLATE_FILE" "$RESOURCE_GROUP" "$APP_NAME" "$UNIQUE_ID" "$SQL_PASSWORD"
deploy_arm_template "$TEMPLATE_FILE" "$RESOURCE_GROUP" "$DEPLOYMENT_NAME" "$APP_NAME" "$UNIQUE_ID" "$LOCATION" "$SQL_PASSWORD"

# Phase 2: Deploy Dashboard
print_info "🖥️  Phase 2: Deploying Dashboard"
echo "=============================="

cd maritime-dashboard

# Install dependencies and build
print_info "📦 Installing dashboard dependencies..."
npm install --silent

print_info "🔨 Building dashboard..."
npm run build

# Deploy dashboard to Azure Static Web Apps
print_info "🌐 Deploying dashboard to Azure Static Web Apps..."
chmod +x deploy.sh
# Pass resource group and location to dashboard deployment
export PRESET_RESOURCE_GROUP="$RESOURCE_GROUP"
export PRESET_LOCATION="$LOCATION"
./deploy.sh

cd ..

# Phase 3: Configuration Summary
print_info "📋 Deployment Summary"
echo "===================="

print_success "🎉 Complete Maritime Platform Deployment Finished!"
echo
print_info "🏗️  Infrastructure Resources:"
az resource list --resource-group "$RESOURCE_GROUP" --query "[].{Name:name, Type:type, Location:location}" --output table

echo
print_info "🔗 Access Points:"
STATIC_WEB_APP_URL=$(az staticwebapp show --name "havila-maritime-dashboard" --resource-group "$RESOURCE_GROUP" --query "defaultHostname" -o tsv 2>/dev/null || echo "Check Azure Portal")
echo "Dashboard URL: https://$STATIC_WEB_APP_URL"
echo "Azure Portal: https://portal.azure.com"

echo
print_info "📊 Get Connection Strings:"
echo "az deployment group show --resource-group $RESOURCE_GROUP --name $DEPLOYMENT_NAME --query 'properties.outputs'"

echo
print_info "🧹 Cleanup Command:"
echo "az group delete --name $RESOURCE_GROUP --yes --no-wait"

echo
print_success "✨ Your Maritime Platform is ready to use!"