#!/bin/bash

# Havila Kystruten Maritime Dashboard Deployment Script
# Deploys the visualization dashboard to Azure Static Web Apps

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
STATIC_WEB_APP_NAME="havila-maritime-dashboard"
SKU="Free"

# Resource group and location will be selected interactively
RESOURCE_GROUP=""
LOCATION=""

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info "Deploying Havila Kystruten Maritime Dashboard to Azure Static Web Apps"
echo

# Check if Azure CLI is installed and authenticated
print_info "Checking Azure CLI..."
if ! command -v az >/dev/null 2>&1; then
    print_error "Azure CLI not found. Please install it from https://aka.ms/azure-cli"
    exit 1
fi

if ! az account show >/dev/null 2>&1; then
    print_warning "Azure CLI not authenticated. Please run 'az login'"
    exit 1
fi

SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
print_success "Authenticated to Azure subscription: $SUBSCRIPTION_NAME"

# Select resource group (or use preset from deploy-all.sh)
if [ -n "$PRESET_RESOURCE_GROUP" ] && [ -n "$PRESET_LOCATION" ]; then
    RESOURCE_GROUP="$PRESET_RESOURCE_GROUP"
    LOCATION="$PRESET_LOCATION"
    print_success "Using preset resource group: $RESOURCE_GROUP in location: $LOCATION"
else
    print_info "Available resource groups:"
    az group list --query "[].{Name:name, Location:location}" --output table

    echo ""
    read -p "Enter the name of the existing resource group to use: " RESOURCE_GROUP

    # Validate the resource group exists and get its location
    print_info "Validating resource group: $RESOURCE_GROUP"
    LOCATION=$(az group show --name "$RESOURCE_GROUP" --query "location" --output tsv 2>/dev/null)

    if [ -z "$LOCATION" ]; then
        print_error "Resource group '$RESOURCE_GROUP' not found."
        echo "Please make sure the resource group exists and you have access to it."
        exit 1
    fi

    print_success "Using resource group: $RESOURCE_GROUP in location: $LOCATION"
fi

# Create Static Web App
print_info "Creating Azure Static Web App..."
az staticwebapp create \
    --name "$STATIC_WEB_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku "$SKU" \
    --source "https://github.com/placeholder/placeholder" \
    --branch "main" \
    --app-location "/" \
    --output-location "/" >/dev/null 2>&1 || {
    print_warning "Static Web App might already exist or creation failed. Continuing..."
}

# Get the Static Web App URL
print_info "Getting Static Web App details..."
STATIC_WEB_APP_URL=$(az staticwebapp show --name "$STATIC_WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "defaultHostname" -o tsv 2>/dev/null || echo "")

if [ -n "$STATIC_WEB_APP_URL" ]; then
    print_success "Static Web App URL: https://$STATIC_WEB_APP_URL"
else
    print_warning "Could not retrieve Static Web App URL. Check Azure Portal for details."
fi

# Create a simple deployment package
print_info "Preparing deployment files..."
DEPLOY_DIR="./deploy"
mkdir -p "$DEPLOY_DIR"

# Copy the main HTML file
cp index.html "$DEPLOY_DIR/"

# Create a staticwebapp.config.json for routing
cat > "$DEPLOY_DIR/staticwebapp.config.json" << 'EOF'
{
  "routes": [
    {
      "route": "/*",
      "serve": "/index.html",
      "statusCode": 200
    }
  ],
  "mimeTypes": {
    ".html": "text/html",
    ".css": "text/css",
    ".js": "application/javascript",
    ".json": "application/json"
  },
  "defaultHeaders": {
    "content-security-policy": "default-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://unpkg.com https://cdn.jsdelivr.net https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io"
  }
}
EOF

print_success "Deployment files prepared in $DEPLOY_DIR/"

# Get deployment token for manual deployment
print_info "Getting deployment token..."
DEPLOYMENT_TOKEN=$(az staticwebapp secrets list --name "$STATIC_WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.apiKey" -o tsv 2>/dev/null || echo "")

if [ -n "$DEPLOYMENT_TOKEN" ]; then
    print_info "Manual deployment instructions:"
    echo
    echo "1. Install Azure Static Web Apps CLI:"
    echo "   npm install -g @azure/static-web-apps-cli"
    echo
    echo "2. Deploy manually:"
    echo "   cd deploy"
    echo "   swa deploy --deployment-token=\"$DEPLOYMENT_TOKEN\""
    echo
else
    print_warning "Could not retrieve deployment token. Use Azure Portal or GitHub Actions for deployment."
fi

print_success "✨ Maritime Dashboard deployment configuration complete!"
echo
print_info "📋 Next Steps:"
echo "   1. Open Azure Portal and navigate to your Static Web App"
echo "   2. Configure custom domain if needed"
echo "   3. Set up CI/CD with GitHub Actions"
echo "   4. Your dashboard will be available at: https://$STATIC_WEB_APP_URL"
echo
print_info "🌟 Dashboard Features:"
echo "   • Real-time fleet monitoring"
echo "   • Environmental compliance tracking"
echo "   • Route optimization insights"
echo "   • System health monitoring"
echo "   • Live data from your Maritime Platform API"
echo