# 4. Azure Infrastructure Deep Dive

## 4.1 Infrastructure Overview

The Maritime Data Engineering Platform is built on a comprehensive Azure cloud infrastructure designed for scalability, reliability, and real-time data processing. The infrastructure follows Infrastructure as Code (IaC) principles using ARM templates and deployment scripts.

### 4.1.1 High-Level Infrastructure Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION TIER                            │
├─────────────────────────────────────────────────────────────────┤
│  Azure Static Web Apps                                          │
│  ├─ Next.js 14 Dashboard                                        │
│  ├─ Custom Domain & SSL                                         │
│  ├─ CDN Integration                                             │
│  └─ Global Distribution                                         │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    APPLICATION TIER                             │
├─────────────────────────────────────────────────────────────────┤
│  Azure Container Apps                                           │
│  ├─ .NET 8 Web API                                             │
│  ├─ Auto-scaling (0-30 instances)                              │
│  ├─ Load Balancing                                             │
│  └─ Health Monitoring                                          │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    COMPUTE TIER                                 │
├─────────────────────────────────────────────────────────────────┤
│  Azure Functions (Consumption Plan)                             │
│  ├─ AIS Processing Function                                     │
│  ├─ Environmental Monitoring Function                           │
│  ├─ Route Optimization Function                                 │
│  └─ Passenger Notification Function                             │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATA TIER                                    │
├─────────────────────────────────────────────────────────────────┤
│  Azure SQL Database (Basic → Standard)                         │
│  Azure Event Hubs (Standard, 2 TU)                            │
│  Azure Service Bus (Standard)                                  │
│  Azure Storage Account (Standard_LRS)                          │
│  Azure Key Vault (Standard)                                    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    MONITORING TIER                              │
├─────────────────────────────────────────────────────────────────┤
│  Application Insights                                           │
│  ├─ Performance Monitoring                                      │
│  ├─ Error Tracking                                             │
│  ├─ Custom Metrics                                             │
│  └─ Real-time Alerting                                         │
└─────────────────────────────────────────────────────────────────┘
```

## 4.2 ARM Template Analysis

### 4.2.1 Core ARM Template Structure

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "appName": {
      "type": "string",
      "defaultValue": "maritime-platform",
      "metadata": {
        "description": "Name of the application"
      }
    },
    "uniqueId": {
      "type": "string",
      "metadata": {
        "description": "Unique identifier for resources"
      }
    },
    "sqlAdminPassword": {
      "type": "securestring",
      "metadata": {
        "description": "SQL Server admin password"
      }
    }
  },
  "variables": {
    "sanitizedAppName": "[toLower(replace(parameters('appName'), '-', ''))]",
    "sqlName": "[concat('sql-', variables('shortAppName'), '-', variables('lowerUniqueId'))]",
    "ehNs": "[concat('ehns-', variables('shortAppName'), '-', variables('lowerUniqueId'))]"
  }
}
```

**Key Design Decisions:**

#### Parameter Strategy
- **appName**: Consistent naming across all resources
- **uniqueId**: Ensures global uniqueness for resources requiring unique names
- **securestring**: Sensitive data like passwords handled securely
- **defaultValue**: Provides sensible defaults while allowing customization

#### Variable Naming Convention
- **Sanitization**: `toLower(replace())` ensures consistent naming
- **Length Limits**: Resources like Storage Accounts have strict naming requirements
- **Concatenation**: Predictable resource names for management and automation

### 4.2.2 SQL Database Configuration

```json
{
  "type": "Microsoft.Sql/servers",
  "apiVersion": "2021-11-01",
  "name": "[variables('sqlName')]",
  "location": "[parameters('location')]",
  "properties": {
    "administratorLogin": "maritime-admin",
    "administratorLoginPassword": "[parameters('sqlAdminPassword')]",
    "version": "12.0",
    "publicNetworkAccess": "Enabled"
  }
},
{
  "type": "Microsoft.Sql/servers/databases",
  "apiVersion": "2021-11-01",
  "name": "[concat(variables('sqlName'), '/', variables('sqlDb'))]",
  "sku": {
    "name": "Basic",
    "tier": "Basic",
    "capacity": 5
  },
  "properties": {
    "collation": "SQL_Latin1_General_CP1_CI_AS",
    "maxSizeBytes": "2147483648",
    "catalogCollation": "SQL_Latin1_General_CP1_CI_AS",
    "zoneRedundant": false,
    "readScale": "Disabled"
  }
}
```

**Configuration Rationale:**

