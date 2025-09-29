#!/bin/bash

# MaritimeIQ Platform - Complete Azure Deployment Script
# Deploys the entire platform including API, dashboard, and Azure services

set -euo pipefail

# Configuration
PLATFORM_NAME="maritime-platform"
RESOURCE_GROUP="maritime-platform-rg"
LOCATION="norwayeast"
UNIQUE_ID=$(date +%s | tail -c 6)

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}"
    echo "=============================================="
    echo "🚢 MaritimeIQ Platform Azure Deployment"
    echo "=============================================="
    echo -e "${NC}"
}

# Check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check if Azure CLI is installed
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install Azure CLI and try again."
        exit 1
    fi
    
    # Check if .NET is installed
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed. Please install .NET 8+ and try again."
        exit 1
    fi
    
    # Check if Node.js is installed
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed. Please install Node.js 18+ and try again."
        exit 1
    fi
    
    print_success "All prerequisites are met."
}

# Azure authentication and setup
setup_azure() {
    print_info "Setting up Azure authentication..."
    
    # Check if already logged in
    if ! az account show &>/dev/null; then
        print_info "Please log in to Azure..."
        az login
    fi
    
    # Display current subscription
    SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
    print_success "Authenticated to Azure subscription: $SUBSCRIPTION_NAME"
    
    # Create resource group if it doesn't exist
    if ! az group show --name "$RESOURCE_GROUP" &>/dev/null; then
        print_info "Creating resource group: $RESOURCE_GROUP"
        az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
        print_success "Resource group created successfully."
    else
        print_info "Using existing resource group: $RESOURCE_GROUP"
    fi
}

# Deploy Azure infrastructure
deploy_infrastructure() {
    print_info "Deploying Azure infrastructure..."
    
    # Generate SQL admin password
    SQL_ADMIN_PASSWORD=$(openssl rand -base64 32)
    
    # Deploy ARM template
    DEPLOYMENT_NAME="maritimeiq-deployment-$(date +%Y%m%d-%H%M%S)"
    
    az deployment group create \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "deployment/azure/azure-infrastructure.json" \
        --parameters \
            appName="$PLATFORM_NAME" \
            uniqueId="$UNIQUE_ID" \
            location="$LOCATION" \
            sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
        --name "$DEPLOYMENT_NAME"
    
    print_success "Azure infrastructure deployed successfully."
    
    # Store SQL password in Key Vault
    KV_NAME="kv-$PLATFORM_NAME-$UNIQUE_ID"
    az keyvault secret set --vault-name "$KV_NAME" --name "SqlAdminPassword" --value "$SQL_ADMIN_PASSWORD" >/dev/null
    print_success "SQL admin password stored in Key Vault."
}

