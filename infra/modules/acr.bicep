param location string
param environment string
param acrName string
param containerAppIdentityObjectId string = ''

resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: (environment == 'dev') ? 'Basic' : 'Standard'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

// Grant AcrPull to Container App managed identity (only when provided)
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(containerAppIdentityObjectId)) {
  name: guid(acr.id, containerAppIdentityObjectId, 'AcrPull')
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-468b-a7be-63835916a4c8')
    principalId: containerAppIdentityObjectId
    principalType: 'ServicePrincipal'
  }
}

output acrId string = acr.id
output acrLoginServer string = acr.properties.loginServer