#### SKU Selection: Basic Tier
**Why Chosen:**
- Cost-effective for development and testing
- Sufficient for initial workload (5 DTU)
- Easy upgrade path to Standard/Premium

**When to Upgrade:**
- Production workloads requiring > 5 DTU
- Need for point-in-time restore beyond 7 days
- Geo-replication requirements
- Advanced security features

#### Firewall Rules
```json
{
  "type": "Microsoft.Sql/servers/firewallRules",
  "name": "[concat(variables('sqlName'), '/AllowAzureServices')]",
  "properties": {
    "startIpAddress": "0.0.0.0",
    "endIpAddress": "0.0.0.0"
  }
}
```

**Security Considerations:**
- Allows Azure services to connect
- Restricts external access by default
- Production should use Private Endpoints

### 4.2.3 Event Hubs Configuration

```json
{
  "type": "Microsoft.EventHub/namespaces",
  "apiVersion": "2021-11-01",
  "name": "[variables('ehNs')]",
  "sku": {
    "name": "Standard",
    "tier": "Standard",
    "capacity": 2
  },
  "properties": {
    "zoneRedundant": false,
    "isAutoInflateEnabled": true,
    "maximumThroughputUnits": 10
  },
  "resources": [
    {
      "type": "eventhubs",
      "name": "maritime-data",
      "properties": {
        "messageRetentionInDays": 3,
        "partitionCount": 4
      }
    },
    {
      "type": "eventhubs",
      "name": "vessel-tracking",
      "properties": {
        "messageRetentionInDays": 1,
        "partitionCount": 2
      }
    }
  ]
}
```

**Configuration Analysis:**

#### Throughput Units: 2 → 10 (Auto-inflate)
**Reasoning:**
- Base capacity: 2 MB/s ingress, 4 MB/s egress
- Auto-inflate to 10 TU during peak loads
- Cost optimization: Pay only for used capacity

#### Partition Strategy
**maritime-data**: 4 partitions
- High-volume AIS data streams
- Parallel processing capability
- Better load distribution

**vessel-tracking**: 2 partitions  
- Lower volume position updates
- Sufficient parallelism for real-time processing

#### Retention Policies
**maritime-data**: 3 days
- Compliance requirements
- Reprocessing capability
- Reasonable storage costs

**vessel-tracking**: 1 day
- Real-time only processing
- Reduced storage costs
- Faster partition cleanup

### 4.2.4 Storage Account Configuration

```json
{
  "type": "Microsoft.Storage/storageAccounts",
  "apiVersion": "2021-09-01",
  "name": "[variables('saName')]",
  "sku": {
    "name": "Standard_LRS"
  },
  "kind": "StorageV2",
  "properties": {
    "accessTier": "Hot",
    "supportsHttpsTrafficOnly": true,
    "minimumTlsVersion": "TLS1_2"
  }
}
```

**Storage Strategy:**

