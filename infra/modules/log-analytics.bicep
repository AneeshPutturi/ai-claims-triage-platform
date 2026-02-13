// =============================================
// Log Analytics Workspace Module
// Description: Centralized logging for all Azure services
// =============================================

@description('Workspace name')
param workspaceName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('Log retention in days (90 for non-prod, 2555 for prod = 7 years)')
param retentionInDays int = 90

// =============================================
// LOG ANALYTICS WORKSPACE
// =============================================

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: retentionInDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// =============================================
// OUTPUTS
// =============================================

output workspaceId string = logAnalyticsWorkspace.id
output workspaceName string = logAnalyticsWorkspace.name
output customerId string = logAnalyticsWorkspace.properties.customerId
