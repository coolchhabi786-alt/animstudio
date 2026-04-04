param location string
param environment string
param namespaceName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
    name: (environment == 'dev') ? 'Standard' : 'Premium'
    tier: (environment == 'dev') ? 'Standard' : 'Premium'
  }
  properties: {
    zoneRedundant: (environment == 'prod')
    minimumTlsVersion: '1.2'
  }

  resource jobsQueue 'queues' = {
    name: 'jobs'
    properties: {
      lockDuration: 'PT5M'
      maxDeliveryCount: 3
      requiresSession: false
    }
  }

  resource completionsQueue 'queues' = {
    name: 'completions'
    properties: {
      lockDuration: 'PT5M'
      maxDeliveryCount: 3
    }
  }

  resource deadLetterQueue 'queues' = {
    name: 'deadletter-retry'
    properties: {
      lockDuration: 'PT5M'
      maxDeliveryCount: 10
    }
  }
}

resource sbRootKey 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' existing = {
  name: 'RootManageSharedAccessKey'
  parent: serviceBusNamespace
}

output serviceBusNamespaceId string = serviceBusNamespace.id
output primaryConnectionString string = sbRootKey.listKeys().primaryConnectionString
