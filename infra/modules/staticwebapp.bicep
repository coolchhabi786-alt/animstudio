param environment string
param location string = resourceGroup().location

@allowed(['Free', 'Standard'])
param skuName string = 'Standard'

// URL of the API Container App — injected as NEXT_PUBLIC_API_BASE_URL at build time.
// Example: 'https://animstudio-prod-api.<random>.<region>.azurecontainerapps.io'
param apiBaseUrl string

resource swa 'Microsoft.Web/staticSites@2022-09-01' = {
  name: 'animstudio-${environment}-web'
  location: location
  sku: {
    name: skuName
    tier: skuName
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

// Runtime environment variables surfaced to Next.js at build + request time.
resource swaSettings 'Microsoft.Web/staticSites/config@2022-09-01' = {
  parent: swa
  name: 'appsettings'
  properties: {
    NEXT_PUBLIC_API_BASE_URL: apiBaseUrl
  }
}

output swaDefaultHostname string = swa.properties.defaultHostname
output swaName string = swa.name
output swaId string = swa.id
