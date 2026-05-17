// Must match the region of the main SQL server (eastasia for dev via dev.json, westus default for prod).
// Always passed in from main.bicep as sqlLocation — never rely on the default.
param location string = 'westus'
// Name of the main animstudio-{env}-sql server — HangfireDB is added as a second database on it.
param sqlServerName string

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' existing = {
  name: sqlServerName
}

resource hangfireDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: 'HangfireDB'
  parent: sqlServer
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
  }
}

// Uses the same sqlAdmin credentials as the main SQL server (same server = same admin)
output hangfireConnectionString string = 'Server=${sqlServer.name}.database.windows.net;Database=${hangfireDatabase.name};User ID=sqlAdmin;Password=securePa$$word123;'
