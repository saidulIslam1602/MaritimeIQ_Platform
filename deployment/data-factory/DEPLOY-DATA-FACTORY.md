# Azure Data Factory Deployment Guide

## Data Factory Created
Your Data Factory is ready: **maritimeiq-data-factory**

---

## Quick Deployment (Azure Portal - Recommended)

### Step 1: Access Data Factory Studio
Click this link (opens in browser):
```
https://adf.azure.com/
```

Or go to Azure Portal → Search "maritimeiq-data-factory"→ Click "Launch Studio"### Step 2: Create Linked Services (4 services)

In Data Factory Studio:
1. Click **"Manage"** (toolbox icon, left sidebar)
2. Click **"Linked services"** → **"+ New"**

#### A. Event Hub Linked Service
- **Name**: `aisdataeventhub`
- **Type**: Azure Event Hubs
- **Event Hub namespace**: `ehns-maritimeplat-70396`
- **Authentication**: Key-based
- **Connection method**: From Azure subscription
- Click **"Test connection"** → **"Create"**

#### B. SQL Database Linked Service 
- **Name**: `maritimesqldb`
- **Type**: Azure SQL Database
- **Server**: `sql-maritimeplat-70396.database.windows.net`
- **Database**: `maritimeplat-db`
- **Authentication**: System Managed Identity
- Click **"Test connection"** → **"Create"**

#### C. Storage Linked Service
- **Name**: `maritimestorage`
- **Type**: Azure Blob Storage
- **Storage account**: `stmaritimeplat70396`
- **Authentication**: Account key or Managed Identity
- Click **"Test connection"** → **"Create"**

#### D. Key Vault Linked Service
- **Name**: `maritimekeyvault` 
- **Type**: Azure Key Vault
- **Key Vault**: `kv-maritimeplat-70396`
- **Authentication**: System Managed Identity
- Click **"Test connection"** → **"Create"**

### Step 3: Import Pipelines (5 pipelines)

1. Click **"Author"** (pencil icon, left sidebar)
2. Click **"+"** → **"Pipeline"** → **"Import from pipeline template"**
3. Or use **"+ → Import from JSON file"**

Import these files one by one from your local folder:
```
deployment/data-factory/maritime-data-ingestion-pipeline.json
deployment/data-factory/maritime-data-transformation-pipeline.json
deployment/data-factory/maritime-data-quality-pipeline.json
deployment/data-factory/maritime-lakehouse-etl-pipeline.json
deployment/data-factory/maritime-realtime-streaming-pipeline.json
```

4. After each import, click **"Publish all"** (top toolbar)

---

## Your 5 Data Pipelines

| Pipeline | Purpose | Trigger |
|----------|---------|---------|
| **maritime-data-ingestion** | Ingest AIS data from Event Hub to SQL | Continuous |
| **maritime-data-transformation** | Transform and enrich vessel data | Scheduled (hourly) |
| **maritime-data-quality** | Data quality validation and cleansing | Scheduled (daily) |
| **maritime-lakehouse-etl** | ETL to Delta Lake/Databricks | Scheduled (nightly) |
| **maritime-realtime-streaming** | Real-time stream processing | Continuous |

---

## Alternative: Deploy via Azure CLI (Advanced)

If you prefer CLI, use the PowerShell-based deployment:

```powershell
# Install required tools
Install-Module -Name Az.DataFactory -Force

# Deploy using PowerShell cmdlets (better than Azure CLI for ADF)
Set-AzDataFactoryV2LinkedService -ResourceGroupName "maritime-platform-rg"`
 -DataFactoryName "maritimeiq-data-factory"`
 -Name "aisdataeventhub"`
 -DefinitionFile "deployment/data-factory/linked-services/eventhub-linked-service.json"```

---

## After Deployment

### Access Your Data Factory:
 **Portal**: https://portal.azure.com → maritimeiq-data-factory 
🎨 **Studio**: https://adf.azure.com 
 **Monitor**: Check pipeline runs in the Monitor tab

### Test Your Pipelines:
1. Go to **Author** tab
2. Click on a pipeline 
3. Click **"Debug"** to test run
4. Check **"Monitor"** tab for results

---

## 🔐 Security Notes

The Data Factory has **System Managed Identity** enabled.

Grant it access to your resources:
```bash
# Give Data Factory access to Key Vault
az keyvault set-policy \
 --name kv-maritimeplat-70396 \
 --object-id da6d4a4a-8beb-459d-ad72-17177a26feb5 \
 --secret-permissions get list

# Give Data Factory access to Storage 
az role assignment create \
 --assignee da6d4a4a-8beb-459d-ad72-17177a26feb5 \
 --role "Storage Blob Data Contributor"\
 --scope /subscriptions/916798ca-14df-467f-9f2a-4d37dfefee0c/resourceGroups/maritime-platform-rg/providers/Microsoft.Storage/storageAccounts/stmaritimeplat70396
```

---

**Recommendation: Use the Azure Portal (Step 1-3 above). It's visual, intuitive, and works perfectly for Data Factory deployments!** 