# Build and deploy the API
deploy_api() {
    print_info "Building and deploying MaritimeIQ API..."
    
    # Build the .NET application
    print_info "Building .NET application..."
    dotnet build --configuration Release
    
    # Create container app
    CONTAINER_APP_NAME="$PLATFORM_NAME-api"
    CONTAINER_ENV_NAME="$PLATFORM_NAME-env"
    
    # Create container app environment
    if ! az containerapp env show --name "$CONTAINER_ENV_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        print_info "Creating Container App environment..."
        az containerapp env create \
            --name "$CONTAINER_ENV_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION"
    fi
    
    # Deploy container app using Docker
    print_info "Deploying API to Azure Container Apps..."
    az containerapp up \
        --name "$CONTAINER_APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --environment "$CONTAINER_ENV_NAME" \
        --source . \
        --ingress external \
        --target-port 8080 \
        --dockerfile "deployment/docker/Dockerfile"
    
    # Get the API URL
    API_URL=$(az containerapp show --name "$CONTAINER_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
    print_success "API deployed at: https://$API_URL"
    
    # Store API URL for dashboard deployment
    echo "NEXT_PUBLIC_MARITIME_API_URL=https://$API_URL" > maritime-dashboard/.env.local
}

# Deploy the dashboard
deploy_dashboard() {
    print_info "Building and deploying Maritime Dashboard..."
    
    cd maritime-dashboard
    
    # Install dependencies and build
    print_info "Installing dashboard dependencies..."
    npm install
    
    print_info "Building Next.js application..."
    npm run build
    
    # Deploy to Azure Static Web Apps
    STATIC_APP_NAME="$PLATFORM_NAME-dashboard"
    
    if ! az staticwebapp show --name "$STATIC_APP_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        print_info "Creating Azure Static Web App..."
        az staticwebapp create \
            --name "$STATIC_APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --sku "Free" \
            --source "https://github.com/your-org/maritime-platform" \
            --branch "main" \
            --app-location "maritime-dashboard" \
            --output-location "out"
    fi
    
    # Get deployment token and provide instructions
    DEPLOYMENT_TOKEN=$(az staticwebapp secrets list \
        --name "$STATIC_APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --query "properties.apiKey" -o tsv)
    
    # Get dashboard URL
    DASHBOARD_URL=$(az staticwebapp show --name "$STATIC_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "defaultHostname" -o tsv)
    
    print_success "Dashboard created at: https://$DASHBOARD_URL"
    print_info "To deploy dashboard content, run:"
    echo "  npm install -g @azure/static-web-apps-cli"
    echo "  swa deploy --deployment-token=\"$DEPLOYMENT_TOKEN\" --app-location . --output-location out"
    
    cd ..
}

# Deploy Azure Functions
deploy_functions() {
    print_info "Deploying Azure Functions..."
    
    cd Functions
    
    FUNCTION_APP_NAME="$PLATFORM_NAME-functions-$UNIQUE_ID"
    STORAGE_ACCOUNT="st${PLATFORM_NAME//[-_]/}${UNIQUE_ID}"
    
    # Create function app
    if ! az functionapp show --name "$FUNCTION_APP_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        print_info "Creating Azure Function App..."
        az functionapp create \
            --name "$FUNCTION_APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --storage-account "$STORAGE_ACCOUNT" \
            --runtime dotnet \
            --runtime-version 8 \
            --functions-version 4 \
            --os-type Linux
    fi
    
    # Deploy functions
    print_info "Publishing functions..."
    func azure functionapp publish "$FUNCTION_APP_NAME"
    
    print_success "Azure Functions deployed successfully."
    
    cd ..
}

# Configure monitoring and alerts
setup_monitoring() {
    print_info "Setting up monitoring and alerts..."
    
    APP_INSIGHTS_NAME="$PLATFORM_NAME-insights"
    
    # Application Insights should already be created by ARM template
    if az monitor app-insights component show --app "$APP_INSIGHTS_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        print_success "Application Insights is configured."
    else
        print_warning "Application Insights not found. Please check ARM template deployment."
    fi
}

# Display deployment summary
show_deployment_summary() {
    print_success "🎉 MaritimeIQ Platform Deployment Complete!"
    echo
    echo "================================================"
    echo "📋 DEPLOYMENT SUMMARY"
    echo "================================================"
    echo
    echo "🏗️  Resource Group: $RESOURCE_GROUP"
    echo "📍 Location: $LOCATION"
    echo "🔢 Unique ID: $UNIQUE_ID"
    echo
    echo "🔗 ENDPOINTS:"
    
    # Get API URL
    if az containerapp show --name "$PLATFORM_NAME-api" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        API_URL=$(az containerapp show --name "$PLATFORM_NAME-api" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
        echo "   🚀 API: https://$API_URL"
        echo "   📖 Swagger: https://$API_URL/swagger"
    fi
    
    # Get Dashboard URL
    if az staticwebapp show --name "$PLATFORM_NAME-dashboard" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        DASHBOARD_URL=$(az staticwebapp show --name "$PLATFORM_NAME-dashboard" --resource-group "$RESOURCE_GROUP" --query "defaultHostname" -o tsv)
        echo "   📊 Dashboard: https://$DASHBOARD_URL"
    fi
    
    echo
    echo "💡 NEXT STEPS:"
    echo "   1. Configure custom domains if needed"
    echo "   2. Set up CI/CD pipelines"
    echo "   3. Configure monitoring alerts"
    echo "   4. Review security settings"
    echo "   5. Test all platform features"
    echo
    echo "📚 View your deployment in Azure Portal:"
    echo "   https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP"
    echo
}

# Main execution
main() {
    print_header
    
    print_info "Starting MaritimeIQ Platform deployment..."
    echo
    
    # Deployment steps
    check_prerequisites
    setup_azure
    deploy_infrastructure
    deploy_api
    deploy_dashboard
    
    # Optional components
    read -p "Deploy Azure Functions? (y/N): " deploy_func
    if [[ $deploy_func =~ ^[Yy]$ ]]; then
        deploy_functions
    fi
    
    setup_monitoring
    show_deployment_summary
    
    print_success "✨ Deployment completed successfully!"
}

# Handle script interruption
trap 'print_error "Deployment interrupted. Some resources may have been created."; exit 1' INT

# Run main function
main "$@"