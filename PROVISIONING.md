# AnimStudio — Infrastructure Provisioning & Local Dev Guide

## Prerequisites

| Tool | Version | Install |
|---|---|---|
| Azure CLI | ≥ 2.60 | `winget install Microsoft.AzureCLI` |
| Docker Desktop | ≥ 4.30 | https://docs.docker.com/desktop/install/windows-install/ |
| .NET SDK | 8.0 | `winget install Microsoft.DotNet.SDK.8` |
| Node.js | ≥ 20 LTS | `winget install OpenJS.NodeJS.LTS` |
| pnpm | ≥ 9 | `npm install -g pnpm` |

---

## 1. Local Development (Docker)

### 1.1 Start SQL Server + Redis

```powershell
# From workspace root
cd C:\Projects\animstudio
docker compose up -d
```

Services started:

| Service | URL | Credentials |
|---|---|---|
| SQL Server 2022 | `localhost:1433` | `sa` / `AnimStudio!Dev123` |
| Redis 7 | `localhost:6379` | password: `AnimRedis!Dev123` |
| Redis Commander UI | http://localhost:8081 | no auth |

### 1.2 Apply EF Core Migrations

```powershell
cd C:\Projects\animstudio\backend

# Identity module (users, teams, subscriptions)
dotnet ef database update `
  --project src\AnimStudio.IdentityModule `
  --startup-project src\AnimStudio.API `
  --context IdentityDbContext

# Shared kernel (outbox, idempotency)
dotnet ef database update `
  --project src\AnimStudio.SharedKernel `
  --startup-project src\AnimStudio.API `
  --context SharedDbContext
```

### 1.3 Run the Backend API

```powershell
cd C:\Projects\animstudio\backend\src\AnimStudio.API
dotnet run
# API: https://localhost:7001  HTTP: http://localhost:5001
# Swagger: http://localhost:5001/swagger
# Hangfire: http://localhost:5001/hangfire
# Health: http://localhost:5001/health
```

### 1.4 Run the Frontend

```powershell
cd C:\Projects\animstudio\frontend

# Copy and fill in env vars
Copy-Item .env.local.example .env.local
# Edit .env.local with your actual values

pnpm install
pnpm dev
# Frontend: http://localhost:3000
```

---

## 2. Azure Infrastructure Provisioning

### 2.1 One-Time Setup

```bash
# Login
az login

# Set your subscription
az account set --subscription "<your-subscription-id>"

# Register required resource providers (first time only)
az provider register --namespace Microsoft.App --wait
az provider register --namespace Microsoft.ContainerRegistry --wait
az provider register --namespace Microsoft.Sql --wait
az provider register --namespace Microsoft.Cache --wait
az provider register --namespace Microsoft.KeyVault --wait
az provider register --namespace Microsoft.ServiceBus --wait
az provider register --namespace Microsoft.SignalRService --wait
az provider register --namespace Microsoft.Storage --wait
az provider register --namespace Microsoft.Cdn --wait
az provider register --namespace Microsoft.OperationalInsights --wait
```

### 2.2 Set SQL Passwords as Variables

> **Never commit passwords to source control.** Use environment variables or Azure Key Vault.

```bash
# PowerShell
$SQL_PASSWORD = "YourStr0ng!Password"
$HANGFIRE_SQL_PASSWORD = "HangfireStr0ng!Password"
```

### 2.3 Deploy DEV Environment

```bash
az deployment sub create \
  --name "animstudio-dev-$(date +%Y%m%d%H%M)" \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.json \
  --parameters sqlAdminPassword="$SQL_PASSWORD" \
               hangfireSqlAdminPassword="$HANGFIRE_SQL_PASSWORD"
```

**PowerShell equivalent:**

```powershell
az deployment sub create `
  --name "animstudio-dev-$(Get-Date -Format yyyyMMddHHmm)" `
  --location eastus `
  --template-file infra/main.bicep `
  --parameters infra/parameters/dev.json `
               sqlAdminPassword="$SQL_PASSWORD" `
               hangfireSqlAdminPassword="$HANGFIRE_SQL_PASSWORD"
```

### 2.4 Deploy PROD Environment

```powershell
az deployment sub create `
  --name "animstudio-prod-$(Get-Date -Format yyyyMMddHHmm)" `
  --location eastus `
  --template-file infra/main.bicep `
  --parameters infra/parameters/prod.json `
               sqlAdminPassword="$SQL_PASSWORD" `
               hangfireSqlAdminPassword="$HANGFIRE_SQL_PASSWORD"
```

### 2.5 What-If (Dry Run)

```powershell
az deployment sub what-if `
  --location eastus `
  --template-file infra/main.bicep `
  --parameters infra/parameters/dev.json `
               sqlAdminPassword="placeholder" `
               hangfireSqlAdminPassword="placeholder"
```

---

## 3. Post-Provisioning Steps

### 3.1 Get Deployment Outputs

```powershell
$outputs = az deployment sub show `
  --name "animstudio-dev-<timestamp>" `
  --query properties.outputs `
  --output json | ConvertFrom-Json

$KEY_VAULT_URI   = $outputs.keyVaultUri.value
$ACR_SERVER      = $outputs.acrLoginServer.value
$APP_HOSTNAME    = $outputs.containerAppHostname.value
$SQL_FQDN        = $outputs.sqlServerFqdn.value
$REDIS_HOSTNAME  = $outputs.redisHostname.value
$RESOURCE_GROUP  = $outputs.resourceGroupName.value

Write-Host "Key Vault: $KEY_VAULT_URI"
Write-Host "ACR: $ACR_SERVER"
Write-Host "App: https://$APP_HOSTNAME"
```

