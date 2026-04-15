param environmentName string
param containerAppName string
param cpuCores int = 2
param memoryInGiB int = 4
param ingressInternal bool

resource containerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: containerAppName
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: resourceId('Microsoft.App/managedEnvironments', environmentName)
    template: {
      containers: [
        {
          name: containerAppName
          image: 'animstudio.azurecr.io/animstudio-api:v1'
          resources: {
            cpu: cpuCores
            memory: memoryInGiB
          }
        }
      ]
      scale: {
        rules: [
          {
            name: 'http-rule'
            custom: {
              type: 'http'
              metadata: {
                concurrency: '50'
              }
            }
          }
        ]
        minReplicas: 1
        maxReplicas: 10
      }
    }
    configuration: {
      ingress: {
        external: false
        targetPort: 8080
        transport: ingressInternal ? 'Internal' : 'Public'
      }
    }
  }
}

output containerAppName string = containerApp.name