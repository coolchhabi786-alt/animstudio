param location string
param environment string
param serverName string
@secure()
param sqlAdminPassword string

resource hangfireSqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
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
  parent: hangfireSqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource hangfireSqlDatabase 'Microsoft.Sql/servers/databases@2022-11-01-preview' = {
  name: 'AnimStudio_Hangfire'
  parent: hangfireSqlServer
  location: location
  sku: {
    name: (environment == 'dev') ? 'Basic' : 'S1'
    tier: (environment == 'dev') ? 'Basic' : 'Standard'
  }
  properties: {
    requestedBackupStorageRedundancy: (environment == 'dev') ? 'Local' : 'Zone'
  }
}

output sqlServerFqdn string = hangfireSqlServer.properties.fullyQualifiedDomainName
output sqlServerId string = hangfireSqlServer.id
