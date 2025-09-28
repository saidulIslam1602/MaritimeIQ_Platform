#!/usr/bin/env bash

# Azure Maritime Platform - Professional Interview Deployment
# This script deploys the entire maritime data platform into Azure Container Apps

set -euo pipefail
IFS=$'\n\t'

# -----------------------------------------------------------------------------
# Styling
# -----------------------------------------------------------------------------
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m'

# -----------------------------------------------------------------------------
# Defaults (can be overridden via CLI flags)
# -----------------------------------------------------------------------------
RESOURCE_GROUP="maritime-platform-rg"
LOCATION="norwayeast"
APP_NAME="maritime-platform"
SQL_ADMIN_PASSWORD=""
UNIQUE_ID="$(date +%s | cut -c7-10)"
IMAGE_TAG=""
SKIP_IMAGE_BUILD="false"

# -----------------------------------------------------------------------------
# Derived variables (populated at runtime)
# -----------------------------------------------------------------------------
SCRIPT_DIR=""
PROJECT_ROOT=""
SANITIZED_APP_NAME=""
SHORT_APP_NAME=""
ACR_NAME=""
REGISTRY_SERVER=""
IMAGE_REPO=""
CONTAINER_IMAGE=""
CONTAINER_APP_ENV=""
DEPLOYMENT_NAME=""
SQL_SERVER_NAME=""
SQL_DATABASE_NAME=""
STORAGE_ACCOUNT_NAME=""
EVENT_HUB_NAMESPACE=""
STREAM_ANALYTICS_JOB=""
APP_INSIGHTS_NAME=""
KEY_VAULT_NAME=""
SERVICE_BUS_NAMESPACE=""
COGNITIVE_ACCOUNT=""
SUBSCRIPTION_NAME=""
SUBSCRIPTION_ID=""

# -----------------------------------------------------------------------------
# Utility output helpers
# -----------------------------------------------------------------------------
print_header() {
    echo -e "${BLUE}==================================================================${NC}"
    echo -e "${PURPLE}    🚢  AZURE MARITIME PLATFORM - FULL DEPLOYMENT  🚢${NC}"
    echo -e "${BLUE}==================================================================${NC}"
    echo -e "${CYAN}Enterprise-grade real-time maritime data platform deployment${NC}"
    echo
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
    exit 1
}

# -----------------------------------------------------------------------------
# Argument parsing and derived value setup
# -----------------------------------------------------------------------------
require_value() {
    local flag="$1"
    shift
    [[ $# -ge 1 ]] || print_error "Missing value for ${flag}"
    echo "$1"
}

ensure_sql_password() {
    if [[ -n "${SQL_ADMIN_PASSWORD}" ]]; then
        return
    fi

    print_info "Generating strong SQL admin password"

    SQL_ADMIN_PASSWORD=$(python3 - <<'PY'
import secrets
import string

alphabet = string.ascii_letters + string.digits + "!@#$%^&*()_+=-"
for _ in range(1000):
    candidate = ''.join(secrets.choice(alphabet) for _ in range(24))
    lower = any(c.islower() for c in candidate)
    upper = any(c.isupper() for c in candidate)
    digit = any(c.isdigit() for c in candidate)
    special = any(c in "!@#$%^&*()_+=-" for c in candidate)
    banned = any(term in candidate.lower() for term in ("maritime", "admin", "password"))
    if lower and upper and digit and special and not banned:
        print(candidate)
        break
else:
    raise SystemExit("Failed to generate strong password")
PY
)

    if [[ -z "${SQL_ADMIN_PASSWORD}" ]]; then
        print_error "Unable to generate SQL admin password"
    fi

    print_success "SQL admin password generated"
}

parse_args() {
    while [[ $# -gt 0 ]]; do
        case "$1" in
            --resource-group|-g)
                RESOURCE_GROUP="$(require_value "$1" "$2")"
                shift 2
                ;;
            --location|-l)
                LOCATION="$(require_value "$1" "$2")"
                shift 2
                ;;
            --app-name|-a)
                APP_NAME="$(require_value "$1" "$2")"
                shift 2
                ;;
            --sql-admin-password)
                SQL_ADMIN_PASSWORD="$(require_value "$1" "$2")"
                shift 2
                ;;
            --unique-id)
                UNIQUE_ID="$(require_value "$1" "$2")"
                shift 2
                ;;
            --image-tag)
                IMAGE_TAG="$(require_value "$1" "$2")"
                shift 2
                ;;
            --skip-image-build)
                SKIP_IMAGE_BUILD="true"
                shift
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
            --)
                shift
                break
                ;;
            --*)
                print_error "Unknown option: $1"
                ;;
            *)
                break
                ;;
        esac
    done
}

