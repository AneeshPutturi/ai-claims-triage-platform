// =============================================
// Resource Group Module
// Description: Creates resource group with naming conventions and tags
// =============================================

targetScope = 'subscription'

@description('Resource group name')
param resourceGroupName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

// =============================================
// RESOURCE GROUP
// =============================================

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// =============================================
// OUTPUTS
// =============================================

output resourceGroupName string = resourceGroup.name
output resourceGroupId string = resourceGroup.id
