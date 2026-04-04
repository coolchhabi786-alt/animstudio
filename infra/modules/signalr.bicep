param location string
param environment string
param signalRName string

resource signalR 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: signalRName
  location: location
  sku: {
    name: (environment == 'dev') ? 'Free_F1' : 'Standard_S1'
    capacity: 1
  }
  properties: {
    features: [
      { flag: 'ServiceMode', value: 'Default' }
      { flag: 'EnableConnectivityLogs', value: 'True' }
    ]
    cors: {
      allowedOrigins: ['*']
    }
    tls: {
      clientCertEnabled: false
    }
  }
}

output signalRId string = signalR.id
output signalRHostname string = signalR.properties.hostName