### 3.2 Grant Your Identity Key Vault Access

```powershell
$MY_OBJECT_ID = az ad signed-in-user show --query id --output tsv

az role assignment create `
  --role "Key Vault Secrets Officer" `
  --assignee $MY_OBJECT_ID `
  --scope (az keyvault show --name animstudio-kv-dev --query id --output tsv)
```

### 3.3 Grant Container App ACR Pull Permission

After the Container App is deployed, get its managed identity and redeploy with it:

```powershell
$PRINCIPAL_ID = az containerapp show `
  --name animstudio-dev-api `
  --resource-group animstudio-dev-rg `
  --query identity.principalId `
  --output tsv

# Redeploy passing the identity so ACR role assignment is created
az deployment sub create `
  --name "animstudio-dev-acr-fix" `
  --location eastus `
  --template-file infra/main.bicep `
  --parameters infra/parameters/dev.json `
               sqlAdminPassword="$SQL_PASSWORD" `
               hangfireSqlAdminPassword="$HANGFIRE_SQL_PASSWORD" `
               containerAppIdentityObjectId="$PRINCIPAL_ID"
```

### 3.4 Apply EF Migrations to Azure SQL

```powershell
# Add your IP to SQL firewall first
$MY_IP = (Invoke-RestMethod -Uri "https://api.ipify.org")
az sql server firewall-rule create `
  --resource-group animstudio-dev-rg `
  --server animstudio-dev-sql `
  --name MyDevMachine `
  --start-ip-address $MY_IP `
  --end-ip-address $MY_IP

# Run migrations (update connection string to Azure SQL)
$env:ConnectionStrings__DefaultConnection = "Server=tcp:$SQL_FQDN,1433;Initial Catalog=AnimStudio;User ID=sqladmin;Password=$SQL_PASSWORD;Encrypt=True;"
dotnet ef database update `
  --project src\AnimStudio.IdentityModule `
  --startup-project src\AnimStudio.API `
  --context IdentityDbContext

$env:ConnectionStrings__HangfireConnection = "Server=tcp:$HANGFIRE_SQL_FQDN,1433;Initial Catalog=AnimStudio_Hangfire;User ID=sqladmin;Password=$HANGFIRE_SQL_PASSWORD;Encrypt=True;"
dotnet ef database update `
  --project src\AnimStudio.SharedKernel `
  --startup-project src\AnimStudio.API `
  --context SharedDbContext
```

### 3.5 Build and Push Docker Image

```powershell
# Login to ACR
az acr login --name $ACR_SERVER.Split('.')[0]

# Build and push
docker build -f backend/src/AnimStudio.API/Dockerfile -t "$ACR_SERVER/animstudio-api:latest" ./backend
docker push "$ACR_SERVER/animstudio-api:latest"

# Update Container App to deploy new image
az containerapp update `
  --name animstudio-dev-api `
  --resource-group animstudio-dev-rg `
  --image "$ACR_SERVER/animstudio-api:latest"
```

---

## 4. App Registration (Entra ID) Setup

The backend and frontend both use Microsoft Entra ID for authentication.

```powershell
# Create the app registration
$APP = az ad app create `
  --display-name "AnimStudio-dev" `
  --sign-in-audience AzureADandPersonalMicrosoftAccount `
  | ConvertFrom-Json

$CLIENT_ID = $APP.appId

# Add redirect URIs
az ad app update `
  --id $CLIENT_ID `
  --web-redirect-uris "http://localhost:3000/api/auth/callback/microsoft-entra-id" `
                      "https://$APP_HOSTNAME/api/auth/callback/microsoft-entra-id"

# Create client secret
$SECRET = az ad app credential reset `
  --id $CLIENT_ID `
  --years 1 `
  | ConvertFrom-Json

Write-Host "CLIENT_ID:     $CLIENT_ID"
Write-Host "CLIENT_SECRET: $($SECRET.password)"
Write-Host "TENANT_ID:     $($SECRET.tenant)"
```

Then store in Key Vault:

```powershell
az keyvault secret set --vault-name animstudio-kv-dev --name Auth--ClientId --value $CLIENT_ID
az keyvault secret set --vault-name animstudio-kv-dev --name Auth--ClientSecret --value $SECRET.password
```

---

## 5. GitHub Actions Secrets

Add the following secrets to your GitHub repository (`Settings > Secrets > Actions`):

| Secret Name | Value |
|---|---|
| `AZURE_CREDENTIALS` | Output of `az ad sp create-for-rbac --sdk-auth` |
| `AZURE_SUBSCRIPTION_ID` | Your subscription ID |
| `SQL_ADMIN_PASSWORD` | Same password used in deployment |
| `HANGFIRE_SQL_ADMIN_PASSWORD` | Same password used in deployment |
| `ACR_LOGIN_SERVER` | e.g. `animstudiodevacr.azurecr.io` |
| `RESOURCE_GROUP` | e.g. `animstudio-dev-rg` |

Create the service principal:

```bash
az ad sp create-for-rbac \
  --name "animstudio-github-actions" \
  --role Contributor \
  --scopes /subscriptions/<subscription-id> \
  --sdk-auth
```

---

## 6. Tear Down (Development Only)

```powershell
# Delete the entire resource group (irreversible!)
az group delete --name animstudio-dev-rg --yes --no-wait

# Stop local Docker services
docker compose down -v
```
