targetScope = 'resourceGroup'

// ─────────────────────────────────────────────────────────────────────────────
// Parameters
// ─────────────────────────────────────────────────────────────────────────────

@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

param location string = resourceGroup().location

// Container App image tags — CI/CD overrides these with a real git-sha/semver tag.
// 'placeholder' tells main.bicep to use a public MCR image so the initial infra deploy
// succeeds before any image has been pushed to ACR.
param apiImageTag string = 'placeholder'
param workerImageTag string = 'placeholder'

// Azure Static Web Apps SKU: 'Free' for dev/low-traffic, 'Standard' for prod
// Standard is required for custom auth redirect rules (Entra External ID)
@allowed(['Free', 'Standard'])
param swaSkuName string = 'Standard'

// ACR login server — output from ACR module, referenced by both Container Apps
param acrLoginServer string = 'animstudio${environment}acr.azurecr.io'

// SQL servers: East US and East US 2 block provisioning on this subscription; West US is the fallback.
// Override via parameters file if your subscription has capacity elsewhere.
param sqlLocation string = 'westus'

// Service Bus namespace name — must match the var in servicebus.bicep.
// Azure reserves the '-sb' suffix; 'sbus' is used instead.
var serviceBusNamespaceName = 'animstudio-${environment}sbus'

// Container App Environment name — passed into containerapp-env and referenced by all Container Apps
var containerAppEnvName = 'animstudio-${environment}-env'

// ─────────────────────────────────────────────────────────────────────────────
// Networking & Container App Environment
// ─────────────────────────────────────────────────────────────────────────────

