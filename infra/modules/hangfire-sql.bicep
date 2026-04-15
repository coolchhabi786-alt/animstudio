resource hangfireSqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: 'AnimStudioHangfireSqlServer'
  location: resourceGroup().location
  properties: {
    administratorLogin: 'hangfireAdmin'
    administratorLoginPassword: 'secureHangfirePa$$word123'
  }
}

resource hangfireDatabase 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  name: 'HangfireDB'
  parent: hangfireSqlServer
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    sku: {
      name: 'Basic'
      tier: 'GeneralPurpose'
      capacity: 1
    }
  }
}

output hangfireConnectionString string = 'Server=${hangfireSqlServer.name}.database.windows.net;Database=${hangfireDatabase.name};User ID=hangfireAdmin;Password=secureHangfirePa$$word123;'