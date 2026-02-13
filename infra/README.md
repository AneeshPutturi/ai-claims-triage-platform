# Infrastructure as Code
## AI-Driven Commercial Claims Intake & Triage Platform

**Version**: 1.0.0  
**Date**: February 2026  
**Status**: LOCKED BASELINE

---

## Overview

This directory contains all Bicep infrastructure definitions for the platform. All Azure resources are provisioned via Bicep—no manual portal clicks, no exceptions.

---

## Structure

```
infra/
├── main.bicep                          # Root orchestration module
├── parameters/
│   ├── dev.parameters.json             # Dev environment parameters
│   ├── staging.parameters.json         # Staging environment parameters
│   └── prod.parameters.json            # Production environment parameters
├── modules/
│   ├── resource-group.bicep            # Resource group with naming conventions
│   ├── log-analytics.bicep             # Centralized logging
│   ├── key-vault.bicep                 # Secret management
│   ├── storage-account.bicep           # Blob storage with lifecycle policies
│   ├── sql-server.bicep                # Azure SQL Server with AAD auth
│   ├── sql-database.bicep              # Azure SQL Database with TDE
│   ├── container-apps-environment.bicep # Container Apps environment
│   └── container-app.bicep             # Backend API container
└── README.md                           # This file
```

---

## Prerequisites

1. **Azure CLI** installed and authenticated
2. **Azure subscription** with appropriate permissions
3. **Azure AD group** for SQL administrators (update SID in sql-server.bicep)
4. **Bicep CLI** (included with Azure CLI 2.20.0+)

---

## Deployment

### Deploy to Dev Environment

```bash
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters parameters/dev.parameters.json
```

### Deploy to Staging Environment

```bash
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters parameters/staging.parameters.json
```

### Deploy to Production Environment

```bash
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters parameters/prod.parameters.json
```

---

## Post-Deployment Steps

### 1. Apply Database Schema

After infrastructure is deployed, apply the V1 schema:

```bash
# Get SQL Server FQDN from deployment outputs
SQL_SERVER=$(az deployment sub show --name <deployment-name> --query properties.outputs.sqlServerName.value -o tsv)

# Run migration script
sqlcmd -S $SQL_SERVER.database.windows.net -d sqldb-claims-dev -G \
  -i ../db/migrations/V1_InitialSchema/999_ExecuteAllV1Migrations.sql
```

### 2. Configure Managed Identity Permissions

Grant Container App managed identity access to SQL and Blob Storage:

```bash
# Get managed identity principal ID
PRINCIPAL_ID=$(az deployment sub show --name <deployment-name> \
  --query properties.outputs.containerAppManagedIdentityPrincipalId.value -o tsv)

# Grant SQL access (run as SQL admin)
sqlcmd -S $SQL_SERVER.database.windows.net -d sqldb-claims-dev -G -Q \
  "CREATE USER [ca-api-claims-intake-dev] FROM EXTERNAL PROVIDER;
   ALTER ROLE db_datareader ADD MEMBER [ca-api-claims-intake-dev];
   ALTER ROLE db_datawriter ADD MEMBER [ca-api-claims-intake-dev];"

# Grant Blob Storage access
STORAGE_ACCOUNT=$(az deployment sub show --name <deployment-name> \
  --query properties.outputs.storageAccountName.value -o tsv)

az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Storage Blob Data Contributor" \
  --scope "/subscriptions/<subscription-id>/resourceGroups/rg-claims-intake-dev/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT"

# Grant Key Vault access
KEY_VAULT=$(az deployment sub show --name <deployment-name> \
  --query properties.outputs.keyVaultName.value -o tsv)

az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/<subscription-id>/resourceGroups/rg-claims-intake-dev/providers/Microsoft.KeyVault/vaults/$KEY_VAULT"
```

### 3. Validate Security Posture

```bash
# Verify SQL public access is disabled
az sql server show --name $SQL_SERVER --resource-group rg-claims-intake-dev \
  --query publicNetworkAccess

# Verify Blob public access is disabled
az storage account show --name $STORAGE_ACCOUNT --resource-group rg-claims-intake-dev \
  --query allowBlobPublicAccess

# Verify Key Vault protection
az keyvault show --name $KEY_VAULT --resource-group rg-claims-intake-dev \
  --query "[enableSoftDelete, enablePurgeProtection]"
```

---

## Parameter Files

Parameter files contain environment-specific values. Create these files before deployment:

### dev.parameters.json

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "environment": {
      "value": "dev"
    },
    "location": {
      "value": "eastus"
    },
    "owner": {
      "value": "platform-team@company.com"
    },
    "costCenter": {
      "value": "IT-Platform"
    }
  }
}
```

---

## Outputs

After deployment, the following outputs are available:

- `resourceGroupName`: Name of the resource group
- `sqlServerName`: Azure SQL Server name
- `sqlDatabaseName`: Azure SQL Database name
- `storageAccountName`: Storage account name
- `keyVaultName`: Key Vault name
- `containerAppUrl`: Backend API URL
- `logAnalyticsWorkspaceId`: Log Analytics workspace ID

Access outputs:

```bash
az deployment sub show --name <deployment-name> --query properties.outputs
```

---

## Infrastructure Baseline Lock

**EFFECTIVE DATE**: February 2026  
**STATUS**: LOCKED BASELINE

This infrastructure configuration is now the LOCKED BASELINE. Changes to infrastructure require:

1. **Justification**: Why is the change necessary?
2. **Impact Analysis**: What resources and applications are affected?
3. **Versioning**: New Bicep modules or parameter changes must be versioned
4. **Review**: Infrastructure changes require architecture review
5. **Testing**: Changes must be validated in dev and staging before production

### What "LOCKED BASELINE" Means

- **No Manual Changes**: All changes must be made via Bicep and source control
- **No Portal Clicks**: Manual portal changes will be overwritten on next deployment
- **Reproducibility**: Infrastructure must be reproducible from source control
- **Audit Trail**: All changes tracked in source control history

### If You Need to Change Infrastructure

1. Create a new branch
2. Modify Bicep files or parameters
3. Document justification in commit message
4. Submit pull request for review
5. Deploy to dev for validation
6. Deploy to staging for validation
7. Deploy to production after approval

---

## Troubleshooting

### Deployment Fails with "Name Already Exists"

Resource names must be globally unique (Storage Account, Key Vault). Modify the naming convention in main.bicep or use different environment names.

### SQL Server AAD Authentication Not Working

Update the Azure AD group SID in `modules/sql-server.bicep` with your actual Azure AD group SID:

```bash
az ad group show --group "SQL Administrators" --query id -o tsv
```

### Container App Cannot Access SQL

Ensure Managed Identity has been granted SQL access (see Post-Deployment Steps).

### Blob Storage Access Denied

Ensure Managed Identity has been granted "Storage Blob Data Contributor" role (see Post-Deployment Steps).

---

## Related Documents

- **Azure Topology**: `/docs/azure-topology.md`
- **Data Model**: `/docs/data-model.md`
- **Migration Strategy**: `/docs/db-migration-strategy.md`

---

**Document Owner**: Infrastructure & Platform Engineering  
**Last Updated**: February 2026  
**Next Review**: Q2 2026
