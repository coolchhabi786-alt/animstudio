param environment string
param tier string

resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: 'animstudio${environment}acr'
  location: resourceGroup().location
  sku: {
    name: tier
  }
  properties: {
    adminUserEnabled: false
  }
}

output acrLoginServer string = acr.properties.loginServer
output acrId string = acr.id
output registryId string = acr.id
