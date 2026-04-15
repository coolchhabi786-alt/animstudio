param redisTier string
param enableGeoReplication bool = false
param privateEndpointEnabled bool

resource redis 'Microsoft.Cache/Redis@2022-09-01' = {
  name: 'AnimStudioCache'
  location: resourceGroup().location
  properties: {
    sku: {
      name: redisTier
      family: 'C'
      capacity: redisTier == 'Standard' ? 2 : 1
    }
    enableGeoReplication: enableGeoReplication
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = if (privateEndpointEnabled) {
  name: '${redis.name}-pe'
  location: resourceGroup().location
  properties: {
    subnet: {
      id: resourceId('Microsoft.Network/virtualNetworks/subnets', 'animstudio-vnet', 'redis-subnet')
    }
    privateLinkServiceConnections: [
      {
        name: '${redis.name}-pls'
        properties: {
          privateLinkServiceId: redis.id
        }
      }
    ]
  }
}

output redisId string = redis.id