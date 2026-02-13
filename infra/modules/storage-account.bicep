// =============================================
// Storage Account Module
// Description: Blob storage for claim documents with lifecycle management
// =============================================

@description('Storage account name (must be globally unique, lowercase, no hyphens)')
param storageAccountName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

// =============================================
// STORAGE ACCOUNT
// =============================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS' // Locally redundant storage (can upgrade to GRS for prod)
  }
  kind: 'StorageV2'
  properties: {
    // Security
    supportsHttpsTrafficOnly: true // Enforce HTTPS
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false // No anonymous access
    
    // Access tier
    accessTier: 'Hot' // Default tier for new blobs
    
    // Blob features
    allowSharedKeyAccess: true // Required for some scenarios, prefer Managed Identity
    
    // Network access
    publicNetworkAccess: 'Enabled' // Can be restricted later
    networkAcls: {
      defaultAction: 'Allow' // Will be changed to Deny with VNet integration
      bypass: 'AzureServices'
    }
  }
}

// =============================================
// BLOB SERVICE
// =============================================

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    // Soft delete for blobs - can recover deleted blobs within 7 days
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    // Versioning - maintains previous versions of blobs
    isVersioningEnabled: true
    
    // Change feed - tracks all changes for audit
    changeFeed: {
      enabled: true
      retentionInDays: 90
    }
  }
}

// =============================================
// CLAIM DOCUMENTS CONTAINER
// =============================================

resource claimDocumentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'claim-documents'
  properties: {
    publicAccess: 'None' // Private access only
    metadata: {
      purpose: 'Legal artifact storage for claim documents'
      retention: 'Subject to lifecycle policy'
    }
  }
}

// =============================================
// LIFECYCLE MANAGEMENT POLICY
// =============================================

resource lifecyclePolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    policy: {
      rules: [
        {
          enabled: true
          name: 'MoveToC oolAfter30Days'
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'claim-documents/'
              ]
            }
            actions: {
              baseBlob: {
                // Move to Cool tier after 30 days
                tierToCool: {
                  daysAfterModificationGreaterThan: 30
                }
                // Move to Archive tier after 365 days
                tierToArchive: {
                  daysAfterModificationGreaterThan: 365
                }
                // Delete after 7 years (2555 days) - regulatory retention
                delete: {
                  daysAfterModificationGreaterThan: 2555
                }
              }
            }
          }
        }
      ]
    }
  }
}

// =============================================
// OUTPUTS
// =============================================

output storageAccountId string = storageAccount.id
output storageAccountName string = storageAccount.name
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output claimDocumentsContainerName string = claimDocumentsContainer.name
