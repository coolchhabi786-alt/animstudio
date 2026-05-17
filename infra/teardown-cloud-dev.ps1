#Requires -Version 7.1
<#
.SYNOPSIS
    Deletes expensive Azure dev resources that are not needed during local development.

.DESCRIPTION
    Keeps only the two services consumed by locally-running code:
      • Service Bus  (animstudio-devsbus)   — jobs + completions queues
      • Storage      (animstudiodevstor)    — Python worker blob uploads

    Deletes everything else:
      Container Apps (API + Worker), Container App Environment, Log Analytics,
      VNet, Static Web App, Container Registry, CDN Profile, SignalR,
      SQL Server + AnimStudioDB (unless -SkipSQL), Key Vault (soft-delete + purge).

    Monthly savings after teardown: ~$128/month
    Remaining cost (minimal mode):  ~$12/month  (SB $10 + Storage $2)

    Re-provision with:  .\provision-cloud-dev.ps1

.EXAMPLE
    .\teardown-cloud-dev.ps1                     # Full teardown (prompts once to confirm)
    .\teardown-cloud-dev.ps1 -WhatIf             # Preview — no changes made
    .\teardown-cloud-dev.ps1 -SkipSQL            # Keep Azure SQL (Docker still used locally)
    .\teardown-cloud-dev.ps1 -Force              # Skip confirmation prompt
#>
param (
    [string] $ResourceGroup = 'animstudio-dev-rg',
    [string] $Environment   = 'dev',

    # Skip deleting the SQL Server and AnimStudioDB (already using docker-compose locally)
    [switch] $SkipSQL,

    # Skip confirmation prompt
    [switch] $Force,

    # Preview mode — print what would be deleted, make no changes
    [switch] $WhatIf
)

$ErrorActionPreference = 'Stop'

# ── Resource names ────────────────────────────────────────────────────────────

$apiApp        = "animstudio-$Environment-api"
$workerApp     = "animstudio-$Environment-worker"
$caEnvName     = "animstudio-$Environment-env"
$lawName       = "animstudio-$Environment-law"
$vnetName      = "animstudio-$Environment-vnet"
$swaName       = "animstudio-$Environment-web"
$acrName       = "animstudio${Environment}acr"
$cdnProfile    = "animstudio-$Environment-cdn"
$signalRName   = "animstudio-$Environment-signalr"
$sqlServerName = "animstudio-$Environment-sql"
$sqlDbName     = "AnimStudioDB"
$kvName        = "animstudio-$Environment-kv"
$kvLocation    = "eastus"   # Key Vault must be purged in the region it was created

# ── Helpers ───────────────────────────────────────────────────────────────────

function Write-Section($title) {
    Write-Host ""
    Write-Host "[ $title ]" -ForegroundColor White
}

function Write-Action($msg) {
    if ($WhatIf) { Write-Host "  [WHAT] $msg" -ForegroundColor DarkCyan }
    else         { Write-Host "  [  >> ] $msg" -ForegroundColor Cyan }
}

function Write-OK($msg)     { Write-Host "  [OK  ] $msg" -ForegroundColor Green }
function Write-Skip($msg)   { Write-Host "  [SKIP] $msg" -ForegroundColor Yellow }
function Write-Missing($msg){ Write-Host "  [----] $msg (not found — already deleted)" -ForegroundColor DarkGray }

