#Requires -Version 7.1
<#
.SYNOPSIS
    Starts the minimal resources needed for LOCAL development and testing.
    Does NOT start Container Apps, Static Web App, or CDN — those are cloud deployments.

.DESCRIPTION
    Tier 1 — default (Frontend + Backend only, no Azure required):
        • Starts SQL Server in Docker  (localhost:1433)
        • Starts Redis in Docker       (localhost:6379)
        • Creates output dir for Python worker file storage
        • Writes frontend .env.local if missing

    Tier 2 — with -FullStack (adds Python AI worker):
        • Everything in Tier 1, plus:
        • Upgrades SignalR to Standard_S1 if currently on Free tier
        • Fetches Service Bus connection string from Key Vault
        • Outputs backend user-secrets command (activates CompletionMessageProcessor)
        • Outputs Python worker environment variables

    The script never starts or touches Container Apps — run start-dev.ps1 for that.

.EXAMPLE
    .\start-local.ps1                     # Tier 1 — Docker only (no Azure login needed)
    .\start-local.ps1 -FullStack          # Tier 2 — Docker + Azure SB + Storage
    .\start-local.ps1 -FullStack -WhatIf  # Preview Tier 2 without changes
    .\start-local.ps1 -ResourceGroup animstudio-dev-rg -Environment dev
#>
param (
    [string] $ResourceGroup = 'animstudio-dev-rg',
    [string] $Environment   = 'dev',

    # Include Azure Service Bus + Storage setup for Python AI worker
    [switch] $FullStack,

    # Preview mode — print what would happen but make no Azure changes
    [switch] $WhatIf
)

$ErrorActionPreference = 'Stop'

# ── Resource names (match infra naming convention) ────────────────────────────

$kvName          = "animstudio-$Environment-kv"
$sbNamespace     = "animstudio-${Environment}sbus"
$storAccount     = "animstudio${Environment}stor"
$signalRName     = "animstudio-$Environment-signalr"

$sqlPassword     = 'AnimStudio!Dev123'
$sqlPort         = 1433
$redisPassword   = 'AnimRedis!Dev123'
$redisPort       = 6379

$backendDir      = "$PSScriptRoot\..\backend\src\AnimStudio.API"
$frontendDir     = "$PSScriptRoot\..\frontend"
$pythonDir       = "$env:USERPROFILE\cartoon_automation"
$localOutputDir  = "$pythonDir\output"

# ── Helpers ────────────────────────────────────────────────────────────────────

function Write-Section($title) {
    Write-Host ""
    Write-Host "[ $title ]" -ForegroundColor White
}

function Write-OK($msg)   { Write-Host "  [OK  ] $msg" -ForegroundColor Green }
function Write-Skip($msg) { Write-Host "  [SKIP] $msg" -ForegroundColor Yellow }
function Write-Info($msg) { Write-Host "  [INFO] $msg" -ForegroundColor DarkGray }
function Write-Action($msg) {
    if ($WhatIf) { Write-Host "  [WHAT] $msg" -ForegroundColor DarkCyan }
    else         { Write-Host "  [  >> ] $msg" -ForegroundColor Cyan }
}

# ── Banner ─────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "AnimStudio — START LOCAL" -ForegroundColor Green
Write-Host "  Tier   : $(if ($FullStack) { 'Tier 2 — Full Stack (Docker + Azure SB + Storage)' } else { 'Tier 1 — Frontend + Backend (Docker only)' })" -ForegroundColor DarkGray
Write-Host "  Env    : $Environment  |  RG: $ResourceGroup" -ForegroundColor DarkGray
if ($WhatIf) { Write-Host "  ** WhatIf — Azure changes previewed only **" -ForegroundColor Yellow }

# ── Azure login check (only needed for FullStack) ─────────────────────────────

if ($FullStack) {
    $account = az account show --query user.name -o tsv 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Error "Not logged in to Azure. Run: az login"
    }
    Write-Host "  Azure  : $account" -ForegroundColor DarkGray
}

# ──────────────────────────────────────────────────────────────────────────────
# SECTION 1 — Docker: SQL Server
# ──────────────────────────────────────────────────────────────────────────────

Write-Section "SQL Server (Docker)"

