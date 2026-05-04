param environment string = 'dev'
param unitCount int = 1

resource signalR 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: 'animstudio-${environment}-signalr'
  location: resourceGroup().location
  kind: 'SignalR'
  sku: {
    name: 'Standard_S1'
    capacity: unitCount
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
    cors: {
      allowedOrigins: ['*']
    }
  }
}

output signalRConnectionString string = 'Endpoint=https://${signalR.properties.hostName};AccessKey=signalRAccessKeyValue;'
