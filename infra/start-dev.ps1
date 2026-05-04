#Requires -Version 7.1
<#
.SYNOPSIS
    Starts AnimStudio dev resources after a stop-dev.ps1 shutdown.

.DESCRIPTION
    - Scales the API Container App back to min 1 replica.
    - If SignalR is on Free SKU, upgrades it back to Standard_S1.
    - If the Front Door endpoint is disabled, re-enables it.
    - Polls until the API is reachable before exiting.

.EXAMPLE
    .\start-dev.ps1
    .\start-dev.ps1 -WhatIf
    .\start-dev.ps1 -ResourceGroup animstudio-prod-rg -Environment prod
#>
param (
    [string] $ResourceGroup = 'animstudio-dev-rg',
    [string] $Environment   = 'dev',

    # Preview mode — print what would happen, make no changes
    [switch] $WhatIf
)

$ErrorActionPreference = 'Stop'

$apiApp      = "animstudio-$Environment-api"
$signalRName = "animstudio-$Environment-signalr"
$cdnProfile  = "animstudio-$Environment-cdn"
$cdnEndpoint = "animstudio-$Environment-assets"

function Write-Action($msg) {
    if ($WhatIf) { Write-Host "[WHATIF] $msg" -ForegroundColor DarkCyan }
    else         { Write-Host "  >>  $msg"    -ForegroundColor Cyan }
}

# ── Preflight ──────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "AnimStudio Dev — START" -ForegroundColor Green
Write-Host "Resource group : $ResourceGroup" -ForegroundColor DarkGray
if ($WhatIf) { Write-Host "** WhatIf — no changes will be made **" -ForegroundColor Yellow }
Write-Host ""

$account = az account show --query user.name -o tsv 2>$null
if ($LASTEXITCODE -ne 0) { Write-Error "Not logged in. Run: az login" }

# ── 1. API Container App → min 1 replica ─────────────────────────────────────

Write-Host "[ Container Apps ]" -ForegroundColor White

$apiMin = az containerapp show -g $ResourceGroup -n $apiApp `
            --query 'properties.template.scale.minReplicas' -o tsv 2>$null

if ($apiMin -ge 1) {
    Write-Host "  [SKIP] $apiApp — already at min $apiMin replicas" -ForegroundColor Yellow
} else {
    Write-Action "$apiApp : min-replicas 0 → 1"
    if (-not $WhatIf) {
        az containerapp update -g $ResourceGroup -n $apiApp `
            --min-replicas 1 --max-replicas 3 --output none
        Write-Host "  [OK  ] $apiApp — min replicas set to 1" -ForegroundColor Green
    }
}

# ── 2. SignalR — upgrade back to Standard_S1 if needed ────────────────────────

Write-Host ""
Write-Host "[ SignalR ]" -ForegroundColor White

$currentSku = az signalr show -g $ResourceGroup -n $signalRName `
                --query 'sku.name' -o tsv 2>$null

if ($currentSku -ne 'Free_F1') {
    Write-Host "  [SKIP] $signalRName — already on $currentSku" -ForegroundColor Yellow
} else {
    Write-Action "$signalRName : Free_F1 → Standard_S1"
    if (-not $WhatIf) {
        az signalr update -g $ResourceGroup -n $signalRName `
            --sku Standard_S1 --unit-count 1 --output none
        Write-Host "  [OK  ] $signalRName upgraded to Standard_S1" -ForegroundColor Green
    }
}

# ── 3. Front Door CDN endpoint — re-enable if disabled ────────────────────────

Write-Host ""
Write-Host "[ Front Door CDN ]" -ForegroundColor White

$endpointState = az afd endpoint show -g $ResourceGroup `
                    --profile-name $cdnProfile --endpoint-name $cdnEndpoint `
                    --query 'enabledState' -o tsv 2>$null

if ($endpointState -eq 'Enabled') {
    Write-Host "  [SKIP] $cdnEndpoint — already enabled" -ForegroundColor Yellow
} else {
    Write-Action "$cdnEndpoint : Disabled → Enabled"
    if (-not $WhatIf) {
        az afd endpoint update -g $ResourceGroup `
            --profile-name $cdnProfile --endpoint-name $cdnEndpoint `
            --enabled-state Enabled --output none
        Write-Host "  [OK  ] $cdnEndpoint re-enabled" -ForegroundColor Green
    }
}

# ── 4. Wait for API to be reachable ───────────────────────────────────────────

if (-not $WhatIf) {
    Write-Host ""
    Write-Host "[ Waiting for API to become ready ]" -ForegroundColor White

    $fqdn = az containerapp show -g $ResourceGroup -n $apiApp `
                --query 'properties.configuration.ingress.fqdn' -o tsv 2>$null
    $apiUrl = "https://$fqdn"

    Write-Host "  API URL: $apiUrl" -ForegroundColor DarkGray

    $attempts = 0
    $maxAttempts = 20
    $ready = $false

    while ($attempts -lt $maxAttempts -and -not $ready) {
        $attempts++
        try {
            $response = Invoke-WebRequest -Uri $apiUrl -TimeoutSec 5 -SkipCertificateCheck -ErrorAction Stop
            if ($response.StatusCode -lt 500) {
                $ready = $true
            }
        } catch { }

        if (-not $ready) {
            Write-Host "  [$attempts/$maxAttempts] Not ready yet — waiting 10s..." -ForegroundColor DarkGray
            Start-Sleep -Seconds 10
        }
    }

    if ($ready) {
        Write-Host "  [OK  ] API is responding at $apiUrl" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] API did not respond after $($maxAttempts * 10)s — may still be starting up" -ForegroundColor Yellow
        Write-Host "         Check: az containerapp show -g $ResourceGroup -n $apiApp --query 'properties.runningStatus'" -ForegroundColor DarkGray
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── All resources started ────────────────────────────────────────" -ForegroundColor Green

$fqdn = az containerapp show -g $ResourceGroup -n $apiApp `
            --query 'properties.configuration.ingress.fqdn' -o tsv 2>$null

Write-Host "  API       : https://$fqdn" -ForegroundColor Cyan

$swaName = az staticwebapp list -g $ResourceGroup --query '[0].defaultHostname' -o tsv 2>$null
if ($swaName) {
    Write-Host "  Frontend  : https://$swaName" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Run .\stop-dev.ps1 -Full when done to minimise costs." -ForegroundColor DarkYellow
Write-Host ""
