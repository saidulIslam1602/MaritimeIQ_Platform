#!/bin/bash

# Real-time Maritime Data Pipeline Deployment Script
# Sets up Azure Event Hubs, Stream Analytics, and Cosmos DB for live data processing

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
RESOURCE_GROUP="maritime-platform-rg"
LOCATION="norwayeast"
SUBSCRIPTION_ID="${AZURE_SUBSCRIPTION_ID:-}"

# Event Hub Configuration
EVENTHUB_NAMESPACE="havila-maritime-events"
EVENTHUB_NAME="maritime-telemetry"
EVENTHUB_CONSUMER_GROUP="analytics"

# Stream Analytics Configuration
STREAM_ANALYTICS_JOB="maritime-analytics"

# Cosmos DB Configuration
COSMOSDB_ACCOUNT="havila-maritime-cosmos"
COSMOSDB_DATABASE="MaritimeData"

# Function to print colored output
print_status() {
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

# Check if Azure CLI is installed and user is logged in
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        exit 1
    fi
    
    # Check if logged in
    if ! az account show &> /dev/null; then
        print_error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    fi
    
    # Set subscription if provided
    if [ -n "$SUBSCRIPTION_ID" ]; then
        az account set --subscription "$SUBSCRIPTION_ID"
        print_success "Using subscription: $SUBSCRIPTION_ID"
    fi
    
    print_success "Prerequisites check completed"
}

# Create Event Hub namespace and hub
setup_event_hub() {
    print_status "Setting up Event Hub..."
    
    # Create Event Hub namespace
    if ! az eventhubs namespace show --name "$EVENTHUB_NAMESPACE" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating Event Hub namespace: $EVENTHUB_NAMESPACE"
        az eventhubs namespace create \
            --name "$EVENTHUB_NAMESPACE" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --sku Standard \
            --enable-auto-inflate true \
            --maximum-throughput-units 10
    else
        print_warning "Event Hub namespace already exists"
    fi
    
    # Create Event Hub
    if ! az eventhubs eventhub show --name "$EVENTHUB_NAME" --namespace-name "$EVENTHUB_NAMESPACE" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating Event Hub: $EVENTHUB_NAME"
        az eventhubs eventhub create \
            --name "$EVENTHUB_NAME" \
            --namespace-name "$EVENTHUB_NAMESPACE" \
            --resource-group "$RESOURCE_GROUP" \
            --partition-count 4 \
            --message-retention 7
    else
        print_warning "Event Hub already exists"
    fi
    
    # Create consumer group
    if ! az eventhubs eventhub consumer-group show --name "$EVENTHUB_CONSUMER_GROUP" --eventhub-name "$EVENTHUB_NAME" --namespace-name "$EVENTHUB_NAMESPACE" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating consumer group: $EVENTHUB_CONSUMER_GROUP"
        az eventhubs eventhub consumer-group create \
            --name "$EVENTHUB_CONSUMER_GROUP" \
            --eventhub-name "$EVENTHUB_NAME" \
            --namespace-name "$EVENTHUB_NAMESPACE" \
            --resource-group "$RESOURCE_GROUP"
    else
        print_warning "Consumer group already exists"
    fi
    
    print_success "Event Hub setup completed"
}

# Create Cosmos DB account and database
setup_cosmos_db() {
    print_status "Setting up Cosmos DB..."
    
    # Create Cosmos DB account
    if ! az cosmosdb show --name "$COSMOSDB_ACCOUNT" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating Cosmos DB account: $COSMOSDB_ACCOUNT"
        az cosmosdb create \
            --name "$COSMOSDB_ACCOUNT" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --kind GlobalDocumentDB \
            --default-consistency-level Session \
            --enable-automatic-failover true \
            --locations regionName="$LOCATION" failoverPriority=0 isZoneRedundant=false
    else
        print_warning "Cosmos DB account already exists"
    fi
    
    # Create database
    if ! az cosmosdb sql database show --account-name "$COSMOSDB_ACCOUNT" --name "$COSMOSDB_DATABASE" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating Cosmos DB database: $COSMOSDB_DATABASE"
        az cosmosdb sql database create \
            --account-name "$COSMOSDB_ACCOUNT" \
            --name "$COSMOSDB_DATABASE" \
            --resource-group "$RESOURCE_GROUP" \
            --throughput 1000
    else
        print_warning "Cosmos DB database already exists"
    fi
    
    # Create containers
    local containers=("TelemetryData" "AnalyticsResults" "Alerts" "VesselPositions")
    for container in "${containers[@]}"; do
        if ! az cosmosdb sql container show --account-name "$COSMOSDB_ACCOUNT" --database-name "$COSMOSDB_DATABASE" --name "$container" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
            print_status "Creating Cosmos DB container: $container"
            az cosmosdb sql container create \
                --account-name "$COSMOSDB_ACCOUNT" \
                --database-name "$COSMOSDB_DATABASE" \
                --name "$container" \
                --resource-group "$RESOURCE_GROUP" \
                --partition-key-path "/vesselId" \
                --throughput 400
        else
            print_warning "Container $container already exists"
        fi
    done
    
    print_success "Cosmos DB setup completed"
}

