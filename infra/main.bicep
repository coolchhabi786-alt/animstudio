targetScope = 'subscription'

module containerAppEnvModule './modules/containerapp-env.bicep' = {
  name: 'deployContainerAppEnv'
  params: {
    // Parameters for the Container Apps Environment
    logAnalyticsWorkspaceName: 'animstudio-law'
    vnetName: 'animstudio-vnet'
    subnetName: 'containerapps-subnet'
    subnetPrefix: '10.0.1.0/24'
  }
}

module containerAppModule './modules/containerapp.bicep' = {
  name: 'deployContainerApp'
  params: {
    environmentName: containerAppEnvModule.outputs.environmentName
    containerAppName: 'AnimStudio.API'
    cpuCores: 2
    memoryInGiB: 4
    ingressInternal: true
  }
}

module redisModule './modules/redis.bicep' = {
  name: 'deployRedis'
  params: {
    redisTier: 'Standard'
    enableGeoReplication: true
    privateEndpointEnabled: true
  }
}

module frontDoorModule './modules/frontdoor.bicep' = {
  name: 'deployFrontDoor'
  params: {
    wafPolicyMode: 'Prevention'
    ddosEnabled: true
    routes: [
      {
        path: '/api/*'
        originGroup: 'api-origin'
        cacheType: 'None'
      },
      {
        path: '/hubs/*'
        originGroup: 'api-origin'
        connectionType: 'WebSocket'
      },
      {
        path: '/static/*'
        originGroup: 'web-origin'
        cacheDuration: '30d'
      },
      {
        path: '/media/*'
        originGroup: 'media-origin'
        cacheDuration: '30d'
      }
    ]
  }
}

module sqlModule './modules/sql.bicep' = {
  name: 'deploySql'
  params: {
    tier: 'GeneralPurpose'
    vCoreCount: 2
    zoneRedundant: true
  }
}

module hangfireSqlModule './modules/hangfire-sql.bicep' = {
  name: 'deployHangfireSql'
}

module serviceBusModule './modules/servicebus.bicep' = {
  name: 'deployServiceBus'
  params: {
    privateEndpointEnabled: true
  }
}

module signalRModule './modules/signalr.bicep' = {
  name: 'deploySignalR'
  params: {
    unitCount: 1
  }
}

module storageModule './modules/storage.bicep' = {
  name: 'deployStorage'
  params: {
    privateEndpointEnabled: true
  }
}

module keyVaultModule './modules/keyvault.bicep' = {
  name: 'deployKeyVault'
  params: {
    enablePurgeProtection: true
  }
}

module acrModule './modules/acr.bicep' = {
  name: 'deployAcr'
  params: {
    tier: 'Standard'
  }
}