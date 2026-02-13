// =============================================
// Azure SQL Server Module
// Description: SQL Server with Azure AD authentication and secure defaults
// =============================================

@description('SQL Server name')
param serverName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('Administrator login (used for initial setup only, AAD is primary)')
@secure()
param administratorLogin string

@description('Administrator password (used for initial setup only)')
@secure()
param administratorPassword string = newGuid() // Generate random password

// =============================================
// SQL SERVER
// =============================================

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    version: '12.0'
    
    // Public network access - disabled for production security
    publicNetworkAccess: 'Disabled' // No public access
    
    // Require Azure AD authentication
    minimalTlsVersion: '1.2'
    
    // Azure AD authentication
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: 'SQL Administrators' // Azure AD group name
      sid: '00000000-0000-0000-0000-000000000000' // Replace with actual Azure AD group SID
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: false // Set to true after AAD is configured
    }
  }
}

// =============================================
// FIREWALL RULES
// =============================================

// Allow Azure services to access server (required for Container Apps)
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// =============================================
// AUDITING
// =============================================

resource auditingSettings 'Microsoft.Sql/servers/auditingSettings@2023-05-01-preview' = {
  parent: sqlServer
  name: 'default'
  properties: {
    state: 'Enabled'
    isAzureMonitorTargetEnabled: true
    retentionDays: 90
  }
}

// =============================================
// ADVANCED THREAT PROTECTION
// =============================================

resource securityAlertPolicies 'Microsoft.Sql/servers/securityAlertPolicies@2023-05-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    state: 'Enabled'
    emailAccountAdmins: true
    retentionDays: 90
  }
}

// =============================================
// OUTPUTS
// =============================================

output serverId string = sqlServer.id
output serverName string = sqlServer.name
output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
