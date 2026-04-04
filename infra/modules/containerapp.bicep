param location string
param environment string
param appName string
param environmentId string
param acrLoginServer string
param keyVaultUri string

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: [
        {
          server: acrLoginServer
          identity: 'system'
        }
      ]
      secrets: []
    }
    template: {
      containers: [
        {
          name: 'animstudio-api'
          image: '${acrLoginServer}/animstudio-api:latest'
          resources: {
            cpu: json((environment == 'dev') ? '0.5' : '2.0')
            memory: (environment == 'dev') ? '1Gi' : '4Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: (environment == 'dev') ? 'Development' : 'Production' }
            { name: 'Azure__KeyVaultUri', value: keyVaultUri }
          ]
        }
      ]
      scale: {
        minReplicas: (environment == 'dev') ? 0 : 1
        maxReplicas: (environment == 'dev') ? 2 : 10
        rules: [
          {
            name: 'http-scaler'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

output appId string = containerApp.id
output appHostname string = containerApp.properties.configuration.ingress.fqdn
output principalId string = containerApp.identity.principalId