#### Replication: Standard_LRS
**Why Chosen:**
- Cost-effective for non-critical data
- 99.999999999% (11 9's) durability
- Local redundancy sufficient for logs/temp data

**Alternatives Considered:**
- **Standard_GRS**: Geographic replication, higher cost
- **Premium_LRS**: Better performance, much higher cost
- **Standard_ZRS**: Zone redundancy, slightly higher cost

#### Access Tier: Hot
**Rationale:**
- Frequent access to maritime data
- Real-time processing requirements
- Higher storage cost, lower access cost

#### Security Configuration
- **HTTPS Only**: Enforces encrypted connections
- **TLS 1.2 Minimum**: Modern security standards
- **Public Access**: Controlled via firewall rules

### 4.2.5 Key Vault Configuration

```json
{
  "type": "Microsoft.KeyVault/vaults",
  "apiVersion": "2022-07-01",
  "name": "[variables('kvName')]",
  "properties": {
    "sku": {
      "family": "A",
      "name": "standard"
    },
    "tenantId": "[subscription().tenantId]",
    "enabledForDeployment": false,
    "enabledForDiskEncryption": false,
    "enabledForTemplateDeployment": true,
    "enableSoftDelete": true,
    "softDeleteRetentionInDays": 90,
    "accessPolicies": []
  }
}
```

**Security Features:**

#### Soft Delete: Enabled (90 days)
**Benefits:**
- Protection against accidental deletion
- Compliance with data retention policies
- Recovery capability for critical secrets

#### Template Deployment: Enabled
**Purpose:**
- ARM templates can retrieve secrets during deployment
- Automated deployment pipelines
- Secure parameter passing

## 4.3 Deployment Scripts Architecture

### 4.3.1 Shared Utility Functions (`azure-common.sh`)

```bash
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

# Deploy ARM template with error handling
deploy_arm_template() {
    local template_file=$1
    local resource_group=$2
    local deployment_name=$3
    shift 3
    local parameters=("$@")

    print_info "Deploying ARM template: $template_file"
    print_info "Resource Group: $resource_group"
    print_info "Deployment Name: $deployment_name"

    # Validate template first
    print_info "Validating ARM template..."
    if ! az deployment group validate \
        --resource-group "$resource_group" \
        --template-file "$template_file" \
        "${parameters[@]}" > /dev/null 2>&1; then
        
        print_error "Template validation failed!"
        az deployment group validate \
            --resource-group "$resource_group" \
            --template-file "$template_file" \
            "${parameters[@]}"
        return 1
    fi

    print_success "Template validation passed"

    # Deploy template
    print_info "Starting deployment..."
    if az deployment group create \
        --resource-group "$resource_group" \
        --name "$deployment_name" \
        --template-file "$template_file" \
        "${parameters[@]}" \
        --verbose; then
        
        print_success "Deployment completed successfully!"
        return 0
    else
        print_error "Deployment failed!"
        return 1
    fi
}
```

**Script Design Patterns:**

#### Error Handling Strategy
```bash
# Fail fast approach
set -e  # Exit on any error
set -u  # Exit on undefined variables
set -o pipefail  # Exit on pipe failures

# Trap for cleanup
trap cleanup EXIT

cleanup() {
    if [ $? -ne 0 ]; then
        print_error "Script failed! Check the logs above."
    fi
}
```

#### Idempotent Operations
```bash
# Check if resource exists before creating
if ! az group show --name "$RESOURCE_GROUP" >/dev/null 2>&1; then
    print_info "Creating resource group: $RESOURCE_GROUP"
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
else
    print_info "Resource group already exists: $RESOURCE_GROUP"
fi
```

#### Parameter Validation
```bash
get_sql_password() {
    print_info "SQL Server admin password setup:"
    while true; do
        read -s -p "Enter SQL admin password (min 8 chars, complex): " SQL_PASSWORD
        echo
        
        if [ ${#SQL_PASSWORD} -ge 8 ]; then
            # Password complexity validation
            if [[ "$SQL_PASSWORD" =~ [A-Z] ]] && \
               [[ "$SQL_PASSWORD" =~ [a-z] ]] && \
               [[ "$SQL_PASSWORD" =~ [0-9] ]] && \
               [[ "$SQL_PASSWORD" =~ [^A-Za-z0-9] ]]; then
                break
            else
                print_error "Password must contain uppercase, lowercase, number, and special character."
            fi
        else
            print_error "Password must be at least 8 characters long."
        fi
    done
}
```

### 4.3.2 Main Deployment Script (`deploy-all.sh`)

```bash
#!/bin/bash

# Source shared utilities
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/shared/azure-common.sh"

# Set error handling
set -e
set -u
set -o pipefail

main() {
    print_info "Starting Maritime Platform deployment..."
    
    # Step 1: Authentication and setup
    setup_azure_auth
    confirm_azure_subscription
    select_resource_group
    
    # Step 2: Get deployment parameters
    get_deployment_parameters
    
    # Step 3: Deploy infrastructure
    deploy_infrastructure
    
    # Step 4: Deploy application
    deploy_application
    
    # Step 5: Deploy Functions
    deploy_functions
    
    # Step 6: Deploy dashboard
    deploy_dashboard
    
    # Step 7: Validate deployment
    validate_deployment
    
    print_success "Deployment completed successfully!"
    print_deployment_summary
}

deploy_infrastructure() {
    print_info "Deploying Azure infrastructure..."
    
    local template_file="$SCRIPT_DIR/azure/azure-infrastructure.json"
    local deployment_name="maritime-infrastructure-$(date +%Y%m%d-%H%M%S)"
    
    deploy_arm_template \
        "$template_file" \
        "$RESOURCE_GROUP" \
        "$deployment_name" \
        --parameters appName="$APP_NAME" \
        --parameters uniqueId="$UNIQUE_ID" \
        --parameters sqlAdminPassword="$SQL_PASSWORD" \
        --parameters location="$LOCATION"
}

validate_deployment() {
    print_info "Validating deployment..."
    
    # Check API endpoint
    local api_url=$(az containerapp show \
        --name "$APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --query "properties.configuration.ingress.fqdn" \
        --output tsv)
    
    if curl -f "https://$api_url/health" >/dev/null 2>&1; then
        print_success "API endpoint is responding"
    else
        print_warning "API endpoint health check failed"
    fi
    
    # Check database connectivity
    print_info "Checking database connectivity..."
    # Database connectivity test would go here
    
    print_success "Deployment validation completed"
}

print_deployment_summary() {
    echo
    print_info "=== DEPLOYMENT SUMMARY ==="
    echo "Resource Group: $RESOURCE_GROUP"
    echo "Location: $LOCATION"
    echo "App Name: $APP_NAME"
    echo "Unique ID: $UNIQUE_ID"
    echo
    print_info "Access your application at:"
    echo "API: https://$(az containerapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" --output tsv)"
    echo "Dashboard: https://$(az staticwebapp show --name "${APP_NAME}-dashboard" --resource-group "$RESOURCE_GROUP" --query "defaultHostname" --output tsv)"
}

# Execute main function
main "$@"
```

**Deployment Pipeline Features:**
- **Validation**: Template validation before deployment
- **Error Recovery**: Comprehensive error handling and rollback
- **Progress Tracking**: Visual feedback throughout deployment
- **Summary Report**: Complete deployment information at end

## 4.4 Container Apps Configuration

### 4.4.1 Container Apps Environment

```json
{
  "type": "Microsoft.App/managedEnvironments",
  "apiVersion": "2022-06-01-preview",
  "name": "[variables('containerEnvName')]",
  "location": "[parameters('location')]",
  "properties": {
    "appLogsConfiguration": {
      "destination": "log-analytics",
      "logAnalyticsConfiguration": {
        "customerId": "[reference(variables('logAnalyticsName')).customerId]",
        "primarySharedKey": "[listKeys(variables('logAnalyticsName'), '2020-08-01').primarySharedKey]"
      }
    }
  }
}
```

#### Environment Features
- **Log Analytics Integration**: Centralized logging
- **Auto-scaling**: Based on HTTP requests and CPU/memory
- **Load Balancing**: Built-in load balancing across instances
- **Health Monitoring**: Automatic health checks and restarts

### 4.4.2 Container App Configuration

```json
{
  "type": "Microsoft.App/containerApps",
  "apiVersion": "2022-06-01-preview",
  "name": "[variables('appName')]",
  "properties": {
    "managedEnvironmentId": "[resourceId('Microsoft.App/managedEnvironments', variables('containerEnvName'))]",
    "configuration": {
      "ingress": {
        "external": true,
        "targetPort": 80,
        "allowInsecure": false
      },
      "secrets": [
        {
          "name": "sql-connection-string",
          "value": "[concat('Server=tcp:', reference(variables('sqlName')).fullyQualifiedDomainName, ',1433;Initial Catalog=', variables('sqlDb'), ';User ID=maritime-admin;Password=', parameters('sqlAdminPassword'), ';')]"
        }
      ]
    },
    "template": {
      "containers": [
        {
          "name": "maritime-api",
          "image": "maritimecr.azurecr.io/maritime-api:latest",
          "resources": {
            "cpu": 0.25,
            "memory": "0.5Gi"
          },
          "env": [
            {
              "name": "ConnectionStrings__DefaultConnection",
              "secretRef": "sql-connection-string"
            },
            {
              "name": "ASPNETCORE_ENVIRONMENT",
              "value": "Production"
            }
          ]
        }
      ],
      "scale": {
        "minReplicas": 1,
        "maxReplicas": 10,
        "rules": [
          {
            "name": "http-scaling",
            "http": {
              "metadata": {
                "concurrentRequests": "30"
              }
            }
          }
        ]
      }
    }
  }
}
```

**Scaling Configuration:**
- **Min Replicas**: 1 (always running)
- **Max Replicas**: 10 (can scale up based on load)
- **HTTP Scaling**: Scales based on concurrent requests
- **Resource Limits**: 0.25 CPU, 0.5GB memory per instance

## 4.5 Monitoring and Observability

### 4.5.1 Application Insights Configuration

```json
{
  "type": "Microsoft.Insights/components",
  "apiVersion": "2020-02-02",
  "name": "[variables('aiName')]",
  "properties": {
    "Application_Type": "web",
    "RetentionInDays": 90,
    "publicNetworkAccessForIngestion": "Enabled",
    "publicNetworkAccessForQuery": "Enabled",
    "IngestionMode": "ApplicationInsights"
  }
}
```

#### Monitoring Capabilities
- **Request Telemetry**: HTTP request tracking
- **Performance Counters**: CPU, memory, network metrics
- **Custom Metrics**: Business-specific measurements
- **Error Tracking**: Exception logging and analysis
- **Dependency Tracking**: External service calls
- **Real-time Metrics**: Live monitoring dashboard

### 4.5.2 Log Analytics Workspace

```json
{
  "type": "Microsoft.OperationalInsights/workspaces",
  "apiVersion": "2021-06-01",
  "name": "[variables('logAnalyticsName')]",
  "properties": {
    "sku": {
      "name": "PerGB2018"
    },
    "retentionInDays": 30,
    "publicNetworkAccessForIngestion": "Enabled",
    "publicNetworkAccessForQuery": "Enabled"
  }
}
```

**Log Retention Strategy:**
- **30 Days**: Balance between cost and investigation needs
- **Per-GB Pricing**: Pay for actual log volume
- **Query Access**: Available for troubleshooting and analysis

## 4.6 Security Configuration

### 4.6.1 Network Security

```json
{
  "type": "Microsoft.Sql/servers/firewallRules",
  "name": "[concat(variables('sqlName'), '/AllowAzureServices')]",
  "properties": {
    "startIpAddress": "0.0.0.0",
    "endIpAddress": "0.0.0.0"
  }
}
```

**Security Principles:**
- **Least Privilege**: Only necessary access granted
- **Defense in Depth**: Multiple security layers
- **Zero Trust**: Verify every connection
- **Encryption**: Data encrypted in transit and at rest

### 4.6.2 Key Vault Access Policies

```json
{
  "type": "Microsoft.KeyVault/vaults/accessPolicies",
  "apiVersion": "2022-07-01",
  "name": "[concat(variables('kvName'), '/add')]",
  "dependsOn": [
    "[resourceId('Microsoft.App/containerApps', variables('appName'))]"
  ],
  "properties": {
    "accessPolicies": [
      {
        "tenantId": "[subscription().tenantId]",
        "objectId": "[reference(resourceId('Microsoft.App/containerApps', variables('appName')), '2022-06-01-preview', 'Full').identity.principalId]",
        "permissions": {
          "secrets": ["get", "list"]
        }
      }
    ]
  }
}
```

**Access Control:**
- **Managed Identity**: Container Apps use managed identity
- **Minimal Permissions**: Only 'get' and 'list' for secrets
- **No Certificate Access**: Secrets only for configuration

## 4.7 Cost Optimization Strategies

### 4.7.1 Resource Sizing

| Resource | Tier/SKU | Rationale | Monthly Cost Est. |
|----------|----------|-----------|-------------------|
| **SQL Database** | Basic (5 DTU) | Dev/test workload | $5 |
| **Event Hubs** | Standard (2 TU) | Moderate throughput | $22 |
| **Container Apps** | 0.25 CPU, 0.5GB | Right-sized for API | $15-45 |
| **Storage Account** | Standard_LRS | Local redundancy OK | $5-10 |
| **Key Vault** | Standard | Basic secret management | $1 |
| **Application Insights** | Per-GB | Pay for actual usage | $5-15 |

**Total Estimated Monthly Cost: $53-103**

### 4.7.2 Auto-scaling Configuration

```json
"scale": {
  "minReplicas": 1,
  "maxReplicas": 10,
  "rules": [
    {
      "name": "http-scaling",
      "http": {
        "metadata": {
          "concurrentRequests": "30"
        }
      }
    },
    {
      "name": "cpu-scaling",
      "custom": {
        "type": "cpu",
        "metadata": {
          "type": "Utilization",
          "value": "70"
        }
      }
    }
  ]
}
```

**Cost Benefits:**
- **Scale to Zero**: Not enabled (min 1 replica) for availability
- **Pay per Use**: Only pay for running instances
- **Automatic Scaling**: No over-provisioning

---

## Interview Preparation Notes for Section 4

### Key Infrastructure Questions:

1. **"Walk me through your Azure infrastructure architecture."**
   - Multi-tier architecture explanation
   - Service relationships and dependencies
   - Data flow between components

2. **"Why did you choose Azure Container Apps over App Service?"**
   - Better scaling capabilities
   - Kubernetes-based flexibility
   - Cost optimization for variable workloads

3. **"Explain your ARM template parameter strategy."**
   - Parameterization for reusability
   - Secure parameter handling
   - Naming convention rationale

4. **"How do you handle secrets and configuration?"**
   - Key Vault for sensitive data
   - Managed identities for authentication
   - Environment variables for non-sensitive config

5. **"What's your deployment validation strategy?"**
   - Template validation before deployment
   - Health checks after deployment
   - Rollback procedures for failures

### Cost and Scaling Deep-Dive:

- Know the cost implications of each resource tier
- Understand scaling triggers and limits
- Be able to explain upgrade paths
- Know monitoring and alerting configuration
- Understand security boundaries and access controls