$dockerRunning = docker info 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  [WARN] Docker Desktop is not running — please start it first." -ForegroundColor Red
    Write-Host "         SQL Server and Redis require Docker Desktop." -ForegroundColor DarkGray
} else {
    $sqlContainer = docker ps -a --filter "name=animstudio-sql" --format "{{.Names}}" 2>$null
    $sqlRunning   = docker ps   --filter "name=animstudio-sql" --format "{{.Names}}" 2>$null

    if ($sqlRunning -eq 'animstudio-sql') {
        Write-Skip "animstudio-sql is already running on port $sqlPort"
    } elseif ($sqlContainer -eq 'animstudio-sql') {
        Write-Action "Starting existing animstudio-sql container"
        if (-not $WhatIf) {
            docker start animstudio-sql | Out-Null
            Write-OK "animstudio-sql started (port $sqlPort)"
        }
    } else {
        Write-Action "Creating new animstudio-sql container (first run)"
        if (-not $WhatIf) {
            docker run `
                --name animstudio-sql `
                -e "ACCEPT_EULA=Y" `
                -e "SA_PASSWORD=$sqlPassword" `
                -p ${sqlPort}:1433 `
                -d mcr.microsoft.com/mssql/server:2022-latest | Out-Null
            Write-OK "animstudio-sql created and started (port $sqlPort)"
            Write-Info "First-time start — wait ~15s for SQL Server to initialise before starting the backend"
        }
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# SECTION 2 — Docker: Redis
# ──────────────────────────────────────────────────────────────────────────────

Write-Section "Redis (Docker)"

if ($LASTEXITCODE -eq 0 -or (docker info 2>$null)) {
    $redisContainer = docker ps -a --filter "name=animstudio-redis" --format "{{.Names}}" 2>$null
    $redisRunning   = docker ps   --filter "name=animstudio-redis" --format "{{.Names}}" 2>$null

    if ($redisRunning -eq 'animstudio-redis') {
        Write-Skip "animstudio-redis is already running on port $redisPort"
    } elseif ($redisContainer -eq 'animstudio-redis') {
        Write-Action "Starting existing animstudio-redis container"
        if (-not $WhatIf) {
            docker start animstudio-redis | Out-Null
            Write-OK "animstudio-redis started (port $redisPort)"
        }
    } else {
        Write-Action "Creating new animstudio-redis container (first run)"
        if (-not $WhatIf) {
            docker run `
                --name animstudio-redis `
                -p ${redisPort}:6379 `
                -d redis:7 redis-server --requirepass $redisPassword | Out-Null
            Write-OK "animstudio-redis created and started (port $redisPort)"
        }
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# SECTION 3 — Local output directory (Python worker file storage)
# ──────────────────────────────────────────────────────────────────────────────

Write-Section "Local Output Directory"

if (Test-Path $localOutputDir) {
    Write-Skip "$localOutputDir already exists"
} else {
    Write-Action "Creating $localOutputDir"
    if (-not $WhatIf) {
        New-Item -ItemType Directory -Force $localOutputDir | Out-Null
        Write-OK "Created $localOutputDir"
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# SECTION 4 — Frontend .env.local (write if missing)
# ──────────────────────────────────────────────────────────────────────────────

Write-Section "Frontend .env.local"

$envLocal = "$frontendDir\.env.local"
if (Test-Path $envLocal) {
    Write-Skip ".env.local already exists — not overwriting"
    Write-Info "  Path: $envLocal"
} else {
    Write-Action "Writing $envLocal"
    if (-not $WhatIf) {
        $envContent = @"
# AnimStudio local dev — generated by start-local.ps1
AUTH_SECRET=local-dev-secret-do-not-use-in-production
NEXTAUTH_URL=http://localhost:3000
AUTH_URL=http://localhost:3000
NEXT_PUBLIC_API_BASE_URL=http://localhost:5001
NEXT_PUBLIC_MOCK_DATA=false
"@
        Set-Content -Path $envLocal -Value $envContent -Encoding UTF8
        Write-OK "Written $envLocal"
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# SECTION 5 — Azure: SignalR + Service Bus (FullStack only)
# ──────────────────────────────────────────────────────────────────────────────

$sbConnectionString = $null

if ($FullStack) {

    # 5a. SignalR — ensure Standard_S1 for real-time hub
    Write-Section "SignalR (Azure)"

    $currentSku = az signalr show -g $ResourceGroup -n $signalRName `
                    --query 'sku.name' -o tsv 2>$null

    if ($currentSku -ne 'Free_F1') {
        Write-Skip "$signalRName — already on $currentSku"
    } else {
        Write-Action "$signalRName : Free_F1 → Standard_S1"
        if (-not $WhatIf) {
            az signalr update -g $ResourceGroup -n $signalRName `
                --sku Standard_S1 --unit-count 1 --output none
            Write-OK "$signalRName upgraded to Standard_S1"
        }
    }

    # 5b. Service Bus — verify namespace exists, fetch connection string from KV
    Write-Section "Service Bus (Azure)"

    $sbState = az servicebus namespace show -g $ResourceGroup -n $sbNamespace `
                 --query 'status' -o tsv 2>$null

    if ($sbState -eq 'Active') {
        Write-OK "$sbNamespace is Active"
    } else {
        Write-Host "  [WARN] Service Bus $sbNamespace status: $sbState" -ForegroundColor Yellow
        Write-Info "  Run deploy.ps1 first to provision the Service Bus namespace."
    }

    # Fetch the connection string from Key Vault
    Write-Info "Fetching ServiceBusConnectionString from Key Vault ($kvName)..."
    $sbConnectionString = az keyvault secret show `
                            --vault-name $kvName `
                            --name ServiceBusConnectionString `
                            --query 'value' -o tsv 2>$null

    if ($sbConnectionString -and $sbConnectionString -notmatch 'PLACEHOLDER') {
        Write-OK "Service Bus connection string retrieved from Key Vault"
    } else {
        Write-Host "  [WARN] Key Vault secret 'ServiceBusConnectionString' is missing or still a placeholder." -ForegroundColor Yellow
        Write-Info "  Run seed-keyvault-dev.ps1 first, or retrieve the connection string manually."
        Write-Info "  az servicebus namespace authorization-rule keys list --resource-group $ResourceGroup --namespace-name $sbNamespace --name RootManageSharedAccessKey --query primaryConnectionString -o tsv"
        $sbConnectionString = $null
    }

    # 5c. Storage — verify account exists
    Write-Section "Blob Storage (Azure)"

    $storState = az storage account show -g $ResourceGroup -n $storAccount `
                   --query 'statusOfPrimary' -o tsv 2>$null

    if ($storState -eq 'available') {
        Write-OK "$storAccount is available"
        # Ensure 'assets' container has public blob access (for local dev: browser can load images directly)
        $containerAccess = az storage container show --account-name $storAccount `
                             --name assets --query 'properties.publicAccess' -o tsv 2>$null
        if ($containerAccess -ne 'blob') {
            Write-Action "Setting assets container to public blob access (for local dev)"
            if (-not $WhatIf) {
                az storage container set-permission --account-name $storAccount `
                    --name assets --public-access blob --output none 2>$null
                Write-OK "assets container set to public blob access"
            }
        } else {
            Write-Skip "assets container already has public blob access"
        }
    } else {
        Write-Host "  [WARN] Storage account $storAccount status: $storState" -ForegroundColor Yellow
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# SUMMARY — Print startup commands for each project
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── Local Dev Ready ─────────────────────────────────────────────────" -ForegroundColor Green
Write-Host ""
Write-Host "  Start each project in a separate terminal:" -ForegroundColor White
Write-Host ""

# Backend
Write-Host "  1) Backend (.NET)" -ForegroundColor Cyan
Write-Host "     cd $backendDir"
if ($FullStack -and $sbConnectionString) {
    Write-Host "     dotnet user-secrets set `"ConnectionStrings:ServiceBus`" `"$sbConnectionString`""  -ForegroundColor DarkYellow
    Write-Host "     (run the user-secrets command once, then:)" -ForegroundColor DarkGray
}
Write-Host "     dotnet run"
Write-Host "     → http://localhost:5001/swagger"
Write-Host ""

# Frontend
Write-Host "  2) Frontend (Next.js)" -ForegroundColor Cyan
Write-Host "     cd $frontendDir"
Write-Host "     npm install   # first time only"
Write-Host "     npm run dev"
Write-Host "     → http://localhost:3000"
Write-Host ""

# Python worker (FullStack only)
if ($FullStack) {
    Write-Host "  3) Python Worker" -ForegroundColor Cyan
    Write-Host "     cd $pythonDir"
    Write-Host "     az login   # if not already logged in"
    Write-Host "     uv sync    # first time only"
    Write-Host "     animstudio_worker"
    Write-Host ""
    Write-Host "  Python worker environment (verify in $pythonDir\.env):" -ForegroundColor DarkGray
    Write-Host "     AZURE_SERVICE_BUS_NAMESPACE=$sbNamespace"
    Write-Host "     AZURE_STORAGE_ACCOUNT=$storAccount"
    Write-Host ""
    Write-Host "  Python worker config ($pythonDir\config\azure.yaml):" -ForegroundColor DarkGray
    Write-Host "     service_bus.namespace_fqdn: $sbNamespace.servicebus.windows.net"
    Write-Host "     blob_storage.account_url:   https://$storAccount.blob.core.windows.net"
}

Write-Host ""
Write-Host "  Dev login: http://localhost:3000  (no credentials needed in dev mode)" -ForegroundColor DarkGray
Write-Host "  API docs:  http://localhost:5001/swagger" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Run .\stop-local.ps1 to stop Docker containers when done." -ForegroundColor DarkYellow
Write-Host ""