set_derived_variables() {
    SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
    PROJECT_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)

    SANITIZED_APP_NAME=$(echo "$APP_NAME" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
    [[ -n "$SANITIZED_APP_NAME" ]] || print_error "Application name must contain at least one alphanumeric character"

    SHORT_APP_NAME=${SANITIZED_APP_NAME:0:12}
    if [[ ${#SHORT_APP_NAME} -lt 3 ]]; then
        SHORT_APP_NAME=$(printf '%-3s' "$SANITIZED_APP_NAME" | tr ' ' 'x')
        SHORT_APP_NAME=${SHORT_APP_NAME:0:3}
    fi

    ACR_NAME="${SHORT_APP_NAME}acr"
    REGISTRY_SERVER="${ACR_NAME}.azurecr.io"
    IMAGE_REPO="${SHORT_APP_NAME}-api"
    if [[ -z "$IMAGE_TAG" ]]; then
        IMAGE_TAG="$UNIQUE_ID"
    fi

    CONTAINER_IMAGE="${REGISTRY_SERVER}/${IMAGE_REPO}:${IMAGE_TAG}"
    CONTAINER_APP_ENV="${SHORT_APP_NAME}-env"
    DEPLOYMENT_NAME="infra-${SHORT_APP_NAME}-${UNIQUE_ID}"

    SQL_SERVER_NAME="sql-${SHORT_APP_NAME}-${UNIQUE_ID}"
    SQL_DATABASE_NAME="${SHORT_APP_NAME}-db"
    STORAGE_ACCOUNT_NAME="st${SHORT_APP_NAME}${UNIQUE_ID}"
    EVENT_HUB_NAMESPACE="ehns-${SHORT_APP_NAME}-${UNIQUE_ID}"
    STREAM_ANALYTICS_JOB="sa-${SHORT_APP_NAME}-${UNIQUE_ID}"
    APP_INSIGHTS_NAME="${SHORT_APP_NAME}-insights"
    KEY_VAULT_NAME="kv-${SHORT_APP_NAME}-${UNIQUE_ID}"
    SERVICE_BUS_NAMESPACE="sb-${SHORT_APP_NAME}-${UNIQUE_ID}"
    COGNITIVE_ACCOUNT="cs${SHORT_APP_NAME}${UNIQUE_ID}"
}

# -----------------------------------------------------------------------------
# Azure helpers
# -----------------------------------------------------------------------------
check_azure_cli() {
    print_info "Verifying Azure CLI availability"

    if ! command -v az >/dev/null 2>&1; then
        print_error "Azure CLI not found. Install it from https://aka.ms/azure-cli."
    fi

    if ! az account show >/dev/null 2>&1; then
        print_warning "Azure CLI not authenticated. Launching 'az login'."
        az login >/dev/null
    fi

    SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    print_success "Using Azure subscription: ${SUBSCRIPTION_NAME}"
}

ensure_cli_extensions() {
    print_info "Configuring Azure CLI to install extensions silently"
    az config set extension.use_dynamic_install=yes_without_prompt >/dev/null
    az config set extension.dynamic_install_allow_preview=true >/dev/null
    print_success "Azure CLI extension auto-install enabled"
}

ensure_provider_registration() {
    local providers=(
        "Microsoft.App"
        "Microsoft.ContainerRegistry"
        "Microsoft.EventHub"
        "Microsoft.KeyVault"
        "Microsoft.ServiceBus"
        "Microsoft.StreamAnalytics"
        "Microsoft.OperationalInsights"
        "Microsoft.Insights"
        "Microsoft.CognitiveServices"
        "Microsoft.Sql"
        "Microsoft.Storage"
    )

    for provider in "${providers[@]}"; do
        local status
        status=$(az provider show --namespace "$provider" --query registrationState -o tsv 2>/dev/null || echo "NotRegistered")
        if [[ "$status" != "Registered" ]]; then
            print_info "Registering provider: $provider"
            az provider register --namespace "$provider" --wait >/dev/null
            print_success "Provider registered: $provider"
        else
            print_info "Provider already registered: $provider"
        fi
    done
}

create_resource_group() {
    print_info "Ensuring resource group '${RESOURCE_GROUP}' in ${LOCATION}"
    az group create \
        --name "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --tags Environment=Interview Project=MaritimePlatform Owner="$(whoami)" \
        --output none
    print_success "Resource group ready"
}

deploy_core_infrastructure() {
    print_info "Deploying shared infrastructure via ARM template"
    az deployment group create \
        --name "$DEPLOYMENT_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "$SCRIPT_DIR/azure-infrastructure.json" \
        --parameters \
            appName="$APP_NAME" \
            uniqueId="$UNIQUE_ID" \
            sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
            location="$LOCATION" \
        --only-show-errors \
        --output none
    print_success "Core infrastructure deployed"
}

ensure_acr() {
    print_info "Ensuring Azure Container Registry '${ACR_NAME}'"

    if az acr show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" >/dev/null 2>&1; then
        print_info "Container Registry exists"
    else
        az acr create \
            --name "$ACR_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --sku Standard \
            --admin-enabled true \
            --output none
        print_success "Container Registry created"
    fi

    az acr update --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --admin-enabled true --output none
    print_success "Container Registry admin user enabled"
}

build_and_push_image() {
    if [[ "$SKIP_IMAGE_BUILD" == "true" ]]; then
        print_info "Skipping container image build. Using pre-built image: ${CONTAINER_IMAGE}"
        return
    fi

    print_info "Building application container image using ACR build"

    local build_output
    if ! build_output=$(az acr build \
        --registry "$ACR_NAME" \
        --image "${IMAGE_REPO}:${IMAGE_TAG}" \
        --file "$PROJECT_ROOT/deployment/docker/Dockerfile" \
        "$PROJECT_ROOT" 2>&1); then
        echo "$build_output" >&2
        if [[ "$build_output" == *"TasksOperationsNotAllowed"* ]]; then
            print_error "ACR Tasks are disabled for this subscription. Please build and push the image via the provided GitHub Actions workflow or enable ACR Tasks, then rerun the deployment."
        fi
        print_error "ACR build failed. Review the log above for details."
    fi

    print_success "Container image published: ${CONTAINER_IMAGE}"
}

ensure_containerapp_environment() {
    if az containerapp env show --name "$CONTAINER_APP_ENV" --resource-group "$RESOURCE_GROUP" >/dev/null 2>&1; then
        print_info "Container Apps environment exists"
    else
        print_info "Creating Container Apps environment '${CONTAINER_APP_ENV}'"
        az containerapp env create \
            --name "$CONTAINER_APP_ENV" \
            --resource-group "$RESOURCE_GROUP" \
            --location "$LOCATION" \
            --output none
        print_success "Container Apps environment created"
    fi
}

deploy_container_app() {
    print_info "Deploying Container App '${APP_NAME}'"

    local acr_username
    local acr_password
    acr_username=$(az acr credential show --name "$ACR_NAME" --query username -o tsv)
    acr_password=$(az acr credential show --name "$ACR_NAME" --query passwords[0].value -o tsv)

    if az containerapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" >/dev/null 2>&1; then
        az containerapp update \
            --name "$APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --image "$CONTAINER_IMAGE" \
            --set-env-vars ASPNETCORE_URLS=http://+:8080 ASPNETCORE_HTTP_PORTS=8080 \
            --output none
        print_success "Container App image updated"
    else
        az containerapp create \
            --name "$APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --environment "$CONTAINER_APP_ENV" \
            --image "$CONTAINER_IMAGE" \
            --ingress external \
            --target-port 8080 \
            --min-replicas 1 \
            --max-replicas 4 \
            --cpu 1.0 \
            --memory 2Gi \
            --registry-server "$REGISTRY_SERVER" \
            --registry-username "$acr_username" \
            --registry-password "$acr_password" \
            --env-vars ASPNETCORE_URLS=http://+:8080 ASPNETCORE_HTTP_PORTS=8080 \
            --output none
        print_success "Container App created"
    fi
}

grant_keyvault_secret_permissions() {
    print_info "Ensuring Key Vault permissions for deployment principal"

    local principal_type
    local principal_name
    principal_type=$(az account show --query "user.type" -o tsv 2>/dev/null | tr '[:upper:]' '[:lower:]')
    principal_name=$(az account show --query "user.name" -o tsv 2>/dev/null || echo "")

    local principal_object_id=""
    local principal_label="$principal_name"
    local role_principal_type="User"

    case "$principal_type" in
        user)
            principal_object_id=$(az ad signed-in-user show --query id -o tsv 2>/dev/null || echo "")
            role_principal_type="User"
            ;;
        serviceprincipal)
            if [[ -n "$principal_name" ]]; then
                principal_object_id=$(az ad sp show --id "$principal_name" --query id -o tsv 2>/dev/null || echo "")
            fi
            role_principal_type="ServicePrincipal"
            ;;
        managedidentity)
            principal_object_id=$(az account show --query "user.assignedIdentityInfo.principalId" -o tsv 2>/dev/null || echo "")
            role_principal_type="ServicePrincipal"
            ;;
        *)
            principal_object_id=$(az ad signed-in-user show --query id -o tsv 2>/dev/null || echo "")
            role_principal_type="User"
            ;;
    esac

    if [[ -z "$principal_object_id" && -n "$principal_name" ]]; then
        principal_object_id=$(az ad user show --id "$principal_name" --query id -o tsv 2>/dev/null || echo "")
    fi

    if [[ -z "$principal_object_id" ]]; then
        print_warning "Unable to resolve current Azure principal. Grant Key Vault access manually if secret storage fails."
        return 1
    fi

    local vault_scope
    vault_scope=$(az keyvault show --name "$KEY_VAULT_NAME" --resource-group "$RESOURCE_GROUP" --query id -o tsv 2>/dev/null || echo "")
    if [[ -z "$vault_scope" ]]; then
        print_warning "Key Vault '$KEY_VAULT_NAME' not found while assigning permissions."
        return 1
    fi

    local rbac_enabled
    rbac_enabled=$(az keyvault show --name "$KEY_VAULT_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.enableRbacAuthorization" -o tsv 2>/dev/null | tr '[:upper:]' '[:lower:]')

    if [[ "$rbac_enabled" == "true" ]]; then
        local role_output=""
        if role_output=$(az role assignment create \
            --assignee-object-id "$principal_object_id" \
            --assignee-principal-type "$role_principal_type" \
            --role "Key Vault Secrets Officer" \
            --scope "$vault_scope" \
            --output none 2>&1); then
            print_success "Key Vault RBAC role assigned to ${principal_label:-current principal}"
            return 0
        else
            if [[ "$role_output" == *"RoleAssignmentExists"* || "$role_output" == *"already exists"* ]]; then
                print_success "Key Vault RBAC role already assigned to ${principal_label:-current principal}"
                return 0
            fi
            print_warning "Failed to assign Key Vault RBAC role automatically. Details: ${role_output}"
            return 1
        fi
    fi

    local policy_output=""
    if policy_output=$(az keyvault set-policy \
        --name "$KEY_VAULT_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --object-id "$principal_object_id" \
        --secret-permissions get list set \
        --output none 2>&1); then
        print_success "Key Vault access policy configured for ${principal_label:-current principal}"
        return 0
    else
        if [[ "$policy_output" == *"AccessPolicyAlreadyExists"* || "$policy_output" == *"already enabled"* ]]; then
            print_success "Key Vault access policy already present for ${principal_label:-current principal}"
            return 0
        fi
        print_warning "Failed to configure Key Vault access policy automatically. Details: ${policy_output}"
        return 1
    fi
}

