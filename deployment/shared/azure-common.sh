#!/bin/bash

# Shared Azure deployment utilities
# This script contains common functions used across all deployment scripts

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Print colored messages
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

# Setup Azure authentication
setup_azure_auth() {
    print_info "Checking Azure CLI..."
    
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        echo "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi

    print_info "Checking Azure login status..."
    if ! az account show &> /dev/null; then
        print_warning "Please login to Azure:"
        az login
    fi

    print_success "Azure CLI authenticated"
}

# Display current subscription and get confirmation
confirm_azure_subscription() {
    print_info "Current Azure subscription:"
    az account show --query "{Name:name, SubscriptionId:id}" --output table

    read -p "Continue with this subscription? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_warning "Deployment cancelled."
        exit 1
    fi
}

# Select resource group interactively
select_resource_group() {
    print_info "Available resource groups:"
    az group list --query "[].{Name:name, Location:location}" --output table

    echo
    read -p "Enter the name of your existing resource group: " RESOURCE_GROUP

    # Validate the resource group exists and get its location
    print_info "Validating resource group: $RESOURCE_GROUP"
    LOCATION=$(az group show --name "$RESOURCE_GROUP" --query "location" --output tsv 2>/dev/null)

    if [ -z "$LOCATION" ]; then
        print_error "Resource group '$RESOURCE_GROUP' not found."
        echo "Please make sure the resource group exists and you have access to it."
        exit 1
    fi

    print_success "Using resource group: $RESOURCE_GROUP in location: $LOCATION"
}

# Get SQL admin password with validation
get_sql_password() {
    print_info "SQL Server admin password setup:"
    while true; do
        read -s -p "Enter SQL admin password (min 8 chars, complex): " SQL_PASSWORD
        echo
        read -s -p "Confirm password: " SQL_PASSWORD_CONFIRM
        echo
        
        if [ "$SQL_PASSWORD" = "$SQL_PASSWORD_CONFIRM" ]; then
            if [ ${#SQL_PASSWORD} -ge 8 ]; then
                break
            else
                print_error "Password must be at least 8 characters long."
            fi
        else
            print_error "Passwords do not match. Please try again."
        fi
    done
    print_success "SQL password configured"
}

# Validate ARM template
validate_arm_template() {
    local template_file=$1
    local resource_group=$2
    local app_name=$3
    local unique_id=$4
    local sql_password=$5
    
    print_info "Validating ARM template..."
    
    if [ ! -f "$template_file" ]; then
        print_error "Template file not found: $template_file"
        exit 1
    fi
    
    az deployment group validate \
        --resource-group "$resource_group" \
        --template-file "$template_file" \
        --parameters appName="$app_name" uniqueId="$unique_id" sqlAdminPassword="$sql_password" \
        --output none

    if [ $? -eq 0 ]; then
        print_success "Template validation successful"
    else
        print_error "Template validation failed. Please check the template."
        exit 1
    fi
}

# Deploy ARM template
deploy_arm_template() {
    local template_file=$1
    local resource_group=$2
    local deployment_name=$3
    local app_name=$4
    local unique_id=$5
    local location=$6
    local sql_password=$7
    
    print_info "Deploying ARM template..."
    
    az deployment group create \
        --resource-group "$resource_group" \
        --name "$deployment_name" \
        --template-file "$template_file" \
        --parameters appName="$app_name" uniqueId="$unique_id" location="$location" sqlAdminPassword="$sql_password" \
        --output table

    if [ $? -eq 0 ]; then
        print_success "ARM template deployment completed"
    else
        print_error "ARM template deployment failed"
        exit 1
    fi
}

# Check if prerequisites are installed
check_prerequisites() {
    local missing_tools=()
    
    if ! command -v az &> /dev/null; then
        missing_tools+=("Azure CLI")
    fi
    
    if [ ${#missing_tools[@]} -gt 0 ]; then
        print_error "Missing prerequisites:"
        for tool in "${missing_tools[@]}"; do
            echo "  - $tool"
        done
        exit 1
    fi
    
    print_success "All prerequisites are installed"
}

# Export functions for use in other scripts
export -f print_info print_success print_warning print_error
export -f setup_azure_auth confirm_azure_subscription select_resource_group
export -f get_sql_password validate_arm_template deploy_arm_template check_prerequisites