param unitCount int

resource signalR 'Microsoft.SignalRService/signalR@2022-08-01' = {
  name: 'AnimStudioSignalR'
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'Standard'
      tier: 'Standard'
      capacity: unitCount
    }
    externalSettings: {
      defaultDomainEnabled: true
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
  }
}

output signalRConnectionString string = 'Endpoint=https://${signalR.properties.hostName};AccessKey=signalRAccessKeyValue;'