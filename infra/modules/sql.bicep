param location string
param environment string
param serverName string
@secure()
param sqlAdminPassword string

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2022-11-01-preview' = {
  name: 'AllowAzureServices'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-11-01-preview' = {
  name: 'AnimStudio'
  parent: sqlServer
  location: location
  sku: {
    name: (environment == 'dev') ? 'GP_S_Gen5_1' : 'GP_Gen5_2'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: (environment == 'dev') ? 1 : 2
  }
  properties: {
    zoneRedundant: (environment == 'prod')
    autoPauseDelay: (environment == 'dev') ? 60 : -1
    requestedBackupStorageRedundancy: (environment == 'dev') ? 'Local' : 'Zone'
  }
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlServerId string = sqlServer.id
