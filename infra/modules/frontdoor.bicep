param location string
param environment string
param profileName string
param containerAppHostname string
param wafMode string = 'Detection'

resource frontDoorProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: profileName
  location: location
  sku: {
    name: (environment == 'dev') ? 'Standard_AzureFrontDoor' : 'Premium_AzureFrontDoor'
  }
}

resource apiEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2023-05-01' = {
  name: 'api'
  parent: frontDoorProfile
  location: location
  properties: {
    enabledState: 'Enabled'
  }
}

resource apiOriginGroup 'Microsoft.Cdn/profiles/originGroups@2023-05-01' = {
  name: 'api-origin-group'
  parent: frontDoorProfile
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    healthProbeSettings: {
      probeIntervalInSeconds: 100
      probePath: '/health/live'
      probeProtocol: 'Https'
      probeRequestType: 'HEAD'
    }
  }
}

resource apiOrigin 'Microsoft.Cdn/profiles/originGroups/origins@2023-05-01' = {
  name: 'container-app-origin'
  parent: apiOriginGroup
  properties: {
    hostName: containerAppHostname
    httpPort: 80
    httpsPort: 443
    originHostHeader: containerAppHostname
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
  }
}

resource apiRoute 'Microsoft.Cdn/profiles/afdEndpoints/routes@2023-05-01' = {
  name: 'api-route'
  parent: apiEndpoint
  dependsOn: [ apiOrigin ]
  properties: {
    originGroup: { id: apiOriginGroup.id }
    supportedProtocols: ['Https']
    patternsToMatch: ['/*']
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
  }
}

// WAF Policy (Premium SKU only)
resource wafPolicy 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2022-05-01' = if (environment == 'prod') {
  name: '${replace(profileName, '-', '')}waf'
  location: 'global'
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: wafMode
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'Microsoft_DefaultRuleSet'
          ruleSetVersion: '2.1'
        }
        {
          ruleSetType: 'Microsoft_BotManagerRuleSet'
          ruleSetVersion: '1.0'
        }
      ]
    }
  }
}

output frontDoorId string = frontDoorProfile.id
output frontDoorEndpointHostname string = apiEndpoint.properties.hostName
