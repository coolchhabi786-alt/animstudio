param environment string
param privateEndpointEnabled bool

// Storage account names: 3-24 chars, lowercase alphanumeric only
var accountName = 'animstudio${environment}stor'
var vnetName = 'animstudio-${environment}-vnet'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: accountName
  location: resourceGroup().location
  sku: {
    name: privateEndpointEnabled ? 'Standard_ZRS' : 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true   // Required for CDN and SWA asset delivery
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource assetsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'assets'
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
}

resource finalsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'finals'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = if (privateEndpointEnabled) {
  name: '${accountName}-pe'
  location: resourceGroup().location
  properties: {
    subnet: {
      id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, 'storage-subnet')
    }
    privateLinkServiceConnections: [
      {
        name: 'StorageConnection'
        properties: {
          privateLinkServiceId: storageAccount.id
          groupIds: ['blob']
        }
      }
    ]
  }
}

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
