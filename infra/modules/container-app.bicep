// =============================================
// Container App Module
// Description: Backend API with Managed Identity and autoscaling
// =============================================

@description('Container App name')
param containerAppName string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('Container Apps Environment ID')
param containerAppsEnvironmentId string

@description('Container image')
param containerImage string

@description('Target port')
param targetPort int = 80

@description('Minimum replicas')
param minReplicas int = 1

@description('Maximum replicas')
param maxReplicas int = 3

@description('CPU cores')
param cpu string = '0.5'

@description('Memory in GB')
param memory string = '1.0Gi'

// =============================================
// CONTAINER APP
// =============================================

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned' // Managed Identity for accessing Azure resources
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: targetPort
        transport: 'auto'
        allowInsecure: false // HTTPS only
      }
      registries: [] // No private registries yet
      secrets: [] // No secrets in config - use Key Vault
    }
    template: {
      containers: [
        {
          name: containerAppName
          image: containerImage
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}

// =============================================
// OUTPUTS
// =============================================

output containerAppId string = containerApp.id
output containerAppName string = containerApp.name
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output managedIdentityPrincipalId string = containerApp.identity.principalId