function ResourceExists($type, $name) {
    $result = az resource show --resource-group $ResourceGroup --name $name `
                --resource-type $type --query "id" -o tsv 2>$null
    return ($LASTEXITCODE -eq 0 -and $result -ne '')
}

# ── Banner ────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "AnimStudio Dev — TEARDOWN CLOUD RESOURCES" -ForegroundColor Red
Write-Host "  Resource group : $ResourceGroup" -ForegroundColor DarkGray
Write-Host "  Environment    : $Environment" -ForegroundColor DarkGray
Write-Host "  Skip SQL       : $SkipSQL" -ForegroundColor DarkGray
if ($WhatIf) { Write-Host "  ** WhatIf — no changes will be made **" -ForegroundColor Yellow }
Write-Host ""
Write-Host "  Keeping (needed for local dev):" -ForegroundColor Green
Write-Host "    Service Bus  : animstudio-${Environment}sbus"   -ForegroundColor Green
Write-Host "    Storage      : animstudio${Environment}stor"    -ForegroundColor Green
Write-Host ""
Write-Host "  Deleting (not used during local dev):" -ForegroundColor Yellow
Write-Host "    Container App API     : $apiApp"       -ForegroundColor Yellow
Write-Host "    Container App Worker  : $workerApp"    -ForegroundColor Yellow
Write-Host "    Container App Env     : $caEnvName"    -ForegroundColor Yellow
Write-Host "    Log Analytics         : $lawName"      -ForegroundColor Yellow
Write-Host "    VNet                  : $vnetName"     -ForegroundColor Yellow
Write-Host "    Static Web App        : $swaName"      -ForegroundColor Yellow
Write-Host "    Container Registry    : $acrName"      -ForegroundColor Yellow
Write-Host "    CDN Profile           : $cdnProfile"   -ForegroundColor Yellow
Write-Host "    SignalR               : $signalRName"  -ForegroundColor Yellow
if (-not $SkipSQL) {
Write-Host "    SQL DB                : $sqlDbName on $sqlServerName" -ForegroundColor Yellow
Write-Host "    SQL Server            : $sqlServerName" -ForegroundColor Yellow
} else {
Write-Host "    SQL Server            : SKIPPED (-SkipSQL)" -ForegroundColor DarkGray
}
Write-Host "    Key Vault             : $kvName  (soft-delete + purge)" -ForegroundColor Yellow
Write-Host ""

# ── Preflight: Azure login ────────────────────────────────────────────────────

$account = az account show --query user.name -o tsv 2>$null
if ($LASTEXITCODE -ne 0) { Write-Error "Not logged in. Run: az login" }
Write-Host "  Logged in as: $account" -ForegroundColor DarkGray

# ── Confirmation ──────────────────────────────────────────────────────────────

if (-not $WhatIf -and -not $Force) {
    Write-Host ""
    $confirm = Read-Host '  Type "yes" to proceed with teardown'
    if ($confirm -ne 'yes') {
        Write-Host "  Teardown cancelled." -ForegroundColor Red
        exit 0
    }
}

# ── Step 1: Container App — API ───────────────────────────────────────────────

Write-Section "Container App: $apiApp"
if (ResourceExists 'Microsoft.App/containerApps' $apiApp) {
    Write-Action "Deleting $apiApp"
    if (-not $WhatIf) {
        az containerapp delete -g $ResourceGroup -n $apiApp --yes --output none
        Write-OK "$apiApp deleted"
    }
} else { Write-Missing $apiApp }

# ── Step 2: Container App — Worker ────────────────────────────────────────────

Write-Section "Container App: $workerApp"
if (ResourceExists 'Microsoft.App/containerApps' $workerApp) {
    Write-Action "Deleting $workerApp"
    if (-not $WhatIf) {
        az containerapp delete -g $ResourceGroup -n $workerApp --yes --output none
        Write-OK "$workerApp deleted"
    }
} else { Write-Missing $workerApp }

# ── Step 3: Container App Environment ─────────────────────────────────────────

Write-Section "Container App Environment: $caEnvName"
if (ResourceExists 'Microsoft.App/managedEnvironments' $caEnvName) {
    Write-Action "Deleting $caEnvName"
    if (-not $WhatIf) {
        az containerapp env delete -g $ResourceGroup -n $caEnvName --yes --output none
        Write-OK "$caEnvName deleted"
    }
} else { Write-Missing $caEnvName }

# ── Step 4: Log Analytics Workspace ───────────────────────────────────────────

Write-Section "Log Analytics Workspace: $lawName"
if (ResourceExists 'Microsoft.OperationalInsights/workspaces' $lawName) {
    Write-Action "Deleting $lawName"
    if (-not $WhatIf) {
        az monitor log-analytics workspace delete -g $ResourceGroup `
            --workspace-name $lawName --yes --force --output none
        Write-OK "$lawName deleted"
    }
} else { Write-Missing $lawName }

# ── Step 5: Virtual Network ────────────────────────────────────────────────────

Write-Section "Virtual Network: $vnetName"
if (ResourceExists 'Microsoft.Network/virtualNetworks' $vnetName) {
    Write-Action "Deleting $vnetName"
    if (-not $WhatIf) {
        az network vnet delete -g $ResourceGroup -n $vnetName --output none
        Write-OK "$vnetName deleted"
    }
} else { Write-Missing $vnetName }

# ── Step 6: Static Web App ─────────────────────────────────────────────────────

Write-Section "Static Web App: $swaName"
if (ResourceExists 'Microsoft.Web/staticSites' $swaName) {
    Write-Action "Deleting $swaName"
    if (-not $WhatIf) {
        az staticwebapp delete -g $ResourceGroup -n $swaName --yes --output none
        Write-OK "$swaName deleted"
    }
} else { Write-Missing $swaName }

# ── Step 7: Container Registry ────────────────────────────────────────────────

Write-Section "Container Registry: $acrName"
if (ResourceExists 'Microsoft.ContainerRegistry/registries' $acrName) {
    Write-Action "Deleting $acrName"
    if (-not $WhatIf) {
        az acr delete -g $ResourceGroup -n $acrName --yes --output none
        Write-OK "$acrName deleted"
    }
} else { Write-Missing $acrName }

