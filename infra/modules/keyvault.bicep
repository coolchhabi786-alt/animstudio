param enablePurgeProtection bool

var secretNames = [
  'SqlConnectionString',
  'HangfireSqlConnectionString',
  'RedisConnectionString',
  'StripeSecretKey',
  'StripeWebhookSecret',
  'ServiceBusConnectionString',
  'SignalRConnectionString',
  'AzureOpenAIKey',
  'AzureOpenAIEndpoint',
  'FalApiKey',
  'AzureCommunicationServicesConnectionString'
]

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' = {
  name: 'AnimStudioKeyVault'
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'standard'
      tier: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: []
    enableSoftDelete: true
    enablePurgeProtection: enablePurgeProtection
  }
}

resource secrets 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = [for secretName in secretNames: {
  name: secretName
  parent: keyVault
  properties: {
    value: 'PLACEHOLDER_VALUE_${secretName}'
  }
}]

module rbac './rbac.bicep' = {
  name: 'AssignKeyVaultRBAC'
  params: {
    principalId: containerAppMI.outputs.principalId
    roleDefinitionId: 'Key Vault Secrets User'
    scope: keyVault.id
  }
}