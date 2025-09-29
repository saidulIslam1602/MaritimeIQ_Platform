#!/bin/bash

# Modern Maritime Dashboard Deployment Script
# Deploys the Next.js React dashboard to Azure Static Web Apps or Container Apps

set -euo pipefail

# Configuration
DASHBOARD_NAME="maritime-dashboard"
RESOURCE_GROUP="${RESOURCE_GROUP:-maritime-platform-rg}"
LOCATION="${LOCATION:-norwayeast}"

print_info() {
    echo -e "\033[0;34m[INFO]\033[0m $1"
}

print_success() {
    echo -e "\033[0;32m[SUCCESS]\033[0m $1"
}

print_warning() {
    echo -e "\033[0;33m[WARNING]\033[0m $1"
}

print_error() {
    echo -e "\033[0;31m[ERROR]\033[0m $1"
}

# Check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check if Node.js is installed
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed. Please install Node.js 18+ and try again."
        exit 1
    fi
    
    # Check if npm is installed
    if ! command -v npm &> /dev/null; then
        print_error "npm is not installed. Please install npm and try again."
        exit 1
    fi
    
    # Check if Azure CLI is installed
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install Azure CLI and try again."
        exit 1
    fi
    
    print_success "All prerequisites are met."
}

# Build the Next.js application
build_dashboard() {
    print_info "Building maritime dashboard..."
    
    # Install dependencies
    print_info "Installing dependencies..."
    npm install
    
    # Build the application
    print_info "Building Next.js application..."
    npm run build
    
    print_success "Dashboard built successfully."
}

# Deploy to Azure Static Web Apps
deploy_static_web_app() {
    print_info "Deploying to Azure Static Web Apps..."
    
    # Check if Static Web App exists
    if ! az staticwebapp show --name "$DASHBOARD_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        print_info "Creating Azure Static Web App..."
        az staticwebapp create \
            --name "$DASHBOARD_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --sku "Free" \
            --source "https://github.com/your-org/maritime-platform" \
            --branch "main" \
            --app-location "/" \
            --output-location "out"
    fi
    
    # Get deployment token
    DEPLOYMENT_TOKEN=$(az staticwebapp secrets list \
        --name "$DASHBOARD_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --query "properties.apiKey" -o tsv)
    
    if [ -n "$DEPLOYMENT_TOKEN" ]; then
        print_info "Deploy with Azure Static Web Apps CLI:"
        echo "  npm install -g @azure/static-web-apps-cli"
        echo "  swa deploy --deployment-token=\"$DEPLOYMENT_TOKEN\" --app-location . --output-location out"
    fi
}

# Deploy to Azure Container Apps (alternative)
deploy_container_app() {
    print_info "Deploying to Azure Container Apps..."
    
    # Create container app environment if it doesn't exist
    if ! az containerapp env show --name "maritime-env" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
        print_info "Creating Container App environment..."
        az containerapp env create \
            --name "maritime-env" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION"
    fi
    
    # Build and deploy container
    print_info "Building and deploying container..."
    az containerapp up \
        --name "$DASHBOARD_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --environment "maritime-env" \
        --source . \
        --ingress external \
        --target-port 3000
}

# Main execution
main() {
    print_info "🚢 Deploying Maritime Intelligence Dashboard"
    echo "=============================================="
    
    check_prerequisites
    build_dashboard
    
    # Prompt for deployment type
    echo
    echo "Choose deployment method:"
    echo "1) Azure Static Web Apps (recommended for static export)"
    echo "2) Azure Container Apps (for full Next.js features)"
    echo
    read -p "Enter your choice (1 or 2): " choice
    
    case $choice in
        1)
            deploy_static_web_app
            ;;
        2)
            deploy_container_app
            ;;
        *)
            print_error "Invalid choice. Please run the script again and choose 1 or 2."
            exit 1
            ;;
    esac
    
    print_success "✨ Maritime Dashboard deployment completed!"
    echo
    print_info "📋 Post-deployment steps:"
    echo "   1. Configure custom domain if needed"
    echo "   2. Set up monitoring and alerts"
    echo "   3. Update API endpoints in configuration"
    echo "   4. Test all dashboard features"
}

# Run main function
main "$@"