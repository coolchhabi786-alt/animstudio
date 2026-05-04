param environment string
param blobEndpoint string  // e.g. https://animstudiodevstor.blob.core.windows.net/

var profileName   = 'animstudio-${environment}-cdn'
var endpointName  = 'animstudio-${environment}-assets'
var originHostName = replace(replace(blobEndpoint, 'https://', ''), '/', '')

resource fdProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: profileName
  location: 'Global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
}

resource fdEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2023-05-01' = {
  name: endpointName
  parent: fdProfile
  location: 'Global'
  properties: {
    enabledState: 'Enabled'
  }
}

resource originGroup 'Microsoft.Cdn/profiles/originGroups@2023-05-01' = {
  name: 'blob-origins'
  parent: fdProfile
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 100
    }
  }
}

resource origin 'Microsoft.Cdn/profiles/originGroups/origins@2023-05-01' = {
  name: 'blob-origin'
  parent: originGroup
  properties: {
    hostName: originHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: originHostName
    priority: 1
    weight: 1000
    enforceCertificateNameCheck: true
  }
}

// Rule set: 7-day cache for images/videos + CORS header
resource ruleSet 'Microsoft.Cdn/profiles/ruleSets@2023-05-01' = {
  name: 'AssetCacheRules'
  parent: fdProfile
}

resource cacheMediaRule 'Microsoft.Cdn/profiles/ruleSets/rules@2023-05-01' = {
  name: 'CacheMediaAndCors'
  parent: ruleSet
  properties: {
    order: 1
    conditions: [
      {
        name: 'UrlFileExtension'
        parameters: {
          typeName: 'DeliveryRuleUrlFileExtensionMatchConditionParameters'
          operator: 'Equal'
          negateCondition: false
          matchValues: [
            'jpg'
            'jpeg'
            'png'
            'gif'
            'webp'
            'svg'
            'mp4'
            'webm'
            'mov'
          ]
          transforms: [
            'Lowercase'
          ]
        }
      }
    ]
    actions: [
      {
        name: 'RouteConfigurationOverride'
        parameters: {
          typeName: 'DeliveryRuleRouteConfigurationOverrideActionParameters'
          cacheConfiguration: {
            queryStringCachingBehavior: 'UseQueryString'
            isCompressionEnabled: 'Enabled'
            // 7 days — safe because asset filenames carry content hashes
            cacheBehavior: 'OverrideAlways'
            cacheDuration: '7.00:00:00'
          }
        }
      }
      {
        name: 'ModifyResponseHeader'
        parameters: {
          typeName: 'DeliveryRuleHeaderActionParameters'
          headerAction: 'Overwrite'
          headerName: 'Access-Control-Allow-Origin'
          value: '*'
        }
      }
    ]
  }
}

resource route 'Microsoft.Cdn/profiles/afdEndpoints/routes@2023-05-01' = {
  name: 'blob-assets-route'
  parent: fdEndpoint
  dependsOn: [origin]
  properties: {
    originGroup: {
      id: originGroup.id
    }
    ruleSets: [
      {
        id: ruleSet.id
      }
    ]
    supportedProtocols: [
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    cacheConfiguration: {
      queryStringCachingBehavior: 'UseQueryString'
      compressionSettings: {
        isCompressionEnabled: true
        contentTypesToCompress: [
          'application/json'
          'text/plain'
        ]
      }
    }
  }
}

output cdnEndpointUrl string = 'https://${fdEndpoint.properties.hostName}'