setup_security() {
    print_info "Storing SQL credentials in Key Vault '${KEY_VAULT_NAME}'"
    grant_keyvault_secret_permissions || true

    local attempt=1
    local max_attempts=5
    while (( attempt <= max_attempts )); do
        if az keyvault secret set \
            --vault-name "$KEY_VAULT_NAME" \
            --name "sql-admin-password" \
            --value "$SQL_ADMIN_PASSWORD" \
            --output none >/dev/null 2>&1; then
            print_success "SQL admin password stored in Key Vault"
            return
        fi

        if (( attempt < max_attempts )); then
            print_info "Waiting for Key Vault permissions to propagate before retrying (${attempt}/${max_attempts})"
            sleep 5
        fi
        ((attempt++))
    done

    print_warning "Could not store SQL password in Key Vault automatically. Verify access policies and retry."
}

deploy_demo_data() {
    print_info "Demo data generators and analytics artifacts are ready for manual execution"
    print_success "Demo data configuration placeholder executed"
}

show_deployment_summary() {
    print_header
    echo -e "${GREEN}🎉 Deployment completed successfully!${NC}\n"

    local app_url
    app_url=$(az containerapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)

    echo -e "${BLUE}🔗 Access:${NC}"
    if [[ -n "$app_url" ]]; then
        echo "   • Maritime Platform API: https://${app_url}"
    else
        echo "   • Maritime Platform API: (ingress pending)"
    fi
    echo "   • Azure Portal Resource Group: https://portal.azure.com/#@/resource/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}"
    echo

    echo -e "${BLUE}📦 Provisioned Services:${NC}"
    echo "   • Container App: ${APP_NAME}"
    echo "   • Container Apps Environment: ${CONTAINER_APP_ENV}"
    echo "   • Container Registry: ${ACR_NAME}"
    echo "   • SQL Server / DB: ${SQL_SERVER_NAME} / ${SQL_DATABASE_NAME}"
    echo "   • Storage Account: ${STORAGE_ACCOUNT_NAME}"
    echo "   • Event Hub Namespace: ${EVENT_HUB_NAMESPACE}"
    echo "   • Stream Analytics Job: ${STREAM_ANALYTICS_JOB}"
    echo "   • Service Bus Namespace: ${SERVICE_BUS_NAMESPACE}"
    echo "   • Cognitive Services Account: ${COGNITIVE_ACCOUNT}"
    echo "   • Application Insights: ${APP_INSIGHTS_NAME}"
    echo "   • Key Vault: ${KEY_VAULT_NAME}"
    echo

    echo -e "${YELLOW}💡 Next steps:${NC}"
    echo "   • Configure application settings and connection strings as needed"
    echo "   • Run Stream Analytics query defined in deployment artifacts"
    echo "   • Trigger demo data generator scripts from './demos'"
    echo

    print_success "Azure Maritime Platform is ready!"
}

