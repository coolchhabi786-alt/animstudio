param privateEndpointEnabled bool

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'AnimStudioServiceBus'
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'Standard'
    }
  }
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

// ── Phase 4: Character LoRA Training queue ────────────────────────────────────
resource characterTrainingQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'character-training'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT30M'    // LoRA training can take 15+ min — generous lock
    maxDeliveryCount: 3
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT1H'
    defaultMessageTimeToLive: 'P1D'
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = if (privateEndpointEnabled) {
  name: 'AnimStudioServiceBus-pe'
  location: resourceGroup().location
  properties: {
    subnet: {
      id: resourceId('Microsoft.Network/virtualNetworks/subnets', 'animstudio-vnet', 'servicebus-subnet')
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

output serviceBusConnectionString string = 'Endpoint=sb://${serviceBusNamespace.name}.servicebus.windows.net/'