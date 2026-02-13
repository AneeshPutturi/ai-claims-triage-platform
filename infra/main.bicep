// =============================================
// Root Bicep Module - AI-Driven Claims Intake & Triage Platform
// Description: Orchestrates all infrastructure modules
// Author: Infrastructure Team
// Date: February 2026
// =============================================

targetScope = 'subscription'

// =============================================
// PARAMETERS
// =============================================

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string

@description('Azure region for all resources')
param location string = 'eastus'

@description('Project name used for resource naming')
param projectName string = 'claims-intake'

@description('Owner tag for resource tracking')
param owner string

@description('Cost center tag for billing')
param costCenter string

// =============================================
// VARIABLES
// =============================================

var resourceGroupName = 'rg-${projectName}-${environment}'
var tags = {
  Environment: environment
  Project: projectName
  Owner: owner
  CostCenter: costCenter
  ManagedBy: 'Bicep'
  DeployedAt: utcNow('yyyy-MM-dd')
}

// =============================================
// RESOURCE GROUP
// =============================================

module resourceGroup 'modules/resource-group.bicep' = {
  name: 'deploy-resource-group'
  params: {
    resourceGroupName: resourceGroupName
    location: location
    tags: tags
  }
}

// =============================================
// LOG ANALYTICS WORKSPACE
// =============================================

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'deploy-log-analytics'
  scope: resourceGroup(resourceGroupName)
  params: {
    workspaceName: 'log-${projectName}-${environment}'
    location: location
    tags: tags
    retentionInDays: environment == 'prod' ? 2555 : 90 // 7 years for prod, 90 days for non-prod
  }
  dependsOn: [
    resourceGroup
  ]
}

// =============================================
// KEY VAULT
// =============================================

module keyVault 'modules/key-vault.bicep' = {
  name: 'deploy-key-vault'
  scope: resourceGroup(resourceGroupName)
  params: {
    keyVaultName: 'kv-${projectName}-${environment}'
    location: location
    tags: tags
  }
  dependsOn: [
    resourceGroup
  ]
}

// =============================================
// STORAGE ACCOUNT
// =============================================

module storage 'modules/storage-account.bicep' = {
  name: 'deploy-storage'
  scope: resourceGroup(resourceGroupName)
  params: {
    storageAccountName: 'st${replace(projectName, '-', '')}${environment}'
    location: location
    tags: tags
  }
  dependsOn: [
    resourceGroup
  ]
}

// =============================================
// AZURE SQL
// =============================================

module sqlServer 'modules/sql-server.bicep' = {
  name: 'deploy-sql-server'
  scope: resourceGroup(resourceGroupName)
  params: {
    serverName: 'sql-${projectName}-${environment}'
    location: location
    tags: tags
    administratorLogin: 'sqladmin' // Used only for initial setup, AAD auth is primary
  }
  dependsOn: [
    resourceGroup
  ]
}

module sqlDatabase 'modules/sql-database.bicep' = {
  name: 'deploy-sql-database'
  scope: resourceGroup(resourceGroupName)
  params: {
    databaseName: 'sqldb-claims-${environment}'
    serverName: sqlServer.outputs.serverName
    location: location
    tags: tags
    skuName: environment == 'prod' ? 'S2' : 'S0' // Standard tier
    maxSizeBytes: environment == 'prod' ? 268435456000 : 2147483648 // 250GB prod, 2GB non-prod
  }
  dependsOn: [
    sqlServer
  ]
}

// =============================================
// CONTAINER APPS ENVIRONMENT
// =============================================

module containerAppsEnv 'modules/container-apps-environment.bicep' = {
  name: 'deploy-container-apps-env'
  scope: resourceGroup(resourceGroupName)
  params: {
    environmentName: 'cae-${projectName}-${environment}'
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
  }
  dependsOn: [
    resourceGroup
    logAnalytics
  ]
}

// =============================================
// BACKEND API CONTAINER APP
// =============================================

module backendApi 'modules/container-app.bicep' = {
  name: 'deploy-backend-api'
  scope: resourceGroup(resourceGroupName)
  params: {
    containerAppName: 'ca-api-${projectName}-${environment}'
    location: location
    tags: tags
    containerAppsEnvironmentId: containerAppsEnv.outputs.environmentId
    containerImage: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest' // Placeholder
    targetPort: 80
    minReplicas: environment == 'prod' ? 2 : 1
    maxReplicas: environment == 'prod' ? 10 : 3
  }
  dependsOn: [
    containerAppsEnv
  ]
}

// =============================================
// OUTPUTS
// =============================================

output resourceGroupName string = resourceGroupName
output sqlServerName string = sqlServer.outputs.serverName
output sqlDatabaseName string = sqlDatabase.outputs.databaseName
output storageAccountName string = storage.outputs.storageAccountName
output keyVaultName string = keyVault.outputs.keyVaultName
output containerAppUrl string = backendApi.outputs.containerAppUrl
output logAnalyticsWorkspaceId string = logAnalytics.outputs.workspaceId
