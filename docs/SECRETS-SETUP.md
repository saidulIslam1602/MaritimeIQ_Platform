# Havila Maritime Platform – Secrets & Connection Strings

This guide explains how to replace the placeholder connection strings with secure, production-grade secrets so the platform can run against Havila’s live Azure estate.

## 1. Preferred pattern: Azure Key Vault + managed identity

1. **Enable a system‑assigned managed identity** on the Azure App Service / Container App that will host the API.
2. **Assign Key Vault access**:
   ```bash
   az keyvault set-policy \
     --name <havila-keyvault-name> \
     --spn <principal-id-of-managed-identity> \
     --secret-permissions get list
   ```
3. **Create secrets using configuration friendly names** (the script `devops/scripts/deploy-havila-maritime-platform.sh` already does this):
   ```bash
   az keyvault secret set --vault-name <havila-keyvault-name> --name ServiceBus--ConnectionString --value "<actual-servicebus-connection>"
   az keyvault secret set --vault-name <havila-keyvault-name> --name EventHub--ConnectionString --value "<actual-eventhub-connection>"
   az keyvault secret set --vault-name <havila-keyvault-name> --name IoTHub--ConnectionString --value "<actual-iothub-connection>"
   az keyvault secret set --vault-name <havila-keyvault-name> --name Database--ConnectionString --value "<sql-connection-string>"
   az keyvault secret set --vault-name <havila-keyvault-name> --name ApplicationInsights--ConnectionString --value "InstrumentationKey=<...>;IngestionEndpoint=<...>"
   az keyvault secret set --vault-name <havila-keyvault-name> --name CognitiveServices--Key --value "<cognitive-services-key>"
   az keyvault secret set --vault-name <havila-keyvault-name> --name PowerBI--ClientSecret --value "<powerbi-service-principal-secret>"
   ```
4. **Deploy** the app. At runtime the `Program.cs` bootstrapping loads Key Vault using `DefaultAzureCredential`. No manual secrets in configuration files are required.

> ℹ️  Secrets named with `--` automatically bind to configuration paths, e.g. `ServiceBus--ConnectionString` populates `ServiceBus:ConnectionString`.

## 2. Alternative: explicit service principal (client secret)

If the hosting environment cannot use a managed identity, populate the following environment variables (or update `config/appsettings.Production.json`) before startup:

```
KeyVault__VaultUri=https://<havila-keyvault-name>.vault.azure.net/
KeyVault__TenantId=<azure-tenant-guid>
KeyVault__ClientId=<app-registration-client-id>
KeyVault__ClientSecret=<app-registration-client-secret>
KeyVault__UseManagedIdentity=false
```

The code automatically falls back to `DefaultAzureCredential` when `UseManagedIdentity` is `true` or the client secret values are omitted.

## 3. Local development overrides

For local debugging without Key Vault you can export environment variables that mirror the configuration keys:

```bash
export ServiceBus__ConnectionString="Endpoint=sb://..."
export EventHub__ConnectionString="Endpoint=sb://..."
export IoTHub__ConnectionString="HostName=..."
export Database__ConnectionString="Server=...;Database=...;User Id=...;Password=..."
export ApplicationInsights__ConnectionString="InstrumentationKey=..."
export CognitiveServices__TextAnalyticsKey="<key>"
export PowerBI__ClientSecret="<secret>"
```

Environment variables take precedence over JSON files, so the application will run with genuine credentials while keeping the repository free from secrets.

## 4. Deployment checklist

- [ ] Managed identity has `get` / `list` permissions on Key Vault secrets.
- [ ] Required secrets are present in Key Vault (`ServiceBus--ConnectionString`, `EventHub--ConnectionString`, etc.).
- [ ] Optional: confirm using `az keyvault secret show --name ServiceBus--ConnectionString --vault-name <havila-keyvault-name>`.
- [ ] Application Insights connection string secret is populated (needed for telemetry).
- [ ] Power BI service principal credentials stored (`PowerBI--ClientSecret`, `PowerBI--ClientId`, `PowerBI--TenantId`).
- [ ] `dotnet test` or integration smoke tests executed after setting secrets.

With these steps the placeholders are eliminated, Key Vault-backed configuration is live, and the platform can authenticate against Havila’s production Azure services without code changes.
