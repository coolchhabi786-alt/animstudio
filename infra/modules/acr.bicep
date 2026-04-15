param tier string

resource acr 'Microsoft.ContainerRegistry/registries@2022-09-01' = {
  name: 'AnimStudioACR'
  location: resourceGroup().location
  properties: {
    sku: {
      name: tier
    }
    adminUserEnabled: false
    policies: {
      quarantinePolicy: {
        status: 'enabled'
      }
    }
  }
}

module rbacContainerApp './rbac.bicep' = {
  name: 'AssignACRPullForContainerApp'
  params: {
    principalId: containerAppMI.outputs.principalId
    roleDefinitionId: 'AcrPull'
    scope: acr.id
  }
}

module rbacGitHub './rbac.bicep' = {
  name: 'AssignACRPushForGitHubActions'
  params: {
    principalId: githubOIDC.outputs.principalId
    roleDefinitionId: 'AcrPush'
    scope: acr.id
  }
}

output acrLoginServer string = acr.properties.loginServer