# Azure Container Registry Setup Guide

## Problem: 403 Forbidden Error on ACR Login

The CI/CD pipeline is failing because the Azure Container Registry authentication is not properly configured.

## Solution: Use Azure Service Principal with Federated Identity

### Step 1: Create Azure Service Principal

```bash
# Set variables
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
RESOURCE_GROUP="maritime-platform-rg"ACR_NAME="maritimeregistry70396"SP_NAME="github-maritimeiq-sp"# Create service principal
az ad sp create-for-rbac \
 --name $SP_NAME \
 --role contributor \
 --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP \
 --sdk-auth
```

### Step 2: Assign ACR Permissions

```bash
# Get Service Principal App ID
SP_APP_ID=$(az ad sp list --display-name $SP_NAME --query [0].appId -o tsv)

# Get ACR Resource ID
ACR_REGISTRY_ID=$(az acr show --name $ACR_NAME --query id -o tsv)

# Assign AcrPush role to Service Principal
az role assignment create \
 --assignee $SP_APP_ID \
 --scope $ACR_REGISTRY_ID \
 --role AcrPush
```

### Step 3: Configure GitHub Secrets

Add the following secrets to your GitHub repository (Settings > Secrets and variables > Actions):

1. **AZURE_CREDENTIALS**: The entire JSON output from Step 1, formatted as:
```json
{
 "clientId": "<APP_ID>",
 "clientSecret": "<PASSWORD>",
 "subscriptionId": "<SUBSCRIPTION_ID>",
 "tenantId": "<TENANT_ID>",
 "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
 "resourceManagerEndpointUrl": "https://management.azure.com/",
 "activeDirectoryGraphResourceId": "https://graph.windows.net/",
 "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
 "galleryEndpointUrl": "https://gallery.azure.com/",
 "managementEndpointUrl": "https://management.core.windows.net/"}
```

2. **ACR_LOGIN_SERVER**: `maritimeregistry70396.azurecr.io`

3. **AZURE_RESOURCE_GROUP**: `maritime-platform-rg`

### Step 4: Verify ACR Access

```bash
# Test ACR login
az acr login --name maritimeregistry70396

# List repositories
az acr repository list --name maritimeregistry70396 --output table
```

## Alternative: Use ACR Admin Credentials (Not Recommended for Production)

If you prefer username/password authentication (less secure):

```bash
# Enable admin user on ACR
az acr update --name maritimeregistry70396 --admin-enabled true

# Get credentials
az acr credential show --name maritimeregistry70396
```

Then set these GitHub secrets:
- **ACR_USERNAME**: Username from the command above
- **ACR_PASSWORD**: Password from the command above

## Troubleshooting

### 403 Forbidden Error
- Verify service principal has AcrPush role
- Check that AZURE_CREDENTIALS secret is properly formatted
- Ensure ACR name matches in workflow and Azure

### Authentication Timeout
- Check if ACR firewall rules are blocking GitHub Actions IPs
- Verify network settings allow public access

### Permission Denied
```bash
# Check role assignments
az role assignment list --assignee <SP_APP_ID> --all -o table

# Verify ACR permissions
az acr check-name --name maritimeregistry70396
```

## Updated Workflow Authentication

The workflows now use:
```yaml
- name: Azure Login
 uses: azure/login@v1
 with:
 creds: ${{ secrets.AZURE_CREDENTIALS }}

- name: Log in to Azure Container Registry
 run: |
 az acr login --name maritimeregistry70396
```

This method is more secure and recommended by Microsoft for GitHub Actions integration.
