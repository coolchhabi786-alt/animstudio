param wafPolicyMode string
param ddosEnabled bool
param routes array

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
  properties: {
    policySettings: {
      mode: wafPolicyMode
      defaultRedirectUri: ''
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'Microsoft_DefaultRuleSet'
          ruleSetVersion: '2.1'
        },
        {
          ruleSetType: 'OWASP'
          ruleSetVersion: '3.2'
        }
      ]
    }
  }
}

resource routesGroup 'Microsoft.Cdn/profiles/originGroups@2021-06-01' = [for route in routes: {
  name: route.originGroup
  parent: frontDoor
  properties: {
    healthProbeSettings: {
      protocol: 'Https'
      path: '/health'
      intervalInSeconds: 60
    }
    backends: [
      {
        address: route.path == '/api/*' || route.path == '/hubs/*' ? 'AnimStudio.API.internal.azurecontainerapp.io' : route.originGroup == 'web-origin' ? 'AnimStudioWebApp.azurestaticapps.net' : 'AnimStudioMedia.blob.core.windows.net'
      }
    ]
  }
}]

output frontDoorId string = frontDoor.id