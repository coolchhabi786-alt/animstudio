targetScope = 'subscription'

// ---------------------------------------------------------------------------
// Parameters
// ---------------------------------------------------------------------------
@description('Short environment name: dev | prod')
@allowed(['dev', 'prod'])
param environmentName string = 'dev'

@description('Azure region for all resources')
param location string = 'eastus'

@description('SQL Server admin password')
@secure()
param sqlAdminPassword string

@description('Hangfire SQL Server admin password')
@secure()
param hangfireSqlAdminPassword string

@description('AAD Tenant ID used by the app for auth')
param tenantId string = tenant().tenantId

@description('Object ID of the Container App managed identity that pulls from ACR')
param containerAppIdentityObjectId string = ''

// ---------------------------------------------------------------------------
// Derived names (unique per env)
// ---------------------------------------------------------------------------
var prefix = 'animstudio-${environmentName}'
var sqlServerName      = '${prefix}-sql'
var hangfireSqlName    = '${prefix}-hfsql'
var redisName          = '${prefix}-redis'
var keyVaultName       = 'animstudio-kv-${environmentName}'   // max 24 chars
var acrName            = 'animstudio${environmentName}acr'    // alphanumeric only
var storageAccountName = 'animstudio${environmentName}sa'     // alphanumeric, max 24 chars
var signalRName        = '${prefix}-signalr'
var serviceBusName     = '${prefix}-sb'
var caEnvName          = '${prefix}-caenv'
var caAppName          = '${prefix}-api'
var frontDoorName      = '${prefix}-afd'
var resourceGroupName  = '${prefix}-rg'

// ---------------------------------------------------------------------------
// Resource group
// ---------------------------------------------------------------------------
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
}

// ---------------------------------------------------------------------------
// ACR
// ---------------------------------------------------------------------------
module acr 'modules/acr.bicep' = {
  name: 'acrDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    acrName: acrName
    containerAppIdentityObjectId: containerAppIdentityObjectId
  }
}

// ---------------------------------------------------------------------------
// SQL Server (app DB)
// ---------------------------------------------------------------------------
module sql 'modules/sql.bicep' = {
  name: 'sqlServerDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    serverName: sqlServerName
    sqlAdminPassword: sqlAdminPassword
  }
}

// ---------------------------------------------------------------------------
// SQL Server (Hangfire DB)
// ---------------------------------------------------------------------------
module hangfireSql 'modules/hangfire-sql.bicep' = {
  name: 'hangfireSqlDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    serverName: hangfireSqlName
    sqlAdminPassword: hangfireSqlAdminPassword
  }
}

// ---------------------------------------------------------------------------
// Redis Cache
// ---------------------------------------------------------------------------
module redis 'modules/redis.bicep' = {
  name: 'redisDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    redisName: redisName
  }
}

// ---------------------------------------------------------------------------
// Service Bus
// ---------------------------------------------------------------------------
module serviceBus 'modules/servicebus.bicep' = {
  name: 'serviceBusDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    namespaceName: serviceBusName
  }
}

// ---------------------------------------------------------------------------
// SignalR
// ---------------------------------------------------------------------------
module signalR 'modules/signalr.bicep' = {
  name: 'signalRDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    signalRName: signalRName
  }
}

// ---------------------------------------------------------------------------
// Blob Storage
// ---------------------------------------------------------------------------
module blobStorage 'modules/storage.bicep' = {
  name: 'blobStorageDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    storageAccountName: storageAccountName
  }
}

// ---------------------------------------------------------------------------
// Container App Environment + App
// ---------------------------------------------------------------------------
module containerAppEnv 'modules/containerapp-env.bicep' = {
  name: 'containerAppEnvDeployment'
  scope: rg
  params: {
    location: location
    environmentName: caEnvName
    vnetAddressPrefix: '10.0.0.0/16'
    subnetPrefix: '10.0.0.0/23'
  }
}

module containerApp 'modules/containerapp.bicep' = {
  name: 'containerAppDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    appName: caAppName
    environmentId: containerAppEnv.outputs.environmentId
    acrLoginServer: acr.outputs.acrLoginServer
    keyVaultUri: keyVault.outputs.keyVaultUri
  }
}

// ---------------------------------------------------------------------------
// Front Door
// ---------------------------------------------------------------------------
module frontDoor 'modules/frontdoor.bicep' = {
  name: 'frontDoorDeployment'
  scope: rg
  params: {
    location: 'global'
    environment: environmentName
    profileName: frontDoorName
    containerAppHostname: containerApp.outputs.appHostname
    wafMode: (environmentName == 'prod') ? 'Prevention' : 'Detection'
  }
}

// ---------------------------------------------------------------------------
// Key Vault  (deployed last — secrets reference SQL/Redis/ServiceBus outputs)
// ---------------------------------------------------------------------------
module keyVault 'modules/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  scope: rg
  params: {
    location: location
    environment: environmentName
    keyVaultName: keyVaultName
    tenantId: tenantId
    sqlConnectionString: 'Server=tcp:${sql.outputs.sqlServerFqdn},1433;Initial Catalog=AnimStudio;User ID=sqladmin;Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;'
    hangfireConnectionString: 'Server=tcp:${hangfireSql.outputs.sqlServerFqdn},1433;Initial Catalog=AnimStudio_Hangfire;User ID=sqladmin;Password=${hangfireSqlAdminPassword};Encrypt=True;TrustServerCertificate=False;'
    redisConnectionString: '${redis.outputs.redisHostname}:6380,password=${redis.outputs.redisPrimaryKey},ssl=True,abortConnect=False'
    serviceBusConnectionString: serviceBus.outputs.primaryConnectionString
    blobConnectionString: blobStorage.outputs.blobConnectionString
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------
output resourceGroupName string = rg.name
output keyVaultUri string = keyVault.outputs.keyVaultUri
output acrLoginServer string = acr.outputs.acrLoginServer
output containerAppHostname string = containerApp.outputs.appHostname
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output redisHostname string = redis.outputs.redisHostname