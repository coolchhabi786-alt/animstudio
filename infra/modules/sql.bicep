param environment string = 'dev'
// East US and East US 2 block SQL on some subscriptions; West US has broader availability.
param location string = 'westus'
param tier string
param vCoreCount int = 1
param zoneRedundant bool = false

var serverName = 'animstudio-${environment}-sql'

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: 'sqlAdmin'
    administratorLoginPassword: 'securePa$$word123'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: 'AnimStudioDB'
  parent: sqlServer
  location: location
  // Basic (dev): DTU model. GeneralPurpose (prod): vCore model.
  sku: tier == 'Basic' ? {
    name: 'Basic'
    tier: 'Basic'
  } : {
    name: 'GP_Gen5_${vCoreCount}'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: vCoreCount
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: tier == 'Basic' ? 2147483648 : 268435456000
    zoneRedundant: zoneRedundant
  }
}

resource defenderSetting 'Microsoft.Sql/servers/databases/securityAlertPolicies@2022-05-01-preview' = {
  name: 'Default'
  parent: sqlDb
  properties: {
    state: 'Enabled'
    emailAccountAdmins: true
  }
}

output sqlServerName string = sqlServer.name
output sqlConnectionString string = 'Server=${sqlServer.name}.database.windows.net;Database=${sqlDb.name};User ID=sqlAdmin;Password=securePa$$word123;'
