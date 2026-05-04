param wafPolicyMode string
// Hostnames injected from main.bicep outputs after Container App + SWA are provisioned
param swaHostname string
param apiHostname string

resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' = {
  name: 'AnimStudioFrontDoor'
  location: 'Global'
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
}

resource wafPolicy 'Microsoft.Network/frontdoorWebApplicationFirewallPolicies@2022-09-01' = {
  name: 'AnimStudioWAFPolicy'
  location: 'Global'
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
  properties: {
    policySettings: {
      mode: wafPolicyMode
      enabledState: 'Enabled'
    }
    managedRules: {
      managedRuleSets: [
        { ruleSetType: 'Microsoft_DefaultRuleSet', ruleSetVersion: '2.1', ruleSetAction: 'Block' }
        { ruleSetType: 'Microsoft_BotManagerRuleSet', ruleSetVersion: '1.0' }
      ]
    }
  }
}

// API origin group (handles /api/* and /hubs/*)
resource apiOriginGroup 'Microsoft.Cdn/profiles/originGroups@2021-06-01' = {
  name: 'api-origin'
  parent: frontDoor
  properties: {
    loadBalancingSettings: { sampleSize: 4, successfulSamplesRequired: 3 }
    healthProbeSettings: {
      probePath: '/api/v1/health'
      probeRequestType: 'GET'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 60
    }
  }
}

resource apiOrigin 'Microsoft.Cdn/profiles/originGroups/origins@2021-06-01' = {
  name: 'api-origin-1'
  parent: apiOriginGroup
  properties: {
    hostName: apiHostname
    httpPort: 80
    httpsPort: 443
    originHostHeader: apiHostname
    priority: 1
    weight: 1000
  }
}

// Web (SWA) origin group (handles /* — all frontend routes)
resource webOriginGroup 'Microsoft.Cdn/profiles/originGroups@2021-06-01' = {
  name: 'web-origin'
  parent: frontDoor
  properties: {
    loadBalancingSettings: { sampleSize: 4, successfulSamplesRequired: 3 }
    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 100
    }
  }
}

resource webOrigin 'Microsoft.Cdn/profiles/originGroups/origins@2021-06-01' = {
  name: 'swa-origin-1'
  parent: webOriginGroup
  properties: {
    hostName: swaHostname
    httpPort: 80
    httpsPort: 443
    originHostHeader: swaHostname
    priority: 1
    weight: 1000
  }
}

output frontDoorId string = frontDoor.id
output frontDoorEndpointHostname string = 'AnimStudioFrontDoor.azurefd.net'