# Create Stream Analytics job
setup_stream_analytics() {
    print_status "Setting up Stream Analytics job..."
    
    # Create Stream Analytics job
    if ! az stream-analytics job show --name "$STREAM_ANALYTICS_JOB" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating Stream Analytics job: $STREAM_ANALYTICS_JOB"
        az stream-analytics job create \
            --name "$STREAM_ANALYTICS_JOB" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --output-error-policy "Drop" \
            --events-outoforder-policy "Adjust" \
            --events-outoforder-max-delay 10 \
            --events-late-arrival-max-delay 5 \
            --data-locale "en-US" \
            --compatibility-level "1.2"
    else
        print_warning "Stream Analytics job already exists"
    fi
    
    print_success "Stream Analytics job setup completed"
}

# Get connection strings and keys
get_connection_info() {
    print_status "Retrieving connection information..."
    
    # Event Hub connection string
    local eventhub_connection_string
    eventhub_connection_string=$(az eventhubs namespace authorization-rule keys list \
        --namespace-name "$EVENTHUB_NAMESPACE" \
        --resource-group "$RESOURCE_GROUP" \
        --name RootManageSharedAccessKey \
        --query primaryConnectionString \
        --output tsv)
    
    # Cosmos DB connection string
    local cosmosdb_connection_string
    cosmosdb_connection_string=$(az cosmosdb keys list \
        --name "$COSMOSDB_ACCOUNT" \
        --resource-group "$RESOURCE_GROUP" \
        --type connection-strings \
        --query "connectionStrings[0].connectionString" \
        --output tsv)
    
    # Create environment file
    cat > .env.production << EOF
# Real-time Maritime Data Pipeline Configuration
AZURE_EVENTHUB_CONNECTION_STRING="$eventhub_connection_string"
AZURE_EVENTHUB_NAME="$EVENTHUB_NAME"
AZURE_COSMOSDB_CONNECTION_STRING="$cosmosdb_connection_string"
AZURE_COSMOSDB_DATABASE="$COSMOSDB_DATABASE"
AZURE_SUBSCRIPTION_ID="$(az account show --query id --output tsv)"
AZURE_RESOURCE_GROUP="$RESOURCE_GROUP"
AZURE_STREAM_ANALYTICS_JOB="$STREAM_ANALYTICS_JOB"
EOF
    
    print_success "Connection information saved to .env.production"
    
    # Display summary
    echo ""
    echo "=================================================="
    echo "        Real-time Data Pipeline Deployed"
    echo "=================================================="
    echo "Event Hub Namespace: $EVENTHUB_NAMESPACE"
    echo "Event Hub Name: $EVENTHUB_NAME"
    echo "Stream Analytics Job: $STREAM_ANALYTICS_JOB"
    echo "Cosmos DB Account: $COSMOSDB_ACCOUNT"
    echo "Resource Group: $RESOURCE_GROUP"
    echo "Location: $LOCATION"
    echo "=================================================="
}

# Create Azure Function for data processing
setup_azure_function() {
    print_status "Setting up Azure Function for data processing..."
    
    local function_app_name="havila-maritime-processor"
    local storage_account="havilamaritimestorage"
    
    # Create storage account for Function App
    if ! az storage account show --name "$storage_account" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating storage account: $storage_account"
        az storage account create \
            --name "$storage_account" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --sku Standard_LRS \
            --kind StorageV2
    fi
    
    # Create Function App
    if ! az functionapp show --name "$function_app_name" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Creating Function App: $function_app_name"
        az functionapp create \
            --name "$function_app_name" \
            --resource-group "$RESOURCE_GROUP" \
            --storage-account "$storage_account" \
            --consumption-plan-location "$LOCATION" \
            --runtime node \
            --runtime-version 18 \
            --functions-version 4 \
            --os-type Linux
    else
        print_warning "Function App already exists"
    fi
    
    print_success "Azure Function setup completed"
}

# Main deployment function
main() {
    echo "=================================================="
    echo "     Havila Kystruten Real-time Data Pipeline"
    echo "            Deployment Script v1.0"
    echo "=================================================="
    echo ""
    
    check_prerequisites
    setup_event_hub
    setup_cosmos_db
    setup_stream_analytics
    setup_azure_function
    get_connection_info
    
    print_success "Real-time data pipeline deployment completed successfully!"
    print_status "You can now start streaming maritime telemetry data to Event Hub: $EVENTHUB_NAME"
    print_status "Stream Analytics will process the data and store results in Cosmos DB"
    print_status "The dashboard will display real-time analytics and visualizations"
}

# Run main function
main "$@"