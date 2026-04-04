param location string
param environment string
param keyVaultName string
param tenantId string
@secure()
param sqlConnectionString string
@secure()
param hangfireConnectionString string
@secure()
param redisConnectionString string
@secure()
param serviceBusConnectionString string
@secure()
param blobConnectionString string

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: (environment == 'prod') ? 90 : 7
    enablePurgeProtection: (environment == 'prod')
  }
}

resource secretSql 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  name: 'ConnectionStrings--DefaultConnection'
  parent: keyVault
  properties: { value: sqlConnectionString }
}

resource secretHangfire 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  name: 'ConnectionStrings--HangfireConnection'
  parent: keyVault
  properties: { value: hangfireConnectionString }
}

resource secretRedis 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  name: 'ConnectionStrings--Redis'
  parent: keyVault
  properties: { value: redisConnectionString }
}

resource secretServiceBus 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  name: 'ConnectionStrings--ServiceBus'
  parent: keyVault
  properties: { value: serviceBusConnectionString }
}

resource secretBlob 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  name: 'ConnectionStrings--BlobStorage'
  parent: keyVault
  properties: { value: blobConnectionString }
}

output keyVaultId string = keyVault.id
output keyVaultUri string = keyVault.properties.vaultUri
