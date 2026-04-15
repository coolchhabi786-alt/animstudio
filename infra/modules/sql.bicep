param tier string
param vCoreCount int
param zoneRedundant bool

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: 'AnimStudioSqlServer'
  location: resourceGroup().location
  properties: {
    administratorLogin: 'sqlAdmin'
    administratorLoginPassword: 'securePa$$word123'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  name: 'AnimStudioDB'
  parent: sqlServer
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 268435456000
    zoneRedundant: zoneRedundant
    sku: {
      name: tier
      tier: 'GeneralPurpose'
      capacity: vCoreCount
    }
  }
}

resource defenderSetting 'Microsoft.Sql/servers/databases/securityAlertPolicies@2022-05-01-preview' = {
  name: sqlDb.name
  parent: sqlDb
  properties: {
    state: 'Enabled'
    emailAccountAdmins: true
  }
}

output sqlConnectionString string = 'Server=${sqlServer.name}.database.windows.net;Database=${sqlDb.name};User ID=sqlAdmin;Password=securePa$$word123;'