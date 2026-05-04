param environment string
param privateEndpointEnabled bool

// Suffix '-sb' is reserved by Azure — using 'sbus' instead.
var namespaceName = 'animstudio-${environment}sbus'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: resourceGroup().location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {}
}

resource jobsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'jobs-queue'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    requiresDuplicateDetection: false
  }
}

resource completionsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'completions-queue'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

resource deadletterRetryQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'deadletter-retry-queue'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 10
  }
}

// Standard tier max lock duration is 5 min. Worker renews the lock every ~4 min for long LoRA jobs.
resource characterTrainingQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'character-training'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT1H'
    defaultMessageTimeToLive: 'P1D'
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = if (privateEndpointEnabled) {
  name: '${namespaceName}-pe'
  location: resourceGroup().location
  properties: {
    subnet: {
      id: resourceId('Microsoft.Network/virtualNetworks/subnets', 'animstudio-${environment}-vnet', 'servicebus-subnet')
    }
    privateLinkServiceConnections: [
      {
        name: 'ServiceBusConnection'
        properties: {
          privateLinkServiceId: serviceBusNamespace.id
        }
      }
    ]
  }
}

var rootKey = listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', '2022-10-01-preview')

output serviceBusNamespaceName string = serviceBusNamespace.name
output namespaceId string = serviceBusNamespace.id
// Full connection string required by KEDA azure-servicebus scaler (connection auth mode)
output serviceBusConnectionString string = rootKey.primaryConnectionString