show_status() {
    print_info "Resource group provisioning state:"
    az group show --name "$RESOURCE_GROUP" --query properties.provisioningState -o tsv
    echo
    print_info "Resources in '${RESOURCE_GROUP}':"
    az resource list --resource-group "$RESOURCE_GROUP" --output table
}

cleanup_deployment() {
    print_warning "This will delete resource group '${RESOURCE_GROUP}' and ALL contained resources. Continue? (y/N)"
    read -r response
    if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        print_info "Deleting resource group '${RESOURCE_GROUP}'"
        az group delete --name "$RESOURCE_GROUP" --yes --no-wait
        print_success "Cleanup initiated"
    else
        print_info "Cleanup cancelled"
    fi
}

show_help() {
    cat <<EOF
Azure Maritime Platform - Deployment Script

Usage:
  $(basename "$0") [command] [options]

Commands:
  deploy        Deploy the full platform (default)
  status        Show resource group status
  cleanup       Delete the resource group and all resources
  help          Display this message

Options:
  -g, --resource-group <name>     Override resource group (default: ${RESOURCE_GROUP})
  -l, --location <azure-region>   Azure region for deployment (default: ${LOCATION})
  -a, --app-name <name>           Application base name (default: ${APP_NAME})
      --sql-admin-password <pw>   SQL admin password (default: auto value)
      --unique-id <suffix>        Override unique suffix for resource names
  -h, --help                      Show help

Examples:
  $(basename "$0") deploy --resource-group maritime-platform-rg --location westeurope
  $(basename "$0") status -g maritime-platform-rg
EOF
}

# -----------------------------------------------------------------------------
# Main entry point
# -----------------------------------------------------------------------------
main() {
    local command="deploy"

    if [[ $# -gt 0 ]]; then
        case "$1" in
            deploy|cleanup|status)
                command="$1"
                shift
                ;;
            help|-h|--help)
                show_help
                exit 0
                ;;
            --*)
                command="deploy"
                ;;
            *)
                print_error "Unknown command: $1"
                ;;
        esac
    fi

    parse_args "$@"

    case "$command" in
        deploy)
            set_derived_variables
            print_header
            check_azure_cli
            ensure_cli_extensions
            ensure_provider_registration
            ensure_sql_password
            create_resource_group
            deploy_core_infrastructure
            ensure_acr
            build_and_push_image
            ensure_containerapp_environment
            deploy_container_app
            setup_security
            deploy_demo_data
            show_deployment_summary
            ;;
        status)
            check_azure_cli
            set_derived_variables
            show_status
            ;;
        cleanup)
            check_azure_cli
            cleanup_deployment
            ;;
        *)
            show_help
            ;;
    esac
}

main "$@"