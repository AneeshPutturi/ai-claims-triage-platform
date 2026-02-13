// =============================================
// Azure SQL Database Module
// Description: Database with TDE, threat detection, and appropriate SKU
// =============================================

@description('Database name')
param databaseName string

@description('SQL Server name')
param serverName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('SKU name (S0, S1, S2, etc.)')
param skuName string = 'S0'

@description('Max size in bytes')
param maxSizeBytes int = 2147483648 // 2GB default

// =============================================
// SQL DATABASE
// =============================================

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  name: '${serverName}/${databaseName}'
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: maxSizeBytes
    
    // Backup and retention
    requestedBackupStorageRedundancy: 'Local' // Can be Geo for prod
    
    // High availability
    zoneRedundant: false // Can be enabled for prod
    
    // Read scale-out (not available in Standard tier)
    readScale: 'Disabled'
  }
}

// =============================================
// TRANSPARENT DATA ENCRYPTION (TDE)
// =============================================

resource transparentDataEncryption 'Microsoft.Sql/servers/databases/transparentDataEncryption@2023-05-01-preview' = {
  name: '${serverName}/${databaseName}/current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [
    sqlDatabase
  ]
}

// =============================================
// THREAT DETECTION
// =============================================

resource threatDetection 'Microsoft.Sql/servers/databases/securityAlertPolicies@2023-05-01-preview' = {
  name: '${serverName}/${databaseName}/Default'
  properties: {
    state: 'Enabled'
    emailAccountAdmins: true
    retentionDays: 90
  }
  dependsOn: [
    sqlDatabase
  ]
}

// =============================================
// OUTPUTS
// =============================================

output databaseId string = sqlDatabase.id
output databaseName string = databaseName
