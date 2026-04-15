param privateEndpointEnabled bool

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'animstudiostorage'
  location: resourceGroup().location
  sku: {
    name: privateEndpointEnabled ? 'Standard_ZRS' : 'Standard_LRS'
    tier: 'Standard'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource assetsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'assets'
  parent: storageAccount
  properties: {
    publicAccess: 'Blob'
  }
}

resource finalsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'finals'
  parent: storageAccount
  properties: {
    publicAccess: 'None'
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = if (privateEndpointEnabled) {
  name: '${storageAccount.name}-pe'
  location: resourceGroup().location
  properties: {
    subnet: {
      id: resourceId('Microsoft.Network/virtualNetworks/subnets', 'animstudio-vnet', 'storage-subnet')
    }
    privateLinkServiceConnections: [
      {
        name: 'StorageConnection'
        properties: {
          privateLinkServiceId: storageAccount.id
        }
      }
    ]
  }
}

output storageAccountName string = storageAccount.name