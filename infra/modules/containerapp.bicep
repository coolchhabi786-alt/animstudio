param environment string
param containerAppEnvironmentName string

// Full image URL — pass MCR placeholder for initial infra deploy, real ACR URL from CI/CD.
param image string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

// Set to the ACR login server when deploying a real image so the registry auth is wired up.
// Leave empty for the initial placeholder deploy (no ACR access required).
param acrLoginServer string = ''

param cpuCores int = 1
param memoryGiB int = 2
param minReplicas int = 1
param maxReplicas int = 10

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'animstudio-${environment}-api'
  location: resourceGroup().location
  identity: { type: 'SystemAssigned' }
  properties: {
    managedEnvironmentId: resourceId('Microsoft.App/managedEnvironments', containerAppEnvironmentName)
    configuration: {
      // Registry config is only needed when pulling from ACR (not for placeholder/public images)
      registries: empty(acrLoginServer) ? [] : [
        { server: acrLoginServer, identity: 'system' }
      ]
      ingress: {
        external: true
        targetPort: 8080
        transport: 'Auto'
        corsPolicy: {
          allowedOrigins: ['*']
          allowedMethods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS']
          allowedHeaders: ['*']
          allowCredentials: false
        }
      }
    }
    template: {
      containers: [
        {
          name: 'animstudio-api'
          image: image
          resources: {
            cpu: json(string(cpuCores))
            memory: '${memoryGiB}Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

output containerAppName string = containerApp.name
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output principalId string = containerApp.identity.principalId
