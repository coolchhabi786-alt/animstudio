param environment string
param enablePurgeProtection bool
// MSI principal IDs granted Key Vault Secrets User role at deployment time
param apiPrincipalId string
param workerPrincipalId string

// OPT-3: 'RedisConnectionString' intentionally absent.
// The API falls back to IMemoryCache when this secret is not present (Program.cs:131-136).
// Re-add it here when the redis module is re-enabled in main.bicep.
var secretNames = [
  'SqlConnectionString'
  'HangfireSqlConnectionString'
  'StripeSecretKey'
  'StripeWebhookSecret'
  'ServiceBusConnectionString'
  'SignalRConnectionString'
  'AzureOpenAIKey'
  'AzureOpenAIEndpoint'
  'FalApiKey'
  'ElevenLabsApiKey'
  'AzureCommunicationServicesConnectionString'
]

// Built-in Key Vault Secrets User role (read secrets, no write/admin)
var kvSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' = {
  name: 'animstudio-${environment}-kv'
  location: resourceGroup().location
  properties: union({
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }, enablePurgeProtection ? { enablePurgeProtection: true } : {})
}

resource secrets 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = [for secretName in secretNames: {
  name: secretName
  parent: keyVault
  properties: {
    value: 'PLACEHOLDER_${secretName}_replace_via_seed-keyvault_ps1'
  }
}]

// Grant API Container App MSI read-only access to all secrets
resource apiKvRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, apiPrincipalId, kvSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', kvSecretsUserRoleId)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Python Worker MSI read-only access (needs ServiceBus, FAL, ElevenLabs keys)
resource workerKvRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, workerPrincipalId, kvSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', kvSecretsUserRoleId)
    principalId: workerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
