param environment string
param location string = resourceGroup().location
param containerAppEnvironmentName string
param serviceBusNamespaceName string
param serviceBusConnectionString string

// Full image URL — pass MCR placeholder for initial infra deploy, real ACR URL from CI/CD.
param image string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

// Set to the ACR login server when deploying a real image.
param acrLoginServer string = ''

param maxReplicas int = 5

// KEDA scale-to-zero: 0 replicas when both queues are empty — you pay nothing.
resource workerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'animstudio-${environment}-worker'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    managedEnvironmentId: resourceId('Microsoft.App/managedEnvironments', containerAppEnvironmentName)
    configuration: {
      secrets: [
        { name: 'sb-connection-string', value: serviceBusConnectionString }
      ]
      registries: empty(acrLoginServer) ? [] : [
        { server: acrLoginServer, identity: 'system' }
      ]
    }
    template: {
      containers: [
        {
          image: image
          name: 'cartoon-automation'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'AZURE_SERVICE_BUS_CONNECTION_STRING', secretRef: 'sb-connection-string' }
            { name: 'AZURE_SERVICE_BUS_JOBS_QUEUE',        value: 'jobs-queue' }
            { name: 'AZURE_SERVICE_BUS_COMPLETIONS_QUEUE', value: 'completions-queue' }
            { name: 'AZURE_SERVICE_BUS_TRAINING_QUEUE',    value: 'character-training' }
            { name: 'ENVIRONMENT',                         value: environment }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'jobs-queue-keda'
            custom: {
              type: 'azure-servicebus'
              metadata: {
                queueName: 'jobs-queue'
                messageCount: '1'
                namespace: serviceBusNamespaceName
              }
              auth: [
                { secretRef: 'sb-connection-string', triggerParameter: 'connection' }
              ]
            }
          }
          {
            name: 'training-queue-keda'
            custom: {
              type: 'azure-servicebus'
              metadata: {
                queueName: 'character-training'
                messageCount: '1'
                namespace: serviceBusNamespaceName
              }
              auth: [
                { secretRef: 'sb-connection-string', triggerParameter: 'connection' }
              ]
            }
          }
        ]
      }
    }
  }
}

output workerPrincipalId string = workerApp.identity.principalId
output workerName string = workerApp.name
