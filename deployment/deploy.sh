#!/bin/bash

# Maritime Platform Azure Deployment Script
# This script deploys the maritime platform infrastructure to Azure

set -e

# Change to the deployment directory if not already there
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Source shared utilities
source "${SCRIPT_DIR}/shared/azure-common.sh"

# Configuration
TEMPLATE_FILE="azure/azure-infrastructure.json"
APP_NAME="maritime-platform"
UNIQUE_ID=$(date +%s | tail -c 6)

# Resource group will be selected interactively
RESOURCE_GROUP=""
LOCATION=""

print_success "🚢 Maritime Platform Azure Deployment"
echo "=========================================="

# Use shared Azure setup functions
check_prerequisites
setup_azure_auth
confirm_azure_subscription
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Deployment cancelled."
    exit 1
fi

# Use shared functions for setup
get_sql_password
select_resource_group

# Deploy infrastructure using shared functions
print_info "🚀 Deploying maritime platform infrastructure..."
echo "This may take 10-15 minutes..."

DEPLOYMENT_NAME="maritime-platform-$(date +%Y%m%d-%H%M%S)"
validate_arm_template "$TEMPLATE_FILE" "$RESOURCE_GROUP" "$APP_NAME" "$UNIQUE_ID" "$SQL_PASSWORD"

deploy_arm_template "$TEMPLATE_FILE" "$RESOURCE_GROUP" "$DEPLOYMENT_NAME" "$APP_NAME" "$UNIQUE_ID" "$LOCATION" "$SQL_PASSWORD"

print_success "🎉 Deployment completed successfully!"

# Show deployment outputs
print_info "📊 Deployment outputs:"
az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query "properties.outputs" \
    --output table

print_info "🔗 Useful commands:"
echo "View resources: az resource list --resource-group $RESOURCE_GROUP --output table"
echo "Monitor deployment: az deployment group show --resource-group $RESOURCE_GROUP --name $DEPLOYMENT_NAME"
echo "Delete resources: az group delete --name $RESOURCE_GROUP --yes --no-wait"