module containerAppEnvModule './modules/containerapp-env.bicep' = {
  name: 'deployContainerAppEnv'
  params: {
    containerAppEnvironmentName: containerAppEnvName
    logAnalyticsWorkspaceName: 'animstudio-${environment}-law'
    vnetName: 'animstudio-${environment}-vnet'
    subnetName: 'containerapps-subnet'
    subnetPrefix: '10.0.0.0/23'  // /23 minimum required for Container App Environment VNet integration
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// AnimStudio API — Container App (always-on, min 1 replica, HTTP KEDA)
// ─────────────────────────────────────────────────────────────────────────────

// For placeholder deploys the registry field is omitted so Azure doesn't validate ACR access.
// CI/CD passes a real imageTag (not 'placeholder') and the acrLoginServer to enable ACR pull.
var apiImage = apiImageTag == 'placeholder'
  ? 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
  : '${acrLoginServer}/animstudio-api:${apiImageTag}'

var workerImage = workerImageTag == 'placeholder'
  ? 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
  : '${acrLoginServer}/cartoon-automation:${workerImageTag}'

module apiContainerApp './modules/containerapp.bicep' = {
  name: 'deployApiContainerApp'
  params: {
    environment: environment
    containerAppEnvironmentName: containerAppEnvModule.outputs.environmentName
    image: apiImage
    acrLoginServer: apiImageTag == 'placeholder' ? '' : acrLoginServer
    cpuCores: 1
    memoryGiB: 2
    minReplicas: 1    // Keep at 1 until Redis is re-added (OPT-3 constraint)
    maxReplicas: 3    // Allow burst scaling; raise to 10 after Redis re-enabled
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// OPT-1: Next.js Frontend — Azure Static Web Apps (replaces Container App)
// Saves $22–31/month vs. a dedicated Container App.
// Deploy token is added to GitHub Secrets after first `az staticwebapp show`.
// ─────────────────────────────────────────────────────────────────────────────

module swaModule './modules/staticwebapp.bicep' = {
  name: 'deployStaticWebApp'
  params: {
    environment: environment
    location: 'eastus2'  // SWA not available in eastus; eastus2 is the closest supported region
    skuName: swaSkuName
    apiBaseUrl: 'https://${apiContainerApp.outputs.containerAppFqdn}'
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// OPT-2: Python Worker — KEDA scale-to-zero on Service Bus queues
// Scales to 0 replicas when jobs-queue + character-training are both empty.
// At startup volume (~10 episodes/month, 40 min each): cost ≈ $0.50–1/month.
// ─────────────────────────────────────────────────────────────────────────────

module serviceBusModule './modules/servicebus.bicep' = {
  name: 'deployServiceBus'
  params: {
    environment: environment
    privateEndpointEnabled: environment == 'prod'
  }
}

module workerContainerApp './modules/containerapp-worker.bicep' = {
  name: 'deployWorkerContainerApp'
  params: {
    environment: environment
    location: location
    containerAppEnvironmentName: containerAppEnvModule.outputs.environmentName
    serviceBusNamespaceName: serviceBusNamespaceName
    serviceBusConnectionString: serviceBusModule.outputs.serviceBusConnectionString
    image: workerImage
    acrLoginServer: workerImageTag == 'placeholder' ? '' : acrLoginServer
    maxReplicas: 5
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// OPT-3: Redis — COMMENTED OUT for initial launch.
// The API falls back to IDistributedMemoryCache automatically when the
// 'Redis' connection string is absent from Key Vault (see Program.cs:131-136).
// Re-enable this module when API maxReplicas is raised above 1.
// ─────────────────────────────────────────────────────────────────────────────

// TODO: uncomment when API scales beyond 1 replica (growth phase)
// module redisModule './modules/redis.bicep' = {
//   name: 'deployRedis'
//   params: {
//     environment: environment
//     redisTier: environment == 'prod' ? 'Standard' : 'Basic'
//     enableGeoReplication: environment == 'prod'
//     privateEndpointEnabled: environment == 'prod'
//   }
// }

// ─────────────────────────────────────────────────────────────────────────────
// Data stores
// ─────────────────────────────────────────────────────────────────────────────

module sqlModule './modules/sql.bicep' = {
  name: 'deploySql'
  params: {
    environment: environment
    location: sqlLocation
    tier: environment == 'prod' ? 'GeneralPurpose' : 'Basic'
    vCoreCount: environment == 'prod' ? 2 : 1
    zoneRedundant: environment == 'prod'
  }
}

module hangfireSqlModule './modules/hangfire-sql.bicep' = {
  name: 'deployHangfireSql'
  params: {
    environment: environment
    location: sqlLocation
  }
}

module storageModule './modules/storage.bicep' = {
  name: 'deployStorage'
  params: {
    environment: environment
    privateEndpointEnabled: environment == 'prod'
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// Messaging & real-time
// ─────────────────────────────────────────────────────────────────────────────

module signalRModule './modules/signalr.bicep' = {
  name: 'deploySignalR'
  params: {
    environment: environment
    unitCount: 1
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// Security & secrets
// ─────────────────────────────────────────────────────────────────────────────

module keyVaultModule './modules/keyvault.bicep' = {
  name: 'deployKeyVault'
  params: {
    environment: environment
    enablePurgeProtection: environment == 'prod'
    apiPrincipalId: apiContainerApp.outputs.principalId
    workerPrincipalId: workerContainerApp.outputs.workerPrincipalId
  }
}

module acrModule './modules/acr.bicep' = {
  name: 'deployAcr'
  params: {
    environment: environment
    tier: environment == 'prod' ? 'Standard' : 'Basic'
  }
}

// Grant AcrPull to both Container App MSIs after ACR and the apps are provisioned.
// This is done here (not inside acr.bicep) to avoid the circular dependency that would
// arise if acr.bicep depended on container app principal IDs while container apps
// depended on the ACR login server.
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource acrForRbac 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: 'animstudio${environment}acr'
}

// BCP120: role assignment name must be calculable at deployment start, so we seed guid() with
// resource names (known) rather than module output values (runtime). principalId itself is
// still a runtime value and is legal in properties — just not in the name field.
resource apiAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceId('Microsoft.ContainerRegistry/registries', 'animstudio${environment}acr'), 'animstudio-${environment}-api', acrPullRoleId)
  scope: acrForRbac
  dependsOn: [acrModule]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: apiContainerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

resource workerAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceId('Microsoft.ContainerRegistry/registries', 'animstudio${environment}acr'), 'animstudio-${environment}-worker', acrPullRoleId)
  scope: acrForRbac
  dependsOn: [acrModule]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: workerContainerApp.outputs.workerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// INFRA-2: Managed Identity — Storage & Service Bus role assignments
// Both MSIs get Blob Data Contributor + Service Bus Data Owner so the apps
// authenticate via MSI rather than embedding connection strings in env vars.
// AcrPull for both MSIs is already assigned above in the ACR RBAC block.
// ─────────────────────────────────────────────────────────────────────────────

var storageBlobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var serviceBusDataOwnerRoleId = '090c5cfd-751d-490a-894a-3ce6f1109419'

resource storageForRbac 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: 'animstudio${environment}stor'
}

resource serviceBusForRbac 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: 'animstudio-${environment}sbus'
}

// API MSI → Storage Blob Data Contributor
resource apiStorageRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceId('Microsoft.Storage/storageAccounts', 'animstudio${environment}stor'), 'animstudio-${environment}-api', storageBlobDataContributorRoleId)
  scope: storageForRbac
  dependsOn: [storageModule]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRoleId)
    principalId: apiContainerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// Worker MSI → Storage Blob Data Contributor
resource workerStorageRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceId('Microsoft.Storage/storageAccounts', 'animstudio${environment}stor'), 'animstudio-${environment}-worker', storageBlobDataContributorRoleId)
  scope: storageForRbac
  dependsOn: [storageModule]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRoleId)
    principalId: workerContainerApp.outputs.workerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// API MSI → Azure Service Bus Data Owner
resource apiServiceBusRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceId('Microsoft.ServiceBus/namespaces', 'animstudio-${environment}sbus'), 'animstudio-${environment}-api', serviceBusDataOwnerRoleId)
  scope: serviceBusForRbac
  dependsOn: [serviceBusModule]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataOwnerRoleId)
    principalId: apiContainerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// Worker MSI → Azure Service Bus Data Owner
resource workerServiceBusRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceId('Microsoft.ServiceBus/namespaces', 'animstudio-${environment}sbus'), 'animstudio-${environment}-worker', serviceBusDataOwnerRoleId)
  scope: serviceBusForRbac
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataOwnerRoleId)
    principalId: workerContainerApp.outputs.workerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// CDN & edge
// ─────────────────────────────────────────────────────────────────────────────

// CDN profile over the public Blob Storage assets container (all environments).
// Caches images + videos for 7 days; busting is by content-hash filename.
module cdnModule './modules/cdn.bicep' = {
  name: 'deployCdn'
  params: {
    environment: environment
    blobEndpoint: storageModule.outputs.blobEndpoint
  }
}

module frontDoorModule './modules/frontdoor.bicep' = if (environment == 'prod') {
  name: 'deployFrontDoor'
  params: {
    wafPolicyMode: 'Prevention'
    swaHostname: swaModule.outputs.swaDefaultHostname
    apiHostname: apiContainerApp.outputs.containerAppFqdn
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// Outputs
// ─────────────────────────────────────────────────────────────────────────────

output apiUrl string = 'https://${apiContainerApp.outputs.containerAppFqdn}'
output swaUrl string = 'https://${swaModule.outputs.swaDefaultHostname}'
output swaName string = swaModule.outputs.swaName
output cdnUrl string = cdnModule.outputs.cdnEndpointUrl
