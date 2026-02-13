// =============================================
// Key Vault Module
// Description: Secure secret storage with RBAC and protection policies
// =============================================

@description('Key Vault name')
param keyVaultName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

// =============================================
// KEY VAULT
// =============================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    
    // RBAC authorization model (not access policies)
    enableRbacAuthorization: true
    
    // Soft delete enabled - secrets can be recovered within 90 days
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    
    // Purge protection - prevents permanent deletion during retention period
    enablePurgeProtection: true
    
    // Network access
    publicNetworkAccess: 'Enabled' // Can be restricted to private endpoints later
    networkAcls: {
      defaultAction: 'Allow' // Will be changed to Deny with VNet integration
      bypass: 'AzureServices'
    }
  }
}

// =============================================
// OUTPUTS
// =============================================

output keyVaultId string = keyVault.id
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