# ── Step 8: CDN Profile (deletes child endpoints automatically) ───────────────

Write-Section "CDN Profile: $cdnProfile"
$cdnExists = az afd profile show -g $ResourceGroup --profile-name $cdnProfile `
               --query "name" -o tsv 2>$null
if ($LASTEXITCODE -eq 0 -and $cdnExists) {
    Write-Action "Deleting CDN profile $cdnProfile (child endpoints auto-deleted)"
    if (-not $WhatIf) {
        az afd profile delete -g $ResourceGroup --profile-name $cdnProfile --yes --output none
        Write-OK "$cdnProfile deleted"
    }
} else { Write-Missing $cdnProfile }

# ── Step 9: SignalR ────────────────────────────────────────────────────────────

Write-Section "SignalR Service: $signalRName"
if (ResourceExists 'Microsoft.SignalRService/SignalR' $signalRName) {
    Write-Action "Deleting $signalRName (~`$49/month Standard_S1)"
    if (-not $WhatIf) {
        az signalr delete -g $ResourceGroup -n $signalRName --yes --output none
        Write-OK "$signalRName deleted"
    }
} else { Write-Missing $signalRName }

# ── Steps 10–11: SQL Server + AnimStudioDB ────────────────────────────────────

if ($SkipSQL) {
    Write-Section "SQL Server: SKIPPED"
    Write-Skip "$sqlServerName — preserved (-SkipSQL flag set)"
} else {
    Write-Section "SQL Database: $sqlDbName on $sqlServerName"
    $dbExists = az sql db show -g $ResourceGroup -s $sqlServerName -n $sqlDbName `
                  --query "name" -o tsv 2>$null
    if ($LASTEXITCODE -eq 0 -and $dbExists) {
        Write-Action "Deleting database $sqlDbName"
        if (-not $WhatIf) {
            az sql db delete -g $ResourceGroup -s $sqlServerName -n $sqlDbName --yes --output none
            Write-OK "$sqlDbName deleted"
        }
    } else { Write-Missing "$sqlDbName (or server $sqlServerName)" }

    Write-Section "SQL Server: $sqlServerName"
    if (ResourceExists 'Microsoft.Sql/servers' $sqlServerName) {
        Write-Action "Deleting SQL server $sqlServerName"
        if (-not $WhatIf) {
            az sql server delete -g $ResourceGroup -n $sqlServerName --yes --output none
            Write-OK "$sqlServerName deleted"
        }
    } else { Write-Missing $sqlServerName }
}

# ── Steps 12–13: Key Vault (soft-delete then purge) ───────────────────────────

Write-Section "Key Vault: $kvName"
if (ResourceExists 'Microsoft.KeyVault/vaults' $kvName) {
    Write-Action "Soft-deleting $kvName"
    if (-not $WhatIf) {
        az keyvault delete -g $ResourceGroup -n $kvName --output none
        Write-OK "$kvName soft-deleted"
    }
    Write-Action "Purging $kvName (frees the name for reprovision)"
    if (-not $WhatIf) {
        az keyvault purge -n $kvName --location $kvLocation --output none
        Write-OK "$kvName purged"
    }
} else {
    # It may be in soft-deleted state already — try to purge just in case
    $softDeleted = az keyvault list-deleted --query "[?name=='$kvName'].name" -o tsv 2>$null
    if ($softDeleted -eq $kvName) {
        Write-Action "$kvName is already soft-deleted — purging"
        if (-not $WhatIf) {
            az keyvault purge -n $kvName --location $kvLocation --output none
            Write-OK "$kvName purged"
        }
    } else {
        Write-Missing $kvName
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── Teardown complete ────────────────────────────────────────────────" -ForegroundColor Green
Write-Host ""
Write-Host "  Still running (monthly cost ~`$12/month):" -ForegroundColor Cyan
Write-Host "    Service Bus Standard  : animstudio-${Environment}sbus  ~`$10/month" -ForegroundColor Cyan
Write-Host "    Storage Account (LRS) : animstudio${Environment}stor   ~`$2/month"  -ForegroundColor Cyan
Write-Host ""
Write-Host "  Estimated savings:  ~`$128/month" -ForegroundColor Green
Write-Host ""
Write-Host "  Your local dev setup is unaffected:" -ForegroundColor White
Write-Host "    docker-compose  → SQL Server (localhost:1433) + Redis (localhost:6379)"
Write-Host "    dotnet run      → API on http://localhost:5001"
Write-Host "    npm run dev     → Frontend on http://localhost:3000"
Write-Host "    uv run worker   → Python worker (uses Azure SB + Storage as before)"
Write-Host ""
Write-Host "  Re-provision full dev environment: .\provision-cloud-dev.ps1" -ForegroundColor DarkYellow
Write-Host ""
